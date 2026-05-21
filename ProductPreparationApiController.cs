using Kalite.API.Context;
using Kalite.API.Dtos.ProductPreparationDto;
using Kalite.API.Entitity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Kalite.API.Dtos.ProductPreparationDto.ProductPreparationDto;

namespace Kalite.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductPreparationApiController : ControllerBase
    {
        private readonly ApiContext _context;

        public ProductPreparationApiController(ApiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string? productName, DateTime? date)
        {
            var query = _context.ProductPreparations.AsQueryable();

            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(x => x.ProductName.Contains(productName));
            }

            if (date.HasValue)
            {
                query = query.Where(x => x.Date.Date == date.Value.Date);
            }

            var values = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
            var dtos = values.Select(x => new ResultProductPreparationDto
            {
                Id = x.Id,
                DocumentNo = x.DocumentNo,
                PublishDate = x.PublishDate,
                RevisionNo = x.RevisionNo,
                RevisionDate = x.RevisionDate,
                PageNo = x.PageNo,
                Date = x.Date,
                ProductName = x.ProductName,
                BatchNo = x.BatchNo,
                DoughTemp = x.DoughTemp,
                DoughPh = x.DoughPh,
                AmbientTemp = x.AmbientTemp,
                FillingTemp = x.FillingTemp,
                Control = x.Control,
                IngredientsData = x.IngredientsData,
                TotalAmount = x.TotalAmount,
                PreparedBy = x.PreparedBy,
                ApprovedBy = x.ApprovedBy,
                CreatedAt = x.CreatedAt
            }).ToList();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var value = await _context.ProductPreparations.FindAsync(id);
            if (value == null) return NotFound();

            var dto = new ResultProductPreparationDto
            {
                Id = value.Id,
                DocumentNo = value.DocumentNo,
                PublishDate = value.PublishDate,
                RevisionNo = value.RevisionNo,
                RevisionDate = value.RevisionDate,
                PageNo = value.PageNo,
                Date = value.Date,
                ProductName = value.ProductName,
                BatchNo = value.BatchNo,
                DoughTemp = value.DoughTemp,
                DoughPh = value.DoughPh,
                AmbientTemp = value.AmbientTemp,
                FillingTemp = value.FillingTemp,
                Control = value.Control,
                IngredientsData = value.IngredientsData,
                TotalAmount = value.TotalAmount,
                PreparedBy = value.PreparedBy,
                ApprovedBy = value.ApprovedBy,
                CreatedAt = value.CreatedAt
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductPreparationDto dto)
        {
            if (dto == null) return BadRequest("Geçersiz veri");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Ürün Hazırlama (Üretim) Kaydını Oluştur
                var entity = new ProductPreparation
                {
                    DocumentNo = dto.DocumentNo,
                    PublishDate = dto.PublishDate,
                    RevisionNo = dto.RevisionNo,
                    RevisionDate = dto.RevisionDate,
                    PageNo = dto.PageNo,
                    Date = dto.Date,
                    ProductName = dto.ProductName,
                    BatchNo = dto.BatchNo, // Bu bizim Lot numaramız olacak
                    DoughTemp = dto.DoughTemp,
                    DoughPh = dto.DoughPh,
                    AmbientTemp = dto.AmbientTemp,
                    FillingTemp = dto.FillingTemp,
                    Control = dto.Control,
                    IngredientsData = dto.IngredientsData,
                    TotalAmount = dto.TotalAmount,
                    PreparedBy = dto.PreparedBy,
                    ApprovedBy = dto.ApprovedBy,
                    CreatedAt = DateTime.Now
                };
                _context.ProductPreparations.Add(entity);
                await _context.SaveChangesAsync(); // ID oluşması için kaydediyoruz

                // 2. Hammadde Stok Düşüm Mantığı
                if (!string.IsNullOrEmpty(dto.IngredientsData))
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var ingredients = System.Text.Json.JsonSerializer.Deserialize<List<IngredientItemDto>>(dto.IngredientsData, options);

                    if (ingredients != null)
                    {
                        foreach (var item in ingredients)
                        {
                            var remainingToDeduct = (double)item.Amount;
                            if (remainingToDeduct <= 0) continue;

                            var itemName = item.Name?.Trim();
                            var itemBatch = item.BatchNo?.Trim();
                            var stockId = item.StockId;

                            // A. Eğer StockId varsa doğrudan o kayıttan düş
                            if (stockId.HasValue && stockId.Value > 0)
                            {
                                var specificStock = await _context.Stocks.FindAsync(stockId.Value);
                                if (specificStock != null)
                                {
                                    if (specificStock.Quantity >= remainingToDeduct) { specificStock.Quantity -= remainingToDeduct; remainingToDeduct = 0; }
                                    else { remainingToDeduct -= specificStock.Quantity; specificStock.Quantity = 0; }
                                    _context.Update(specificStock);
                                }
                            }

                            // B. Eğer hala düşülecek miktar varsa Lot No ve İsim eşleşmesine bak
                            if (remainingToDeduct > 0)
                            {
                                var matchingStocks = await _context.Stocks
                                    .Where(x => x.StockName == itemName && x.LotNo == itemBatch && x.Quantity > 0)
                                    .OrderBy(x => x.Id)
                                    .ToListAsync();

                                foreach (var stock in matchingStocks)
                                {
                                    if (remainingToDeduct <= 0) break;
                                    if (stock.Quantity >= remainingToDeduct) { stock.Quantity -= remainingToDeduct; remainingToDeduct = 0; }
                                    else { remainingToDeduct -= stock.Quantity; stock.Quantity = 0; }
                                    _context.Update(stock);
                                }
                            }

                            // C. Hala düşülecek miktar varsa (Lot eşleşmediyse) aynı isimdeki en eski stoktan düş (FIFO)
                            if (remainingToDeduct > 0)
                            {
                                var fallbackStocks = await _context.Stocks
                                    .Where(x => x.StockName == itemName && x.Quantity > 0)
                                    .OrderBy(x => x.Id)
                                    .ToListAsync();

                                foreach (var stock in fallbackStocks)
                                {
                                    if (remainingToDeduct <= 0) break;
                                    if (stock.Quantity >= remainingToDeduct) { stock.Quantity -= remainingToDeduct; remainingToDeduct = 0; }
                                    else { remainingToDeduct -= stock.Quantity; stock.Quantity = 0; }
                                    _context.Update(stock);
                                }
                            }
                        }
                    }
                }

                // 3. 🔥 İZLENEBİLİRLİK İÇİN ETİKET (LABEL) OLUŞTURMA
                // Bu adım hazırlanan ürünün "Label Index" sayfasında görünmesini sağlar.
                var newLabel = new ProductLabel
                {
                    LotNo = dto.BatchNo, // Üretim Parti No = Etiket Lot No
                    StockName = dto.ProductName,
                    Quantity = (double)dto.TotalAmount,
                    DepoId = dto.DepoId, // UI'dan gelen hedef depo
                    CreatedAt = DateTime.Now,
                    AktifMi = true,
                    Barcod = dto.BatchNo,
                    GoodsReceiptId = null, // 🔥 Üretim ürünü olduğu için Mal Kabul ID'si yok
                    ParentLotNo = "URETIM" // İzlenebilirlik için bu etiketin bir üretimden geldiğini işaretleyebilirsinizkonsol
                    // Bu ürünün bir üretimden geldiğini anlamak için gerekirse ek alan kullanılabilir
                };
                _context.ProductLabels.Add(newLabel);
                await _context.SaveChangesAsync();

                // 4. 🔥 ÜRETİLEN ÜRÜNÜ STOK TABLOSUNA EKLEME
                // Böylece üretilen ürün de stoklarda miktar olarak görünür.
                var targetDepo = await _context.Depos.FindAsync(dto.DepoId);
                var finishedProductStock = new Stock
                {
                    StockName = dto.ProductName,
                    LotNo = dto.BatchNo,
                    Quantity = (double)dto.TotalAmount,
                    DepoAdi = targetDepo?.DepoAdi ?? "Üretim Deposu",
                    CreatedAt = DateTime.Now
                };
                _context.Stocks.Add(finishedProductStock);

                // 5. 🔥 İLK TRANSFER KAYDI (Geçmiş Hareketler için)
                var initialTransfer = new Transfer
                {
                    ProductLabelId = newLabel.Id,
                    DepoId = dto.DepoId,
                    Tarih = DateTime.Now
                };
                _context.Transfers.Add(initialTransfer);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = "Üretim başarıyla kaydedildi, stoklar düşüldü ve ürün etiketi oluşturuldu.", LotNo = dto.BatchNo });
            }
            catch (System.Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Hata: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateProductPreparationDto dto)
        {
            var value = await _context.ProductPreparations.FindAsync(dto.Id);
            if (value == null) return NotFound();

            value.DocumentNo = dto.DocumentNo;
            value.PublishDate = dto.PublishDate;
            value.RevisionNo = dto.RevisionNo;
            value.RevisionDate = dto.RevisionDate;
            value.PageNo = dto.PageNo;
            value.Date = dto.Date;
            value.ProductName = dto.ProductName;
            value.BatchNo = dto.BatchNo;
            value.DoughTemp = dto.DoughTemp;
            value.DoughPh = dto.DoughPh;
            value.AmbientTemp = dto.AmbientTemp;
            value.FillingTemp = dto.FillingTemp;
            value.Control = dto.Control;
            value.IngredientsData = dto.IngredientsData;
            value.TotalAmount = dto.TotalAmount;
            value.PreparedBy = dto.PreparedBy;
            value.ApprovedBy = dto.ApprovedBy;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var value = await _context.ProductPreparations.FindAsync(id);
                if (value == null) return NotFound();

                // Stock Reversion Logic
                if (!string.IsNullOrEmpty(value.IngredientsData))
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var ingredients = System.Text.Json.JsonSerializer.Deserialize<List<IngredientItemDto>>(value.IngredientsData, options);
                    if (ingredients != null)
                    {
                        foreach (var item in ingredients)
                        {
                            var amountToRevert = (double)item.Amount;
                            if (amountToRevert <= 0) continue;

                            var itemName = item.Name?.Trim();
                            var itemBatch = item.BatchNo?.Trim();
                            var stockId = item.StockId;

                            Stock stock = null;
                            if (stockId.HasValue && stockId.Value > 0)
                            {
                                stock = await _context.Stocks.FindAsync(stockId.Value);
                            }

                            if (stock == null)
                            {
                                stock = await _context.Stocks
                                    .Where(x => x.StockName == itemName && x.LotNo == itemBatch)
                                    .OrderByDescending(x => x.Id)
                                    .FirstOrDefaultAsync();
                            }

                            if (stock != null)
                            {
                                stock.Quantity += amountToRevert;
                                _context.Update(stock);
                            }
                            else
                            {
                                // If exact lot not found (unlikely), try finding any record for this product to put it back
                                var fallbackStock = await _context.Stocks
                                    .Where(x => x.StockName == itemName)
                                    .OrderByDescending(x => x.Id)
                                    .FirstOrDefaultAsync();

                                if (fallbackStock != null)
                                {
                                    fallbackStock.Quantity += amountToRevert;
                                    _context.Update(fallbackStock);
                                }
                            }
                        }
                    }
                }

                _context.ProductPreparations.Remove(value);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok();
            }
            catch (System.Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Silme işlemi sırasında hata oluştu: {ex.Message}");
            }
        }
    }
}

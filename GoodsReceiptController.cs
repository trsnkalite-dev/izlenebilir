using Kalite.API.Context;
using Kalite.API.Dtos.LabelDto;
using Kalite.API.Entitity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Kalite.API.Dtos.GoodsReceiptDtos.GoodsReceiptDto;
using static Kalite.API.Dtos.ProductPreparationDto.ProductPreparationDto;

namespace Kalite.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoodsReceiptController : ControllerBase

     {
        private readonly ApiContext _context;
        private static readonly System.Threading.SemaphoreSlim _semaphore = new System.Threading.SemaphoreSlim(1, 1);
        public GoodsReceiptController(ApiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string? companyName, DateTime? acceptanceDate, string? warehouseName, string? stockName)
        {
            var query = _context.GoodsReceipts.AsQueryable();

            if (!string.IsNullOrEmpty(companyName))
            {
                query = query.Where(x => x.CompanyName.Contains(companyName));
            }

            if (!string.IsNullOrEmpty(stockName))
            {
                query = query.Where(x => x.StockName.Contains(stockName));
            }

            if (!string.IsNullOrEmpty(warehouseName))
            {
                query = query.Where(x => x.DepoAdi == warehouseName);
            }

            if (acceptanceDate.HasValue)
            {
                query = query.Where(x => x.AcceptanceDate.Value.Date == acceptanceDate.Value.Date);
            }

            var values = await query
                .OrderBy(x => x.StockName)
                .ToListAsync();

            var result = values.Select(x => new ResultGoodsReceiptDto
            {
                Id = x.Id,
                AcceptanceDate = x.AcceptanceDate,
                StockName = x.StockName,
                StockKodu = x.StockKodu,
                Barcod = x.Barcod,
                CompanyName = x.CompanyName ?? "",
                Quantity = x.Quantity,
                HalalFoodSafetyCompliance = x.HalalFoodSafetyCompliance ?? "",
                PackagingAppearance = x.PackagingAppearance ?? "",
                ExpiryDate = x.ExpiryDate,
                VehicleCleanliness = x.VehicleCleanliness ?? "",
                AcceptanceStatus = x.AcceptanceStatus ?? "",
                Temperature = x.Temperature ?? "",
                LotNo = x.LotNo,
                DepoAdi = x.DepoAdi,
                ReceivedBy = x.ReceivedBy ?? "",

                Grup = x.Grup,
                AllergenInfo = x.AllergenInfo
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var value = await _context.GoodsReceipts.FindAsync(id);
            if (value == null) return NotFound();

            var result = new ResultGoodsReceiptDto
            {
                Id = value.Id,
                AcceptanceDate = value.AcceptanceDate,
                StockName = value.StockName,
                StockKodu = value.StockKodu,
                Barcod = value.Barcod,
                CompanyName = value.CompanyName ?? "",
                Quantity = value.Quantity,
                HalalFoodSafetyCompliance = value.HalalFoodSafetyCompliance ?? "",
                PackagingAppearance = value.PackagingAppearance ?? "",
                ExpiryDate = value.ExpiryDate,
                VehicleCleanliness = value.VehicleCleanliness ?? "",
                AcceptanceStatus = value.AcceptanceStatus ?? "",
                Temperature = value.Temperature ?? "",
                LotNo = value.LotNo,
                DepoAdi = value.DepoAdi,
                ReceivedBy = value.ReceivedBy ?? "",

                Grup = value.Grup,
                AllergenInfo = value.AllergenInfo
            };

            return Ok(result);
        }


        [HttpGet("barcode/{lotNo}")]
        public async Task<ActionResult<ResultGoodsReceiptDto>> GetByBarcode(string lotNo)
        {
            if (string.IsNullOrWhiteSpace(lotNo))
                return BadRequest("LotNo boş");

            var label = await _context.GoodsReceipts
                .AsNoTracking()
                .Where(x => x.LotNo == lotNo)
                .Select(x => new ResultGoodsReceiptDto
                {
                    LotNo = x.LotNo,
                    StockName = x.StockName,
                    Quantity = x.Quantity ,

                    DepoId = x.DepoId,

                    // 🔥 KRİTİK
                    CompanyName = x.CompanyName,
                    CreatedDate = DateTime.Now,
                    ExpiryDate = x.ExpiryDate
                })
                .FirstOrDefaultAsync();

            if (label == null)
                return NotFound("Etiket bulunamadı");

            return Ok(label);
        }


        [HttpPost("latest-bulk")]
        public async Task<IActionResult> GetLatestBulk([FromBody] List<string> productNames)
        {
            var results = new Dictionary<string, object>();

            foreach (var name in productNames)
            {
                // Find all records for this product that are NOT rejected
                var query = _context.GoodsReceipts.Where(x => x.StockName == name && x.AcceptanceStatus != "Red");

                // Prioritize entries that HAVE quantity left, then the latest one
                var latestRecord = await query
                    .OrderByDescending(x => x.Quantity > 0)
                    .ThenByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                var totalQuantity = await query.SumAsync(x => x.Quantity);

                if (latestRecord != null)
                {
                    results[name] = new
                    {
                        Id = latestRecord.Id,
                        LotNo = latestRecord.LotNo,
                        DepoAdi = latestRecord.DepoAdi,
                        ExpiryDate = latestRecord.ExpiryDate?.ToString("dd.MM.yyyy"),
                        CurrentQuantity = totalQuantity // Returning the total sum of all lots for this product
                    };
                }
            }

            return Ok(results);
        }

      
        [HttpPost]
        public async Task<IActionResult> Create(CreateGoodsReceiptDto dto)
        {
            if (dto == null) return BadRequest();

            await _semaphore.WaitAsync();
            try
            {
                var stockKodu = string.IsNullOrWhiteSpace(dto.StockKodu) ? null : dto.StockKodu.Trim();
                var lotNo = string.IsNullOrWhiteSpace(dto.LotNo) ? null : dto.LotNo.Trim();
                var status = dto.AcceptanceStatus?.Trim();

                // 1. Double Submission Check (Strict time-based deduplication)
                // Check if an identical submission happened very recently (within 15 seconds)
                var recentlyAdded = await _context.GoodsReceipts
                    .Where(x => x.StockKodu == stockKodu && x.LotNo == lotNo && x.AcceptanceStatus == status)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (recentlyAdded != null && recentlyAdded.Quantity == dto.Quantity &&
                    recentlyAdded.CreatedAt.HasValue && (DateTime.Now - recentlyAdded.CreatedAt.Value).TotalSeconds < 15)
                {
                    return Ok(); // Skip duplicate submission
                }

                // 2. Aggregate logic (Merging into existing Lot Record to maintain "Current Stock" view)
                // We use a robust check for null/empty matches
                var existingReceipt = await _context.GoodsReceipts.FirstOrDefaultAsync(x =>
                    (x.StockKodu == stockKodu || (string.IsNullOrEmpty(x.StockKodu) && string.IsNullOrEmpty(stockKodu))) &&
                    (x.LotNo == lotNo || (string.IsNullOrEmpty(x.LotNo) && string.IsNullOrEmpty(lotNo))) &&
                    x.AcceptanceStatus == status);

                if (existingReceipt != null)
                {
                    // Update existing record (Cumulative Stock)
                    existingReceipt.Quantity += dto.Quantity;
                    existingReceipt.CompanyName = dto.CompanyName;
                    existingReceipt.DepoAdi = dto.DepoAdi;
                    existingReceipt.ExpiryDate = dto.ExpiryDate ?? existingReceipt.ExpiryDate;
                    existingReceipt.AcceptanceDate = dto.AcceptanceDate;
                    existingReceipt.Grup = dto.Grup;
                    existingReceipt.AllergenInfo = dto.AllergenInfo;
                    existingReceipt.Temperature = dto.Temperature;
                    existingReceipt.ReceivedBy = dto.ReceivedBy;

                    existingReceipt.CreatedAt = DateTime.Now; // Update timestamp for deduplication helper
                    _context.Update(existingReceipt);
                }
                else
                {
                    // Create New Record
                    var receipt = new GoodsReceipt
                    {
                        StockName = dto.StockName,
                        StockKodu = stockKodu,
                        Barcod = dto.Barcod,
                        CompanyName = dto.CompanyName,
                        Quantity = dto.Quantity,
                        LotNo = lotNo,
                        DepoAdi = dto.DepoAdi,
                        AcceptanceDate = dto.AcceptanceDate,
                        HalalFoodSafetyCompliance = dto.HalalFoodSafetyCompliance,
                        PackagingAppearance = dto.PackagingAppearance,
                        ExpiryDate = dto.ExpiryDate,
                        VehicleCleanliness = dto.VehicleCleanliness,
                        AcceptanceStatus = status,
                        Temperature = dto.Temperature,
                        ReceivedBy = dto.ReceivedBy,

                        Grup = dto.Grup,
                        AllergenInfo = dto.AllergenInfo,
                        CreatedAt = DateTime.Now
                    };
                    _context.GoodsReceipts.Add(receipt);
                }

                // --- Sync with Stock Table ---
                if (status == "Kabul")
                {
                    var stock = await _context.Stocks.FirstOrDefaultAsync(x =>
                        x.StockName == dto.StockName &&
                        x.DepoAdi == dto.DepoAdi);

                    if (stock != null)
                    {
                        stock.Quantity +=Convert.ToInt32( dto.Quantity);
                        stock.ExpiryDate = dto.ExpiryDate;
                        stock.CompanyName = dto.CompanyName;
                        stock.LotNo = lotNo;
                        stock.StockKodu = stockKodu;
                        stock.Barcod = dto.Barcod;
                        _context.Update(stock);
                    }
                    else
                    {
                        var newStock = new Stock
                        {
                            StockName = dto.StockName,
                            StockKodu = stockKodu,
                            Barcod = dto.Barcod,
                            LotNo = lotNo,
                            ExpiryDate = dto.ExpiryDate,
                            AllergenInfo = dto.AllergenInfo,
                            CompanyName = dto.CompanyName,
                            DepoAdi = dto.DepoAdi,
                            Quantity = Convert.ToInt32(dto.Quantity),
                            Grup = dto.Grup
                        };
                        _context.Stocks.Add(newStock);
                    }
                }
                // -----------------------------

                await _context.SaveChangesAsync();
                return Ok();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateGoodsReceiptDto dto)
        {
            var value = await _context.GoodsReceipts.FindAsync(dto.Id);
            if (value == null) return NotFound();

            var oldQuantity = value.Quantity;
            var oldStatus = value.AcceptanceStatus;
            var oldLot = value.LotNo;
            var oldDepo = value.DepoAdi;
            var oldKodu = value.StockKodu;

            // Update Transaction Record
            value.AcceptanceDate = dto.AcceptanceDate;
            value.StockName = dto.StockName;
            value.StockKodu = dto.StockKodu;
            value.Barcod = dto.Barcod;
            value.CompanyName = dto.CompanyName;
            value.Quantity = dto.Quantity;
            value.HalalFoodSafetyCompliance = dto.HalalFoodSafetyCompliance;
            value.PackagingAppearance = dto.PackagingAppearance;
            value.ExpiryDate = dto.ExpiryDate;
            value.VehicleCleanliness = dto.VehicleCleanliness;
            value.AcceptanceStatus = dto.AcceptanceStatus;
            value.Temperature = dto.Temperature;
            value.LotNo = dto.LotNo;
            value.DepoAdi = dto.DepoAdi;
            value.ReceivedBy = dto.ReceivedBy;

            value.Grup = dto.Grup;
            value.AllergenInfo = dto.AllergenInfo;

            // --- Stock Adjustment ---
            // If it was accepted before, revert old quantity
            if (oldStatus == "Kabul")
            {
                var oldStock = await _context.Stocks.FirstOrDefaultAsync(x => x.StockName == value.StockName && x.DepoAdi == oldDepo);
                if (oldStock != null)
                {
                  oldStock.Quantity -= Convert.ToInt32 ( oldQuantity);
                }
            }

            // If it is accepted now, add new quantity
            if (dto.AcceptanceStatus == "Kabul")
            {
                var newStock = await _context.Stocks.FirstOrDefaultAsync(x => x.StockName == dto.StockName && x.DepoAdi == dto.DepoAdi);
                if (newStock != null)
                {
                    newStock.Quantity += Convert.ToInt32 (dto.Quantity);
                    newStock.ExpiryDate = dto.ExpiryDate;
                    newStock.CompanyName = dto.CompanyName;
                    newStock.LotNo = dto.LotNo;
                    newStock.StockKodu = dto.StockKodu;
                }
                else
                {
                    _context.Stocks.Add(new Stock
                    {
                        StockName = dto.StockName,
                        StockKodu = dto.StockKodu,
                        Barcod = dto.Barcod,
                        LotNo = dto.LotNo,
                        ExpiryDate = dto.ExpiryDate,
                        AllergenInfo = dto.AllergenInfo,
                        CompanyName = dto.CompanyName,
                        DepoAdi = dto.DepoAdi,
                        Quantity = Convert.ToInt32 (dto.Quantity),
                        Grup = dto.Grup
                    });
                }
            }
            // ------------------------

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var value = await _context.GoodsReceipts.FindAsync(id);
            if (value == null) return NotFound();

            // --- Stock Sync ---
            if (value.AcceptanceStatus == "Kabul")
            {
                var stock = await _context.Stocks.FirstOrDefaultAsync(x => x.StockName == value.StockName && x.DepoAdi == value.DepoAdi);
                if (stock != null)
                {
                    stock.Quantity -= Convert.ToInt32 (value.Quantity);
                }
            }
            // ------------------

            _context.GoodsReceipts.Remove(value);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
 
}


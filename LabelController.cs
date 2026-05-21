using Kalite.API.Context;
using Kalite.API.Dtos.LabelDto;
using Kalite.API.Entitity;
using Kalite.API.Services.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Kalite.API.Dtos.LabelDto.LabelDto;

namespace Kalite.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabelController : ControllerBase
    {
        private readonly IProductLabelService _service;
        private readonly ApiContext _context;
        private readonly ILogService _logService;

        public LabelController(IProductLabelService service, ApiContext context, ILogService logService)
        {
            _service = service;
            _context = context;
            _logService = logService;
        }






        // ✅ ETİKET OLUŞTUR

        [HttpPost("generate")]
        public async Task<IActionResult> Create([FromBody] LabelDto.CreateLabelDto dto)
        {
            if (dto == null)
                return BadRequest("Veri boş");

            if (dto.GoodsReceiptId <= 0)
                return BadRequest("urun seçiniz");

            if (dto.DepoId <= 0)
                return BadRequest("Depo seçiniz");

            var result = await _service.CreateLabel(
                dto.GoodsReceiptId,
                dto.Quantity ?? 0,
                dto.DepoId
            );

            return Ok(result);
        }


        [HttpGet]
        public async Task<ActionResult<List<LabelDto.ProductLabelResponseDto>>> GetAll()
        {
            try
            {

                var labels = await _context.ProductLabels
                  .AsNoTracking()
                  .Select(x => new LabelDto.ProductLabelResponseDto
                  {
                      LotNo = x.LotNo,
                      StockName = x.StockName,
                     Quantity = x.Quantity ?? 0,
                     
                      DepoId = x.DepoId,
                      CompanyName= x.CompanyName,
                     CreatedAt = x.CreatedAt,
                      ExpiryDate = x.ExpiryDate,
                  })
                  .ToListAsync();

                return Ok(labels);




            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, ex.Message);
            }

        }



        [HttpGet("barcode/{lotNo}")]
        public async Task<ActionResult<LabelDto.ProductLabelResponseDto>> GetByBarcode(string lotNo)
        {
            if (string.IsNullOrWhiteSpace(lotNo)) return BadRequest("LotNo boş");

            var label = await _context.ProductLabels
                .AsNoTracking()
                .Where(x => x.LotNo == lotNo)
                .Select(x => new LabelDto.ProductLabelResponseDto
                {
                    LotNo = x.LotNo,
                    StockName = x.StockName,
                    Quantity = x.Quantity ?? 0,
                    DepoId = x.DepoId,
                    CompanyName = x.CompanyName,
                    CreatedAt = x.CreatedAt,
                    ExpiryDate = x.ExpiryDate,
                    ParentLotNo = x.ParentLotNo
                })
                .FirstOrDefaultAsync();

            if (label == null) return NotFound("Etiket bulunamadı");

            // 🔥 ÜRETİM İZLENEBİLİRLİĞİ: Bu Lot bir üretime mi ait?
            var preparation = await _context.ProductPreparations
                .FirstOrDefaultAsync(x => x.BatchNo == lotNo);

            if (preparation != null && !string.IsNullOrEmpty(preparation.IngredientsData))
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var ingredients = System.Text.Json.JsonSerializer.Deserialize<List<IngredientDetailDto>>(preparation.IngredientsData, options);
                label.ProductionIngredients = ingredients;
            }

            return Ok(label);
        }

        // ✅ TRANSFER
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] LabelDto.TransferRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Veri boş");

            if (string.IsNullOrWhiteSpace(dto.LotNo))
                return BadRequest("LotNo boş olamaz");

            if (dto.DepoId <= 0)
                return BadRequest("Geçerli DepoId giriniz");

            await _service.Transfer(dto.LotNo, dto.DepoId);

            return Ok("Transfer başarılı");
        }




        [HttpPost("process-label")]
        public async Task<IActionResult> ProcessLabel([FromBody] LabelDto.ProcessLabelDto dto)
        {
            // 1. Ana gövdeyi bul
            var parent = await _context.ProductLabels
                .FirstOrDefaultAsync(x => x.LotNo == dto.ParentLotNo && x.AktifMi);

            if (parent == null) return NotFound("Ana gövde lotu bulunamadı.");
            if (parent.Quantity< dto.YeniMiktar) return BadRequest("Ana gövdede yeterli stok yok!");

            // 2. Ana gövde miktarını düş
            parent.Quantity -= dto.YeniMiktar;
            if (parent.Quantity <= 0) parent.AktifMi = false;

            // 3. Yeni parça etiketini oluştur
            var newLabel = new ProductLabel
            {
                LotNo = DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString().Substring(0, 4).ToUpper(),
                ParentLotNo = parent.LotNo,
               StockName = dto.YeniUrunAdi,
               Quantity = dto.YeniMiktar,
                DepoId = dto.HedefDepoId,
                CompanyName = parent.CompanyName,
               
               CreatedAt= DateTime.Now,
             ExpiryDate= parent.ExpiryDate,
                GoodsReceiptId = parent.GoodsReceiptId,
                AktifMi = true,

                // ✅ BARKOD ALANINI BURADA DOLDURUYORUZ
                // Barkod olarak LotNo'yu kullanıyorsanız:
               Barcod = DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString().Substring(0, 4).ToUpper()
            };
            _context.ProductLabels.Add(newLabel);
            await _context.SaveChangesAsync(); // Önce kaydedelim ki ID oluşsun

            // 4. ✅ Parçalama için Transfer Kaydı (İzlenebilirlik)
            var transfer = new Transfer
            {
                ProductLabelId = newLabel.Id,
                DepoId = dto.HedefDepoId,
                Tarih = DateTime.Now
            };
            await _context.Transfers.AddAsync(transfer);

            // 5. ✅ Log Kaydı
            await _logService.AddLog(newLabel.Id, "PROCESS",
                $"Ürün {parent.LotNo} lotundan parçalandı ve {dto.YeniMiktar}kg olarak oluşturuldu.", "admin");

            await _context.SaveChangesAsync();

            return Ok(new { lotNo = newLabel.LotNo });
        }




        [HttpGet("history/{lotNo}")]
        public async Task<IActionResult> GetHistory(string lotNo)
        {


            var history = await _context.Transfers
                .AsNoTracking()
                .Include(t => t.Depo)
                .Include(t => t.ProductLabel)
                .Where(t => t.ProductLabel.LotNo == lotNo)
                .OrderByDescending(t => t.Tarih)
                .Select(t => new
                {
                    t.Tarih,
                    DepoAdi = t.Depo.DepoAdi,
                    // Eğer Transfer tablosunda Miktar alanı yoksa ProductLabel'dakini alıyoruz
                   Quantity= t.ProductLabel.Quantity ?? 0,
                    StockName = t.ProductLabel.StockName ?? "Tanımsız",
                    Islem = "Transfer"
                })
                .ToListAsync();

            return Ok(history);
        }
    


        [HttpGet("trace/{lotNo}")]
        public async Task<ActionResult<LabelDto.TraceDto>> Trace(string lotNo)
        {
            var label = await _context.ProductLabels
                .Include(x => x.GoodsReceipt)
                .Include(x => x.Transfers).ThenInclude(t => t.Depo)
                .FirstOrDefaultAsync(x => x.LotNo == lotNo);

            if (label == null) return NotFound();

            var traceDto = new LabelDto.TraceDto
            {
                LotNo = label.LotNo,
                StockName = label.StockName,
               Quantity = label.Quantity ?? 0,
               
                DepoId = label.DepoId,
                ParentLotNo = label.ParentLotNo,
                GoodsReceipe = new LabelDto.GoodsReceipeDto
                {
                    Id = label.GoodsReceipt.Id,
                    CompanyName = label.GoodsReceipt.CompanyName,
                   CreatedAt = label.GoodsReceipt.CreatedAt
                },
                Transfers = label.Transfers.Select(t => new LabelDto.TransferDto
                {
                    Depo = t.Depo.DepoAdi,
                    Tarih = t.Tarih
                }).ToList()
            };

            return Ok(traceDto);
        }

        // ✅ SİL
        [HttpDelete("{lotNo}")]
        public async Task<IActionResult> Delete(string lotNo)
        {
            if (string.IsNullOrWhiteSpace(lotNo))
                return BadRequest("LotNo boş");

            var labels = await _context.ProductLabels
                .Where(x => x.LotNo == lotNo)
                .ToListAsync();

            if (!labels.Any())
                return NotFound("Lot bulunamadı");

            _context.ProductLabels.RemoveRange(labels);
            await _context.SaveChangesAsync();

            return Ok("Silindi");
        }

    }
}

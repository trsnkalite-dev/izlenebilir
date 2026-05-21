using Kalite.API.Context;
using Kalite.API.Dtos.StockDto;
using Kalite.API.Entitity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kalite.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly ApiContext _context;

        public StocksController(ApiContext context)
        {
            _context = context;
        }

        [HttpPost("latest-bulk")]
        public async Task<IActionResult> GetLatestBulk([FromBody] List<string> productNames)
        {
            var results = new Dictionary<string, object>();

            foreach (var name in productNames)
            {
                if (string.IsNullOrEmpty(name)) continue;

                var searchName = name.Trim();
                var value = await _context.Stocks
                    .Where(x => x.StockName.ToLower() == searchName.ToLower() && x.Quantity > 0)
                    .OrderBy(x => x.ExpiryDate)
                    .FirstOrDefaultAsync();

                if (value == null)
                {
                    value = await _context.Stocks
                        .Where(x => x.StockName.ToLower() == searchName.ToLower())
                        .OrderByDescending(x => x.ExpiryDate)
                        .FirstOrDefaultAsync();
                }

                if (value != null)
                {
                    results[name] = new
                    {
                        Id = value.Id,
                        LotNo = value.LotNo,
                        ExpiryDate = value.ExpiryDate?.ToString("dd.MM.yyyy"),
                        CurrentQuantity = value.Quantity,
                        DepoAdi = value.DepoAdi
                    };
                }
            }

            return Ok(results);
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResultStockDto>>> GetStocks(string? search)
        {
            var query = _context.Stocks.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.StockName.Contains(search) || s.LotNo.Contains(search));
            }

            var stocks = await query.OrderBy(s => s.ExpiryDate).ToListAsync();
            var dtos = stocks.Select(s => new ResultStockDto
            {
                Id = s.Id,
                StockName = s.StockName,
                StockKodu = s.StockKodu,
                Barcod = s.Barcod,
                LotNo = s.LotNo,
                ExpiryDate = s.ExpiryDate,
                AllergenInfo = s.AllergenInfo,
                CompanyName = s.CompanyName,
                DepoAdi = s.DepoAdi,
                Quantity = s.Quantity,
                Grup = s.Grup,
                CreatedAt = s.CreatedAt ?? DateTime.Now
            }).ToList();

            return Ok(dtos);
        }
        [HttpGet("ByGroup")]
        public async Task<IActionResult> GetByGroup([FromQuery] string? group)
        {
            if (string.IsNullOrEmpty(group))
            {
                var allStocks = await _context.Stocks.OrderBy(s => s.ExpiryDate).ToListAsync();
                var allResult = allStocks
                    .GroupBy(x => x.StockName)
                    .Select(g => g.First())
                    .Select(x => new
                    {
                        id = x.Id,
                        stockName = x.StockName,
                        stockKodu = x.StockKodu ?? "",
                        barcod = x.Barcod ?? "",
                        companyName = x.CompanyName ?? ""
                    })
                    .ToList();
                return Ok(allResult);
            }

            string searchGroup = group.Trim();
            var stocks = await _context.Stocks.ToListAsync();

            var filtered = stocks
                .Where(x => string.Equals((x.Grup ?? "").Trim(), searchGroup, StringComparison.OrdinalIgnoreCase))
                .GroupBy(x => x.StockName)
                .Select(g => g.First())
                .OrderBy(x => x.StockName)
                .Select(x => new
                {
                    id = x.Id,
                    stockName = x.StockName,
                    stockKodu = x.StockKodu ?? "",
                    barcod = x.Barcod ?? "",
                    companyName = x.CompanyName ?? ""
                })
                .ToList();

            return Ok(filtered);
        }

     
        //public async Task<IActionResult> GetByGroup([FromQuery] string group)
        //{
        //    if (string.IsNullOrEmpty(group))
        //    {
        //        var allStocks = await _context.Stocks.OrderBy(s => s.ExpiryDate).ToListAsync();
        //        var allResult = allStocks
        //            .GroupBy(x => x.StockName)
        //            .Select(g => g.First())
        //            .Select(x => new
        //            {
        //                id = x.Id,
        //                stockName = x.StockName,
        //                stockKodu = x.StockKodu ?? "",
        //                barcod = x.Barcod ?? "",
        //                companyName = x.CompanyName ?? ""
        //            })
        //            .ToList();
        //        return Ok(allResult);
        //    }

        //    string searchGroup = group.Trim();
        //    var stocks = await _context.Stocks
        //        .ToListAsync();

        //    var filtered = stocks
        //        .Where(x => string.Equals((x.Grup ?? "").Trim(), searchGroup, StringComparison.OrdinalIgnoreCase))
        //        .GroupBy(x => x.StockName)
        //        .Select(g => g.First())
        //        .OrderBy(x => x.StockName)
        //        .Select(x => new
        //        {
        //            id = x.Id,
        //            stockName = x.StockName,
        //            stockKodu = x.StockKodu ?? "",
        //            barcod = x.Barcod ?? "",
        //            companyName = x.CompanyName ?? ""
        //        })
        //        .ToList();

        //    return Ok(filtered);
        //}



        [HttpPost]
        public async Task<ActionResult<Stock>> PostStock([FromBody] CreateStockDto stockDto)
        {
            var existingStock = await _context.Stocks.FirstOrDefaultAsync(x =>
                x.StockKodu == stockDto.StockKodu &&
                x.LotNo == stockDto.LotNo &&
                x.DepoAdi == stockDto.DepoAdi);

            if (existingStock != null)
            {
                existingStock.Quantity += stockDto.Quantity;
                existingStock.ExpiryDate = stockDto.ExpiryDate;
                existingStock.CompanyName = stockDto.CompanyName;
                existingStock.Grup = stockDto.Grup;

                await _context.SaveChangesAsync();
                return Ok(existingStock);
            }

            var stock = new Stock
            {
                StockName = stockDto.StockName,
                StockKodu = stockDto.StockKodu,
                Barcod = stockDto.Barcod,
                LotNo = stockDto.LotNo,
                ExpiryDate = stockDto.ExpiryDate,
                AllergenInfo = stockDto.AllergenInfo,
                CompanyName = stockDto.CompanyName,
                DepoAdi = stockDto.DepoAdi,
                Quantity = stockDto.Quantity,
                Grup = stockDto.Grup
            };

            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();

            return Ok(stock);
        }

        [HttpGet("ByName")]
        public async Task<IActionResult> GetByStockName([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "İsim boş olamaz" });

            string searchName = name.Trim();
            var turkishCulture = System.Globalization.CultureInfo.GetCultureInfo("tr-TR");

            var allStocks = await _context.Stocks.OrderByDescending(x => x.Id).ToListAsync();

            var stock = allStocks.FirstOrDefault(x => x.StockName == searchName);

            string normalize(string s) => new string(s.Where(c => !char.IsWhiteSpace(c)).ToArray())
                .ToLower(turkishCulture);

            if (stock == null)
            {
                stock = allStocks.FirstOrDefault(x =>
                    x.StockName != null &&
                    x.StockName.Trim().Equals(searchName, StringComparison.CurrentCultureIgnoreCase));
            }

            if (stock == null)
            {
                string normalizedSearch = normalize(searchName);
                stock = allStocks.FirstOrDefault(x =>
                    x.StockName != null &&
                    normalize(x.StockName) == normalizedSearch);
            }

            if (stock == null)
            {
                stock = allStocks.FirstOrDefault(x =>
                    x.StockName != null &&
                    x.StockName.Contains(searchName, StringComparison.CurrentCultureIgnoreCase));
            }

            if (stock == null)
            {
                if (searchName.Length >= 5)
                {
                    string shortName = searchName.Substring(0, 5).ToLower(turkishCulture);
                    stock = allStocks.FirstOrDefault(x =>
                        x.StockName != null &&
                        x.StockName.ToLower(turkishCulture).StartsWith(shortName));
                }
            }

            if (stock == null)
            {
                var availableNames = allStocks.Select(s => s.StockName).ToList();
                return Ok(new
                {
                    success = false,
                    message = $"Stok bulunamadı. Aranan: '{searchName}'",
                    normalizedSearch = normalize(searchName),
                    availableNames = availableNames,
                    count = allStocks.Count
                });
            }

            return Ok(new
            {
                success = true,
                stockKodu = stock.StockKodu ?? "",
                barcod = stock.Barcod ?? "",
                companyName = stock.CompanyName ?? ""
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStockById(int id)
        {
            var stock = await _context.Stocks
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (stock == null) return NotFound();
            return Ok(stock);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto stockDto)
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null) return NotFound();

            stock.StockName = stockDto.StockName;
            stock.StockKodu = stockDto.StockKodu;
            stock.Barcod = stockDto.Barcod;
            stock.LotNo = stockDto.LotNo;
            stock.ExpiryDate = stockDto.ExpiryDate;
            stock.AllergenInfo = stockDto.AllergenInfo;
            stock.CompanyName = stockDto.CompanyName;
            stock.DepoAdi = stockDto.DepoAdi;
            stock.Quantity = stockDto.Quantity;
            stock.Grup = stockDto.Grup;

            await _context.SaveChangesAsync();
            return Ok(stockDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStock(int id)
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null) return NotFound();

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();
            return Ok(stock);
        }

        [HttpPost("deduct")]
        public async Task<IActionResult> DeductStock([FromBody] StockDeductApiDto deductDto)
        {
            var stock = await _context.Stocks.FirstOrDefaultAsync(x =>
                x.StockName == deductDto.StockName &&
                x.LotNo == deductDto.LotNo);

            if (stock != null)
            {
                stock.Quantity -= deductDto.Quantity;
                if (stock.Quantity < 0) stock.Quantity = 0;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Stok düşürüldü", newQuantity = stock.Quantity });
            }

            return NotFound(new { message = "Stok bulunamadı" });
        }

    }


    public class StockDeductApiDto
    {
        public string StockName { get; set; }
        public string LotNo { get; set; }
        public double Quantity { get; set; }
    }
}


  




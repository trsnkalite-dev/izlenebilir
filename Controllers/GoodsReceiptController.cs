using KaliteWeb.UI.Dto.StockDto;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static KaliteWeb.UI.Dto.GoodsReceiptDto.GoodsReceiptDto;
using static KaliteWeb.UI.Dto.ProductPreparationDto.ProductPreparationDto;

namespace KaliteWeb.UI.Controllers
{
    public class GoodsReceiptController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GoodsReceiptController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        // private readonly IArgoxPrintService _printService;


      public async Task<IActionResult> Index(string? companyName, string? warehouseName, string? stockName)
        {
            var client = _httpClientFactory.CreateClient();
            var url = "https://localhost:7241/api/GoodsReceipt";

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var allRecords = await response.Content.ReadFromJsonAsync<List<ResultGoodsReceiptDto>>();

                // Filter
                var query = allRecords?.AsQueryable() ?? new List<ResultGoodsReceiptDto>().AsQueryable();
                if (!string.IsNullOrEmpty(companyName)) query = query.Where(x => x.CompanyName != null && x.CompanyName.Contains(companyName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(stockName)) query = query.Where(x => x.StockName != null && x.StockName.Contains(stockName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(warehouseName)) query = query.Where(x => x.DepoAdi == warehouseName);

                var filteredList = query.ToList();

                // GROUP BY Product for the "Anlık Stok" view
                var summaryList = filteredList
                    .GroupBy(x => x.StockName)
                    .Select(g => new ResultGoodsReceiptDto
                    {
                        StockName = g.Key,
                        Quantity = g.Sum(s => s.Quantity),
                        DepoAdi = string.Join(", ", g.Select(s => s.DepoAdi).Distinct()),
                        StockKodu = g.FirstOrDefault()?.StockKodu,
                        Grup = g.FirstOrDefault()?.Grup,
                        ExpiryDate = g.Min(s => s.ExpiryDate)
                    })
                    .OrderBy(x => x.StockName)
                    .ToList();

                // Dashboard Statistics
                ViewBag.TotalStockQty = summaryList.Sum(s => s.Quantity);
                ViewBag.ExpiredCount = filteredList.Count(s => s.ExpiryDate.HasValue && s.ExpiryDate < DateTime.Now && s.Quantity > 0);
                ViewBag.UpcomingCount = filteredList.Count(s => s.ExpiryDate.HasValue && s.ExpiryDate >= DateTime.Now && s.ExpiryDate <= DateTime.Now.AddDays(30) && s.Quantity > 0);

                ViewBag.CompanyName = companyName;
                ViewBag.StockName = stockName;
                ViewBag.SelectedWarehouse = warehouseName;
                ViewBag.Warehouses = GetWarehouses();

                return View(summaryList);
            }

            ViewBag.Warehouses = GetWarehouses();
            return View(new List<ResultGoodsReceiptDto>());
        }
        public async Task<IActionResult> Details(string stockName, int? id)
        {
            if (string.IsNullOrEmpty(stockName) && !id.HasValue) return RedirectToAction("Index");

            var client = _httpClientFactory.CreateClient();

            // 1. Get Inbound Movements (Mal Kabul Girişleri)
            var receiptsResponse = await client.GetAsync("https://localhost:7241/api/GoodsReceipt");
            var allReceipts = receiptsResponse.IsSuccessStatusCode
                ? await receiptsResponse.Content.ReadFromJsonAsync<List<ResultGoodsReceiptDto>>()
                : new List<ResultGoodsReceiptDto>();

            var inboundMovements = allReceipts
                .Where(x => (id.HasValue ? x.Id == id.Value : x.StockName == stockName))
                .Select(x => new ProductMovementDto
                {
                    Id = x.Id,
                    Date = x.AcceptanceDate ?? DateTime.Now,
                    Type = "Giriş (Mal Kabul)",
                    LotNo = x.LotNo,
                    Quantity = (double)x.Quantity,
                    Description = $"{x.CompanyName} - {x.DepoAdi}",
                    Inout = "IN",
                    IsAuditLog = false
                }).ToList();

            // 2. Get Outbound Movements (Üretim Çıkışları)
            var preparationsResponse = await client.GetAsync("https://localhost:7241/api/ProductPreparationApi");
            var allPreps = preparationsResponse.IsSuccessStatusCode
                ? await preparationsResponse.Content.ReadFromJsonAsync<List<ResultProductPreparationDto>>()
                : new List<ResultProductPreparationDto>();

            var outboundMovements = new List<ProductMovementDto>();
            foreach (var prep in allPreps)
            {
                if (string.IsNullOrEmpty(prep.IngredientsData)) continue;

                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var ingredients = JsonSerializer.Deserialize<List<IngredientItemDto>>(prep.IngredientsData, options);

                    var usages = ingredients?.Where(i =>
                        (id.HasValue && i.StockId.HasValue) ? i.StockId == id.Value : i.Name == stockName
                    ).ToList();

                    if (usages != null && usages.Any())
                    {
                        foreach (var usage in usages)
                        {
                            outboundMovements.Add(new ProductMovementDto
                            {
                                Id = prep.Id,
                                Date = prep.Date,
                                Type = "Çıkış (Üretim)",
                                LotNo = usage.BatchNo,
                                Quantity = (double)usage.Amount,
                                Description = $"Belge: {prep.DocumentNo} - Hazırlanan: {prep.ProductName}",
                                Inout = "OUT"
                            });
                        }
                    }
                }
                catch { /* parse error skip */ }
            }

            var fullHistory = inboundMovements.Concat(outboundMovements)
                .OrderByDescending(x => x.Date)
                .ToList();

            ViewBag.StockName = stockName ?? inboundMovements.FirstOrDefault()?.Description;
            ViewBag.StockId = id;

            // Inbound total sum
            ViewBag.InboundTotal = inboundMovements.Sum(x => x.Quantity);
            // Outbound total sum 
            ViewBag.OutboundTotal = outboundMovements.Sum(x => x.Quantity);

            // Current Total calculation
            ViewBag.CurrentTotal = ViewBag.InboundTotal - ViewBag.OutboundTotal;

            return View(fullHistory);
        }

      
       

        public class ProductMovementDto
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public string Type { get; set; }
            public string LotNo { get; set; }
            public double Quantity { get; set; }
            public string Description { get; set; }
            public string Inout { get; set; }
            public bool IsAuditLog { get; set; }
        }

        private List<string> GetWarehouses()
        {
            return new List<string>
            {
                "A-1(+4C°)",
                "A-2(+4C°)",
                "A-3(-18C°)",
                "A-4(-18C°)",
                "C-1(-18C°)",
                "C-2(-18C°)",
                "B3(-18C°)",
                "B1(+4C°)",
                "B4(0-4C°)",
                "Baharat Deposu",
                "Ambalaj Deposu",
            };
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7241/api/Stocks");
            if (response.IsSuccessStatusCode)
            {
                var stocks = await response.Content.ReadFromJsonAsync<List<ResultStockDto>>();
                ViewBag.StockList = stocks.OrderBy(x => x.StockName).ToList();
            }
            else
            {
                ViewBag.StockList = new List<ResultStockDto>();
            }

            ViewBag.Warehouses = GetWarehouses();
            return View(new CreateGoodsReceiptDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateGoodsReceiptDto dto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsJsonAsync("https://localhost:7241/api/GoodsReceipt", dto);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Bağlantı Hatası: {ex.Message}");
            }

            // Repopulate ViewBags on failure
            var stocksResponse = await _httpClientFactory.CreateClient().GetAsync("https://localhost:7241/api/Stocks");
            if (stocksResponse.IsSuccessStatusCode)
            {
                var stocks = await stocksResponse.Content.ReadFromJsonAsync<List<ResultStockDto>>();
                ViewBag.StockList = stocks.OrderBy(x => x.StockName).ToList();
            }
            ViewBag.Warehouses = GetWarehouses();

            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var client = _httpClientFactory.CreateClient();

            // Fetch stocks for dropdown
            var stocksResponse = await client.GetAsync("https://localhost:7241/api/Stocks");
            if (stocksResponse.IsSuccessStatusCode)
            {
                var stocks = await stocksResponse.Content.ReadFromJsonAsync<List<ResultStockDto>>();
                ViewBag.StockList = stocks.OrderBy(x => x.StockName).ToList();
            }
            else
            {
                ViewBag.StockList = new List<ResultStockDto>();
            }

            ViewBag.Warehouses = GetWarehouses();
            var response = await client.GetAsync($"https://localhost:7241/api/GoodsReceipt/{id}");
            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadFromJsonAsync<UpdateGoodsReceiptDto>();
                return View(value);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateGoodsReceiptDto dto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PutAsJsonAsync("https://localhost:7241/api/GoodsReceipt", dto);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Bağlantı Hatası: {ex.Message}");
            }

            // Repopulate ViewBags on failure
            var stocksResponse = await _httpClientFactory.CreateClient().GetAsync("https://localhost:7241/api/Stocks");
            if (stocksResponse.IsSuccessStatusCode)
            {
                var stocks = await stocksResponse.Content.ReadFromJsonAsync<List<ResultStockDto>>();
                ViewBag.StockList = stocks.OrderBy(x => x.StockName).ToList();
            }
            ViewBag.Warehouses = GetWarehouses();

            return View(dto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient();
            await client.DeleteAsync($"https://localhost:7241/api/GoodsReceipt/{id}");
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> PrintLabel(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7241/api/GoodsReceipt/{id}");
            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadFromJsonAsync<ResultGoodsReceiptDto>();
                return View(value);
            }
            return NotFound();
        }
     
      
    }
}

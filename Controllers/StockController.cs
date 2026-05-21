using KaliteWeb.UI.Dto.StockDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using static KaliteWeb.UI.Dto.GoodsReceiptDto.GoodsReceiptDto;
using static KaliteWeb.UI.Dto.ProductPreparationDto.ProductPreparationDto;


namespace KaliteWeb.UI.Controllers
{
    public class StockController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl = "https://localhost:7241/api/Stocks"; // Changed to localhost:3000


        public StockController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

        }


        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            int pageSize = 9;
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(_apiBaseUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var allStocks = JsonConvert.DeserializeObject<List<ResultStockDto>>(jsonData);


                // 1. ARAMA FİLTRESİ
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    allStocks = allStocks.Where(s =>
                        (s.StockName?.ToLower().Contains(search) ?? false) ||
                        (s.LotNo?.ToLower().Contains(search) ?? false) ||
                        (s.CompanyName?.ToLower().Contains(search) ?? false) ||
                        (s.DepoAdi?.ToLower().Contains(search) ?? false)
                    ).ToList();
                }

                // 2. SAYFALAMA MANTIĞI
                var totalCount = allStocks.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var pagedStocks = allStocks
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // View tarafına bilgileri gönder
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalCount;
                ViewBag.SearchSearch = search;
                ViewBag.Warehouses = GetWarehouses();

                return View(pagedStocks);
            }
            return View(new List<ResultStockDto>());
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

        // Yeni Kayıt Sayfası
        [HttpGet]
        public IActionResult CreateStock()
        {
            ViewBag.Warehouses = GetWarehouses();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateStock(CreateStockDto createStockDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createStockDto);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_apiBaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"API Hatası: {response.StatusCode} - {errorContent}");
            ViewBag.Warehouses = GetWarehouses();
            return View(createStockDto);
        }

        // Güncelleme Sayfası
        [HttpGet]
        public async Task<IActionResult> UpdateStock(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var value = JsonConvert.DeserializeObject<UpdateStockDto>(jsonData);
                ViewBag.Warehouses = GetWarehouses();
                return View(value);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(int id, UpdateStockDto updateStockDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(updateStockDto);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{_apiBaseUrl}/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"API Hatası ({response.StatusCode}): {errorContent}");
                ViewBag.Warehouses = GetWarehouses();
                return View(updateStockDto);
            }
        }

        // Silme İşlemi
        public async Task<IActionResult> DeleteStock(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{_apiBaseUrl}/{id}");

            return RedirectToAction("Index");
        }
    }
}






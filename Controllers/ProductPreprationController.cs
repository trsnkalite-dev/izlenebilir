using KaliteWeb.UI.Dto.DepoDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using static KaliteWeb.UI.Dto.ProductPreparationDto.ProductPreparationDto;

namespace KaliteWeb.UI.Controllers
{
    public class ProductPreprationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductPreprationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // --- YARDIMCI METOT: Depoları API'den Çekip ViewBag'e Atar ---
        private async Task LoadDeposToViewBag()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var response = await client.GetAsync("https://localhost:7241/api/Depo");

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();

                    var values = JsonConvert.DeserializeObject<List<ResultDepoDto>>(jsonData);

                    ViewBag.Depos = values;
                }
                else
                {
                    ViewBag.Depos = new List<ResultDepoDto>();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Depos = new List<ResultDepoDto>();
            }
        }

        public async Task<IActionResult> Index(string? productName, DateTime? date)
        {
            var client = _httpClientFactory.CreateClient();
            var url = "https://localhost:7241/api/ProductPreparationApi";

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(productName)) queryParams.Add($"productName={productName}");
            if (date.HasValue) queryParams.Add($"date={date.Value:yyyy-MM-dd}");

            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams);
            }

            var responseMessage = await client.GetAsync(url);
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultProductPreparationDto>>(jsonData);

                ViewBag.ProductName = productName;
                ViewBag.Date = date?.ToString("yyyy-MM-dd");

                return View(values);
            }
            return View(new List<ResultProductPreparationDto>());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDeposToViewBag(); // Depoları yükle
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductPreparationDto dto)
        {
            // Reçete verisinden toplam miktarı hesapla
            try
            {
                var settings = new JsonSerializerSettings { Culture = System.Globalization.CultureInfo.InvariantCulture };
                var ingredients = JsonConvert.DeserializeObject<List<IngredientItemDto>>(dto.IngredientsData, settings);
                if (ingredients != null)
                {
                    dto.TotalAmount = ingredients.Sum(x => x.Amount);
                }
            }
            catch { }

            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(dto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client.PostAsync("https://localhost:7241/api/ProductPreparationApi", stringContent);

            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                var errorContent = await responseMessage.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"API Hatası: {responseMessage.StatusCode} - {errorContent}");

                await LoadDeposToViewBag(); // Hata durumunda dropdown'ı tekrar doldur
                return View(dto);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.DeleteAsync($"https://localhost:7241/api/ProductPreparationApi/{id}");
            if (responseMessage.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Kayıt başarıyla silindi ve stoklar iade edildi.";
                return RedirectToAction("Index");
            }

            var error = await responseMessage.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Silme işlemi başarısız: {error}";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            await LoadDeposToViewBag(); // Depoları yükle

            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"https://localhost:7241/api/ProductPreparationApi/{id}");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var value = JsonConvert.DeserializeObject<UpdateProductPreparationDto>(jsonData);
                return View(value);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateProductPreparationDto dto)
        {
            try
            {
                var settings = new JsonSerializerSettings { Culture = System.Globalization.CultureInfo.InvariantCulture };
                var ingredients = JsonConvert.DeserializeObject<List<IngredientItemDto>>(dto.IngredientsData, settings);
                if (ingredients != null)
                {
                    dto.TotalAmount = ingredients.Sum(x => x.Amount);
                }
            }
            catch { }

            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(dto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client.PutAsync("https://localhost:7241/api/ProductPreparationApi", stringContent);

            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                var errorContent = await responseMessage.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"API Hatası: {responseMessage.StatusCode} - {errorContent}");

                await LoadDeposToViewBag(); // Hata durumunda dropdown'ı tekrar doldur
                return View(dto);
            }
        }

        [HttpGet]
        [Route("ProductPreparation/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0) return RedirectToAction("Index");
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"https://localhost:7241/api/ProductPreparationApi/{id}");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var value = JsonConvert.DeserializeObject<ResultProductPreparationDto>(jsonData);
                if (value == null) return RedirectToAction("Index");
                return View(value);
            }
            return RedirectToAction("Index");
        }
    }
}

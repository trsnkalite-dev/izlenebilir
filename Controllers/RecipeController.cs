using KaliteWeb.UI.Dto.StockDto;
using KaliteWeb.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace KaliteWeb.UI.Controllers
{
    public class RecipeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IActionResult Index()
        {
            var recipes = RecipeServices.GetStaticRecipes();
            return View(recipes);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduction(int recipeId)
        {
            var recipe = RecipeServices.GetStaticRecipes().FirstOrDefault(x => x.Id == recipeId);

            // Stok bilgilerini API'den çekiyoruz (Lot, SKT, Alerjen için)
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7241/api/Recipe");
            var jsonData = await response.Content.ReadAsStringAsync();
            var allStocks = JsonConvert.DeserializeObject<List<ResultStockDto>>(jsonData);

            ViewBag.AllStocks = allStocks; // JavaScript tarafında eşleştirme yapmak için
            return View(recipe);
        }
    }
}

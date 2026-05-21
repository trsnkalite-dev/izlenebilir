using KaliteWeb.UI.Dto.CustomerDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace KaliteWeb.UI.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string ApiUrl = "https://localhost:7241/api/Customer";
        public CustomerController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }




        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync(ApiUrl);
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultCustomerDto>>(jsonData);
                return this.View(values);
            }
            return this.View(new List<ResultCustomerDto>());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return this.View();
        }


      
        [HttpPost]
        public async Task<IActionResult> Create(CreateCustomerDto dto)
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsJsonAsync(ApiUrl, dto);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Müşteri eklendi";
                return RedirectToAction("Create");
            }

            // 🔥 HATA MESAJINI GÖR
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", error);

            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{ApiUrl}/{id}");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var value = JsonConvert.DeserializeObject<UpdateCustomerDto>(jsonData);
                return this.View(value);
            }
            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateCustomerDto updateCustomerDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(updateCustomerDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client.PutAsync(ApiUrl, stringContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                return this.RedirectToAction("Index");
            }
            return this.View();
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.DeleteAsync($"{ApiUrl}/{id}");
            if (responseMessage.IsSuccessStatusCode)
            {
                return this.RedirectToAction("Index");
            }
            return this.RedirectToAction("Index");
        }
    }
}


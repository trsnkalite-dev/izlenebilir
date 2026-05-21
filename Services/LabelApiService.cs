using KaliteWeb.UI.Dto.LabelDto;
using System.Text.Json;
using static KaliteWeb.UI.Dto.LabelDto.LabelDto;

namespace KaliteWeb.UI.Services
{
    public class LabelApiService
    {
        private readonly HttpClient _httpClient;

        public LabelApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7241/");
        }


        // WebUI/Service/LabelApiService.cs içerisine ekle
        public async Task<ProductLabelResponseDto> GetLabelByLotNo(string lotNo)
        {
            var response = await _httpClient.GetAsync($"api/Label/barcode/{lotNo}");

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception(err);
            }

            return await response.Content.ReadFromJsonAsync<LabelDto.ProductLabelResponseDto>();
        }


        // 🔥 CREATE

        public async Task CreateLabel(CreateLabelDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Label/generate", dto);

            var error = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(error); // 🔥 GERÇEK HATA
        }
        public async Task<List<GoodsReceipeDto>> GetSlaughter()
        {
            var response = await _httpClient.GetAsync("api/GoodsReceipt");

            if (!response.IsSuccessStatusCode)
                throw new Exception("GoodsReceipt API hatası");

            return await response.Content.ReadFromJsonAsync<List<GoodsReceipeDto>>();
        }
        public async Task AddLabel(LabelDto.ProductLabelResponseDto model)
        {
            // API'ye POST isteği gönderiyoruz
            var response = await _httpClient.PostAsJsonAsync("api/Label", model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception("API Hatası: " + error);
            }
        }
        public async Task<object> GetTrace(string lotNo)
        {
            var response = await _httpClient.GetAsync($"api/label/trace/{lotNo}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Trace alınamadı");

            return await response.Content.ReadFromJsonAsync<object>();
        }

        // 🔥 GET LABELS
        public async Task<List<LabelDto.ProductLabelResponseDto>> GetAllLabels()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Label");

                // Yanıtın içeriğini string olarak al (Hata mesajını görmek için)
                string jsonContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API Hata Kodu: {response.StatusCode}, İçerik: {jsonContent}");
                }

                // Eğer JSON boş geliyorsa burası boş liste döner
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<List<LabelDto.ProductLabelResponseDto>>(jsonContent, options);

                return data ?? new List<LabelDto.ProductLabelResponseDto>();
            }
            catch (Exception ex)
            {
                // HATAYI BURAYA YAZDIRIN
                System.Diagnostics.Debug.WriteLine("HATA: " + ex.Message);
                return new List<LabelDto.ProductLabelResponseDto>();
            }
        }


        // 🔥 GET DEPOS
        public async Task<List<DepoDto>> GetDepos()
        {
            var response = await _httpClient.GetAsync("api/Depo");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Depo listesi alınamadı");

            return await response.Content.ReadFromJsonAsync<List<DepoDto>>();
        }

        // 🔥 TRANSFER
        public async Task Transfer(TransferRequestDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Label/transfer", dto);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Transfer hatası");
        }
        public async Task Delete(string lotNo)
        {
            var response = await _httpClient.DeleteAsync($"api/Label/{lotNo}");

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception(err); // 🔥 gerçek hata
            }
        }
        public async Task<List<LabelHistoryDto>> GetLabelHistory(string lotNo)
        {
            var response = await _httpClient.GetAsync($"api/Label/history/{lotNo}");

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            return await response.Content.ReadFromJsonAsync<List<LabelHistoryDto>>();
        }
        public class LabelHistoryDto
        {
            public DateTime Tarih { get; set; }
            public string DepoAdi { get; set; }
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using KaliteWeb.UI.Services;
using KaliteWeb.UI.Dto.LabelDto;

namespace KaliteWeb.UI.Controllers
{
    public class LabelController : Controller
    {
        private readonly LabelApiService _apiService;

        public LabelController(LabelApiService apiService)
        {
            _apiService = apiService;
        }


        // WebUI/Controllers/LabelController.cs içerisine ekle
        [HttpGet("GetLabelByLotNo")]
        public async Task<IActionResult> GetLabelByLotNo(string lotNo)
        {
            var label = await _apiService.GetLabelByLotNo(lotNo);

            if (label == null)
                return NotFound();

            return Ok(label);
        }


        public async Task<IActionResult> Index()
        {
            var labels = await _apiService.GetAllLabels();
            var depos = await _apiService.GetDepos();
            var goodsreceipts = await _apiService.GetSlaughter();

            ViewBag.Depos = depos;
            ViewBag.GoodsReceipts = goodsreceipts;

            return View(labels ?? new List<LabelDto.ProductLabelResponseDto>());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LabelDto.CreateLabelDto dto)
        {
            if (dto == null || (dto.GoodsReceiptId == 0))
            {
                return BadRequest("Veri eksik gönderildi.");
            }
            try
            {
                await _apiService.CreateLabel(dto);

                var data = await _apiService.GetAllLabels();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }




        // İsmi Generate yaptık ki çakışma olmasın
        [HttpPost]
        public async Task<IActionResult> Generate(LabelDto.CreateLabelDto dto)
        {
            await _apiService.CreateLabel(dto);
            var newList = await _apiService.GetAllLabels(); // Listeyi güncellemek için
            return Json(newList);
        }

        [HttpPost]
        public async Task<IActionResult> Transfer(LabelDto.TransferRequestDto dto)
        {
            await _apiService.Transfer(dto);
            return Ok(await _apiService.GetAllLabels());
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string lotNo)
        {
            await _apiService.Delete(lotNo);
            return Ok(await _apiService.GetAllLabels());
        }
    }
}

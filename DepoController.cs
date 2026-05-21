using Kalite.API.Context;
using Kalite.API.Entitity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kalite.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepoController : ControllerBase
    {
        private readonly ApiContext _context;

        public DepoController(ApiContext context)
        {
            _context = context;
        }
     
        [HttpGet]
        public IActionResult DepoList()
        {
            var values = _context.Depos.Select(x => new {
                DepoId = x.DepoId, // UI'daki isimle aynı olduğundan emin olun
                DepoAdi = x.DepoAdi
            }).ToList();

            return Ok(values);
        }

        [HttpPost]
        public IActionResult CreateDepo(Depo depo)
        {
            _context.Depos.Add(depo);
            _context.SaveChanges();
            return Ok("Depo sisteme başarıyla eklendi");
        }
    }
}

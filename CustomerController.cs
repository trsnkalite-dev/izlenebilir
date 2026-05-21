using Kalite.API.Context;
using Kalite.API.Dtos.CustomerDto;
using Kalite.API.Entitity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kalite.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ApiContext _context;

        public CustomerController(ApiContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var values = await _context.Customers.ToListAsync();
            return Ok(values);
        }


        [HttpPost]
        public async Task<ActionResult<Stock>> PostCustomer([FromBody] CreateCustomerDto createCustomerDto)
        {
            var cust = new Customer
            {
                CustomerName = createCustomerDto.CustomerName,

            };

            _context.Customers.Add(cust);
            await _context.SaveChangesAsync();

            return Ok(createCustomerDto);
        }
        // stocksController.cs içine ekle
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomerById(int id)
        {
            var cust = await _context.Customers.FindAsync(id);
            if (cust == null) return NotFound();
            return Ok(cust);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto stockDto)
        {
            var stock = await _context.Customers.FindAsync(id);
            if (stock == null) return NotFound();

            stock.CustomerName = stockDto.CustomerName;


            await _context.SaveChangesAsync();
            return Ok(stockDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var stock = await _context.Customers.FindAsync(id);
            if (stock == null) return NotFound();

            _context.Customers.Remove(stock);
            await _context.SaveChangesAsync();
            return Ok(stock);
        }
    }
}


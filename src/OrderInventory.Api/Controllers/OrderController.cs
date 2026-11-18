using Microsoft.AspNetCore.Mvc;
using OrderInventory.Application.DTOs;
using OrderInventory.Application.Interfaces;

namespace OrderInventory.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrderService orderService) : ControllerBase
    {


        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            await orderService.CreateAsync(createOrderDto);

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(int id, CancellationToken cancellationToken)
        {
            await orderService.CancelOrderAsync(id, cancellationToken);

            return NoContent();
        }

        [HttpGet("{id:int}")]
        public async Task<OrderDto> GetOrder(int id)
        {
            return await orderService.GetByIdAsync(id);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using OrderInventory.Application;
using OrderInventory.Application.DTOs;
using OrderInventory.Application.Interfaces;

namespace OrderInventory.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IProductService productService) : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await productService.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound(new { message = $"ID'si {id} olan ürün bulunamadı." });
            }

            return Ok(product);
        }

        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProduct)
        {
            await productService.CreateAsync(createProduct);

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto updateProduct)
        {
            await productService.UpdateAsync(updateProduct);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await productService.DeleteAsync(id);

            return NoContent();
        }

        [HttpGet("GetAllProducts")]
        public async Task<PagedResult<ProductDto>> GetAllProducts([FromQuery] GetAllProductsRequest request)
        {
            return await productService.GetAllProducts(request);
        }
    }
}
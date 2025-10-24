using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingCartApi.Services;

namespace ShoppingCartApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly CartService _cartService;
    private readonly ProductService _productService;

    public CartController(CartService cartService, 
    ProductService productService)
    {
        _cartService = cartService;
        _productService = productService;
    }

    private string UserId = "";

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var cart = await _cartService.GetCartAsync(UserId);
        return Ok(cart);
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] AddCartItemDto dto)
    {
        await _cartService.AddItemAsync(UserId, dto.ProductId, dto.Quantity);
        await _productService.EnqueueProductQuantityAsync(dto.ProductId, dto.Quantity);
        return Ok("Item adicionado.");
    }

    [HttpDelete("remove/{productId}")]
    public async Task<IActionResult> Remove(int productId)
    {
        await _cartService.RemoveItemAsync(UserId, productId);
        return Ok("Item removido.");
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        await _cartService.ClearCartAsync(UserId);
        return Ok("Carrinho limpo.");
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        await _cartService.CheckoutAsync(UserId);
        return Ok("Compra finalizada.");
    }
}

public record AddCartItemDto(int ProductId, int Quantity, decimal Price);

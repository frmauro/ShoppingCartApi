using Microsoft.EntityFrameworkCore;
using ShoppingCartApi.Data;
using ShoppingCartApi.Models;

namespace ShoppingCartApi.Services;

public class CartService
{
    private readonly AppDbContext _db;
    public CartService(AppDbContext db)
    {
        _db = db;
    }

// Obtém carrinho ativo (status=0)
    public async Task<Cart?> GetActiveCartAsync(string userId)
    {
        return await _db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Cart)
            .Where(c => c.UserId == userId && c.Status == 0)
            .FirstOrDefaultAsync();
    }

    // Cria carrinho novo
    public async Task<Cart> CreateCartAsync(string userId)
    {
        var cart = new Cart
        {
            UserId = userId,
            Status = 0,
            CreatedAt = DateTime.UtcNow
        };

        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        return cart;
    }

    // Adiciona item ao carrinho
    public async Task AddItemAsync(string userId, int productId, int quantity)
    {
        var cart = await GetActiveCartAsync(userId) ?? await CreateCartAsync(userId);
        var product = await _db.CartItems.FindAsync(productId);

        if (product == null)
            throw new Exception("Produto não encontrado");

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            var item = new CartItem
            {
                ProductId = product.Id,
                //NameProduct = product.Name,
                Quantity = quantity,
                CartId = cart.Id
            };
            _db.CartItems.Add(item);
        }

        cart.TotalPrice = await CalculateTotal(cart.Id);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(string userId, int productId)
    {
        var cart = await GetActiveCartAsync(userId);
        if (cart == null) return;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _db.CartItems.Remove(item);
            cart.TotalPrice = await CalculateTotal(cart.Id);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<decimal> CalculateTotal(int cartId)
    {
        var items = await _db.CartItems
            .Where(i => i.CartId == cartId)
            .ToListAsync();

        var total = 0m;
        foreach (var item in items)
        {
            var product = await _db.CartItems.FindAsync(item.ProductId);
            if (product != null)
                total += product.Price * item.Quantity;
        }
        return total;
    }

    public async Task<Cart?> GetCartAsync(string userId)
    {
        return await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == 0);
    }

    public async Task ClearCartAsync(string userId)
    {
        var cart = await GetActiveCartAsync(userId);
        if (cart == null) return;

        _db.CartItems.RemoveRange(cart.Items);
        cart.TotalPrice = 0;
        await _db.SaveChangesAsync();
    }

    public async Task CheckoutAsync(string userId)
    {
        var cart = await GetActiveCartAsync(userId);
        if (cart == null) return;

        cart.Status = 1; // finalizado
        await _db.SaveChangesAsync();
    }


}
namespace ShoppingCartApi.Models;
public class Cart
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty; // FK para ApplicationUser.Id
    public ICollection<CartItem> Items { get; set; }
    public decimal TotalPrice { get; set; }
    public int Status { get; set; } = 0; // 0=ativo, 1=finalizado
    public DateTime CreatedAt { get; set; }
}
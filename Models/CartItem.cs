namespace ShoppingCartApi.Models;
public class CartItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? NameProduct { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
     // Relacionamento com o Cart
    public int CartId { get; set; }
    public Cart? Cart { get; set; }
}
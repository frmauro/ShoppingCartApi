using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShoppingCartApi.Models;

namespace ShoppingCartApi.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Relação: Cart -> CartItem (1:N)
        builder.Entity<Cart>()
            .HasMany(c => c.Items)
            .WithOne(i => i.Cart!)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relação: ApplicationUser -> Cart (1:N)
        // builder.Entity<ApplicationUser>()
        //     .HasMany(u => u.Carts)
        //     .WithOne()
        //     .HasForeignKey(c => c.UserId)
        //     .OnDelete(DeleteBehavior.Cascade);

    }
}
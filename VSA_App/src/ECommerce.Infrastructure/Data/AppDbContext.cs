using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace ECommerce.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                entity.Property(e => e.Price).HasConversion<double>();
            else
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Category).WithMany(c => c.Products).HasForeignKey(e => e.CategoryId);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                entity.Property(e => e.UnitPrice).HasConversion<double>();
            else
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Cart).WithMany(c => c.Items).HasForeignKey(e => e.CartId);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
        });

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Using a fixed Guid so the generated JWT tokens survive database wipes during development
        var adminId = Guid.Parse("5cb9c57f-b005-45e9-b00a-d321d8013cd9");
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = adminId,
            Email = "demo@store.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo1234!"),
            FirstName = "Demo",
            LastName = "User",
            CreatedAt = DateTime.UtcNow
        });

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Slug = "electronics", ImageUrl = "https://picsum.photos/seed/electronics/400/400" },
            new Category { Id = 2, Name = "Clothing", Slug = "clothing", ImageUrl = "https://picsum.photos/seed/clothing/400/400" },
            new Category { Id = 3, Name = "Books", Slug = "books", ImageUrl = "https://picsum.photos/seed/books/400/400" },
            new Category { Id = 4, Name = "Home & Garden", Slug = "home-garden", ImageUrl = "https://picsum.photos/seed/home-garden/400/400" },
            new Category { Id = 5, Name = "Sports", Slug = "sports", ImageUrl = "https://picsum.photos/seed/sports/400/400" }
        );

        var products = new List<Product>();
        var random = new Random(42);
        string[] prefixes = { "Premium", "Ultra", "Essential", "Pro", "Smart", "Classic", "Modern" };
        string[] bases = { "Widget", "Device", "Tool", "Appliance", "Gear", "System" };
        
        for (int i = 1; i <= 24; i++)
        {
            // First 10 products are explicitly distributed 2 per category (Categories 1-5)
            int categoryId = i <= 10 ? ((i - 1) / 2) + 1 : random.Next(1, 6);
            string name = $"{prefixes[random.Next(prefixes.Length)]} {bases[random.Next(bases.Length)]} {i}";
            // Prices adjusted for Naira context (between ₦15,000 and ₦1,500,000)
            decimal price = Math.Round((decimal)(random.NextDouble() * 1485000 + 15000), 2);
            
            products.Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = $"A fantastic {name} that exceeds your expectations with cutting-edge features.",
                Price = price,
                StockQuantity = random.Next(10, 100),
                ImageUrl = $"https://picsum.photos/seed/{name.Replace(" ", "")}/400/400",
                CategoryId = categoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        
        modelBuilder.Entity<Product>().HasData(products);
    }
}

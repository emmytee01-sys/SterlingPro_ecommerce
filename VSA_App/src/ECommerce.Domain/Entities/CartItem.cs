using System;

namespace ECommerce.Domain.Entities;

public class CartItem
{
    public int Id { get; set; }
    
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = default!;
    
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

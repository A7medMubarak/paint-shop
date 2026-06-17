using PaintShop.Domain.Enums;

namespace PaintShop.Domain.Entities;

public class StockTransfer
{
    public int Id { get; set; }
    public InventoryLocation FromLocation { get; set; }
    public InventoryLocation ToLocation { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
    public User CreatedBy { get; set; } = null!;
    public ICollection<StockMovement> Movements { get; set; } = [];
}

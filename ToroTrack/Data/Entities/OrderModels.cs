using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToroTrack.Data.Entities
{
    public class AssetOrder
    {
        public int Id { get; set; }

        public string ClientId { get; set; } = "";
        [ForeignKey("ClientId")]
        public ApplicationUser? Client { get; set; }

        public int ProjectId { get; set; } // Links order to the specific project
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }
        public string DeliveryAddress { get; set; } = "";
        public string ContactNumber { get; set; } = "";

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Low Stock, Fulfilled

        public decimal TotalCost { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int AssetOrderId { get; set; }

        public int CatalogItemId { get; set; }
        [ForeignKey("CatalogItemId")]
        public CatalogItem? CatalogItem { get; set; }

        public int Quantity { get; set; }
        public decimal PriceAtOrder { get; set; }
    }
}
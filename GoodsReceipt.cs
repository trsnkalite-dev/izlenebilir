using System.ComponentModel.DataAnnotations;

namespace Kalite.API.Entitity
{
    public class GoodsReceipt
    {
        [Key]
        public int Id { get; set; }
        public DateTime? AcceptanceDate { get; set; }
        public string? StockName { get; set; }
        public string? StockKodu { get; set; }
        public string? Barcod { get; set; }
        public string? CompanyName { get; set; }
        public decimal Quantity { get; set; }
        public string? HalalFoodSafetyCompliance { get; set; } // Uygun / Uygun Değil
        public string? PackagingAppearance { get; set; } // Uygun / Uygun Değil
        public DateTime? ExpiryDate { get; set; } // SKT, TETT
        public string? VehicleCleanliness { get; set; } // Uygun / Uygun Değil
        public string? AcceptanceStatus { get; set; } // Kabul / Red
        public string? Temperature { get; set; }
        public string? LotNo { get; set; }
        public string? ReceivedBy { get; set; }
        public string? DepoAdi { get; set; }
        public int  DepoId { get; set; }
        public string Grup { get; set; }
        public string? AllergenInfo { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}

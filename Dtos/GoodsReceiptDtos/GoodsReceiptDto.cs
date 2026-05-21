using System.ComponentModel.DataAnnotations;

namespace Kalite.API.Dtos.GoodsReceiptDtos
{
    public class GoodsReceiptDto
    {
        public class ResultGoodsReceiptDto
        {
            [Key]
            public int Id { get; set; }

            public DateTime? AcceptanceDate { get; set; }
            public string? StockName { get; set; }
            public int StockId { get; set; }
            public string? StockKodu { get; set; } // Kullanıcı formdan bu kodu seçtiğinde stok tablosunda eşleşecek
            public string? Barcod { get; set; }
            public string? CompanyName { get; set; }
            public decimal Quantity { get; set; }
            public string? HalalFoodSafetyCompliance { get; set; }
            public string? PackagingAppearance { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public string? VehicleCleanliness { get; set; }
            public string? AcceptanceStatus { get; set; } // "Kabul" veya "Red"
            public string? Temperature { get; set; }
            public string? LotNo { get; set; }
            public string? ReceivedBy { get; set; }
            public string? DepoAdi { get; set; }
            public  int  DepoId { get; set; }
            public string Grup { get; set; }
            public DateTime CreatedDate { get; set; } = DateTime.Now;
            public string? AllergenInfo { get; set; }
        }

        public class CreateGoodsReceiptDto
        {
            public DateTime? AcceptanceDate { get; set; }
            public string? StockName { get; set; }
            public string? StockKodu { get; set; }
            public string? Barcod { get; set; }
            public string? CompanyName { get; set; }
            public decimal Quantity { get; set; } // Bu decimal kalsın
            public string? HalalFoodSafetyCompliance { get; set; }
            public string? PackagingAppearance { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public string? VehicleCleanliness { get; set; }
            public string? AcceptanceStatus { get; set; }
            public string? Temperature { get; set; }
            public string? LotNo { get; set; }
            public string? ReceivedBy { get; set; }
            public string? DepoAdi { get; set; }
            public string? Grup { get; set; } // ? eklendi
            public string? AllergenInfo { get; set; } // ? eklendi
            public DateTime CreatedDate { get; set; } = DateTime.Now;
        }

        public class UpdateGoodsReceiptDto
        {
            [Key]
            public int Id { get; set; }

            public DateTime? AcceptanceDate { get; set; }
            public string? StockName { get; set; }
            public string? StockKodu { get; set; } // Kullanıcı formdan bu kodu seçtiğinde stok tablosunda eşleşecek
            public string? Barcod { get; set; }
            public string? CompanyName { get; set; }
            public decimal Quantity { get; set; }
            public string? HalalFoodSafetyCompliance { get; set; }
            public string? PackagingAppearance { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public string? VehicleCleanliness { get; set; }
            public string? AcceptanceStatus { get; set; } // "Kabul" veya "Red"
            public string? Temperature { get; set; }
            public string? LotNo { get; set; }
            public string? ReceivedBy { get; set; }
            public string? DepoAdi { get; set; }
            public string Grup { get; set; }
            public string? AllergenInfo { get; set; }
        }
    }
}

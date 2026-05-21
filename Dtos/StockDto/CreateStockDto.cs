using System.ComponentModel.DataAnnotations;

namespace Kalite.API.Dtos.StockDto
{
   

        public class CreateStockDto
        {
    
        public string StockName { get; set; }
        public string? StockKodu { get; set; }
        public string? Barcod { get; set; }
        public string LotNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? AllergenInfo { get; set; }
        public string? CompanyName { get; set; }
        public string? DepoAdi { get; set; }
        public double Quantity { get; set; }
        public string Grup { get; set; } //canlı, ham madde, yardımcı madde, katkı maddesi
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }

    
}

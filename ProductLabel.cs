using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Xml;

namespace Kalite.API.Entitity
{
    public class ProductLabel
    {
        [Key]
        public int Id { get; set; }

        public int? GoodsReceiptId { get; set; }
        public virtual GoodsReceipt? GoodsReceipt { get; set; }

        public string StockName { get; set; }
        public string? CompanyName { get; set; }
        public double? Quantity { get; set; }
        public string? ParentLotNo { get; set; } // 
        public string? LotNo { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; } // SKT, TETT

        public string? Barcod { get; set; }

        public int DepoId { get; set; }
        public bool AktifMi { get; set; } = true;
        public List<Transfer> Transfers { get; set; }
    }
}

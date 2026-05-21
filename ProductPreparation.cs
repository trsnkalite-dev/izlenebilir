using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kalite.API.Entitity
{
    public class ProductPreparation
    {
      
            [Key]
            public int Id { get; set; }
            public int DepoId { get; set; }
            public string DocumentNo { get; set; } = "TE-FR-77";
            public DateTime PublishDate { get; set; }
            public string RevisionNo { get; set; }
            public string RevisionDate { get; set; }
            public string PageNo { get; set; } = "1/1";

            public DateTime Date { get; set; }
            public string ProductName { get; set; }
             public string BatchNo { get; set; }
            public string DoughTemp { get; set; }
            public string DoughPh { get; set; }
            public string AmbientTemp { get; set; }
            public string FillingTemp { get; set; }
            public string Control { get; set; } = "KYS";

            public string IngredientsData { get; set; } // JSON string of List<IngredientItem>
            public decimal TotalAmount { get; set; }

            public string PreparedBy { get; set; }
            public string ApprovedBy { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.Now;
        
    }

    public class IngredientItem
    {
        public string Name { get; set; }
        public string LotNo { get; set; }
        public string ExpiryDate { get; set; }
        public decimal Amount { get; set; }
    }
}


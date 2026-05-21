using Kalite.API.Entitity;
using System.ComponentModel.DataAnnotations;

namespace Kalite.API.Dtos.ProductPreparationDto
{
    public class ProductPreparationDto
    {
      
            public class ResultProductPreparationDto
            {
                public int Id { get; set; }
                public string DocumentNo { get; set; }
                public DateTime PublishDate { get; set; }
                public string RevisionNo { get; set; }
                public string RevisionDate { get; set; }
                public string PageNo { get; set; }
                public DateTime Date { get; set; }
                public string ProductName { get; set; }
                public string BatchNo { get; set; }
                public string DoughTemp { get; set; }
                public string DoughPh { get; set; }
                public string AmbientTemp { get; set; }
                public string FillingTemp { get; set; }
                public string Control { get; set; }
                public string IngredientsData { get; set; }
                public decimal TotalAmount { get; set; }
                public string PreparedBy { get; set; }
                public string ApprovedBy { get; set; }
                public DateTime CreatedAt { get; set; }
            }

            public class CreateProductPreparationDto
            {
                public string DocumentNo { get; set; }
                public DateTime PublishDate { get; set; }
                public string RevisionNo { get; set; }
                public string RevisionDate { get; set; }
                public string PageNo { get; set; }
                public DateTime Date { get; set; }
                public string ProductName { get; set; }
                public string BatchNo { get; set; }
                public string DoughTemp { get; set; }
                public string DoughPh { get; set; }
                public string AmbientTemp { get; set; }
                public string FillingTemp { get; set; }
                public string Control { get; set; }
                public string IngredientsData { get; set; }
                public decimal TotalAmount { get; set; }
                public string PreparedBy { get; set; }
                public string ApprovedBy { get; set; }
                public int DepoId { get; set; } // Hazırlanan ürünün gireceği depo
        }
    }

            public class UpdateProductPreparationDto
            {
                public int Id { get; set; }
                public string DocumentNo { get; set; }
                public DateTime PublishDate { get; set; }
                public string RevisionNo { get; set; }
                public string RevisionDate { get; set; }
                public string PageNo { get; set; }
                public DateTime Date { get; set; }
                public string ProductName { get; set; }
                public string BatchNo { get; set; }
                public string DoughTemp { get; set; }
                public string DoughPh { get; set; }
                public string AmbientTemp { get; set; }
                public string FillingTemp { get; set; }
                public string Control { get; set; }
                public string IngredientsData { get; set; }
                public decimal TotalAmount { get; set; }
                public string PreparedBy { get; set; }
                public string ApprovedBy { get; set; }
            }

            public class IngredientItemDto
            {
                public int? StockId { get; set; }
                public string Name { get; set; }
                public string BatchNo { get; set; }
                public string ExpiryDate { get; set; }
                public decimal Amount { get; set; }
            }
        
    
}


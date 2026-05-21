namespace Kalite.API.Dtos.RecipeDtos
{
    public class CreateProductionDto
    {
        public string RecipeName { get; set; }
        public decimal TotalWeight { get; set; }
        public DateTime ProductionDate { get; set; }
        public List<ProductionItemDto> Items { get; set; }
    }
    public class ProductionItemDto
    {
        public string StockName { get; set; }
        public string LotNo { get; set; }
        public DateTime? SKT { get; set; }
        public decimal Quantity { get; set; }
    }
    public class ResultRecipeDto
    {
        public int Id { get; set; }
        public string RecipeName { get; set; }
        public List<ResultRecipeItemDto> RecipeItems
        {
            get; set;
        }
    }
    public class ResultRecipeItemDto
    {
        public string StockName { get; set; }
        public decimal Percentage { get; set; } // Hesapladığımız % oran
        public bool IsMainIngredient { get; set; } // Et gibi elle girilecek ürün mü?
    }

}

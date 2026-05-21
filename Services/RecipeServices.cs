using static KaliteWeb.UI.Dto.RecipeDto.RecipeDto;

namespace KaliteWeb.UI.Services
{
    public static class RecipeServices
    {
        public static List<ResultRecipeDto> GetStaticRecipes()
        {
            return new List<ResultRecipeDto>
            {
                new ResultRecipeDto
                {
                    Id = 1,
                    RecipeName = "Piliç Döner Eti Marineli",
                    RecipeItems = new List<ResultRecipeItemDto>
                    {
                        // Ana Girdiler (Elle giriş yapılacaklar)
                        new() { StockName = "PİLİÇ BUT FİLLET DERİLİ", Percentage = 61.83m, IsMainIngredient = true },
                        new() { StockName = "PİLİÇ GÖĞÜS FİLLET DERİLİ", Percentage = 31.36m, IsMainIngredient = true },
                        
                        // Yardımcı Malzemeler (Otomatik hesaplanacaklar)
                        new() { StockName = "SIVI AYÇİÇEK YAĞI", Percentage = 1.27m, IsMainIngredient = false },
                        new() { StockName = "KONSERVE SALÇA-BİBER", Percentage = 1.15m, IsMainIngredient = false },
                        new() { StockName = "KONSERVE SALÇA-DOMATES", Percentage = 0.92m, IsMainIngredient = false },
                        new() { StockName = "W200/300", Percentage = 0.92m, IsMainIngredient = false },
                        new() { StockName = "TUZ", Percentage = 0.75m, IsMainIngredient = false },
                        new() { StockName = "TATLI KIRMIZI TOZ BİBER", Percentage = 0.58m, IsMainIngredient = false },
                        new() { StockName = "G-PHOS SUPER", Percentage = 0.46m, IsMainIngredient = false },
                        new() { StockName = "KIRMIZI PUL BİBER", Percentage = 0.23m, IsMainIngredient = false },
                        new() { StockName = "YOGURT", Percentage = 0.18m, IsMainIngredient = false },
                        new() { StockName = "SARIMSAK GRANÜL", Percentage = 0.11m, IsMainIngredient = false },
                        new() { StockName = "KİMYON TOZ", Percentage = 0.11m, IsMainIngredient = false },
                        new() { StockName = "TOZ KARABİBER", Percentage = 0.11m, IsMainIngredient = false }
                    }
                }
            };
        }
    }
}

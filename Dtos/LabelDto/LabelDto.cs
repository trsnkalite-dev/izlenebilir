namespace Kalite.API.Dtos.LabelDto
{
    public class LabelDto
    {
        public class CreateLabelDto
        {
          

            public int DepoId { get; set; }
            public int GoodsReceiptId { get; set; }
            public double? Quantity { get; set; }
            //public string MevcutDepo { get; set; }

            
        }
        public class ProcessLabelDto
        {
            public string ParentLotNo { get; set; } // Hangi gövdeden parçalandı?
            public string YeniUrunAdi { get; set; } // Örn: Pirzola
            public double YeniMiktar { get; set; }   // Örn: 5 kg
            public int HedefDepoId { get; set; }    // Örn: A3 deposu
        }
        public class ProductLabelResponseDto
        {
            public string LotNo { get; set; }
            public string StockName { get; set; }
            public double Quantity { get; set; }
            
            public int DepoId { get; set; }

            public string CompanyName { get; set; }
            public string ParentLotNo { get; set; }
            public DateTime? CreatedAt { get; set; } = DateTime.Now;
            public DateTime? ExpiryDate { get; set; } // SKT, TETT
            public List<IngredientDetailDto>? ProductionIngredients { get; set; } // Reçete bilgisi için


        }
        public class IngredientDetailDto
        {
            public string Name { get; set; }
            public string BatchNo { get; set; }
            public decimal Amount { get; set; }
        }
        public class TransferRequestDto
        {
            public string LotNo { get; set; }
            public int DepoId { get; set; }
        }
        public class TraceDto
        {
            public string LotNo { get; set; }
            public string StockName { get; set; }
            public double Quantity { get; set; }
           
            public int DepoId { get; set; }
            public string? ParentLotNo { get; set; } // 
            public GoodsReceipeDto GoodsReceipe { get; set; }
            public List<TransferDto> Transfers { get; set; }
        }


        public class GoodsReceipeDto
        {
            public int Id { get; set; }
            public string CompanyName { get; set; }
            public string StockName { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        public class TransferDto
        {
            public string Depo { get; set; }
            public DateTime Tarih { get; set; }
        }
        public class DepoDto
        {
            public int DepoId { get; set; }
            public string DepoAdi { get; set; }
        }
    }
}

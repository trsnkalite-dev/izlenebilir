using System.Text.Json.Serialization;

namespace Kalite.API.Entitity
{
    public class Transfer
    {
        public int Id { get; set; }

        public int ProductLabelId { get; set; }

        [JsonIgnore] // 🔥 EKLE
        public ProductLabel ProductLabel { get; set; }

        public int DepoId { get; set; }

        public Depo Depo { get; set; } // 🔥 ŞART (navigation)

        public DateTime Tarih { get; set; } = DateTime.Now;
    }
}

using Kalite.API.Entitity;

namespace Kalite.API.Services.Abstract
{
    public interface IProductLabelService
    {
        // ✅ Artık depoId kullanıyoruz
        Task<ProductLabel> CreateLabel(int goodsrecipeId, double quantity, int depoId);

        // ✅ Transfer de depoId
        Task Transfer(string lotNo, int depoId);

        // ✅ Lot numarasına göre bul
        Task<ProductLabel> GetByLotNo(string lotNo);

        // ✅ Tüm etiketleri getir
        Task<List<ProductLabel>> GetAll();

        // ⚠️ Burayı da düzeltelim
        Task<List<ProductLabel>> GetByDepo(int depoId);

        // ✅ Etiket pasif
        Task Deactivate(string lotNo);
        Task DeleteByLotNo(string lotNo);
    }
}

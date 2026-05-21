using Kalite.API.Context;
using Kalite.API.Entitity;
using Microsoft.EntityFrameworkCore;

namespace Kalite.API.Services.Abstract
{
    public class ProductLabelService:IProductLabelService
    {
        private readonly ApiContext _context;
        private readonly ILogService _logService;

        public ProductLabelService(ApiContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }
        public async Task<ProductLabel> CreateLabel(int goodsReceipeId, double quantity, int depoId)
        {
            var slaughter = await _context.GoodsReceipts.FindAsync(goodsReceipeId);
            if (slaughter == null) throw new Exception("Kesim kaydı bulunamadı");

            var depo = await _context.Depos.FindAsync(depoId);
            if (depo == null) throw new Exception("Depo bulunamadı");

            // BUGÜNÜN TARİHİNİ ALALIM
            string bugun = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            // 🔥 DÜZELTME: Bugün basılan TÜM etiketleri say (Hangi mal kabul olduğu fark etmez)
            int siraNo = await _context.ProductLabels
                .CountAsync(x => x.LotNo.StartsWith(bugun)) + 1;

            // Lot No: 20260521-001, 20260521-002... şeklinde devam eder
            string lotNo = $"{bugun}-{siraNo:D3}";

            var label = new ProductLabel
            {
                GoodsReceiptId = goodsReceipeId,
                StockName = $"{slaughter.StockName} Eti",
                CompanyName = slaughter.CompanyName,
                Quantity = quantity,
                LotNo = lotNo,
                CreatedAt = DateTime.Now, // Etiketin basıldığı gerçek zaman
                ExpiryDate = slaughter.ExpiryDate,
                Barcod = GenerateEan128(lotNo, quantity),
                DepoId = depoId,
                AktifMi = true
            };

            // ... geri kalan kayıt işlemleri aynı ...
            await _context.ProductLabels.AddAsync(label);
            await _context.SaveChangesAsync();

            // ✅ İlk transfer
            var transfer = new Transfer
            {
                ProductLabelId = label.Id,
                DepoId = depoId,
                Tarih = DateTime.Now

            };

            await _context.Transfers.AddAsync(transfer);
            await _context.SaveChangesAsync();

            await _logService.AddLog(label.Id, "CREATE", $"Ürün {depo.DepoAdi} deposuna oluşturuldu", "admin");

            return label;

        }
        //public async Task<ProductLabel> CreateLabel(int goodsReceipeId, double quantity, int depoId)
        //{
        //    var slaughter = await _context.GoodsReceipts.FindAsync(goodsReceipeId);
        //    if (slaughter == null)
        //        throw new Exception("Kesim kaydı bulunamadı");

        //    var depo = await _context.Depos.FindAsync(depoId);
        //    if (depo == null)
        //        throw new Exception("Depo bulunamadı");

        //    int siraNo = await _context.ProductLabels
        //        .CountAsync(x => x.GoodsReceiptId == goodsReceipeId) + 1;

        //    string lotNo = $"{slaughter.CreatedAt:yyyyMMdd}-{siraNo:D3}";
        //    var label = new ProductLabel
        //    {
        //        GoodsReceiptId = goodsReceipeId,
        //        StockName = $"{slaughter.StockName} Eti",

        //     CompanyName = slaughter.CompanyName, // ✅ BURASI


        //      Quantity = quantity,
        //        LotNo = lotNo,
        //       CreatedAt = slaughter.CreatedAt,
        //        ExpiryDate = slaughter.ExpiryDate,
        //        Barcod = GenerateEan128(lotNo, quantity),
        //        DepoId = depoId,
        //        AktifMi = true
        //    };


        //    await _context.ProductLabels.AddAsync(label);
        //    await _context.SaveChangesAsync();

        //    // ✅ İlk transfer
        //    var transfer = new Transfer
        //    {
        //        ProductLabelId = label.Id,
        //        DepoId = depoId,
        //        Tarih = DateTime.Now

        //    };

        //    await _context.Transfers.AddAsync(transfer);
        //    await _context.SaveChangesAsync();

        //    await _logService.AddLog(label.Id, "CREATE", $"Ürün {depo.DepoAdi} deposuna oluşturuldu", "admin");

        //    return label;
        //}

        private string GenerateEan128(string lot, double weight)
        {
            string gtin = "01234567890128";
            string weightStr = ((int)(weight * 100)).ToString("D6");

            return $"(01){gtin}(10){lot}(3102){weightStr}";
        }

        public async Task Transfer(string lotNo, int depoId)
        {
            var label = await _context.ProductLabels
                .Include(x => x.Transfers)
                .FirstOrDefaultAsync(x => x.LotNo == lotNo);

            if (label == null)
                throw new Exception("Label bulunamadı");

            var depo = await _context.Depos.FindAsync(depoId);
            if (depo == null)
                throw new Exception("Depo bulunamadı");

            var transfer = new Transfer
            {
                ProductLabelId = label.Id,
                DepoId = depoId,
                Tarih = DateTime.Now
            };

            await _context.Transfers.AddAsync(transfer);

            // 🔥 SON DEPO GÜNCELLE
            label.DepoId = depoId;

            await _context.SaveChangesAsync();

            await _logService.AddLog(label.Id, "TRANSFER",
                $"Ürün {depo.DepoAdi} deposuna taşındı", "admin");
        }

        public async Task<ProductLabel> GetByLotNo(string lotNo)
        {
            return await _context.ProductLabels
                .Include(x => x.Transfers)
                .FirstOrDefaultAsync(x => x.LotNo == lotNo);
        }

        public async Task<List<ProductLabel>> GetAll()
        {
            return await _context.ProductLabels
                .Include(x => x.Transfers)
                .Where(x => x.AktifMi)
                .ToListAsync();
        }

        public async Task<List<ProductLabel>> GetByDepo(int depoId)
        {
            return await _context.ProductLabels
                .Where(x => x.DepoId == depoId && x.AktifMi) // 🔥 optimize
                .ToListAsync();
        }

        public async Task Deactivate(string lotNo)
        {
            var label = await _context.ProductLabels
                .FirstOrDefaultAsync(x => x.LotNo == lotNo);

            if (label == null)
                throw new Exception("Label bulunamadı");

            label.AktifMi = false;

            await _context.SaveChangesAsync();

            await _logService.AddLog(label.Id, "EXIT", "Ürün çıkışı yapıldı", "admin");
        }
        public async Task DeleteByLotNo(string lotNo)
        {
            var label = await _context.ProductLabels
                .FirstOrDefaultAsync(x => x.LotNo == lotNo);

            if (label == null)
                throw new Exception("Etiket bulunamadı");

            // 🔥 Bağlı transferleri sil
            var transfers = await _context.Transfers
                .Where(x => x.ProductLabelId == label.Id)
                .ToListAsync();

            _context.Transfers.RemoveRange(transfers);

            // 🔥 Etiketi sil
            _context.ProductLabels.Remove(label);

            await _context.SaveChangesAsync();
        }



    }
}

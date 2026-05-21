using Kalite.API.Context;
using Kalite.API.Entitity;
using Kalite.API.Services.Abstract;

namespace Kalite.API.Services.Concrete
{
    public class LogService: ILogService
    {
        private readonly ApiContext _context;

        public LogService(ApiContext context)
        {
            _context = context;
        }
        public async Task AddLog(int labelId, string action, string desc, string user)
        {
            var log = new LabelLog
            {
                LabelId = labelId,
                Action = action,
                Description = desc,
                LogDate = DateTime.UtcNow, // 🔥 daha doğru
                User = user
            };

            await _context.LabelLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}

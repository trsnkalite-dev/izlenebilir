namespace Kalite.API.Services.Abstract
{
    public interface ILogService
    {
        Task AddLog(int labelId, string action, string desc, string user);
    }
}

using ZatcaService.Helpers;
using ZatcaService.Models;

namespace ZatcaService.Services
{
    public interface IGatewaySettingService
    {
        GatewaySetting GetGatewaySetting();
    }

    public class GatewaySettingService : IGatewaySettingService
    {
        private readonly AppDbContext _dbContext;

        public GatewaySettingService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public GatewaySetting GetGatewaySetting()
        {
            return SettingInitializer.GetOrCreateGatewaySetting(_dbContext);
        }
    }

}

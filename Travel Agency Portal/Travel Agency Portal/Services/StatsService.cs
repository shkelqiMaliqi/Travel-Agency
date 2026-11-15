using Travel_Agency_Portal.Data;

namespace Travel_Agency_Portal.Services;

public sealed class StatsService(StatsRepository repository) : OperationService
{
    public OperationResult GetAdminStats() => Ok(repository.GetAdminStats());
}

using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;

namespace OrquestadorFrontend.Data.Repositories;

// Contract for the Home dashboard. Today only KPIs flow through it; if the
// home screen grows to need more dashboard-level data (e.g. greetings,
// banners), it goes here too instead of leaking model lookups into pages.
public interface IDashboardRepository
{
    Task<IReadOnlyList<DashboardKpi>> GetKpisAsync(UserRole role);
}

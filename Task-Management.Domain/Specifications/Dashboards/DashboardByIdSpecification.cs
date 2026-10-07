using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Dashboards;

public class DashboardByIdSpecification : BaseSpecification<Dashboard>
{
    public DashboardByIdSpecification(int dashboardId)
        : base(d => d.Id == dashboardId)
    {
        AddInclude(d => d.Space!);
        AddInclude(d => d.Owner!);
    }
}

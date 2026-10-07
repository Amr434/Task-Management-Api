using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Dashboards;

public class DashboardsForUserSpecification : BaseSpecification<Dashboard>
{
    public DashboardsForUserSpecification(int userId)
        : base(d =>
            d.Space != null &&
            (d.Space.OwnerId == userId || d.Space.Members.Any(m => m.Id == userId)))
    {
        AddInclude(d => d.Space!);
        AddInclude(d => d.Owner!);
        AddOrderByDescending(d => d.UpdatedAt);
    }
}

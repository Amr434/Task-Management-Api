using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Lists;

public class ListsByProjectSpecification : BaseSpecification<List>
{
    public ListsByProjectSpecification(int projectId) : base(l => l.ProjectId == projectId)
    {
        AddOrderBy(l => l.Order);
    }
}

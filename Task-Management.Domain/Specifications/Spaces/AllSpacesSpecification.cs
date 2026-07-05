using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Spaces;

public class AllSpacesSpecification : BaseSpecification<Space>
{
    public AllSpacesSpecification() : base(s => true)
    {
        AddOrderBy(s => s.Name);
    }
}

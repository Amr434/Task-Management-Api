using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Auth;

public class RefreshTokenWithUserSpecification : BaseSpecification<RefreshToken>
{
    public RefreshTokenWithUserSpecification(string token)
        : base(rt => rt.Token == token)
    {
        AddInclude(rt => rt.User);
    }
}

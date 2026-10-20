using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Users;

public class UserByEmailSpecification : BaseSpecification<User>
{
    public UserByEmailSpecification(string email)
        : base(u => u.Email == email)
    {
    }
}

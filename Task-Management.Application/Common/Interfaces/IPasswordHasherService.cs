using Task_Management.Domain.Entities;

namespace Task_Management.Application.Common.Interfaces;

public interface IPasswordHasherService
{
    string Hash(User user, string password);
    bool Verify(User user, string passwordHash, string providedPassword);
}

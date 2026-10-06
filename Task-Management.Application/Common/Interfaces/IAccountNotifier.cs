namespace Task_Management.Application.Common.Interfaces;

// Implemented in Infrastructure with email: tells someone an admin created an
// account for them, with the temporary password they must change at first login.
public interface IAccountNotifier
{
    Task AccountCreatedAsync(string email, string firstName, string createdByName, string temporaryPassword);
}

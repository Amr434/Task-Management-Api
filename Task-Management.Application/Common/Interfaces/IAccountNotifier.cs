namespace Task_Management.Application.Common.Interfaces;

// Implemented in Infrastructure with email: account messages sent to one person.
public interface IAccountNotifier
{
    // An admin created an account for them, with the temporary password they
    // must change at first login.
    Task AccountCreatedAsync(string email, string firstName, string createdByName, string temporaryPassword);

    // They asked to reset a forgotten password: sends the link that lets them
    // choose a new one. resetToken goes in the link; it is never stored as-is.
    Task PasswordResetRequestedAsync(string email, string firstName, string resetToken, int validForMinutes);
}

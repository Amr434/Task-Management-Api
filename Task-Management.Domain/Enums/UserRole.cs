namespace Task_Management.Domain.Enums;

public enum UserRole
{
    Member = 0,
    Admin = 1,
    // Exactly one seeded account. Manages Admins and Members; can never be
    // created, promoted to, edited or deactivated through the API.
    SuperAdmin = 2
}

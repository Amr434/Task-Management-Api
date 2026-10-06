using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;

namespace Task_Management.Infrastructure.Email;

internal static class EmailRecipients
{
    // Active users with an email address; unknown ids are skipped.
    public static async Task<List<User>> LoadAsync(IUnitOfWork unitOfWork, IEnumerable<int> userIds)
    {
        var users = new List<User>();
        foreach (var id in userIds.Distinct())
        {
            var user = await unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user is { IsActive: true } && !string.IsNullOrWhiteSpace(user.Email))
            {
                users.Add(user);
            }
        }
        return users;
    }

    // Same format TaskHistory uses for names, so history values can be matched.
    public static string Name(User user)
    {
        var name = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrEmpty(name) ? user.Email : name;
    }
}

using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    Task<int> CompleteAsync();

    // History entries saved by CompleteAsync since the last call; clears the list.
    // Lets the notification step see every change a command made.
    IReadOnlyList<TaskActivity> TakeSavedActivities();
}

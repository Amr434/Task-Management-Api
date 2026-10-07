using System.Collections;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;

namespace Task_Management.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly TaskManagementDbContext _context;
    private Hashtable _repositories;
    private readonly List<TaskActivity> _savedActivities = new();

    public UnitOfWork(TaskManagementDbContext context)
    {
        _context = context;
        _repositories = new Hashtable();
    }

    public async Task<int> CompleteAsync()
    {
        var added = _context.ChangeTracker.Entries<TaskActivity>()
            .Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        var result = await _context.SaveChangesAsync();
        _savedActivities.AddRange(added);
        return result;
    }

    public IReadOnlyList<TaskActivity> TakeSavedActivities()
    {
        var saved = _savedActivities.ToList();
        _savedActivities.Clear();
        return saved;
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity).Name;

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(GenericRepository<>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context);

            _repositories.Add(type, repositoryInstance);
        }

        return (IGenericRepository<TEntity>)_repositories[type]!;
    }
}

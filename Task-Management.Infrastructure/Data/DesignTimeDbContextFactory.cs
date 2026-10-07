using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Task_Management.Infrastructure.Data;

// Lets `dotnet ef` generate migrations against this project directly, without
// booting the Api project (whose win-x86 RuntimeIdentifier for MonsterASP
// publishing breaks EF's design-time assembly load). `migrations add` never
// connects to a database, so a placeholder connection string is enough — the
// real one is applied at runtime by Program.cs, which also auto-migrates.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TaskManagementDbContext>
{
    public TaskManagementDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb;Trusted_Connection=True;")
            .Options;
        return new TaskManagementDbContext(options);
    }
}

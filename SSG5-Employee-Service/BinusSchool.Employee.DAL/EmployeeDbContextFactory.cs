using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BinusSchool.Persistence.EmployeeDb
{
    public class EmployeeDbContextFactory : DesignTimeDbContextFactory<EmployeeDbContext>, IDesignTimeDbContextFactory<EmployeeDbContext>
    {
        protected override EmployeeDbContext CreateDbContextInstance(DbContextOptions<EmployeeDbContext> options)
        {
            return new EmployeeDbContext(options);
        }
    }
}

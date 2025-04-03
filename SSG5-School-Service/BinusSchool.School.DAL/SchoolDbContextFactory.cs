using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BinusSchool.Persistence.SchoolDb
{
    public class SchoolDbContextFactory : DesignTimeDbContextFactory<SchoolDbContext>, IDesignTimeDbContextFactory<SchoolDbContext>
    {
        protected override SchoolDbContext CreateDbContextInstance(DbContextOptions<SchoolDbContext> options)
        {
            return new SchoolDbContext(options);
        }
    }
}
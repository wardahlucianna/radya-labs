using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BinusSchool.Persistence.TeachingDb
{
    public class TeachingDbContextFactory : DesignTimeDbContextFactory<TeachingDbContext>, IDesignTimeDbContextFactory<TeachingDbContext>
    {
        protected override TeachingDbContext CreateDbContextInstance(DbContextOptions<TeachingDbContext> options)
        {
            return new TeachingDbContext(options);
        }
    }
}
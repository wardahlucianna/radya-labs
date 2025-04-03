using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BinusSchool.Persistence.DocumentDb
{
    public class DocumentDbContextFactory : DesignTimeDbContextFactory<DocumentDbContext>, IDesignTimeDbContextFactory<DocumentDbContext>
    {
        protected override DocumentDbContext CreateDbContextInstance(DbContextOptions<DocumentDbContext> options)
        {
            return new DocumentDbContext(options);
        }
    }
}
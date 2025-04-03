using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BinusSchool.Persistence.UserDb
{
    public class UserDbContextFactory : DesignTimeDbContextFactory<UserDbContext>, IDesignTimeDbContextFactory<UserDbContext>
    {
        protected override UserDbContext CreateDbContextInstance(DbContextOptions<UserDbContext> options)
        {
            return new UserDbContext(options);
        }
    }
}
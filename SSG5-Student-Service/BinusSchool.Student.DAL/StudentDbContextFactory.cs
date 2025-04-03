using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BinusSchool.Persistence.StudentDb
{
    public class StudentDbContextFactory : DesignTimeDbContextFactory<StudentDbContext>, IDesignTimeDbContextFactory<StudentDbContext>
    {
        protected override StudentDbContext CreateDbContextInstance(DbContextOptions<StudentDbContext> options)
        {
            return new StudentDbContext(options);
        }
    }
}
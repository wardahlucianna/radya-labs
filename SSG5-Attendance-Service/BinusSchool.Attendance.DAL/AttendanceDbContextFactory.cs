using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BinusSchool.Persistence.AttendanceDb
{
    public class AttendanceDbContextFactory : DesignTimeDbContextFactory<AttendanceDbContext>, IDesignTimeDbContextFactory<AttendanceDbContext>
    {
        protected override AttendanceDbContext CreateDbContextInstance(DbContextOptions<AttendanceDbContext> options)
        {
            return new AttendanceDbContext(options);
        }
    }
}

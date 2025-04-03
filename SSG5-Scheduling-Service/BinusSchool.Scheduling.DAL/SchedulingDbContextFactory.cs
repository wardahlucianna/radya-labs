using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BinusSchool.Persistence.SchedulingDb
{
    public class SchedulingDbContextFactory : DesignTimeDbContextFactory<SchedulingDbContext>, IDesignTimeDbContextFactory<SchedulingDbContext>
    {
        protected override SchedulingDbContext CreateDbContextInstance(DbContextOptions<SchedulingDbContext> options)
        {
            return new SchedulingDbContext(options);
        }
    }
}

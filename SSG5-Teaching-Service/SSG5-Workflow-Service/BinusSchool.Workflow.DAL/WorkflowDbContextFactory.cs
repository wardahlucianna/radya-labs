using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BinusSchool.Persistence.WorkflowDb
{
    public class WorkflowDbContextFactory : DesignTimeDbContextFactory<WorkflowDbContext>, IDesignTimeDbContextFactory<WorkflowDbContext>
    {
        protected override WorkflowDbContext CreateDbContextInstance(DbContextOptions<WorkflowDbContext> options)
        {
            return new WorkflowDbContext(options);
        }
    }
}
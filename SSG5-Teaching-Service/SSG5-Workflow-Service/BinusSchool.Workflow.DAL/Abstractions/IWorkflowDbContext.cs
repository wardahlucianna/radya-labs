using BinusSchool.Domain.Abstractions;

namespace BinusSchool.Persistence.WorkflowDb.Abstractions
{
    public interface IWorkflowDbContext : IAppDbContext<IWorkflowEntity> { }
}
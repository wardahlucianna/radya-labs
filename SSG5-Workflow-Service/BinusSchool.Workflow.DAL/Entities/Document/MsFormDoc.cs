using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.WorkflowDb.Entities.Document
{
    public class MsFormDoc : AuditEntity, IWorkflowEntity
    {
    }

    internal class MsFormDocConfiguration : AuditEntityConfiguration<MsFormDoc>
    {
        public override void Configure(EntityTypeBuilder<MsFormDoc> builder)
        {
            base.Configure(builder);
        }
    }
}

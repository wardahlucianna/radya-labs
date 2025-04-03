using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtStudentDataApprovalStatus : AuditNoUniqueEntity, IStudentEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdApprovalStatus { get; set; }
        public string ApprovalStatusName { get; set; }

        public virtual ICollection<TrStudentInfoUpdate> StudentInfoUpdate { get; set; }

    }
    internal class LtStudentDataApprovalStatusConfiguration : AuditNoUniqueEntityConfiguration<LtStudentDataApprovalStatus>
    {
        public override void Configure(EntityTypeBuilder<LtStudentDataApprovalStatus> builder)
        {
            builder.HasKey(p => p.IdApprovalStatus);  
                       
            builder.Property(x => x.ApprovalStatusName)             
                .HasMaxLength(50);

            base.Configure(builder);
        }
    }
}

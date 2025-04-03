using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtParentSalaryGroup : AuditNoUniqueEntity, IStudentEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdParentSalaryGroup { get; set; }
        public string ParentSalaryGroupName { get; set; }
        public virtual ICollection<MsParent> Parent { get; set; }

    }
    internal class LtParentSalaryGroupConfiguration : AuditNoUniqueEntityConfiguration<LtParentSalaryGroup>
    {
        public override void Configure(EntityTypeBuilder<LtParentSalaryGroup> builder)
        {
            builder.HasKey(p => p.IdParentSalaryGroup);

            builder.Property(x => x.ParentSalaryGroupName)
                .HasMaxLength(50);

            base.Configure(builder);
        }

    }

}

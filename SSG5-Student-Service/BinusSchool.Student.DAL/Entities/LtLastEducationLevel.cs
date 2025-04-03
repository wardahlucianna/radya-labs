using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtLastEducationLevel : AuditNoUniqueEntity, IStudentEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdLastEducationLevel { get; set; }
        public string LastEducationLevelName { get; set; }
        public virtual ICollection<MsParent> Parent { get; set; }
    }
    internal class LtLastEducationLevelConfiguration : AuditNoUniqueEntityConfiguration<LtLastEducationLevel>
    {
        public override void Configure(EntityTypeBuilder<LtLastEducationLevel> builder)
        {
            builder.HasKey(p => p.IdLastEducationLevel);

            builder.Property(x => x.LastEducationLevelName)
                .HasMaxLength(100);

            base.Configure(builder);    
        }
    }
}

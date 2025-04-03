using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtLastEducationLevelStudent : AuditNoUniqueEntity, IStudentEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdLastEducationLevel { get; set; }
        public string LastEducationLevelName { get; set; }      
    }
    internal class LtLastEducationLevelStudentConfiguration : AuditNoUniqueEntityConfiguration<LtLastEducationLevelStudent>
    {
        public override void Configure(EntityTypeBuilder<LtLastEducationLevelStudent> builder)
        {
            builder.HasKey(p => p.IdLastEducationLevel);

            builder.Property(x => x.LastEducationLevelName)
                .HasMaxLength(100);

            base.Configure(builder);
        }
    }

}

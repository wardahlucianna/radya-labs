using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtBinusianStatus : AuditNoUniqueEntity, IStudentEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int16 BinusianStatus { get; set; }
        public string BinusianStatusName { get; set; }

        public virtual ICollection<MsParent> Parent { get; set; }
    }
    internal class LtBinusianStatusConfiguration : AuditNoUniqueEntityConfiguration<LtBinusianStatus>
    {
        public override void Configure(EntityTypeBuilder<LtBinusianStatus> builder)
        {
            builder.HasKey(p => p.BinusianStatus);

            builder.Property(x => x.BinusianStatusName)
                .HasMaxLength(50);

            base.Configure(builder);
        }

    }
}

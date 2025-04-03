using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtAliveStatus : AuditNoUniqueEntity, IStudentEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int16 AliveStatus { get; set; }
        public string AliveStatusName { get; set; }

        public virtual ICollection<MsParent> Parent { get; set; }
    }
    internal class LtAliveStatusConfiguration : AuditNoUniqueEntityConfiguration<LtAliveStatus>
    {
        public override void Configure(EntityTypeBuilder<LtAliveStatus> builder)
        {
            builder.HasKey(p => p.AliveStatus);

            builder.Property(x => x.AliveStatusName)
                .HasMaxLength(50);

            base.Configure(builder);
        }

    }
}

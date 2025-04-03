using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtStayingWith : AuditNoUniqueEntity, IStudentEntity
    {

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int16 IdStayingWith { get; set; }
        public string StayingWithName { get; set; }

        public virtual ICollection<MsStudent> Student { get; set; }
    }
    internal class LtStayingWithConfiguration : AuditNoUniqueEntityConfiguration<LtStayingWith>
    {
        public override void Configure(EntityTypeBuilder<LtStayingWith> builder)
        {
            builder.HasKey(p => p.IdStayingWith);

            builder.Property(x => x.IdStayingWith)
                .IsRequired();


            builder.Property(x => x.StayingWithName)
                .HasMaxLength(50);

            base.Configure(builder);
        }

    }
}

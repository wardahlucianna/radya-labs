using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.School
{
    public class MsGradePathway : AuditEntity, IDocumentEntity
    {
        public string IdGrade { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<MsGradePathwayClassroom> GradePathwayClassrooms { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }

    }

    internal class MsGradePathwayConfiguration : AuditEntityConfiguration<MsGradePathway>
    {
        public override void Configure(EntityTypeBuilder<MsGradePathway> builder)
        {
            builder.Property(x => x.IdGrade)
                .HasMaxLength(36);

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.GradePathways)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsGradePathway_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

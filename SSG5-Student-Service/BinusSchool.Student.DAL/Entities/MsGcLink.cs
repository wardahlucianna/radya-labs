using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsGcLink : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string Link { get; set; }
        public string LinkDescription { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }    
        public virtual ICollection<MsGcLinkGrade> GcLinkGrades { get; set; }
        public virtual ICollection<MsGcLinkLogo> GcLinkLogo { get; set; }
    }

    internal class MsGcLinkConfiguration : AuditEntityConfiguration<MsGcLink>
    {
        public override void Configure(EntityTypeBuilder<MsGcLink> builder)
        {
            builder.Property(p => p.Link).IsRequired().HasMaxLength(450);
            builder.Property(p => p.LinkDescription).IsRequired().HasMaxLength(450);

            builder.HasOne(x => x.AcademicYear)
             .WithMany(x => x.GcLink)
             .HasForeignKey(fk => fk.IdAcademicYear)
             .HasConstraintName("FK_MsGcLink_MsAcademicYear")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}

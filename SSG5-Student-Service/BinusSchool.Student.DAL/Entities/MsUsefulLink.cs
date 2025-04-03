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
    public class MsUsefulLink : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsUsefulLinkGrade> UsefulLinkGrade { get; set; } 
    }

    internal class MsUsefulLinkConfiguration : AuditEntityConfiguration<MsUsefulLink>
    {
        public override void Configure(EntityTypeBuilder<MsUsefulLink> builder)
        {
            builder.Property(x => x.Description).HasMaxLength(450);
            builder.Property(x => x.Link).HasMaxLength(100);

            builder.HasOne(x => x.AcademicYear)
             .WithMany(x => x.UsefulLink)
             .HasForeignKey(fk => fk.IdAcademicYear)
             .HasConstraintName("FK_MsUsefulLink_MsAcademicYear")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}

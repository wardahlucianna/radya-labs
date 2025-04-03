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
    public class MsUniversityPortal : AuditEntity, IStudentEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string IdSchool { get; set; }
        public string IdSchoolFrom { get; set; }
        public bool IsLogoAsSquareImage { get; set; }
        public bool IsShareOtherSchool { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsSchool SchoolFrom { get; set; }
        public virtual ICollection<MsUniversityPortalApproval> UniversityPortalApproval { get; set; }
        public virtual ICollection<MsUniversityPortalFactSheet> UniversityPortalFactSheet { get; set; }
        public virtual ICollection<MsUniversityPortalLogo> UniversityPortalLogo { get; set; }

    }

    internal class MsUniversityPortalConfiguration : AuditEntityConfiguration<MsUniversityPortal>
    {
        public override void Configure(EntityTypeBuilder<MsUniversityPortal> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(450);
            builder.Property(x => x.ContactPerson).HasMaxLength(30);
            builder.Property(x => x.Email).HasMaxLength(50);
            builder.Property(x => x.Website).IsRequired().HasMaxLength(50);

            builder.HasOne(x => x.School)
             .WithMany(x => x.UniversityPortal)
             .HasForeignKey(fk => fk.IdSchool)
             .HasConstraintName("FK_MsUniversityPortal_MsSchool")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();
            
            builder.HasOne(x => x.SchoolFrom)
             .WithMany(x => x.UniversityPortalFrom)
             .HasForeignKey(fk => fk.IdSchoolFrom)
             .HasConstraintName("FK_MsUniversityPortalFrom_MsSchoolFrom")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}

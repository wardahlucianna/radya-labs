using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsCounselor : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string OfficerLocation { get; set; }
        public string ExtensionNumber { get; set; }
        public string OtherInformation { get; set; }
        public string IdRole { get; set; }
        public string IdPosition { get; set; }

        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsUser User { get; set; }
        public virtual LtRole Role { get; set; }
        public virtual LtPosition Position { get; set; }

        public virtual ICollection<MsCounselorGrade> CounselorGrade { get; set; }
        public virtual ICollection<MsCounselorPhoto> CounselorPhoto { get; set; }
        public virtual ICollection<TrCounselingServicesEntry> CounselingServicesEntry { get; set; }
    }

    internal class MsCounselorConfiguration : AuditEntityConfiguration<MsCounselor>
    {
        public override void Configure(EntityTypeBuilder<MsCounselor> builder)
        {
            builder.Property(x => x.OfficerLocation).HasMaxLength(100);
            builder.Property(x => x.ExtensionNumber).HasMaxLength(20);
            builder.Property(x => x.ExtensionNumber).HasMaxLength(150);

            builder.HasOne(x => x.AcademicYear)
             .WithMany(x => x.Counselor)
             .HasForeignKey(fk => fk.IdAcademicYear)
             .HasConstraintName("FK_MsCounselor_MsAcademicYear")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.Role)
             .WithMany(x => x.Counselors)
             .HasForeignKey(fk => fk.IdRole)
             .HasConstraintName("FK_MsCounselor_LtRole")
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Position)
             .WithMany(x => x.Counselors)
             .HasForeignKey(fk => fk.IdPosition)
             .HasConstraintName("FK_MsCounselor_LtPosition")
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
             .WithMany(x => x.Counselor)
             .HasForeignKey(fk => fk.IdUser)
             .HasConstraintName("FK_MsCounselor_MsUser")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}

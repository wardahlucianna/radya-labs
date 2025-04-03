using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentExit : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdUserFather { get; set; }
        public string FatherEmail { get; set; }
        public string FatherPhone { get; set; }
        public string IdUserMother { get; set; }
        public string MotherEmail { get; set; }
        public string MotherPhone { get; set; }
        public DateTime StartExit { get; set; }
        public string Explain { get; set; }
        public bool IsMeetSchoolTeams { get; set; }
        public string NewSchoolName { get; set; }
        public string NewSchoolCity { get; set; }
        public string NewSchoolCountry { get; set; }
        public StatusExitStudent Status { get; set; }
        public string IdHomeroom { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public virtual MsParent UserFather { get; set; }
        public virtual MsParent UserMother { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual ICollection<TrStudentExitReason> TrStudentExitReasons { get; set; }
        public virtual ICollection<TrStudentExitStatus> StudentExitStatuses { get; set; }

    }

    internal class TrStudentExitConfiguration : AuditEntityConfiguration<TrStudentExit>
    {
        public override void Configure(EntityTypeBuilder<TrStudentExit> builder)
        {
            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdHomeroomStudent)
                 .HasMaxLength(36)
                 .IsRequired();

            builder.Property(x => x.IdUserFather)
               .HasMaxLength(36);

            builder.Property(x => x.FatherEmail)
                .HasMaxLength(100);

            builder.Property(x => x.FatherPhone)
               .HasMaxLength(100);

            builder.Property(x => x.IdUserMother)
               .HasMaxLength(36);

            builder.Property(x => x.MotherEmail)
                .HasMaxLength(100);

            builder.Property(x => x.MotherPhone)
               .HasMaxLength(100);

            builder.Property(x => x.StartExit)
                .IsRequired();

            builder.Property(x => x.Explain)
               .HasMaxLength(1054)
               .IsRequired();

            builder.Property(x => x.IsMeetSchoolTeams)
               .IsRequired();

            builder.Property(x => x.NewSchoolName)
              .HasMaxLength(100)
              .IsRequired();

            builder.Property(x => x.NewSchoolCity)
              .HasMaxLength(100)
              .IsRequired();

            builder.Property(x => x.NewSchoolCountry)
              .HasMaxLength(100)
              .IsRequired();

            builder.Property(e => e.Status).HasMaxLength(maxLength: 20)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (StatusExitStudent)Enum.Parse(typeof(StatusExitStudent), valueFromDb))
               .IsRequired();

            builder.Property(x => x.IdHomeroom)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.EffectiveDate);

            builder.HasOne(x => x.AcademicYear)
              .WithMany(y => y.StudentExits)
              .HasForeignKey(fk => fk.IdAcademicYear)
              .HasConstraintName("FK_StudentExit_MsAcademicYear")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Homeroom)
              .WithMany(y => y.StudentExits)
              .HasForeignKey(fk => fk.IdHomeroom)
              .HasConstraintName("FK_StudentExit_MsHomeroom")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.HomeroomStudent)
              .WithMany(y => y.StudentExits)
              .HasForeignKey(fk => fk.IdHomeroomStudent)
              .HasConstraintName("FK_StudentExit_MsStudent")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.UserFather)
              .WithMany(y => y.StudentExitFathers)
              .HasForeignKey(fk => fk.IdUserFather)
              .HasConstraintName("FK1_StudentExit_MsUser")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.UserMother)
              .WithMany(y => y.StudentExitMothers)
              .HasForeignKey(fk => fk.IdUserMother)
              .HasConstraintName("FK2_StudentExit_MsUser")
              .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }

    }
}

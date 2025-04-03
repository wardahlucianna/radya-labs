using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.Scheduling
{
    public class MsHomeroomStudent : AuditEntity, IStudentEntity
    {
        public string IdHomeroom { get; set; }
        public string IdStudent { get; set; }
        public int Semester { get; set; }
        //public Gender Gender { get; set; }
        //public string Religion { get; set; }

        public virtual ICollection<MsHomeroomStudentEnrollment> HomeroomStudentEnrollments { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual ICollection<MsStudentFreezeMeritDemerit> StudentFreezeMeritDemerits { get; set; }
        public virtual ICollection<TrEntryMeritStudent> EntryMeritStudents { get; set; }
        public virtual ICollection<TrEntryDemeritStudent> EntryDemeritStudents { get; set; }
        public virtual ICollection<TrStudentPoint> StudentPoints { get; set; }
        public virtual ICollection<TrExperience> TrExperiences { get; set; }
        public virtual ICollection<TrCasAdvisorStudent> TrCasAdvisorStudents { get; set; }
        public virtual MsStudentExitSetting StudentExitSetting { get; set; }
        public virtual ICollection<TrStudentExit> StudentExits { get; set; }

    }

    internal class MsHomeroomStudentConfiguration : AuditEntityConfiguration<MsHomeroomStudent>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroomStudent> builder)
        {
            builder.HasOne(x => x.Student)
                .WithMany(x => x.MsHomeroomStudents)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsHomeroomStudent_MsStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(x => x.Semester)
               .IsRequired();

            //builder.Property(x => x.Gender)
            //    .HasConversion<string>()
            //    .HasMaxLength(6)
            //    .IsRequired();

            //builder.Property(x => x.Religion)
            //    .HasMaxLength(36)
            //    .IsRequired();

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.HomeroomStudents)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_MsHomeroomStudent_MsHomeroom")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.StudentDb.Entities.Employee
{
    public class MsStaff : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdBinusian { get; set; }
        public string ShortName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IdSchool { get; set; }
        public string InitialName { get; set; }
        public int? IdDesignation { get; set; }
        public string BinusianEmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string GenderName { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual LtDesignation Designation { get; set; }
        public virtual ICollection<MsHomeroomTeacher> HomeroomTeachers { get; set; }
        public virtual ICollection<MsLessonTeacher> LessonTeachers { get; set; }
        public virtual ICollection<MsSurveyRespondent> SurveyRespondents { get; set; }
        public virtual ICollection<TrProfileDataFieldPrivilege> ProfileDataFieldPrivileges { get; set; }

    }

    internal class MsStaffConfiguration : AuditNoUniqueEntityConfiguration<MsStaff>
    {
        public override void Configure(EntityTypeBuilder<MsStaff> builder)
        {

            builder.HasKey(x => x.IdBinusian);

            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36);

            builder.Property(x => x.ShortName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.LastName)
               .HasMaxLength(100);

            builder.Property(x => x.IdSchool)
               .HasMaxLength(36);

            //builder.Property(x => x.IdEmployee)
            //   .HasMaxLength(36);   

            builder.Property(x => x.InitialName)
               .HasMaxLength(5);

            builder.Property(x => x.BinusianEmailAddress)
                .HasMaxLength(60);

            builder.Property(x => x.PersonalEmailAddress)
                .HasMaxLength(60);

            builder.Property(x => x.GenderName)
                .HasMaxLength(60);

            builder.HasOne(x => x.School)
               .WithMany(y => y.Staffs)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_MsStaff_MsSchool")
               .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Designation)
                .WithMany(x => x.Staffs)
                .HasForeignKey(fk => fk.IdDesignation)
                .HasConstraintName("FK_MsStaff_LtDesignation")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}

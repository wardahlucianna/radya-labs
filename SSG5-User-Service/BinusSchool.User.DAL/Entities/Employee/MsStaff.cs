using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities.Employee
{
    public class MsStaff : AuditNoUniqueEntity, IUserEntity
    {
        public string IdBinusian { get; set; }    
        public string ShortName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IdSchool { get; set; }
        public virtual ICollection<MsLessonTeacher> LessonTeachers { get; set; }
        public virtual ICollection<MsHomeroomTeacher> HomeroomTeachers { get; set; }
        public virtual MsSchool School { get; set; }
        public string InitialName { get; set; }
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

            builder.HasOne(x => x.School)
              .WithMany(x => x.Staffs)
              .HasForeignKey(fk => fk.IdSchool)
              .HasConstraintName("FK_MsStaff_MsSchool")
              .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.InitialName)
               .HasMaxLength(5);

            base.Configure(builder);
        }
    }
}

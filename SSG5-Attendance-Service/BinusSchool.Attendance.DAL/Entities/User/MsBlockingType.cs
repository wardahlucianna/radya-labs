using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.User
{
    public class MsBlockingType : AuditEntity, IAttendanceEntity
    {
        public string IdSchool { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Order { get; set; }
        public virtual MsSchool School { get; set; }

        public virtual ICollection<MsBlockingCategoryType> BlockingCategoryTypes { get; set; }
        public virtual ICollection<MsStudentBlocking> StudentBlockings { get; set; }
        public virtual ICollection<MsBlockingTypeAtdSetting> BlockingTypeAtdSetting { get; set; }
        public virtual ICollection<HMsStudentBlocking> HistoryStudentBlockings { get; set; }
    }

    internal class MsBlockingTypeConfiguration : AuditEntityConfiguration<MsBlockingType>
    {
        public override void Configure(EntityTypeBuilder<MsBlockingType> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.IdSchool)
              .HasMaxLength(36)
              .IsRequired();

            builder.HasOne(x => x.School)
               .WithMany(x => x.BlockingTypes)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_MsBlockingType_MsSchool")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.Property(x => x.Order)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

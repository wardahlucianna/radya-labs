using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.User
{
    public class MsBlockingCategory : AuditEntity, IAttendanceEntity
    {
        public string IdSchool { get; set; }
        public string Name { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsBlockingCategoryType> BlockingCategoryTypes { get; set; }
        public virtual ICollection<MsStudentBlocking> StudentBlockings { get; set; }
        public virtual ICollection<HMsStudentBlocking> HistoryStudentBlockings { get; set; }
    }

    internal class MsBlockingCategoryConfiguration : AuditEntityConfiguration<MsBlockingCategory>
    {
        public override void Configure(EntityTypeBuilder<MsBlockingCategory> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.IdSchool)
               .HasMaxLength(36)
               .IsRequired();


            builder.HasOne(x => x.School)
                .WithMany(x => x.BlockingCategorys)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsBlockingCategory_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

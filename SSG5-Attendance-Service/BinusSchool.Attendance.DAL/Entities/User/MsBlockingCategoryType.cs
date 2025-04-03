using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.User
{
    public class MsBlockingCategoryType : AuditEntity, IAttendanceEntity
    {
        public string IdBlockingCategory { get; set; }
        public string IdBlockingType { get; set; }
        public virtual MsBlockingCategory BlockingCategory { get; set; }
        public virtual MsBlockingType BlockingType { get; set; }
    }

    internal class MsBlockingCategoryTypeConfiguration : AuditEntityConfiguration<MsBlockingCategoryType>
    {
        public override void Configure(EntityTypeBuilder<MsBlockingCategoryType> builder)
        {
            builder.Property(x => x.IdBlockingCategory)
                .HasMaxLength(36);

            builder.Property(x => x.IdBlockingType)
                .HasMaxLength(36);

            builder.HasOne(x => x.BlockingCategory)
               .WithMany(x => x.BlockingCategoryTypes)
               .HasForeignKey(fk => fk.IdBlockingCategory)
               .HasConstraintName("FK_MsBlockingCategoryType_MsBlockingCategory")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.BlockingType)
                .WithMany(x => x.BlockingCategoryTypes)
                .HasForeignKey(fk => fk.IdBlockingType)
                .HasConstraintName("FK_MsBlockingCategoryType_MsBlockingType")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

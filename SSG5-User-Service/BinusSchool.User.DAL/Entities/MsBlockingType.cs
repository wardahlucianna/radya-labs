using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsBlockingType : AuditEntity, IUserEntity
    {
        public string IdSchool { get; set; }
        public string Name { get; set; }
        public string IdFeature { get; set; }
        public string Category { get; set; }
        public int Order { get; set; }
        public virtual MsFeature Feature { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsBlockingCategoryType> BlockingCategoryTypes { get; set; }
        public virtual ICollection<MsBlockingTypeSubFeature> BlockingTypeSubFeature { get; set; }
        public virtual ICollection<MsStudentBlocking> StudentBlockings { get; set; }
        public virtual ICollection<HMsStudentBlocking> HistoryStudentBlockings { get; set; }
    }

    internal class MsBlockingTypeConfiguration : AuditEntityConfiguration<MsBlockingType>
    {
        public override void Configure(EntityTypeBuilder<MsBlockingType> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Order)
                .IsRequired();

            builder.HasOne(x => x.Feature)
                .WithMany(x => x.BlockingTypes)
                .HasForeignKey(fk => fk.IdFeature)
                .HasConstraintName("FK_MsBlockingType_MsFeature")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.School)
            .WithMany(x => x.BlockingTypes)
            .HasForeignKey(fk => fk.IdSchool)
            .HasConstraintName("FK_MsBlockingType_MsSchool")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

            base.Configure(builder);
        }
    }
}

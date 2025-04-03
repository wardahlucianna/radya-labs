using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.AttendanceDb.Entities.User
{
    public class MsBlockingMessage : AuditEntity, IAttendanceEntity
    {
        public string IdSchool { get; set; }
        public string Content { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsBlockingMessageConfiguration : AuditEntityConfiguration<MsBlockingMessage>
    {
        public override void Configure(EntityTypeBuilder<MsBlockingMessage> builder)
        {
            builder.Property(x => x.Content)
                .HasMaxLength(int.MaxValue)
                .IsRequired();

            builder.Property(x => x.IdSchool)
              .HasMaxLength(36)
              .IsRequired();

            builder.HasOne(x => x.School)
               .WithMany(x => x.BlockingMessages)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_MsBlockingMessage_MsSchool")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            base.Configure(builder);
        }
    }
}

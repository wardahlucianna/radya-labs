using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsBlockingMessage : AuditEntity, IUserEntity
    {
        public string IdSchool { get; set; }
        public string IdCategory { get; set; }
        public string Content { get; set; }
        public virtual MsBlockingCategory Category { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsBlockingMessageConfiguration : AuditEntityConfiguration<MsBlockingMessage>
    {
        public override void Configure(EntityTypeBuilder<MsBlockingMessage> builder)
        {
            builder.Property(x => x.Content)
                .HasMaxLength(int.MaxValue)
                .IsRequired();

            builder.HasOne(x => x.Category)
                .WithMany(x => x.BlockingMessage)
                .HasForeignKey(fk => fk.IdCategory)
                .HasConstraintName("FK_MsBlockingMessage_MsBlockingCategory")
                .OnDelete(DeleteBehavior.NoAction);

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

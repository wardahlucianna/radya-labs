using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrMessageAttachment : AuditEntity, IUserEntity
    {
        public string IdMessage { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public int Filesize { get; set; }
        public virtual TrMessage Message { get; set; }
    }

    internal class TrMessageAttachmentConfiguration : AuditEntityConfiguration<TrMessageAttachment>
    {
        public override void Configure(EntityTypeBuilder<TrMessageAttachment> builder)
        {
            builder.HasOne(x => x.Message)
                 .WithMany(x => x.MessageAttachments)
                 .HasForeignKey(fk => fk.IdMessage)
                 .HasConstraintName("FK_TrMessageAttachment_TrMessage")
                 .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired();
            builder.Property(p => p.Url).HasMaxLength(450);
            builder.Property(p => p.Filename).HasMaxLength(350).IsRequired();
            base.Configure(builder);
        }
    }
}

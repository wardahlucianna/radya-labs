using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventAttachment : AuditEntity, ISchedulingEntity
    {
        public string IdEvent { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        /// <summary>
        /// File size in kb
        /// </summary>
        public decimal Filesize { get; set; }
        public virtual TrEvent Event { get; set; }
    }

    internal class TrEventAttachmentConfiguration : AuditEntityConfiguration<TrEventAttachment>
    {
        public override void Configure(EntityTypeBuilder<TrEventAttachment> builder)
        {
            builder.HasOne(x => x.Event)
            .WithMany(x => x.EventAttachments)
            .HasForeignKey(fk => fk.IdEvent)
            .HasConstraintName("FK_TrEventAttachment_TrEvent")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.Property(p => p.Url).HasMaxLength(450).IsRequired();
            builder.Property(p => p.Filename).HasMaxLength(200).IsRequired();
            builder.Property(p => p.Filetype).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Filesize)
            .HasColumnType("decimal(18,2)")
            .IsRequired(true);

            base.Configure(builder);
        }
    }
}

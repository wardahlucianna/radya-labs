using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventAttachment : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEvent { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        /// <summary>
        /// File size in kb
        /// </summary>
        public decimal Filesize { get; set; }
        public virtual HTrEvent Event { get; set; }
    }

    internal class HTrEventAttachmentConfiguration : AuditNoUniqueEntityConfiguration<HTrEventAttachment>
    {
        public override void Configure(EntityTypeBuilder<HTrEventAttachment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventAttachment).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.Event)
            .WithMany(x => x.EventAttachments)
            .HasForeignKey(fk => fk.IdEvent)
            .HasConstraintName("FK_HTrEventAttachment_HTrEvent")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.Property(p => p.Url).HasMaxLength(450).IsRequired();
            builder.Property(p => p.Filename).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Filetype).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Filesize)
            .HasColumnType("decimal(18,2)")
            .IsRequired(true);

            base.Configure(builder);
        }
    }
}

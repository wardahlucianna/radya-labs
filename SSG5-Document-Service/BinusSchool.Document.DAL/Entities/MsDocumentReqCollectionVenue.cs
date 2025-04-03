using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsDocumentReqCollectionVenue : AuditEntity, IDocumentEntity
    {
        public string IdVenue { get; set; }
        public string IdSchool { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsDocumentReqCollectionVenueConfiguration : AuditEntityConfiguration<MsDocumentReqCollectionVenue>
    {
        public override void Configure(EntityTypeBuilder<MsDocumentReqCollectionVenue> builder)
        {
            builder.Property(x => x.IdVenue)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdSchool)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Venue)
                .WithMany(x => x.DocumentReqCollectionVenues)
                .HasForeignKey(fk => fk.IdVenue)
                .HasConstraintName("FK_MsDocumentReqCollectionVenue_MsVenue")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.DocumentReqCollectionVenues)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsDocumentReqCollectionVenue_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

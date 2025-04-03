using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsDocumentReqApplicantCollection : AuditEntity, IDocumentEntity
    {
        public string IdDocumentReqApplicant { get; set; }
        public DateTime FinishDate { get; set; }
        public DateTime? ScheduleCollectionDateStart { get; set; }
        public DateTime? ScheduleCollectionDateEnd { get; set; }
        public string Remarks { get; set; }
        public string IdVenue { get; set; }
        public string CollectedBy { get; set; }
        public DateTime? CollectedDate { get; set; }
        public virtual MsDocumentReqApplicant DocumentReqApplicant { get; set; }
        public virtual MsVenue Venue { get; set; }
    }

    internal class MsDocumentReqApplicantCollectionConfiguration : AuditEntityConfiguration<MsDocumentReqApplicantCollection>
    {
        public override void Configure(EntityTypeBuilder<MsDocumentReqApplicantCollection> builder)
        {
            builder.Property(x => x.IdDocumentReqApplicant)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Remarks)
                .HasMaxLength(500);

            builder.Property(x => x.IdVenue)
                .HasMaxLength(36);

            builder.Property(x => x.CollectedBy)
                .HasMaxLength(250);

            builder.HasOne(x => x.DocumentReqApplicant)
                .WithMany(x => x.DocumentReqApplicantCollections)
                .HasForeignKey(fk => fk.IdDocumentReqApplicant)
                .HasConstraintName("FK_MsDocumentReqApplicantCollection_MsDocumentReqApplicant")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Venue)
                .WithMany(x => x.DocumentReqApplicantCollections)
                .HasForeignKey(fk => fk.IdVenue)
                .HasConstraintName("FK_MsDocumentReqApplicantCollection_MsVenue")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}

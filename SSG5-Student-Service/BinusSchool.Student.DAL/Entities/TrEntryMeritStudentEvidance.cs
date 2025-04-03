using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrEntryMeritStudentEvidance : AuditEntity, IStudentEntity
    {
        public string IdEntryMeritStudent { get; set; }
        public string OriginalName { get; set; }
        public int FileSize { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string FileType { get; set; }
        public virtual TrEntryMeritStudent EntryMeritStudent { get; set; }
    }
    internal class TrEntryMeritStudentEvidanceConfiguration : AuditEntityConfiguration<TrEntryMeritStudentEvidance>
    {
        public override void Configure(EntityTypeBuilder<TrEntryMeritStudentEvidance> builder)
        {
            builder.Property(p => p.Url).HasMaxLength(450);
            builder.Property(p => p.OriginalName).HasMaxLength(100);
            builder.Property(p => p.FileName).HasMaxLength(100);
            builder.Property(p => p.FileType).HasMaxLength(10);
            builder.Property(x => x.FileSize).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.EntryMeritStudent)
             .WithMany(x => x.EntryMeritStudentEvidances)
             .HasForeignKey(fk => fk.IdEntryMeritStudent)
             .HasConstraintName("FK_TrEntryMeritStudentEvidance_TrEntryMeritStudent")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsCounselorPhoto : AuditEntity, IStudentEntity
    {
        public string IdCounselor { get; set; }
        public string OriginalName { get; set; }
        public int FileSize { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string FileType { get; set; }

        public MsCounselor Counselor { get; set; }

    }

    internal class MsCounselorPhototConfiguration : AuditEntityConfiguration<MsCounselorPhoto>
    {
        public override void Configure(EntityTypeBuilder<MsCounselorPhoto> builder)
        {
            builder.Property(p => p.Url).HasMaxLength(450);
            builder.Property(p => p.OriginalName).HasMaxLength(100);
            builder.Property(p => p.FileName).HasMaxLength(100);
            builder.Property(p => p.FileType).HasMaxLength(10);
            builder.Property(x => x.FileSize).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.Counselor)
             .WithMany(x => x.CounselorPhoto)
             .HasForeignKey(fk => fk.IdCounselor)
             .HasConstraintName("FK_MsCounselorPhoto_MsCounselor")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}

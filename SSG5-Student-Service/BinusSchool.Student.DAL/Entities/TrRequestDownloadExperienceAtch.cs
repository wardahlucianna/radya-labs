using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Common.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrRequestDownloadExperienceAtch : AuditEntity, IStudentEntity
    {
        public string IdRequestDownloadExperience { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileType { get; set; }
        public virtual TrRequestDownloadExperience RequestDownloadExperience { get; set; }
    }

    internal class TrRequestDownloadExperienceAtchConfiguration : AuditEntityConfiguration<TrRequestDownloadExperienceAtch>
    {
        public override void Configure(EntityTypeBuilder<TrRequestDownloadExperienceAtch> builder)
        {
            builder.Property(x => x.IdRequestDownloadExperience)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.FileName)
                .HasMaxLength(256);

            builder.Property(x => x.FileUrl)
                .HasMaxLength(256);
            
            builder.Property(x => x.FileType)
                .HasMaxLength(10)
                .IsRequired();

            builder.HasOne(x => x.RequestDownloadExperience)
                .WithMany(x => x.TrRequestDownloadExperienceAtchs)
                .IsRequired()
                .HasForeignKey(fk => fk.IdRequestDownloadExperience)
                .HasConstraintName("FK_TrRequestDownloadExperienceAtch_TrRequestDownloadExperience")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}

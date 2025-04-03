using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrServiceAsActionMapping : AuditEntity, IStudentEntity
    {
        public string IdServiceAsActionEvidence { get; set; }
        public string IdMappingLearningOutcome { get; set; }
        
        public virtual TrServiceAsActionEvidence ServiceAsActionEvidence { get; set; }
        public virtual MsMappingLearningOutcome MappingLearningOutcome { get; set; }
    }

    internal class TrServiceAsActionMappingConfiguration : AuditEntityConfiguration<TrServiceAsActionMapping>
    {
        public override void Configure(EntityTypeBuilder<TrServiceAsActionMapping> builder)
        {
            builder.Property(x => x.IdServiceAsActionEvidence).IsRequired().HasMaxLength(36);
            builder.Property(x => x.IdMappingLearningOutcome).IsRequired().HasMaxLength(36);

            builder.HasOne(x => x.ServiceAsActionEvidence)
                .WithMany(x => x.LOMappings)
                .HasForeignKey(fk => fk.IdServiceAsActionEvidence)
                .HasConstraintName("FK_TrServiceAsActionMapping_TrServiceAsActionEvidence")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.MappingLearningOutcome)
                .WithMany(x => x.EvidenceMappings)
                .HasForeignKey(fk => fk.IdMappingLearningOutcome)
                .HasConstraintName("FK_TrServiceAsActionMapping_MsMappingLearningOutcome")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

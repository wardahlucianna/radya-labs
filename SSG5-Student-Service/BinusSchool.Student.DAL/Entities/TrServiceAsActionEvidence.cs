using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrServiceAsActionEvidence : AuditEntity, IStudentEntity
    {
        public string IdServiceAsActionForm { get; set; }
        public string EvidenceType { get; set; }

        public virtual TrServiceAsActionForm ServiceAsActionForm { get; set; }
        
        public virtual ICollection<TrServiceAsActionComment> Comments { get; set; }
        public virtual ICollection<TrServiceAsActionUpload> Uploads { get; set; }
        public virtual ICollection<TrServiceAsActionMapping> LOMappings { get; set; }
    }

    internal class TrExperienceEvidenceConfiguration : AuditEntityConfiguration<TrServiceAsActionEvidence>
    {
        public override void Configure(EntityTypeBuilder<TrServiceAsActionEvidence> builder)
        {
            builder.Property(x => x.IdServiceAsActionForm).IsRequired().HasMaxLength(36);

            builder.Property(x => x.EvidenceType).IsRequired().HasMaxLength(10);

            builder.HasOne(x => x.ServiceAsActionForm)
                .WithMany(x => x.ServiceAsActionEvidences)
                .HasForeignKey(fk => fk.IdServiceAsActionForm)
                .HasConstraintName("FK_TrExperienceEvidence_TrExperienceForm")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

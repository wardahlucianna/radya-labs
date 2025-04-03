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
    public class TrEvidencesComment : AuditEntity, IStudentEntity
    {
        public string IdEvidences { get; set; }
        public string IdUserComment { get; set; }
        public string Comment { get; set; }
        public virtual TrEvidences Evidences { get; set; }
        public virtual MsUser UserComment { get; set; }
    }

    internal class TrEvidencesCommentConfiguration : AuditEntityConfiguration<TrEvidencesComment>
    {
        public override void Configure(EntityTypeBuilder<TrEvidencesComment> builder)
        {
            builder.Property(x => x.IdEvidences)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdUserComment)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Comment)
                .HasMaxLength(256);

            builder.HasOne(x => x.UserComment)
                .WithMany(x => x.TrEvidencesComments)
                .IsRequired()
                .HasForeignKey(fk => fk.IdUserComment)
                .HasConstraintName("FK_TrEvidencesComment_MsUser")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Evidences)
                .WithMany(x => x.TrEvidencesComments)
                .IsRequired()
                .HasForeignKey(fk => fk.IdEvidences)
                .HasConstraintName("FK_TrEvidencesComment_TrEvidences")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}

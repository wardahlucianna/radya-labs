using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrServiceAsActionComment : AuditEntity, IStudentEntity
    {
        public string IdServiceAsActionEvidence { get; set; }
        public string IdCommentator { get; set; }
        public string CommentDesc { get; set; }

        public virtual TrServiceAsActionEvidence ExperienceEvidence { get; set; }
        public virtual MsUser Comentator { get; set; }
    }

    internal class TrCommentConfiguration : AuditEntityConfiguration<TrServiceAsActionComment>
    {
        public override void Configure(EntityTypeBuilder<TrServiceAsActionComment> builder)
        {
            builder.Property(x => x.IdServiceAsActionEvidence).IsRequired().HasMaxLength(36);

            builder.Property(x => x.IdCommentator).IsRequired().HasMaxLength(36);

            builder.Property(x => x.CommentDesc).IsRequired().HasMaxLength(500);

            builder.HasOne(x => x.ExperienceEvidence)
                .WithMany(x => x.Comments)
                .HasForeignKey(fk => fk.IdServiceAsActionEvidence)
                .HasConstraintName("FK_TrComment_TrExperienceEvidence")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Comentator)
                .WithMany(x => x.ServiceAsActionComments)
                .HasForeignKey(fk => fk.IdCommentator)
                .HasConstraintName("FK_TrComment_MsStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

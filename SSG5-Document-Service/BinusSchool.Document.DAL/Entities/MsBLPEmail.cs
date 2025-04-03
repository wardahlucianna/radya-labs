using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.User;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsBLPEmail : AuditEntity, IDocumentEntity
    {
        public string IdBLPSetting { get; set; }
        public string IdSurveyCategory { get; set; }
        public string IdRoleGroup { get; set; }
        public BLPFinalStatus BLPFinalStatus { get; set; }
        public string Description { get; set; }
        public string StatusDescription { get; set; }
        public string EmailSubject { get; set; }
        public string FilePath { get; set; }
        public bool ShowSurveyAnswer { get; set; }
        public AuditAction AuditAction { get; set; }
        public virtual LtSurveyCategory SurveyCategory { get; set; }
        public virtual MsBLPSetting BLPSetting { get; set; }
        public virtual LtRoleGroup RoleGroup { get; set; }
        public virtual ICollection<MsBLPEmailAdditionalReceiver> BLPEmailAdditionalReceivers { get; set; }
    }

    internal class MsBLPEmailConfiguration : AuditEntityConfiguration<MsBLPEmail>
    {
        public override void Configure(EntityTypeBuilder<MsBLPEmail> builder)
        {
            builder.Property(x => x.IdBLPSetting)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.IdSurveyCategory)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.IdRoleGroup)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.BLPFinalStatus)
              .IsRequired();

            builder.Property(x => x.FilePath)
              .HasMaxLength(250);

            builder.Property(x => x.EmailSubject)
              .HasMaxLength(100)
              .IsRequired();

            builder.Property(x => x.AuditAction)
              .IsRequired();

            builder.HasOne(x => x.SurveyCategory)
                .WithMany(x => x.BLPEmails)
                .HasForeignKey(fk => fk.IdSurveyCategory)
                .HasConstraintName("FK_BLPEmails_LtSurveyCategory")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.BLPSetting)
                .WithMany(x => x.BLPEmails)
                .HasForeignKey(fk => fk.IdBLPSetting)
                .HasConstraintName("FK_BLPEmails_MsBLPSetting")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.RoleGroup)
                .WithMany(x => x.BLPEmails)
                .HasForeignKey(fk => fk.IdRoleGroup)
                .HasConstraintName("FK_BLPEmails_LtRoleGroup")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}

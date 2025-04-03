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
    public class TrExperienceStatusChangeHs : AuditEntity, IStudentEntity
    {
        public string IdExperience { get; set; }
        public string IdUserApproval { get; set; }
        public DateTime ExperienceStatusChangeDate { get; set; }
        public string Note { get; set; }
        public virtual TrExperience Experience { get; set; }
        public virtual MsUser UserApproval { get; set; }
    }

    internal class TrExperienceStatusChangeHsConfiguration : AuditEntityConfiguration<TrExperienceStatusChangeHs>
    {
        public override void Configure(EntityTypeBuilder<TrExperienceStatusChangeHs> builder)
        {
            builder.Property(x => x.IdExperience)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Note)
                .HasMaxLength(255);

            builder.Property(x => x.IdUserApproval)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Experience)
                .WithMany(x => x.TrExperienceStatusChangeHs)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExperience)
                .HasConstraintName("FK_TrExperienceStatusChangeHs_TrExperience")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.UserApproval)
                .WithMany(x => x.TrExperienceStatusChangeHs)
                .IsRequired()
                .HasForeignKey(fk => fk.IdUserApproval)
                .HasConstraintName("FK_TrExperienceStatusChangeHs_MsUser")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}

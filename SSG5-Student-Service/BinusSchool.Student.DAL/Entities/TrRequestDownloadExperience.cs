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
    public class TrRequestDownloadExperience : AuditEntity, IStudentEntity
    {
        public string IdUserRequest { get; set; }
        public RequestDownloadState State { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ErrorMessage { get; set; }
        public virtual MsUser User { get; set; }
        public virtual ICollection<TrRequestDownloadExperienceAtch> TrRequestDownloadExperienceAtchs { get; set; }
    }

    internal class TrRequestDownloadExperienceConfiguration : AuditEntityConfiguration<TrRequestDownloadExperience>
    {
        public override void Configure(EntityTypeBuilder<TrRequestDownloadExperience> builder)
        {
            builder.Property(x => x.IdUserRequest)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.ErrorMessage)
                .HasColumnType("TEXT");

            builder.Property(x => x.State)
                .HasMaxLength(maxLength: 20)
                .HasConversion(valueToDb =>
                valueToDb.ToString(),
                valueFromDb => (RequestDownloadState)Enum.Parse(typeof(RequestDownloadState), valueFromDb))
                .IsRequired();


            builder.HasOne(x => x.User)
                .WithMany(x => x.TrRequestDownloadExperiences)
                .IsRequired()
                .HasForeignKey(fk => fk.IdUserRequest)
                .HasConstraintName("FK_TrRequestDownloadExperience_MsUser")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}

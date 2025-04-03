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
    public class TrCasAdvisor : AuditEntity, IStudentEntity
    {
        public string IdUserCAS { get; set; }
        public string IdAcademicYear { get; set; }
        public virtual MsUser UserCAS { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<TrCasAdvisorStudent> TrCasAdvisorStudents { get; set; }
    }

    internal class TrCasAdvisorConfiguration : AuditEntityConfiguration<TrCasAdvisor>
    {
        public override void Configure(EntityTypeBuilder<TrCasAdvisor> builder)
        {
            builder.Property(x => x.IdUserCAS)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.UserCAS)
                .WithMany(x => x.TrCasAdvisors)
                .IsRequired()
                .HasForeignKey(fk => fk.IdUserCAS)
                .HasConstraintName("FK_TrCasAdvisor_MsUser")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.TrCasAdvisors)
                .IsRequired()
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_TrCasAdvisor_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}

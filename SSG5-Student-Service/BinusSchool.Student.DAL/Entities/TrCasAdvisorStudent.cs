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
    public class TrCasAdvisorStudent : AuditEntity, IStudentEntity
    {
        public string IdCasAdvisor { get; set; }
        public string IdHomeroomStudent { get; set; }
        public virtual TrCasAdvisor CasAdvisor{ get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
    }

    internal class TrCasAdvisorStudentConfiguration : AuditEntityConfiguration<TrCasAdvisorStudent>
    {
        public override void Configure(EntityTypeBuilder<TrCasAdvisorStudent> builder)
        {
            builder.Property(x => x.IdCasAdvisor)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdHomeroomStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.CasAdvisor)
                .WithMany(x => x.TrCasAdvisorStudents)
                .IsRequired()
                .HasForeignKey(fk => fk.IdCasAdvisor)
                .HasConstraintName("FK_TrCasAdvisorStudent_TrCasAdvisor")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.HomeroomStudent)
                .WithMany(x => x.TrCasAdvisorStudents)
                .IsRequired()
                .HasForeignKey(fk => fk.IdHomeroomStudent)
                .HasConstraintName("FK_TrCasAdvisorStudent_MsHomeroomStudent")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}

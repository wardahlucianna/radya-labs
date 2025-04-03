using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsImmersionPeriod : AuditEntity, ISchedulingEntity
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string Name { get; set; }
        public DateTime RegistrationStartDate { get; set; }
        public DateTime RegistrationEndDate { get; set; }
        //public bool Status { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsImmersion> Immersions { get; set; }
    }
    internal class MsImmersionPeriodConfiguration : AuditEntityConfiguration<MsImmersionPeriod>
    {
        public override void Configure(EntityTypeBuilder<MsImmersionPeriod> builder)
        {
            builder.Property(x => x.IdAcademicYear)
             .HasMaxLength(36)
             .IsRequired();

            builder.Property(x => x.Name)
             .HasMaxLength(100)
             .IsRequired();

            builder.Property(x => x.RegistrationStartDate)
             .HasColumnType(typeName: "datetime2");

            builder.Property(x => x.RegistrationEndDate)
             .HasColumnType(typeName: "datetime2");

            builder.HasOne(x => x.AcademicYear)
                 .WithMany(y => y.MsImmersionPeriods)
                 .HasForeignKey(fk => fk.IdAcademicYear)
                 .HasConstraintName("FK_MsImmersionPeriod_MsAcademicYear")
                 .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}

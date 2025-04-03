using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsExtracurricularExternalCoach : AuditEntity, ISchedulingEntity
    {
        public string IdExternalCoach { get; set; }
        public string Name { get; set; }
        public string IdExtracurricularExtCoachTaxStatus { get; set; }
        public string NPWP { get; set; }
        public string AccountBank { get; set; }
        public string AccountBankBranch { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string IdSchool { get; set; }
        public virtual LtExtracurricularExtCoachTaxStatus ExtracurricularExtCoachTaxStatus { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsUser User { get; set; }
        public virtual ICollection<TrExtracurricularExternalCoachAtt> ExtracurricularExternalCoachAtts { get; set; }
        public virtual ICollection<MsExtracurricularExtCoachMapping> ExtracurricularExtCoachMappings { get; set; }

    }
    internal class MsExtracurricularExternalCoachConfiguration : AuditEntityConfiguration<MsExtracurricularExternalCoach>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularExternalCoach> builder)
        {
            builder.Property(p => p.IdExternalCoach)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(p => p.Name)
               .HasMaxLength(128)
               .IsRequired();

            builder.Property(p => p.IdExtracurricularExtCoachTaxStatus)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.NPWP)
                .HasMaxLength(50);              

            builder.Property(p => p.AccountBank)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(p => p.AccountBankBranch)
                .HasMaxLength(128);

            builder.Property(p => p.AccountNumber)
                 .HasMaxLength(50)
                 .IsRequired();

            builder.Property(p => p.AccountName)
                 .HasMaxLength(128)
                 .IsRequired();

            builder.Property(p => p.IdSchool)
               .HasMaxLength(36)
               .IsRequired();


            builder.HasOne(x => x.ExtracurricularExtCoachTaxStatus)
                .WithMany(x => x.ExtracurricularExternalCoachs)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExtracurricularExtCoachTaxStatus)
                .HasConstraintName("FK_MsExtracurricularExternalCoach_LtExtracurricularExtCoachTaxStatus")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.School)
              .WithMany(x => x.ExtracurricularExternalCoachs)
              .IsRequired()
              .HasForeignKey(fk => fk.IdSchool)
              .HasConstraintName("FK_MsExtracurricularExternalCoach_MsSchool")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.User)
                .WithMany(x => x.ExtracurricularExternalCoachs)
                .IsRequired()
                .HasForeignKey(fk => fk.Id)
                .HasConstraintName("FK_MsExtracurricularExternalCoach_MsUser")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);

        }
    }
}

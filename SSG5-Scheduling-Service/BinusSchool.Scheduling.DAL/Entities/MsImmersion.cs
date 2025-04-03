using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsImmersion : AuditEntity, ISchedulingEntity
    {
        public int Semester { get; set; }
        public string Destination { get; set; }
        public string IdImmersionPeriod { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdBinusianPIC { get; set; }
        public string PICEmail { get; set; }
        public string PICPhone { get; set; }
        public int MinParticipant { get; set; }
        public int MaxParticipant { get; set; }
        public string IdCurrency { get; set; }
        public string IdImmersionPaymentMethod { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal TotalCost { get; set; }
        public string PosterFileName { get; set; }
        public string BrochureFileName { get; set; }
        public virtual MsImmersionPeriod ImmersionPeriod { get; set; }
        public virtual MsStaff Staff { get; set; }
        public virtual MsCurrency Currency { get; set; }
        public virtual LtImmersionPaymentMethod ImmersionPaymentMethod { get; set; }
        public virtual ICollection<TrImmersionGradeMapping> ImmersionGradeMappings { get; set; }
    }

    internal class MsImmersionConfiguration : AuditEntityConfiguration<MsImmersion>
    {
        public override void Configure(EntityTypeBuilder<MsImmersion> builder)
        {
            builder.Property(x => x.Destination)
             .HasMaxLength(100)
             .IsRequired();

            builder.Property(x => x.IdImmersionPeriod)
             .HasMaxLength(36)
             .IsRequired();

            builder.Property(x => x.Description)
             .HasMaxLength(1000);

            builder.Property(x => x.StartDate)
             .HasColumnType(typeName: "datetime2");

            builder.Property(x => x.EndDate)
             .HasColumnType(typeName: "datetime2");

            builder.Property(x => x.IdBinusianPIC)
             .HasMaxLength(36)
             .IsRequired();

            builder.Property(x => x.PICEmail)
             .HasMaxLength(36)
             .IsRequired();

            builder.Property(x => x.PICPhone)
             .HasMaxLength(36)
             .IsRequired();

            builder.Property(x => x.IdCurrency)
             .HasMaxLength(36)
             .IsRequired();

            builder.Property(x => x.IdImmersionPaymentMethod)
             .HasMaxLength(36)
             .IsRequired();

            builder.Property(x => x.RegistrationFee)
             .HasColumnType("money");

            builder.Property(x => x.TotalCost)
             .HasColumnType("money");

            builder.Property(x => x.PosterFileName)
             .HasMaxLength(250);

            builder.Property(x => x.BrochureFileName)
             .HasMaxLength(250);

            builder.HasOne(x => x.ImmersionPeriod)
                 .WithMany(y => y.Immersions)
                 .HasForeignKey(fk => fk.IdImmersionPeriod)
                 .HasConstraintName("FK_MsImmersion_MsImmersionPeriod")
                 .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Staff)
                 .WithMany(y => y.Immersions)
                 .HasForeignKey(fk => fk.IdBinusianPIC)
                 .HasConstraintName("FK_MsImmersion_MsStaff")
                 .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Currency)
                 .WithMany(y => y.Immersions)
                 .HasForeignKey(fk => fk.IdCurrency)
                 .HasConstraintName("FK_MsImmersion_MsCurrency")
                 .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ImmersionPaymentMethod)
                 .WithMany(y => y.Immersions)
                 .HasForeignKey(fk => fk.IdImmersionPaymentMethod)
                 .HasConstraintName("FK_MsImmersion_LtImmersionPaymentMethod")
                 .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}

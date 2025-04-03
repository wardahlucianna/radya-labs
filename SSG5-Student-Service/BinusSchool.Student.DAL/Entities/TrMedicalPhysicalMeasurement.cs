using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrMedicalPhysicalMeasurement : AuditEntity, IStudentEntity
    {
        public string IdUser { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string BloodPressure { get; set; }
        public decimal BodyTemperature { get; set; }
        public int? HeartRate { get; set; }
        public int? Saturation { get; set; }
        public int? RespiratoryRate { get; set; }
        public DateTime MeasurementDate { get; set; }
        public string MeasurementPIC { get; set; }
    }

    internal class TrMedicalPhysicalMeasurementConfiguration : AuditEntityConfiguration<TrMedicalPhysicalMeasurement>
    {
        public override void Configure(EntityTypeBuilder<TrMedicalPhysicalMeasurement> builder)
        {

            builder.Property(x => x.Height)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Weight)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.BloodPressure)
                .HasMaxLength(100);

            builder.Property(x => x.BodyTemperature)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.MeasurementPIC)
                .HasMaxLength(100)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

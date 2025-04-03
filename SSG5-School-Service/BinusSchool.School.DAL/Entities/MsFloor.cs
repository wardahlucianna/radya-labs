using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsFloor : AuditEntity, ISchoolEntity
    {
        public string IdBuilding { get; set; }
        public string FloorName { get; set; }
        public bool HasLocker { get; set; }
        public string LockerTowerCodeName { get; set; }
        public string Description { get; set; }
        public string? URL { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public decimal? FileSize { get; set; }
        public bool? IsShowFloorLayout { get; set; }
        public virtual MsBuilding Building { get; set; }
        public virtual ICollection<MsVenueMapping> VenueMappings { get; set; }
        public virtual ICollection<MsLocker> Lockers { get; set; }
    }

    internal class MsFloorConfiguration : AuditEntityConfiguration<MsFloor>
    {
        public override void Configure(EntityTypeBuilder<MsFloor> builder)
        {
            builder.HasOne(x => x.Building)
                .WithMany(x => x.Floors)
                .HasForeignKey(fk => fk.IdBuilding)
                .HasConstraintName("FK_MsFloor_MsBuilding")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(x => x.FloorName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.LockerTowerCodeName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FileSize)
                .HasColumnType("decimal(18,2)");

            base.Configure(builder);
        }
    }
}

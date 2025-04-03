using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsFloor : AuditEntity, IStudentEntity
    {
        public string IdBuilding { get; set; }
        public string FloorName { get; set; }
        public bool HasLocker { get; set; }
        public string LockerTowerCodeName { get; set; }
        public virtual MsBuilding Building { get; set; }
        public virtual ICollection<MsLockerAllocation> LockerAllocations { get; set; }
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
                .HasMaxLength(50);

            builder.Property(x => x.LockerTowerCodeName)
                .HasMaxLength(50);

            base.Configure(builder);
        }
    }
}

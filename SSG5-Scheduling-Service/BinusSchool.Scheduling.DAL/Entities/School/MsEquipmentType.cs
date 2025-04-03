using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsEquipmentType : AuditEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public string EquipmentTypeName { get; set; }
        public string IdReservationOwner { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsReservationOwner ReservationOwner { get; set; }
        public virtual ICollection<MsEquipment> Equipments { get; set; }
    }

    internal class MsEquipmentTypeConfiguration : AuditEntityConfiguration<MsEquipmentType>
    {
        public override void Configure(EntityTypeBuilder<MsEquipmentType> builder)
        {
            builder.Property(x => x.EquipmentTypeName)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.EquipmentTypes)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSchool_MsEquipmentType")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.ReservationOwner)
                .WithMany(x => x.EquipmentTypes)
                .HasForeignKey(fk => fk.IdReservationOwner)
                .HasConstraintName("FK_MsReservationOwner_MsEquipmentType")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsReservationOwner : AuditEntity, ISchoolEntity
    {
        public string IdSchool { get; set; }
        public string OwnerName { get; set; }
        public bool IsPICVenue { get; set; }
        public bool IsPICEquipment { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsReservationOwnerEmail> ReservationOwnerEmails { get; set; }
        public virtual ICollection<MsVenueMapping> VenueMappings { get; set; }
        public virtual ICollection<HMsReservationOwner> ReservationOwners { get; set; }
        public virtual ICollection<MsEquipmentType> EquipmentTypes { get; set; }

    }

    internal class MsReservationOwnerConfiguration : AuditEntityConfiguration<MsReservationOwner>
    {
        public override void Configure(EntityTypeBuilder<MsReservationOwner> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.ReservationOwners)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSchool_MsReservationOwner")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }

}

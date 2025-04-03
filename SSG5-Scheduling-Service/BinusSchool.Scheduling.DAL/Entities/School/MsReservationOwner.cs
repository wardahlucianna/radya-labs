using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsReservationOwner : AuditEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public string OwnerName { get; set; }
        public bool IsPICVenue { get; set; }
        public bool IsPICEquipment { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsVenueMapping> VenueMappings { get; set; }
        public virtual ICollection<MsReservationOwnerEmail> ReservationOwnerEmails { get; set; }
        public virtual ICollection<MsEquipmentType> EquipmentTypes { get; set; }

    }

    internal class MsReservationOwnerConfiguration : AuditEntityConfiguration<MsReservationOwner>
    {
        public override void Configure(EntityTypeBuilder<MsReservationOwner> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.ReservationOwners)
                .HasForeignKey(fk => fk.IdSchool)
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}

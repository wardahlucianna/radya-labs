using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsReservationOwnerEmail : AuditEntity, ISchedulingEntity
    {
        public string IdReservationOwner { get; set; }
        public string OwnerEmail { get; set; }
        public bool IsOwnerEmailTo { get; set; }
        public bool IsOwnerEmailCC { get; set; }
        public bool IsOwnerEmailBCC { get; set; }
        public virtual MsReservationOwner ReservationOwner { get; set; }
    }

    internal class MsReservationOwnerEmailConfiguration : AuditEntityConfiguration<MsReservationOwnerEmail>
    {
        public override void Configure(EntityTypeBuilder<MsReservationOwnerEmail> builder)
        {
            builder.HasOne(x => x.ReservationOwner)
                .WithMany(x => x.ReservationOwnerEmails)
                .HasForeignKey(fk => fk.IdReservationOwner)
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}

using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsReservationOwnerEmail : AuditEntity, ISchoolEntity
    {
        public string IdReservationOwner { get; set; }
        public string OwnerEmail { get; set; }
        public bool IsOwnerEmailTo { get; set; }
        public bool IsOwnerEmailCC { get; set; }
        public bool IsOwnerEmailBCC { get; set; }
        public virtual MsReservationOwner ReservationOwner { get; set; }
        public virtual ICollection<HMsReservationOwnerEmail> ReservationOwnerEmails { get; set; }
    }

    internal class MsReservationOwnerEmailConfiguration : AuditEntityConfiguration<MsReservationOwnerEmail>
    {
        public override void Configure(EntityTypeBuilder<MsReservationOwnerEmail> builder)
        {
            builder.HasOne(x => x.ReservationOwner)
                .WithMany(x => x.ReservationOwnerEmails)
                .HasForeignKey(fk => fk.IdReservationOwner)
                .HasConstraintName("FK_MsReservationOwner_MsReservationOwnerEmail")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class HMsReservationOwner : AuditNoUniqueEntity, ISchoolEntity
    {
        public string IdHMsReservationOwner { get; set; }
        public string IdReservationOwner { get; set; }
        public string OwnerName { get; set; }
        public string IdSchool { get; set; }
        public bool IsPICVenue { get; set; }
        public bool IsPICEquipment { get; set; }
        public virtual MsReservationOwner ReservationOwner { get; set; }
    }

    internal class HMsReservationOwnerConfiguration : AuditNoUniqueEntityConfiguration<HMsReservationOwner>
    {
        public override void Configure(EntityTypeBuilder<HMsReservationOwner> builder)
        {
            builder.HasKey(x => x.IdHMsReservationOwner);

            builder.Property(x => x.IdHMsReservationOwner)
               .HasMaxLength(36)
               .IsRequired();


            builder.HasOne(x => x.ReservationOwner)
                .WithMany(x => x.ReservationOwners)
                .HasForeignKey(fk => fk.IdReservationOwner)
                .HasConstraintName("FK_MsReservationOwner_HMsReservationOwner")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

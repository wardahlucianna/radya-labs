using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class HMsReservationOwnerEmail : AuditNoUniqueEntity, ISchoolEntity
    {
        public string IdHMsReservationOwnerEmail { get; set; }
        public string IdReservationOwnerEmail { get; set; }
        public string IdReservationOwner {  get; set; }
        public string OwnerEmail { get; set; }
        public bool IsOwnerEmailTo { get; set; }
        public bool IsOwnerEmailCC { get; set; }
        public bool IsOwnerEmailBCC { get; set; }
        public virtual MsReservationOwnerEmail ReservationOwnerEmail { get; set; }
    }

    internal class HMsReservationOwnerEmailConfiguration : AuditNoUniqueEntityConfiguration<HMsReservationOwnerEmail>
    {

        public override void Configure(EntityTypeBuilder<HMsReservationOwnerEmail> builder)
        {
            builder.HasKey(x => x.IdHMsReservationOwnerEmail);

            builder.Property(x => x.IdHMsReservationOwnerEmail)
               .HasMaxLength(36)
               .IsRequired();

            builder.HasOne(x => x.ReservationOwnerEmail)
                .WithMany(x => x.ReservationOwnerEmails)
                .HasForeignKey(fk => fk.IdReservationOwnerEmail)
                .HasConstraintName("FK_MsReservationOwnerEmail_HMsReservationOwnerEmail")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }

    }
}

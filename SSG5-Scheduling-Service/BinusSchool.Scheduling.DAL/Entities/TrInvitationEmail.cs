using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrInvitationEmail : AuditEntity, ISchedulingEntity
    {
        public string IdHomeroomStudent { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public DateTime? LastSendEmailInvitation { get; set; }
        public InvitationBookingInitiateBy InitiateBy { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual TrInvitationBookingSetting InvitationBookingSetting { get; set; }
    }

    internal class TrInvitationEmailConfiguration : AuditEntityConfiguration<TrInvitationEmail>
    {
        public override void Configure(EntityTypeBuilder<TrInvitationEmail> builder)
        {
            builder.HasOne(x => x.HomeroomStudent)
                .WithMany(x => x.InvitationEmails)
                .HasForeignKey(fk => fk.IdHomeroomStudent)
                .HasConstraintName("FK_TrInvitationEmail_MsHomeroomStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.HomeroomStudent)
                .WithMany(x => x.InvitationEmails)
                .HasForeignKey(fk => fk.IdHomeroomStudent)
                .HasConstraintName("FK_TrInvitationEmail_MsHomeroomStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.InvitationBookingSetting)
                 .WithMany(x => x.InvitationEmails)
                 .HasForeignKey(fk => fk.IdInvitationBookingSetting)
                 .HasConstraintName("FK_TrInvitationEmail_TrInvitationBookingSetting")
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired();

            builder.Property(e => e.InitiateBy).HasMaxLength(maxLength: 10)
              .HasConversion(valueToDb =>
              valueToDb.ToString(),
              valueFromDb => (InvitationBookingInitiateBy)Enum.Parse(typeof(InvitationBookingInitiateBy), valueFromDb));

            base.Configure(builder);
        }
    }
}

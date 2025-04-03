using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.User
{
    public class LtRole : CodeEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public string IdRoleGroup { get; set; }
        public virtual ICollection<MsUserRole> UserRoles { get; set; }
        public virtual LtRoleGroup RoleGroup { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsSettingEmailScheduleRealization> SettingEmailScheduleRealizations{ get; set; }
        public virtual ICollection<TrInvitationBookingSettingVenueDtl> InvitationBookingSettingVenueDtl { get; set; }
        public virtual ICollection<TrInvBookingSettingRoleParticipant> InvBookingSettingRoleParticipants { get; set; }
        public virtual ICollection<MsEmailRecepient> EmailRecepients { get; set; }
        public virtual ICollection<MsSpecialRoleVenue> SpecialRoleVenues { get; set; }
        public virtual ICollection<TrRolePosition> RolePositions { get; set; }
    }

    internal class LtRoleConfiguration : CodeEntityConfiguration<LtRole>
    {
        public override void Configure(EntityTypeBuilder<LtRole> builder)
        {
            builder.HasOne(x => x.RoleGroup)
               .WithMany(x => x.Roles)
               .HasForeignKey(fk => fk.IdRoleGroup)
               .HasConstraintName("FK_LtRole_LtRoleGroup")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();
            builder.HasOne(x => x.School)
               .WithMany(x => x.Roles)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_LtRole_MsSchool")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();
            base.Configure(builder);
        }
    }
}

using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsHomeroomOfficer : AuditEntity, ISchedulingEntity
    {
        public string IdUserHomeroomCaptain { get; set; }
        public bool? CaptainCanAssignClassDiary { get; set; }
        public string IdUserHomeroomViceCaptain { get; set; }
        public bool? ViceCaptainCanAssignClassDiary { get; set; }
        public string IdUserHomeroomSecretary { get; set; }
        public bool? SecretaryCanAssignClassDiary { get; set; }
        public string IdUserHomeroomTreasurer { get; set; }
        public bool? TreasurerCanAssignClassDiary { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsUser HomeroomCaptain { get; set; }
        public virtual MsUser HomeroomViceCaptain { get; set; }
        public virtual MsUser HomeroomSecretary { get; set; }
        public virtual MsUser HomeroomTreasurer { get; set; }
    }

    internal class MsHomeroomOfficerConfiguration : AuditEntityConfiguration<MsHomeroomOfficer>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroomOfficer> builder)
        {
            builder.HasOne(x => x.HomeroomCaptain)
              .WithMany(x => x.HomeroomCaptains)
              .HasForeignKey(fk => fk.IdUserHomeroomCaptain)
              .HasConstraintName("FK_MsHomeroomOffice_MsUser_IdHomeroomCaptain")
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.HomeroomViceCaptain)
              .WithMany(x => x.HomeroomViceCaptains)
              .HasForeignKey(fk => fk.IdUserHomeroomViceCaptain)
              .HasConstraintName("FK_MsHomeroomOffice_MsUser_IdHomeroomViceCaptain")
              .OnDelete(DeleteBehavior.Restrict);

           builder.HasOne(x => x.HomeroomSecretary)
              .WithMany(x => x.HomeroomSecretaries)
              .HasForeignKey(fk => fk.IdUserHomeroomSecretary)
              .HasConstraintName("FK_MsHomeroomOffice_MsUser_IdUserHomeroomSecretary")
              .OnDelete(DeleteBehavior.Restrict);

           builder.HasOne(x => x.HomeroomTreasurer)
              .WithMany(x => x.HomeroomTreasurers)
              .HasForeignKey(fk => fk.IdUserHomeroomTreasurer)
              .HasConstraintName("FK_MsHomeroomOffice_MsUser_IdUserHomeroomTreasurer")
              .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}

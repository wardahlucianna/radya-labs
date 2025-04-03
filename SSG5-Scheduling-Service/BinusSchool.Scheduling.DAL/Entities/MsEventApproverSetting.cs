using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsEventApproverSetting : AuditEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public string IdApprover1 { get; set; }
        public string IdApprover2 { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsUser Approver1 { get; set; }
        public virtual MsUser Approver2 { get; set; }
    }

    internal class MsEventApproverSettingConfiguration : AuditEntityConfiguration<MsEventApproverSetting>
    {
        public override void Configure(EntityTypeBuilder<MsEventApproverSetting> builder)
        {
            builder.HasOne(x => x.School)
              .WithMany(x => x.EventApproverSettings)
              .HasForeignKey(fk => fk.IdSchool)
              .HasConstraintName("FK_MsEventApproverSetting_MsSchool")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.Approver1)
             .WithMany(x => x.Approver1)
             .HasForeignKey(fk => fk.IdApprover1)
             .HasConstraintName("FK_MsEventApproverSetting_IdApprover1_MsUser")
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Approver2)
            .WithMany(x => x.Approver2)
            .HasForeignKey(fk => fk.IdApprover2)
            .HasConstraintName("FK_MsEventApproverSetting_IdApprover2_MsUser")
            .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}

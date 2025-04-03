using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventApprover : AuditEntity, ISchedulingEntity
    {
        public string IdEvent { get; set; }
        public string IdUser { get; set; }
        public int OrderNumber { get; set; }
        public virtual TrEvent Event { get; set; }
        public virtual MsUser User { get; set; }
    }
    internal class TrEventApproverConfiguration : AuditEntityConfiguration<TrEventApprover>
    {
        public override void Configure(EntityTypeBuilder<TrEventApprover> builder)
        {
            builder.HasOne(x => x.Event)
              .WithMany(x => x.EventApprovers)
              .HasForeignKey(fk => fk.IdEvent)
              .HasConstraintName("FK_TrEventApprover_TrEvent")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.User)
             .WithMany(x => x.EventApprovers)
             .HasForeignKey(fk => fk.IdUser)
             .HasConstraintName("FK_TrEventApprover_Msuser")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}

using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventApproval : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEvent { get; set; }
        ///<summary>Value for section <br/>
        /// 1. Event <br/>
        /// 2. Award
        /// </summary>
        public string Section { get; set; }
        public int State { get; set; }
        public bool IsApproved { get; set; }
        public string Reason { get; set; }
        public string IdUser { get; set; }
        public virtual TrEvent Event { get; set; }
        public virtual MsUser User { get; set; }
    }
    internal class HTrEventApprovalConfiguration : AuditNoUniqueEntityConfiguration<HTrEventApproval>
    {
        public override void Configure(EntityTypeBuilder<HTrEventApproval> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventApproval).Name)
                .HasMaxLength(36);

            builder.Property(x => x.Section).HasMaxLength(17).IsRequired();

            builder.Property(x => x.Reason).HasMaxLength(450);

            builder.HasOne(x => x.Event)
             .WithMany(x => x.EventApprovals)
             .HasForeignKey(fk => fk.IdEvent)
             .HasConstraintName("FK_HTrEventApproval_TrEvent")
             .OnDelete(DeleteBehavior.Cascade)
             .IsRequired();

            builder.HasOne(x => x.User)
             .WithMany(x => x.EventApprovals)
             .HasForeignKey(fk => fk.IdUser)
             .HasConstraintName("FK_HTrEventApproval_MsUser")
             .OnDelete(DeleteBehavior.Cascade)
             .IsRequired();
            base.Configure(builder);
        }
    }
}

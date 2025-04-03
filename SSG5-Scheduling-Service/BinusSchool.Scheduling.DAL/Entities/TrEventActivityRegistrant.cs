using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventActivityRegistrant : AuditEntity, ISchedulingEntity
    {
        public string IdEventActivity { get; set; }
        public string IdUser { get; set; }
        public virtual TrEventActivity EventActivity { get; set; }
        public virtual MsUser User { get; set; }
    }
    internal class TrEventActivityRegistrantConfiguration : AuditEntityConfiguration<TrEventActivityRegistrant>
    {
        public override void Configure(EntityTypeBuilder<TrEventActivityRegistrant> builder)
        {
            builder.HasOne(x => x.EventActivity)
              .WithMany(x => x.EventActivityRegistrants)
              .HasForeignKey(fk => fk.IdEventActivity)
              .HasConstraintName("FK_TrEventActivityRegistrant_TrEventActivity")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.User)
              .WithMany(x => x.EventActivityRegistrants)
              .HasForeignKey(fk => fk.IdUser)
              .HasConstraintName("FK_TrEventActivityRegistrant_MsUser")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            base.Configure(builder);
        }
    }
}

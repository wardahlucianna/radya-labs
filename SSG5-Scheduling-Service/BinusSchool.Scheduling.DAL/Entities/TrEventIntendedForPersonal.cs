using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventIntendedForPersonal : AuditEntity, ISchedulingEntity
    {
        public string IdUser { get; set; }
        public string IdEventIntendedFor { get; set; }
        public virtual MsUser User { get; set; }
        public virtual TrEventIntendedFor EventIntendedFor { get; set; }
    }

    internal class TrEventIntendedForPersonalConfiguration : AuditEntityConfiguration<TrEventIntendedForPersonal>
    {
        public override void Configure(EntityTypeBuilder<TrEventIntendedForPersonal> builder)
        {
            builder.HasOne(x => x.EventIntendedFor)
            .WithMany(x => x.EventIntendedForPersonals)
            .HasForeignKey(fk => fk.IdEventIntendedFor)
            .HasConstraintName("FK_TrEventIntendedForPersonal_TrEventIntendedFor")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.User)
            .WithMany(x => x.EventIntendedForPersonals)
            .HasForeignKey(fk => fk.IdUser)
            .HasConstraintName("FK_TrEventIntendedForPersonal_MsUser")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            base.Configure(builder);
        }
    }
}

using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventIntendedForPosition : AuditEntity, ISchedulingEntity
    {
        public string IdEventIntendedFor { get; set; }
        public string IdTeacherPosition { get; set; }
        public virtual TrEventIntendedFor EventIntendedFor { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }

    }

    internal class TrEventIntendedForPositionConfiguration : AuditEntityConfiguration<TrEventIntendedForPosition>
    {
        public override void Configure(EntityTypeBuilder<TrEventIntendedForPosition> builder)
        {
            builder.HasOne(x => x.EventIntendedFor)
                .WithMany(x => x.EventIntendedForPositions)
                .HasForeignKey(fk => fk.IdEventIntendedFor)
                .HasConstraintName("FK_TrEventIntendedForPosition_TrEventIntendedFor")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();


            builder.HasOne(x => x.TeacherPosition)
               .WithMany(x => x.EventIntendedForPositions)
               .HasForeignKey(fk => fk.IdTeacherPosition)
               .HasConstraintName("FK_TrEventIntendedForPosition_MsTeacherPosition")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}

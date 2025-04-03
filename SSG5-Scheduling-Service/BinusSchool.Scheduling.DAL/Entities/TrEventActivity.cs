using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventActivity : AuditEntity, ISchedulingEntity
    {
        public string IdEvent { get; set; }
        public string IdActivity { get; set; }
        public virtual MsActivity Activity { get; set; }
        public virtual TrEvent Event { get; set; }
        public virtual ICollection<TrEventActivityPIC> EventActivityPICs { get; set; }
        public virtual ICollection<TrEventActivityRegistrant> EventActivityRegistrants { get; set; }
        public virtual ICollection<TrEventActivityAward> EventActivityAwards { get; set; }
        public virtual ICollection<TrEventActivityAwardTeacher> EventActivityAwardTeachers { get; set; }
    }
    internal class TrEventActivityConfiguration : AuditEntityConfiguration<TrEventActivity>
    {
        public override void Configure(EntityTypeBuilder<TrEventActivity> builder)
        {
            builder.HasOne(x => x.Event)
              .WithMany(x => x.EventActivities)
              .HasForeignKey(fk => fk.IdEvent)
              .HasConstraintName("FK_TrEventActivity_TrEvent")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.Activity)
              .WithMany(x => x.EventActivities)
              .HasForeignKey(fk => fk.IdActivity)
              .HasConstraintName("FK_TrEventActivity_TrActivity")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            base.Configure(builder);
        }
    }
}

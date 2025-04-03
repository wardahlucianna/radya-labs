using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsActivity : CodeEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<TrEventActivity> EventActivities { get; set; }
        public virtual ICollection<HTrEventActivity> HistoryEventActivities { get; set; }
    }

    internal class MsActivityConfiguration : CodeEntityConfiguration<MsActivity>
    {
        public override void Configure(EntityTypeBuilder<MsActivity> builder)
        {
            builder.HasOne(x => x.School)
               .WithMany(x => x.Activities)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_MsSchool_MsActivity")
               .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}

using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsSessionSet : CodeEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsSession> Sessions { get; set; }
        public virtual ICollection<TrAscTimetable> AscTimetables { get; set; }
    }

    internal class MsSessionSetConfiguration : CodeEntityConfiguration<MsSessionSet>
    {
        public override void Configure(EntityTypeBuilder<MsSessionSet> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.SessionSets)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSessionSet_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}

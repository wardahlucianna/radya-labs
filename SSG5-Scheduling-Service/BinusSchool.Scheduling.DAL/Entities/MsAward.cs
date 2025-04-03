using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsAward :  CodeEntity, ISchedulingEntity
    {
        public bool IsSetRecommendation { get; set; }
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<TrEventActivityAward> EventActivityAwards { get; set; }
        public virtual ICollection<TrEventActivityAwardTeacher> EventActivityAwardTeachers { get; set; }
        public virtual ICollection<HTrEventActivityAward> HistoryEventActivityAwards { get; set; }
        public virtual ICollection<HTrEventActivityAwardTeacher> HistoryEventActivityAwardTeacher { get; set; }
    }

    internal class MsAwardConfiguration : CodeEntityConfiguration<MsAward>
    {
        public override void Configure(EntityTypeBuilder<MsAward> builder)
        {
            builder.HasOne(x => x.School)
               .WithMany(x => x.Awards)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_MsAward_MsSchool")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}

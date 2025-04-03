using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsSubjectGroup : CodeEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsSubject> Subjects { get; set; }
    }

    internal class MsSubjectGroupConfiguration : CodeEntityConfiguration<MsSubjectGroup>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectGroup> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.SubjectGroups)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSubjectGroup_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();


            base.Configure(builder);
        }
    }
}

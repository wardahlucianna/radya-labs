using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsClassDiaryTypeSetting : AuditEntity, ISchedulingEntity
    {
        public string IdAcademicyear { get; set; }
        public string TypeName { get; set; }
        public int OccurrencePerDay { get; set; }
        public int MinimumStartDay { get; set; }
        public bool AllowStudentEntryClassDiary { get; set; }
        public virtual MsAcademicYear Academicyear { get; set; }
        public ICollection<TrClassDiary> ClassDiaries { get; set; }
        public ICollection<HTrClassDiary> HistoryClassDiaries { get; set; }
        public ICollection<MsClassDiaryLessonExclude> ClassDiaryLessonExcludes { get; set; }
    }

    internal class MsClassDiaryTypeSettingConfiguration : AuditEntityConfiguration<MsClassDiaryTypeSetting>
    {
        public override void Configure(EntityTypeBuilder<MsClassDiaryTypeSetting> builder)
        {
            builder.Property(x => x.OccurrencePerDay).HasDefaultValue(1);

            builder.Property(x => x.OccurrencePerDay).HasDefaultValue(1);

            builder.Property(x => x.TypeName).IsRequired().HasMaxLength(50);

            builder.HasOne(x => x.Academicyear)
                .WithMany(x => x.ClassDiaryTypeSettings)
                .HasForeignKey(fk => fk.IdAcademicyear)
                .HasConstraintName("FK_MsClassDiaryTypeSetting_MsAcademicyear")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

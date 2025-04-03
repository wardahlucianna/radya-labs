using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsClassDiaryLessonExclude : AuditEntity, ISchedulingEntity
    {
        public string IdClassDiaryTypeSetting { get; set; }
        public string IdLesson { get; set; }
        public virtual MsClassDiaryTypeSetting ClassDiaryTypeSetting { get; set; }
        public virtual MsLesson Lesson { get; set; }
    }

    internal class MsClassDiaryLessonExcludeConfiguration : AuditEntityConfiguration<MsClassDiaryLessonExclude>
    {
        public override void Configure(EntityTypeBuilder<MsClassDiaryLessonExclude> builder)
        {

            builder.HasOne(x => x.ClassDiaryTypeSetting)
              .WithMany(x => x.ClassDiaryLessonExcludes)
              .HasForeignKey(fk => fk.IdClassDiaryTypeSetting)
              .HasConstraintName("FK_MsClassDiaryExclude_MsClassDiaryTypeSetting")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.Lesson)
            .WithMany(x => x.ClassDiaryLessonExcludes)
            .HasForeignKey(fk => fk.IdLesson)
            .HasConstraintName("FK_MsClassDiaryExclude_MsLesson")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            base.Configure(builder);
        }
    }
}

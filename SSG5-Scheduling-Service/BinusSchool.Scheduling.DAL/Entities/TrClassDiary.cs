using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrClassDiary : AuditEntity, ISchedulingEntity
    {
        public string IdClassDiaryTypeSetting { get; set; }
        /// <summary>
        /// Idhomeroom get from MsHomeroomTeacher
        /// </summary>
        public string IdHomeroom { get; set; }
        public string IdLesson { get; set; }
        public DateTime ClassDiaryDate { get; set; }
        public string ClassDiaryTopic { get; set; }
        public string ClassDiaryDescription { get; set; }
        public string Status { get; set; }
        public string ClassDiaryUserCreate { get; set; }
        public virtual MsClassDiaryTypeSetting ClassDiaryTypeSetting { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsLesson Lesson { get; set; }
        public virtual MsUser User { get; set; }
        public virtual ICollection<TrClassDiaryAttachment> ClassDiaryAttachments { get; set; }
        public virtual ICollection<HTrClassDiary> HistoryClassDiaries { get; set; }
    }

    internal class MsClassDiaryConfiguration : AuditEntityConfiguration<TrClassDiary>
    {
        public override void Configure(EntityTypeBuilder<TrClassDiary> builder)
        {
            builder.HasOne(x => x.Lesson)
               .WithMany(x => x.ClassDiaries)
               .HasForeignKey(fk => fk.IdLesson)
               .HasConstraintName("FK_TrClassDiary_MsLesson")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.ClassDiaryTypeSetting)
                .WithMany(x => x.ClassDiaries)
                .HasForeignKey(fk => fk.IdClassDiaryTypeSetting)
                .HasConstraintName("FK_TrClassDiary_MsClassDiaryTypeSetting")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.ClassDiaries)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_TrClassDiary_MsHomeroom")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.User)
               .WithMany(x => x.ClassDiaries)
               .HasForeignKey(fk => fk.ClassDiaryUserCreate)
               .HasConstraintName("FK_TrClassDiary_MsUser")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.Property(p => p.ClassDiaryTopic).HasMaxLength(350);
            builder.Property(p => p.ClassDiaryDescription).HasMaxLength(450);
            builder.Property(p => p.ClassDiaryUserCreate).HasMaxLength(36).IsRequired();

            builder.Property(p => p.Status).HasMaxLength(30);

            base.Configure(builder);
        }
    }
}

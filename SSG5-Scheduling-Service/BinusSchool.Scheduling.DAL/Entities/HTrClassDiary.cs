using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrClassDiary : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string IdHTrClassDiary { get; set; }
        public string IdTrClassDiary { get; set; }
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
        public string DeleteReason { get; set; }
        public string Note { get; set; }
        public virtual MsClassDiaryTypeSetting ClassDiaryTypeSetting { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsLesson Lesson { get; set; }
        public virtual TrClassDiary ClassDiary { get; set; }
        public virtual ICollection<HTrClassDiaryAttachment> HistoryClassDiaryAttachments { get; set; }
    }

    internal class HTrClassDiaryConfiguration : AuditNoUniqueEntityConfiguration<HTrClassDiary>
    {
        public override void Configure(EntityTypeBuilder<HTrClassDiary> builder)
        {
            builder.HasKey(x => x.IdHTrClassDiary);

            builder.HasOne(x => x.Lesson)
               .WithMany(x => x.HistoryClassDiaries)
               .HasForeignKey(fk => fk.IdLesson)
               .HasConstraintName("FK_HTrClassDiary_MsLesson")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.ClassDiaryTypeSetting)
               .WithMany(x => x.HistoryClassDiaries)
               .HasForeignKey(fk => fk.IdClassDiaryTypeSetting)
               .HasConstraintName("FK_HTrClassDiary_MsClassDiaryTypeSetting")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.ClassDiaryTypeSetting)
                .WithMany(x => x.HistoryClassDiaries)
                .HasForeignKey(fk => fk.IdClassDiaryTypeSetting)
                .HasConstraintName("FK_HTrClassDiary_MsClassDiaryTypeSetting")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.HistoryClassDiaries)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_HTrClassDiary_MsHomeroom")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.ClassDiary)
                .WithMany(x => x.HistoryClassDiaries)
                .HasForeignKey(fk => fk.IdTrClassDiary)
                .HasConstraintName("FK_HTrClassDiary_TrClassDiary")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.Property(p => p.ClassDiaryTopic).HasMaxLength(50);
            builder.Property(p => p.IdHTrClassDiary).HasMaxLength(36).IsRequired();
            builder.Property(p => p.ClassDiaryDescription).HasMaxLength(450);

            builder.Property(p => p.Status).HasMaxLength(30); 
            builder.Property(p => p.DeleteReason).HasMaxLength(450); 
            builder.Property(p => p.Note).HasMaxLength(100); 

            base.Configure(builder);
        }
    }
}

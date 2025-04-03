using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventActivityAwardTeacher : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEventActivity { get; set; }
        public string IdAward { get; set; }
        public string IdStaff { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public decimal Filesize { get; set; }
        public string OriginalFilename { get; set; }
        public virtual HTrEventActivity EventActivity { get; set; }
        public virtual MsAward Award { get; set; }
        public virtual MsStaff Staff { get; set; }
    }
    internal class HTrEventActivityAwardTeacherConfiguration : AuditNoUniqueEntityConfiguration<HTrEventActivityAwardTeacher>
    {
        public override void Configure(EntityTypeBuilder<HTrEventActivityAwardTeacher> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventActivityAwardTeacher).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.EventActivity)
            .WithMany(x => x.EventActivityAwardTeachers)
            .HasForeignKey(fk => fk.IdEventActivity)
            .HasConstraintName("FK_HTrEventActivityAwardTeacher_HTrEventActivity")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.Award)
            .WithMany(x => x.HistoryEventActivityAwardTeacher)
            .HasForeignKey(fk => fk.IdAward)
            .HasConstraintName("FK_HTrEventActivityAwardTeacher_MsAward")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.Staff)
            .WithMany(x => x.HistoryEventActivityAwardTeachers)
            .HasForeignKey(fk => fk.IdStaff)
            .HasConstraintName("FK_HTrEventActivityAwardTeacher_MsStaff")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.Property(p => p.Url).HasMaxLength(450);
            builder.Property(p => p.Filename).HasMaxLength(100);
            builder.Property(p => p.Filetype).HasMaxLength(10);
            builder.Property(x => x.Filesize)
            .HasColumnType("decimal(18,2)");

            base.Configure(builder);
        }
    }
}

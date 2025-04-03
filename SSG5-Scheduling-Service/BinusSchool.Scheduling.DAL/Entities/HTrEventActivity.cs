using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventActivity : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEvent { get; set; }
        public string IdActivity { get; set; }
        public virtual MsActivity Activity { get; set; }
        public virtual HTrEvent Event { get; set; }
        public virtual ICollection<HTrEventActivityPIC> EventActivityPICs { get; set; }
        public virtual ICollection<HTrEventActivityRegistrant> EventActivityRegistrants { get; set; }
        public virtual ICollection<HTrEventActivityAward> EventActivityAwards { get; set; }
        public virtual ICollection<HTrEventActivityAwardTeacher> EventActivityAwardTeachers { get; set; }
    }
    internal class HTrEventActivityConfiguration : AuditNoUniqueEntityConfiguration<HTrEventActivity>
    {
        public override void Configure(EntityTypeBuilder<HTrEventActivity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventActivity).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.Event)
              .WithMany(x => x.EventActivities)
              .HasForeignKey(fk => fk.IdEvent)
              .HasConstraintName("FK_HTrEventActivity_HTrEvent")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.Activity)
              .WithMany(x => x.HistoryEventActivities)
              .HasForeignKey(fk => fk.IdActivity)
              .HasConstraintName("FK_HTrEventActivity_MsActivity")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            base.Configure(builder);
        }
    }
}

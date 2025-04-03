using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrVisitorSchool : AuditEntity, ISchedulingEntity
    {
        public string VisitorName { get; set; }
        public string BookName { get; set; }
        public DateTime VisitorDate { get; set; }
        public string IdVenue { get; set; }
        public string IdUserVisitor { get; set; }
        public string IdUserBook { get; set; }
        public string IdAcademicYear { get; set; }
        public string Description { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual MsUser UserVisitor { get; set; }
        public virtual MsUser UserBook { get; set; }
    }

    internal class TrVisitorSchoolConfiguration : AuditEntityConfiguration<TrVisitorSchool>
    {
        public override void Configure(EntityTypeBuilder<TrVisitorSchool> builder)
        {
            builder.Property(x => x.VisitorName).HasMaxLength(100);
            builder.Property(x => x.BookName).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(450);

            builder.HasOne(x => x.Venue)
               .WithMany(x => x.VisitorSchools)
               .HasForeignKey(fk => fk.IdVenue)
               .HasConstraintName("FK_TrVisitorSchool_MsVenue")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.UserVisitor)
               .WithMany(x => x.VisitorSchoolsVisitor)
               .HasForeignKey(fk => fk.IdUserVisitor)
               .HasConstraintName("FK_TrVisitorSchool_MsUserVisitor")
               .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.UserBook)
               .WithMany(x => x.VisitorSchoolsBook)
               .HasForeignKey(fk => fk.IdUserBook)
               .HasConstraintName("FK_TrVisitorSchool_MsUserBook")
               .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.AcademicYear)
               .WithMany(x => x.VisitorSchoolsBook)
               .HasForeignKey(fk => fk.IdAcademicYear)
               .HasConstraintName("FK_TrVisitorSchool_MsAcademicYear")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            base.Configure(builder);
        }
    }
}

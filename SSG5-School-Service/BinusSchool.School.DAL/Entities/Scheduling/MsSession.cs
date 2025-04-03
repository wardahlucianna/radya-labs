using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities.Scheduling
{
    public class MsSession : AuditEntity, ISchoolEntity
    {
        public string IdSessionSet { get; set; }
        public string IdDay { get; set; }
        public string IdGradePathway { get; set; }
        public int SessionID { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DurationInMinutes { get; set; }

        public virtual MsSessionSet SessionSet { get; set; }
        public virtual LtDay Day { get; set; }
        public virtual MsGradePathway GradePathway { get; set; }
    }

    internal class MsSessionConfiguration : AuditEntityConfiguration<MsSession>
    {
        public override void Configure(EntityTypeBuilder<MsSession> builder)
        {
            builder.Property(x => x.SessionID)
                .IsRequired();
            
            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(x => x.Alias)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(x => x.StartTime)
                .IsRequired();
            
            builder.Property(x => x.EndTime)
                .IsRequired();

            builder.Property(x => x.DurationInMinutes)
                .IsRequired();

            builder.HasOne(x => x.SessionSet)
                .WithMany(x => x.Sessions)
                .HasForeignKey(fk => fk.IdSessionSet)
                .HasConstraintName("FK_MsSession_MsSessionSet")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Day)
                .WithMany(x => x.Sessions)
                .HasForeignKey(fk => fk.IdDay)
                .HasConstraintName("FK_MsSession_LtDay")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.GradePathway)
                .WithMany(x => x.Sessions)
                .HasForeignKey(fk => fk.IdGradePathway)
                .HasConstraintName("FK_MsSession_MsGradePathway")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

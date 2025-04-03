using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.AttendanceDb.Abstractions;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class HTrMoveStudentHomeroom : AuditNoUniqueEntity, IAttendanceEntity
    {
        public string IdHTrMoveStudentHomeroom { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdHomeroomNew { get; set; }
        public string IdHomeroomOld { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsSendEmail { get; set; }
        public bool? IsSync { get; set; }
        public bool IsShowHistory { get; set; }
        public DateTime? DateSync { get; set; }
        public string Note { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual MsHomeroom HomeroomOld { get; set; }
        public virtual MsHomeroom HomeroomNew { get; set; }
    }

    internal class HTrMoveStudentHomeroomConfiguration : AuditNoUniqueEntityConfiguration<HTrMoveStudentHomeroom>
    {
        public override void Configure(EntityTypeBuilder<HTrMoveStudentHomeroom> builder)
        {
            builder.HasKey(x => x.IdHTrMoveStudentHomeroom);

            builder.HasOne(x => x.HomeroomStudent)
               .WithMany(x => x.HTrMoveStudentHomerooms)
               .HasForeignKey(fk => fk.IdHomeroomStudent)
               .HasConstraintName("FK_HTrMoveStudentHomeroom_MsHomeroomStudent")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.HomeroomOld)
               .WithMany(x => x.HTrMoveStudentHomeroomsOld)
               .HasForeignKey(fk => fk.IdHomeroomOld)
               .HasConstraintName("FK_HTrMoveStudentHomeroom_MsHomeroomOld")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.HomeroomNew)
                .WithMany(x => x.HTrMoveStudentHomeroomsNew)
                .HasForeignKey(fk => fk.IdHomeroomNew)
                .HasConstraintName("FK_HTrMoveStudentHomeroom_MsHomeroomNew")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.Property(p => p.IdHTrMoveStudentHomeroom).HasMaxLength(36).IsRequired();

            base.Configure(builder);
        }
    }
}

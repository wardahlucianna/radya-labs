using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventActivityAwardTeacher : AuditEntity, ISchedulingEntity
    {
        public string IdEventActivity { get; set; }
        public string IdAward { get; set; }
        public string IdStaff { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public decimal Filesize { get; set; }
        public string OriginalFilename { get; set; }
        public virtual TrEventActivity EventActivity { get; set; }
        public virtual MsAward Award { get; set; }
        public virtual MsStaff Staff { get; set; }
    }
    internal class TrEventActivityAwardTeacherConfiguration : AuditEntityConfiguration<TrEventActivityAwardTeacher>
    {
        public override void Configure(EntityTypeBuilder<TrEventActivityAwardTeacher> builder)
        {

            builder.HasOne(x => x.EventActivity)
            .WithMany(x => x.EventActivityAwardTeachers)
            .HasForeignKey(fk => fk.IdEventActivity)
            .HasConstraintName("FK_TrEventActivityAwardTeacher_TrEventActivity")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.Award)
            .WithMany(x => x.EventActivityAwardTeachers)
            .HasForeignKey(fk => fk.IdAward)
            .HasConstraintName("FK_TrEventActivityAwardTeacher_MsAward")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.Staff)
            .WithMany(x => x.EventActivityAwardTeachers)
            .HasForeignKey(fk => fk.IdStaff)
            .HasConstraintName("FK_TrEventActivityAward_MsStaff")
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

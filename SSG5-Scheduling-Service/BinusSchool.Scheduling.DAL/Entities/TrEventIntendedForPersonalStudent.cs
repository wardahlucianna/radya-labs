using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventIntendedForPersonalStudent : AuditEntity, ISchedulingEntity
    {
        public string IdStudent { get; set; }
        public string IdEventIntendedFor { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual TrEventIntendedFor EventIntendedFor { get; set; }
    }

    internal class TrEventIntendedForPersonalStudentConfiguration : AuditEntityConfiguration<TrEventIntendedForPersonalStudent>
    {
        public override void Configure(EntityTypeBuilder<TrEventIntendedForPersonalStudent> builder)
        {
            builder.HasOne(x => x.EventIntendedFor)
             .WithMany(x => x.EventIntendedForPersonalStudents)
             .HasForeignKey(fk => fk.IdEventIntendedFor)
             .HasConstraintName("FK_TrEventIntendedForPersonalStudent_TrEventIntendedFor")
             .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.Student)
              .WithMany(x => x.TrEventIntendedForPersonalStudents)
              .HasForeignKey(fk => fk.IdStudent)
              .HasConstraintName("FK_TrEventIntendedForPersonalStudent_MsStudent")
              .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

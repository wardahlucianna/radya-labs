using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventIntendedForDepartment : AuditEntity, ISchedulingEntity
    {
        public string IdEventIntendedFor { get; set; }
        public string IdDepartment { get; set; }

        public virtual TrEventIntendedFor EventIntendedFor { get; set; }
        public virtual MsDepartment Department { get; set; }
    }

    internal class TrEventIntendedForDepartmentConfiguration : AuditEntityConfiguration<TrEventIntendedForDepartment>
    {
        public override void Configure(EntityTypeBuilder<TrEventIntendedForDepartment> builder)
        {
            //builder.Property(x => x.IdDepartment)
            //    .HasMaxLength(36)
            //    .IsRequired();

            builder.HasOne(x => x.EventIntendedFor)
                .WithMany(x => x.EventIntendedForDepartments)
                .HasForeignKey(fk => fk.IdEventIntendedFor)
                .HasConstraintName("FK_TrEventIntendedForDepartment_TrEventIntendedFor")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();


            builder.HasOne(x => x.Department)
               .WithMany(x => x.TrEventIntendedForDepartments)
               .HasForeignKey(fk => fk.IdDepartment)
               .HasConstraintName("FK_TrEventIntendedForDepartment_MsDepartment")
               .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

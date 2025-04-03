using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentPoint : AuditEntity, IStudentEntity
    {
        public string IdHomeroomStudent { get; set; }
        public int MeritPoint { get; set; }
        public int DemeritPoint { get; set; }
        public string IdSanctionMapping { get; set; }
        public string IdLevelOfInteraction { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual MsSanctionMapping SanctionMapping { get; set; }
        public virtual MsLevelOfInteraction LevelOfInteraction { get; set; }
    }
    internal class TrStudentPointConfiguration : AuditEntityConfiguration<TrStudentPoint>
    {
        public override void Configure(EntityTypeBuilder<TrStudentPoint> builder)
        {
            builder.HasOne(x => x.HomeroomStudent)
              .WithMany(x => x.StudentPoints)
              .HasForeignKey(fk => fk.IdHomeroomStudent)
              .HasConstraintName("FK_TrStudentPoint_MsHomeroomStudent")
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SanctionMapping)
               .WithMany(x => x.StudentPoints)
               .HasForeignKey(fk => fk.IdSanctionMapping)
               .HasConstraintName("FK_TrStudentPoint_MsSanctionMapping")
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LevelOfInteraction)
             .WithMany(x => x.StudentPoints)
             .HasForeignKey(fk => fk.IdLevelOfInteraction)
             .HasConstraintName("FK_TrStudentPoint_MsLevelOfInteraction")
             .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}

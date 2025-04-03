using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsSanctionMappingAttentionBy : AuditEntity, IStudentEntity
    {
        public string IdSanctionMapping { get; set; }
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public string IdUser { get; set; }
        public virtual MsSanctionMapping SanctionMapping { get; set; }
        public virtual LtRole Role { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class MsSanctionMappingAttentionByConfiguration : AuditEntityConfiguration<MsSanctionMappingAttentionBy>
    {
        public override void Configure(EntityTypeBuilder<MsSanctionMappingAttentionBy> builder)
        {
            

            builder.HasOne(x => x.SanctionMapping)
                .WithMany(x => x.SanctionMappingAttentionBies)
                .HasForeignKey(fk => fk.IdSanctionMapping)
                .HasConstraintName("FK_MsSanctionMappingAttentionBy_MsSanctionMapping")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.Role)
              .WithMany(x => x.SanctionMappingAttentionBies)
              .HasForeignKey(fk => fk.IdRole)
              .HasConstraintName("FK_MsSanctionMappingAttentionBy_LtRole")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.TeacherPosition)
              .WithMany(x => x.SanctionMappingAttentionBies)
              .HasForeignKey(fk => fk.IdTeacherPosition)
              .HasConstraintName("FK_MsSanctionMappingAttentionBy_MsTeacherPosition")
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
              .WithMany(x => x.SanctionMappingAttentionBies)
              .HasForeignKey(fk => fk.IdUser)
              .HasConstraintName("FK_MsSanctionMappingAttentionBy_MsUser")
              .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}

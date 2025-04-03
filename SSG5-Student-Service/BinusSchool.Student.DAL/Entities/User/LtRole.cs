using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;

namespace BinusSchool.Persistence.StudentDb.Entities.User
{
    public class LtRole : AuditEntity, IStudentEntity
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string IdRoleGroup { get; set; }
        public virtual LtRoleGroup RoleGroup { get; set; }
        public virtual ICollection<MsCounselor> Counselors { get; set; }
        public virtual ICollection<TrRolePosition> RolePositions { get; set; }
        public virtual ICollection<MsSanctionMappingAttentionBy> SanctionMappingAttentionBies { get; set; }
        public virtual ICollection<MsUserRole> UserRoles { get; set; }
    }

    internal class LtRoleConfiguration : AuditEntityConfiguration<LtRole>
    {
        public override void Configure(EntityTypeBuilder<LtRole> builder)
        {
            builder.Property(x => x.Code)
               .HasMaxLength(50)
               .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            builder.HasOne(x => x.RoleGroup)
             .WithMany(x => x.Roles)
             .HasForeignKey(fk => fk.IdRoleGroup)
             .HasConstraintName("FK_LtRole_LtRoleGroup")
             .OnDelete(DeleteBehavior.Cascade)
             .IsRequired();
            base.Configure(builder);
        }
    }
}

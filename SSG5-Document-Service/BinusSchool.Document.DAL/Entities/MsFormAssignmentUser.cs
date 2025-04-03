using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.DocumentDb.Entities.User;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsFormAssignmentUser : AuditEntity, IDocumentEntity
    {
        public string IdFormAssignmentRole { get; set; }
        public string IdUser { get; set; }

        public virtual MsFormAssignmentRole FormAssignmentRole { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class MsFormAssignmentUserConfiguration : AuditEntityConfiguration<MsFormAssignmentUser>
    {
        public override void Configure(EntityTypeBuilder<MsFormAssignmentUser> builder)
        {
            builder.Property(x => x.IdUser)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.FormAssignmentRole)
                .WithMany(x => x.FormAssignmentUsers)
                .HasForeignKey(fk => fk.IdFormAssignmentRole)
                .HasConstraintName("FK_MsFormAssignmentUser_MsFormAssignmentRole")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.FormAssignmentUsers)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsFormAssignmentUser_MsUser")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}

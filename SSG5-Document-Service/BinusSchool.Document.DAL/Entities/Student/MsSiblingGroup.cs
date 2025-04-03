using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.Student
{
    public class MsSiblingGroup : AuditUniquelessEntity, IDocumentEntity
    {
        public string IdStudent { get; set; }

        public virtual MsStudent Student { get; set; }
    }

    internal class MsSiblingGroupConfiguration : AuditUniquelessEntityConfiguration<MsSiblingGroup>
    {
        public override void Configure(EntityTypeBuilder<MsSiblingGroup> builder)
        {
            builder.HasKey(p => new{p.IdStudent,p.Id});
           
            builder.Property(x => x.IdStudent)
                .HasMaxLength(36)
                .IsRequired();
            
            builder.HasOne(x => x.Student)
                .WithOne( y => y.SiblingGroup)
                .HasForeignKey<MsSiblingGroup>( fk => fk.IdStudent)
                .HasConstraintName("FK_MsSiblingGroup_MsStudent")
                .OnDelete(DeleteBehavior.Cascade);
            
            base.Configure(builder);
        }
    }
}

using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsSiblingGroup : AuditUniquelessEntity, IStudentEntity
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

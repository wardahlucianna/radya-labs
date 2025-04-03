using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtParentRelationship : AuditNoUniqueEntity, IStudentEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdParentRelationship { get; set; }
        public string IdParentRole { get; set; }
        public string RelationshipNameEng { get; set; }
        public string RelationshipNameInd { get; set; }
        public virtual ICollection<MsParent> Parent { get; set; }
        public virtual LtParentRole ParentRole { get; set; }
    }
    internal class LtParentRelationshipConfiguration : AuditNoUniqueEntityConfiguration<LtParentRelationship>
    {
        public override void Configure(EntityTypeBuilder<LtParentRelationship> builder)
        {
            builder.HasKey(p => p.IdParentRelationship);

            builder.Property(x => x.IdParentRole)
                .HasMaxLength(36);

            builder.Property(x => x.RelationshipNameEng)
                .HasMaxLength(50);

            builder.Property(x => x.RelationshipNameInd)
                .HasMaxLength(50);        

            builder.HasOne(x => x.ParentRole)
                .WithMany( y => y.ParentRelationship)
                .HasForeignKey( fk => fk.IdParentRole)
                .HasConstraintName("FK_LtParentRelationship_LtParentRole")
                .OnDelete(DeleteBehavior.Restrict);        

            base.Configure(builder);    
        }
    }
}

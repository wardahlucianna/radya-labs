using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class TrTimeTablePrefHeader : AuditEntity, ITeachingEntity
    {
        public string IdParent { get; set; }
        public bool Status { get; set; }
        public bool IsParent { get; set; }
        public bool IsMerge { get; set; }
        public bool CanDelete { get; set; }

        public virtual TrTimeTablePrefHeader Parent { get; set; }
        public virtual ICollection<TrTimeTablePrefHeader> Childs { get; set; }
        public virtual ICollection<TrTimetablePrefDetail> TimetablePrefDetails { get; set; }
        public virtual MsSubjectCombination SubjectCombination { get; set; }
    }

    internal class TrTimeTablePrefHeaderConfiguration : AuditEntityConfiguration<TrTimeTablePrefHeader>
    {
        public override void Configure(EntityTypeBuilder<TrTimeTablePrefHeader> builder)
        {
            builder.Property(x => x.Status)
                .IsRequired();
                
            builder.Property(x => x.IsParent)
                .IsRequired();

            builder.Property(x => x.IsMerge)
                .IsRequired();

            builder.Property(x => x.CanDelete)
                .IsRequired();

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Childs)
                .HasForeignKey(fk => fk.IdParent)
                .HasConstraintName("FK_TrTimetablePrefHeader_TrTimetablePrefHeader")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}

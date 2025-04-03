using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsConcernCategory : AuditEntity, IStudentEntity
    {
        public string ConcernCategoryName { get; set; }
        public string IdSchool { get; set; }
        public virtual ICollection<TrCounselingServicesEntryConcern> CounselingServicesEntryConcern { get; set; }
        public virtual MsSchool Schools { get; set; }
    }

    internal class MsConcernCategoryConfiguration : AuditEntityConfiguration<MsConcernCategory>
    {
        public override void Configure(EntityTypeBuilder<MsConcernCategory> builder)
        {
            builder.Property(x => x.ConcernCategoryName).IsRequired().HasMaxLength(50);

            builder.Property(x => x.IdSchool).IsRequired().HasMaxLength(36);

            builder.HasOne(x => x.Schools)
                .WithMany(x => x.ConcernCategories)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsConcernCategory_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

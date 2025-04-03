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
    public class MsCounselingCategory : AuditEntity, IStudentEntity
    {
        public string CounselingCategoryName { get; set; }  
        public string IdSchool { get; set; }
        public virtual ICollection<TrCounselingServicesEntry> CounselingServicesEntry { get; set; }  
        public virtual MsSchool Schools { get; set; }
    }

    internal class MsCounselingCategoryConfiguration : AuditEntityConfiguration<MsCounselingCategory>
    {
        public override void Configure(EntityTypeBuilder<MsCounselingCategory> builder)
        {
            builder.Property(x => x.CounselingCategoryName).IsRequired().HasMaxLength(50);

            builder.Property(x => x.IdSchool).IsRequired().HasMaxLength(36);

            builder.HasOne(x => x.Schools)
                .WithMany(x => x.CounselingCategories)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsCounselingCategory_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

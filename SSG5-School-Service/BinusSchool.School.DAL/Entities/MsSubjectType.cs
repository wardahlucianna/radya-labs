using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsSubjectType : CodeEntity, ISchoolEntity
    {
        public string IdSchool { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsSubject> Subjects { get; set; }
    }

    internal class MsSubjectTypeConfiguration : CodeEntityConfiguration<MsSubjectType>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectType> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.SubjectTypes)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSubjectType_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);

            builder.Property(x => x.Code)
                .HasMaxLength(36);
        }
    }
}

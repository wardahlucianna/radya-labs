using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.DAL.Entities
{
    public class LtSubjectSelectionCountry : AuditEntity, IStudentEntity
    {
        public string IdSchool { get; set; }
        public string CountryName { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsMappingCountryGrade> MappingCountryGrades { get; set; }
    }

    internal class LtSubjectSelectionCountryConfiguration : AuditEntityConfiguration<LtSubjectSelectionCountry>
    {
        public override void Configure(EntityTypeBuilder<LtSubjectSelectionCountry> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.SubjectSelectionCountries)
                .HasForeignKey(x => x.IdSchool)
                .HasConstraintName("FK_LtSubjectSelectionCountry_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
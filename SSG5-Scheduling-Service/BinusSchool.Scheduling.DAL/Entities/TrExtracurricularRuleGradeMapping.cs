using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrExtracurricularRuleGradeMapping : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricularRule { get; set; }
        public string IdGrade { get; set; }
        public virtual MsExtracurricularRule ExtracurricularRule { get; set; }
        public virtual MsGrade Grade { get; set; }
        
    }
    internal class TrExtracurricularRuleGradeMappingConfiguration : AuditEntityConfiguration<TrExtracurricularRuleGradeMapping>
    {
        public override void Configure(EntityTypeBuilder<TrExtracurricularRuleGradeMapping> builder)
        {
            
            builder.Property(x => x.IdExtracurricularRule)
                  .HasMaxLength(36)
                  .IsRequired();

            builder.Property(x => x.IdGrade)
                 .HasMaxLength(36)
                 .IsRequired();

            builder.HasOne(x => x.ExtracurricularRule)
                 .WithMany(y => y.ExtracurricularRuleGradeMappings)
                 .HasForeignKey(fk => fk.IdExtracurricularRule)
                 .HasConstraintName("FK_TrExtracurricularRuleGradeMapping_MsExtracurricular")
                 .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne(x => x.Grade)
                 .WithMany(y => y.ExtracurricularRuleGradeMappings)
                 .HasForeignKey(fk => fk.IdGrade)
                 .HasConstraintName("FK_TrExtracurricularRuleGradeMapping_MsGrade")
                 .OnDelete(DeleteBehavior.NoAction);


            base.Configure(builder);

        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class LtProjectPhase : CodeEntity, ISchoolEntity
    {
        public virtual ICollection<MsProjectPipeline> ProjectPipelines { get; set; }
    }

    internal class LtProjectPhaseConfiguration : CodeEntityConfiguration<LtProjectPhase>
    {
        public override void Configure(EntityTypeBuilder<LtProjectPhase> builder)
        {
            base.Configure(builder);
        }
    }
}

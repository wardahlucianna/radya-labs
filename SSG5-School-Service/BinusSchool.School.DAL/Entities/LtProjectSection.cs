using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class LtProjectSection : CodeEntity, ISchoolEntity
    {
        public virtual ICollection<MsProjectPipeline> ProjectPipelines { get; set; }
    }

    internal class LtProjectSectionConfiguration : CodeEntityConfiguration<LtProjectSection>
    {
        public override void Configure(EntityTypeBuilder<LtProjectSection> builder)
        {
            base.Configure(builder);
        }
    }
}

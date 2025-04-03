using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities.User
{
    public class MsUser : AuditEntity, ISchoolEntity
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool IsActiveDirectory { get; set; }
        public bool Status { get; set; }
        public virtual ICollection<MsSanctionMappingAttentionBy> SanctionMappingAttentionBies { get; set; }
        public virtual ICollection<MsUserPlatform> UserPlatforms { get; set; }
        public virtual ICollection<TrPublishSurveyUser> PublishSurveyUsers { get; set; }
        public virtual ICollection<TrSurvey> Surveys { get; set; }
    }

    internal class MsUserConfiguration : AuditEntityConfiguration<MsUser>
    {
        public override void Configure(EntityTypeBuilder<MsUser> builder)
        {
            builder.Property(x => x.DisplayName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Username)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasMaxLength(128);

            base.Configure(builder);
        }
    }
}

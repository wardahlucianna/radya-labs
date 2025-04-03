﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsLevelOfInteraction : AuditEntity,ISchoolEntity
    {
        public string IdSchool { get; set; }
        public string NameLevelOfInteraction { get; set; }
        public bool IsUseApproval { get; set; }
        public string IdParentLevelOfInteraction { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsLevelOfInteraction Parent { get; set; }
        public virtual ICollection<MsLevelOfInteraction> Children { get; set; }
        public virtual ICollection<MsMeritDemeritMapping> MeritDemeritMappings { get; set; }
    }

    internal class MsLevelOfInteractionConfiguration : AuditEntityConfiguration<MsLevelOfInteraction>
    {
        public override void Configure(EntityTypeBuilder<MsLevelOfInteraction> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.LevelOfInteractions)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsLevelOfInteraction_MsSchool")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.Parent)
               .WithMany(x => x.Children)
               .HasForeignKey(fk => fk.IdParentLevelOfInteraction)
               .HasConstraintName("FK_MsLevelOfInteraction_MsLevelOfInteraction")
               .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}

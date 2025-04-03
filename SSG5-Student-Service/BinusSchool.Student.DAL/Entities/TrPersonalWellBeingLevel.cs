using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrPersonalWellBeingLevel : AuditEntity, IStudentEntity
    {
        public string IdPersonalWellBeing {get; set;}
        public string IdLevel {get; set;}

        public virtual TrPersonalWellBeing PersonalWellBeing { get; set;}   
        public virtual MsLevel Level { get; set;}   
    }

    internal class TrPersonalWellBeingLevelConfiguration : AuditEntityConfiguration<TrPersonalWellBeingLevel>
    {
        public override void Configure(EntityTypeBuilder<TrPersonalWellBeingLevel> builder)
        {
            builder.HasOne(x => x.PersonalWellBeing)
             .WithMany(x => x.PersonalWellBeingLevel)
             .HasForeignKey(fk => fk.IdPersonalWellBeing)
             .HasConstraintName("FK_TrPersonalWellBeingLevel_TrPersonalWellBeing")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.Level)
             .WithMany(x => x.PersonalWellBeingLevel)
             .HasForeignKey(fk => fk.IdLevel)
             .HasConstraintName("FK_TrPersonalWellBeingLevel_MsLevel")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }




}

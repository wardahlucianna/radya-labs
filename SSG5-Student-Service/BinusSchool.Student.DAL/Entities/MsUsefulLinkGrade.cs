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
    public class MsUsefulLinkGrade : AuditEntity, IStudentEntity
    {
        public string IdUsefulLink { get; set; }    
        public string IdGrade { get; set; }    

        public virtual MsUsefulLink UsefulLink { get; set; }
        public virtual MsGrade Grade { get; set; }
    }

    internal class MsUsefulLinkGradeonfiguration : AuditEntityConfiguration<MsUsefulLinkGrade>
    {
        public override void Configure(EntityTypeBuilder<MsUsefulLinkGrade> builder)
        {
            builder.HasOne(x => x.UsefulLink)
             .WithMany(x => x.UsefulLinkGrade)
             .HasForeignKey(fk => fk.IdUsefulLink)
             .HasConstraintName("FK_MsUsefulLinkGrade_MsUsefulLink")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.Grade)
             .WithMany(x => x.UsefulLinkGrade)
             .HasForeignKey(fk => fk.IdGrade)
             .HasConstraintName("FK_MsUsefulLinkGrade_MsGrade")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}

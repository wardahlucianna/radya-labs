using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.Teaching
{
    public class MsTeacherPosition : CodeEntity, IStudentEntity
    {
        public string IdPosition { get; set; }
        public string IdSchool { get; set; }
        public virtual LtPosition Position { get; set; }
        public virtual MsSchool School { get; set; }

        public virtual ICollection<MsHomeroomTeacher> HomeroomTeachers { get; set; }
        public virtual ICollection<MsNonTeachingLoad> NonTeachingLoads { get; set; }
        public virtual ICollection<MsMeritDemeritApprovalSetting> Approval1 { get; set; }
        public virtual ICollection<MsMeritDemeritApprovalSetting> Approval2 { get; set; }
        public virtual ICollection<MsMeritDemeritApprovalSetting> Approval3 { get; set; }
        public virtual ICollection<MsSanctionMappingAttentionBy> SanctionMappingAttentionBies { get; set; }
        public virtual ICollection<TrRolePosition> RolePositions { get; set; }
    }

    internal class MsTeacherPositionConfiguration : CodeEntityConfiguration<MsTeacherPosition>
    {
        public override void Configure(EntityTypeBuilder<MsTeacherPosition> builder)
        {
           
            builder.HasOne(x => x.Position)
                .WithMany(x => x.TeacherPositions)
                .HasForeignKey(fk => fk.IdPosition)
                .HasConstraintName("FK_MsTeacherPosition_LtPosition")
                .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired();

            builder.HasOne(x => x.School)
               .WithMany(x => x.TeacherPositions)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_MsTeacherPosition_MsSchool")
               .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}

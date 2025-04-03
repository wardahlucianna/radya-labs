using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrCourseworkAnecdotalStudentSeen : AuditEntity, IStudentEntity
    {
        public string IdCourseworkAnecdotalStudent { get; set; }
        public string IdUserSeen { get; set; }
        public virtual TrCourseworkAnecdotalStudent CourseworkAnecdotalStudent { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class TrCourseworkAnecdotalStudentSeenConfiguration : AuditEntityConfiguration<TrCourseworkAnecdotalStudentSeen>
    {
        public override void Configure(EntityTypeBuilder<TrCourseworkAnecdotalStudentSeen> builder)
        {
            builder.HasOne(x => x.User)
                 .WithMany(x => x.PortfolioSeens)
                 .HasForeignKey(fk => fk.IdUserSeen)
                 .HasConstraintName("FK_TrCourseworkAnecdotalStudentSeen_MsUser")
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired();

            builder.HasOne(x => x.CourseworkAnecdotalStudent)
                 .WithMany(x => x.Seens)
                 .HasForeignKey(fk => fk.IdCourseworkAnecdotalStudent)
                 .HasConstraintName("FK_TrCourseworkAnecdotalStudentSeen_TrCourseworkAnecdotalStudent")
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired();

            base.Configure(builder); 

        }
    }

}

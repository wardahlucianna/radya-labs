using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrCourseworkAnecdotalStudentComment : AuditEntity, IStudentEntity
    {
        public string IdCourseworkAnecdotalStudent { get; set; }
        public string IdUserComment { get; set; }
        public string Comment { get; set; }
        public virtual TrCourseworkAnecdotalStudent CourseworkAnecdotalStudent { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class TrCourseworkAnecdotalStudentCommentConfiguration : AuditEntityConfiguration<TrCourseworkAnecdotalStudentComment>
    {
        public override void Configure(EntityTypeBuilder<TrCourseworkAnecdotalStudentComment> builder)
        {        
            builder.Property(x => x.Comment)
                .IsRequired(true)
                .HasMaxLength(int.MaxValue);

            builder.HasOne(x => x.User)
                 .WithMany(x => x.PortfolioComments)
                 .HasForeignKey(fk => fk.IdUserComment)
                 .HasConstraintName("FK_TrCourseworkAnecdotalStudentComment_MsUser")
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired();

            builder.HasOne(x => x.CourseworkAnecdotalStudent)
                 .WithMany(x => x.Comments)
                 .HasForeignKey(fk => fk.IdCourseworkAnecdotalStudent)
                 .HasConstraintName("FK_TrCourseworkAnecdotalStudentComment_TrCourseworkAnecdotalStudent")
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired();

            base.Configure(builder); 

        }
    }

}

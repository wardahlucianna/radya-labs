using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrReflectionStudentComment : AuditEntity, IStudentEntity
    {
        public string IdReflectionStudent { get; set; }
        public string IdUserComment { get; set; }
        public string Comment { get; set; }
        public virtual TrReflectionStudent ReflectionStudent { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class TrReflectionStudentCommentConfiguration : AuditEntityConfiguration<TrReflectionStudentComment>
    {
        public override void Configure(EntityTypeBuilder<TrReflectionStudentComment> builder)
        {        
            builder.Property(x => x.Comment)
                .IsRequired(true)
                .HasMaxLength(int.MaxValue);

            builder.HasOne(x => x.User)
                 .WithMany(x => x.ReflectionComments)
                 .HasForeignKey(fk => fk.IdUserComment)
                 .HasConstraintName("FK_TrReflectionStudentComment_MsUser")
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired();

            builder.HasOne(x => x.ReflectionStudent)
                 .WithMany(x => x.Comments)
                 .HasForeignKey(fk => fk.IdReflectionStudent)
                 .HasConstraintName("FK_TrReflectionStudent_TrReflectionStudentComment")
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired();

            base.Configure(builder); 

        }
    }

}

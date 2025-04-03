using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentInfoUpdate : AuditEntity, IStudentEntity
    {
        public string IdUser { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string Constraint1 { get; set; }
        public string Constraint2 { get; set; }
        public string Constraint3 { get; set; }
        public string OldFieldValue { get; set; }
        public string CurrentFieldValue { get; set; }
        public string Constraint1Value { get; set; }
        public string Constraint2Value { get; set; }
        public string Constraint3Value { get; set; }    
        public DateTime? RequestedDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public int IdApprovalStatus { get; set; }
        public string Notes { get; set; }
        public int IsParentUpdate { get; set; }
        public string RequestedBy { get; set; }

        public virtual LtStudentDataApprovalStatus StudentDataApprovalStatus { get; set; }
      
    }
    internal class TrStudentInfoUpdateConfiguration : AuditEntityConfiguration<TrStudentInfoUpdate>
    {
        public override void Configure(EntityTypeBuilder<TrStudentInfoUpdate> builder)
        {
            //builder.HasKey(p => new {p.IdUser,p.TableName,p.FieldName,p.DateIn});  

            builder.Property(x => x.IdUser)             
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.TableName)             
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FieldName)             
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Constraint1)             
                .HasMaxLength(50);

            builder.Property(x => x.Constraint2)             
                .HasMaxLength(50);

            builder.Property(x => x.Constraint3)             
                .HasMaxLength(50);        

            builder.Property(x => x.OldFieldValue)             
                .HasMaxLength(350);      

            builder.Property(x => x.CurrentFieldValue)             
                .HasMaxLength(350);    

            builder.Property(x => x.Constraint1Value)             
                .HasMaxLength(50);

            builder.Property(x => x.Constraint2Value)             
                .HasMaxLength(50);

            builder.Property(x => x.Constraint3Value)             
                .HasMaxLength(50);      

            builder.Property(x => x.RequestedDate)              
                .HasColumnType(typeName: "datetime2");    

             builder.Property(x => x.ApprovalDate)              
                .HasColumnType(typeName: "datetime2");        

            builder.Property(x => x.Notes)             
                .HasMaxLength(300);      

            builder.Property(x => x.RequestedBy)             
                .HasMaxLength(100);       


            builder.HasOne(x => x.StudentDataApprovalStatus)
                .WithMany( y => y.StudentInfoUpdate)
                .HasForeignKey( fk => fk.IdApprovalStatus)
                .HasConstraintName("FK_TrStudentInfoUpdate_LtStudentDataApprovalStatus")
                .OnDelete(DeleteBehavior.NoAction);     

            base.Configure(builder);
        }

    }
}

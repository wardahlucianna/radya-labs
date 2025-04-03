using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsBankAccountInformation : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdBank { set; get; }
        public string IdStudent { set; get; }
        public string AccountNumberCurrentValue { set; get; }
        public string AccountNameCurrentValue { set; get; }
        public string BankAccountNameCurrentValue { set; get; }
        public string AccountNumberNewValue { set; get; }
        public string AccountNameNewValue { set; get; }
        public string BankAccountNameNewValue { set; get; }
        public DateTime? RequestedDate { set; get; }
        public DateTime? ApprovalDate { set; get; }
        public DateTime? RejectDate { set; get; }
        public int Status { set; get; }
        public string Notes { set; get; }
        public virtual MsBank Bank { get; set; }
        public virtual MsStudent Student { get; set; }

    }
    internal class MsBankAccountInformationConfiguration : AuditNoUniqueEntityConfiguration<MsBankAccountInformation>
    {
        public override void Configure(EntityTypeBuilder<MsBankAccountInformation> builder)
        {
            builder.HasKey(p => new {p.IdStudent,p.IdBank});             

            builder.Property(x => x.IdStudent)             
                .HasMaxLength(36);

             builder.Property(x => x.IdStudent)             
                .HasMaxLength(36);    

            builder.Property(x => x.AccountNameCurrentValue)     
                .HasColumnType("VARCHAR(100)")           
                .HasMaxLength(100);    

            builder.Property(x => x.AccountNumberCurrentValue)     
                .HasColumnType("VARCHAR(100)")           
                .HasMaxLength(100);    

            builder.Property(x => x.BankAccountNameCurrentValue)                          
                .HasMaxLength(100);        

             builder.Property(x => x.AccountNameNewValue)     
                .HasColumnType("VARCHAR(100)")           
                .HasMaxLength(100);    

            builder.Property(x => x.AccountNumberNewValue)     
                .HasColumnType("VARCHAR(100)")           
                .HasMaxLength(100);    

            builder.Property(x => x.BankAccountNameNewValue)                          
                .HasMaxLength(100);    

            builder.Property(x => x.Notes)     
                .HasColumnType("VARCHAR(200)")           
                .HasMaxLength(200);     

            builder.HasOne(x => x.Bank)
                .WithMany( y => y.BankAccountInformation)
                .HasForeignKey( fk => fk.IdBank)
                .HasConstraintName("FK_MsBankAccountInformation_MsBank")
                .OnDelete(DeleteBehavior.NoAction);

             builder.HasOne(x => x.Student)
                .WithMany( y => y.BankAccountInformation)
                .HasForeignKey( fk => fk.IdStudent)
                .HasConstraintName("FK_MsBankAccountInformation_MsStudent")
                .OnDelete(DeleteBehavior.NoAction);


             base.Configure(builder); 
        }
    }
}

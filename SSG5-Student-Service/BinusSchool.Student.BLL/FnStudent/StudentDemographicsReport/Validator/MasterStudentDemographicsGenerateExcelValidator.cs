using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport.Validator
{
    public class MasterStudentDemographicsGenerateExcelValidator : AbstractValidator<MasterStudentDemographicsGenerateExcelRequest>
    {
        public MasterStudentDemographicsGenerateExcelValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
        }
    }
}

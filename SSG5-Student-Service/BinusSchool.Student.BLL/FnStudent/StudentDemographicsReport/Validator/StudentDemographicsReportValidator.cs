using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport.Validator
{
    public class StudentDemographicsReportValidator : AbstractValidator<StudentDemographicsReportRequest>
    {
        public StudentDemographicsReportValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotNull();
            RuleFor(x => x.ViewCategoryType).NotEmpty();
        }
    }
}

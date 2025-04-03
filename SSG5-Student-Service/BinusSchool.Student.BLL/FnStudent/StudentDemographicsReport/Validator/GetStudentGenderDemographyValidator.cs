using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport.Validator
{
    public class GetStudentGenderDemographyValidator : AbstractValidator<GetStudentGenderDemographyRequest>
    {
        public GetStudentGenderDemographyValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
        }
    }
}

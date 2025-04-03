using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.OnlineRegistration.Validator
{
    public class GetExtracurricularListByStudentValidator : AbstractValidator<GetExtracurricularListByStudentRequest>
    {
        public GetExtracurricularListByStudentValidator()
        {
            //RuleFor(x => x.IdAcademicYear).NotEmpty();
            //RuleFor(x => x.IdGrade).NotEmpty();
            //RuleFor(x => x.Semester).NotEmpty();
            //RuleFor(x => x.IdHomeroom).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
        }
    }
}

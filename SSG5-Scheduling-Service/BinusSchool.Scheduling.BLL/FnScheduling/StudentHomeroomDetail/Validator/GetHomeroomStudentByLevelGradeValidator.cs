using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentHomeroomDetail;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.StudentHomeroomDetail.Validator
{
    public class GetHomeroomStudentByLevelGradeValidator : AbstractValidator<GetHomeroomStudentByLevelGradeRequest>
    {
        public GetHomeroomStudentByLevelGradeValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
        }
    }
}

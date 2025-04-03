using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MapStudentPathway.Validator
{
    public class CopyMapStudentPathwayValidation : AbstractValidator<CopyMapStudentPathwayRequest>
    {
        public CopyMapStudentPathwayValidation()
        {
            RuleFor(x => x.MapStudentPathway).NotEmpty().ForEach(maps => maps.ChildRules(map =>
            {
                map.RuleFor(e => e.IdStudentGradeNextAy).NotEmpty().WithMessage("Id student grade cannot empty");
                map.RuleFor(e => e.IdPathwayNextAcademicYear).NotEmpty().WithMessage("Id pathway cannot empty");
            }));
        }
    }
}

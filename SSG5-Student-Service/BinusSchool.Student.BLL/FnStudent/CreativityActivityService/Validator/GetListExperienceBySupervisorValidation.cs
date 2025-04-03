using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CreativityActivityService.Validator
{
    public class GetListExperienceBySupervisorValidation : AbstractValidator<GetListExperienceBySupervisorRequest>
    {
        public GetListExperienceBySupervisorValidation()
        {
            RuleFor(x => x.IdAcademicYear.Count>0).NotEmpty();
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
        }
    }
}

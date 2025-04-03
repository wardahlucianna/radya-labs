using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.BLPGroup;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.BLPGroup.Validator
{
    public class UpdateBLPGroupStudentValidator : AbstractValidator<List<UpdateBLPGroupStudentRequest>>
    {
        public UpdateBLPGroupStudentValidator()
        {
            RuleFor(model => model)
                .NotEmpty()
                .ForEach(data => data.ChildRules(data =>
                {
                    data.RuleFor(x => x.IdAcademicYear).NotEmpty();
                    data.RuleFor(x => x.Semester).NotEmpty();
                    data.RuleFor(x => x.IdStudent).NotEmpty();
                    data.RuleFor(x => x.IdBLPGroup).NotEmpty();
                    data.RuleFor(x => x.IdBLPStatus).NotEmpty();
                    data.RuleFor(x => x.IdHomeroom).NotEmpty();
                }));
        }
    }
}

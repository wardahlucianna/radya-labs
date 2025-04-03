using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator
{
    public class DeleteStudentParticipantValidator : AbstractValidator<List<DeleteStudentParticipantRequest>>
    {

        public DeleteStudentParticipantValidator()
        {
            RuleFor(model => model)
               .NotEmpty()
               .ForEach(data => data.ChildRules(data =>
               {
                   data.RuleFor(x => x.IdStudent).NotEmpty();
                   data.RuleFor(x => x.IdHomeroom).NotEmpty();
                   data.RuleFor(x => x.IdExtracurricular).NotEmpty();
               }));
        }
    }
}

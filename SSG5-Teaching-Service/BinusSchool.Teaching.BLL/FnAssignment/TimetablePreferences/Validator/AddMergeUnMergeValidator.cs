using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.TimetablePreferences.Validator
{
    public class AddMergeUnMergeValidator : AbstractValidator<AddMergeUnmergeRequest>
    {
        public AddMergeUnMergeValidator()
        {
            RuleFor(p => p.Id).NotEmpty();
            When(x => x.IsMarge, () =>
            {
                RuleForEach(x => x.ChildId).NotNull();
            });
        }
    }
}

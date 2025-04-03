using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.SubjectGroupSettings;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.SubjectSelection.SubjectGroupSettings.Validator
{
    public class DeleteSubjectGroupSettingsSubjectSelectionGroupValidator : AbstractValidator<DeleteSubjectGroupSettingsSubjectSelectionGroupRequest>
    {
        public DeleteSubjectGroupSettingsSubjectSelectionGroupValidator()
        {
            RuleFor(x => x.IdSubjectSelectionGroup).NotEmpty();
        }
    }
}

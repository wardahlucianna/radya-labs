using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculumMapping.Validator
{
    public class DeleteSubjectSelectionCurriculumMappingValidator : AbstractValidator<DeleteSubjectSelectionCurriculumMappingRequest>
    {
        public DeleteSubjectSelectionCurriculumMappingValidator()
        {
            RuleFor(x => x.CurriculumGroup).NotEmpty();
        }
    }
}

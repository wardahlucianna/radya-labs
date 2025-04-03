using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculumMapping.Validator
{
    public class SaveSubjectSelectionCurriculumMappingValidator : AbstractValidator<SaveSubjectSelectionCurriculumMappingRequest>
    {
        public SaveSubjectSelectionCurriculumMappingValidator()
        {
            RuleFor(x => x.IdSubjectSelectionCurriculum).NotEmpty();
            RuleFor(x => x.IdGrade).NotEmpty();
        }
    }
}

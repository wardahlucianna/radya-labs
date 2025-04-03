using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculum.Validator
{
    public class SaveSubjectSelectionCurriculumValidator : AbstractValidator<SaveSubjectSelectionCurriculumRequest>
    {
        public SaveSubjectSelectionCurriculumValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.CurriculumName).NotEmpty();
        }
    }
}

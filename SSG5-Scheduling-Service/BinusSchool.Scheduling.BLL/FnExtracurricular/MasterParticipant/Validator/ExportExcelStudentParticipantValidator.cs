using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator
{
    public class ExportExcelStudentParticipantValidator : AbstractValidator<ExportExcelStudentParticipantRequest>
    {
        public ExportExcelStudentParticipantValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            RuleForEach(x => x.IdExtracurricular).NotEmpty();
        }
    }
}

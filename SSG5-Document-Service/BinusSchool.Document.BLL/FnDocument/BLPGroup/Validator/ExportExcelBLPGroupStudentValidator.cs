using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.BLPGroup;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.BLPGroup.Validator
{
    public class ExportExcelBLPGroupStudentValidator : AbstractValidator<ExportExcelBLPGroupStudentRequest>
    {
        public ExportExcelBLPGroupStudentValidator()
        {
            RuleFor(x => x.IdAcademicYear)
                .NotEmpty();

            //RuleFor(x => x.IdLevel)
            //    .NotEmpty();

            //RuleFor(x => x.IdGrade)
            //    .NotEmpty();

            //RuleFor(x => x.IdHomeroom)
            //    .NotEmpty();

            RuleFor(x => x.Semester)
                .NotEmpty();
        }
    }
}

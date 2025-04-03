using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.StudentPhoto;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.StudentPhoto.Validator
{
    public class SaveStudentPhotoDataValidator : AbstractValidator<SaveStudentPhotoDataRequest>
    {
        public SaveStudentPhotoDataValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
            RuleFor(x => x.IdBinusian).NotEmpty();
            RuleFor(x => x.FileName).NotEmpty();
            RuleFor(x => x.FilePath).NotEmpty();
        }
    }
}

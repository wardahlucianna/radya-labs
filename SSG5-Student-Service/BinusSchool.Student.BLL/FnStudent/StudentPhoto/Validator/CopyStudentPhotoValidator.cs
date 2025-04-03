using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.StudentPhoto;
using FluentValidation;


namespace BinusSchool.Student.FnStudent.StudentPhoto.Validator
{
    public class CopyStudentPhotoValidator : AbstractValidator<CopyStudentPhotoRequest>
    {
        public CopyStudentPhotoValidator()
        {
            RuleFor(x => x.IdAcademicYearFrom).NotEmpty();
            RuleFor(x => x.IdAcademicYearDest).NotEmpty();
            RuleFor(x => x.IdStudents).NotEmpty();
        }
    }
}

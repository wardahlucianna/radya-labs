using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using BinusSchool.Data.Model.Scoring.FnScoring.PMBenchmark.PMBenchmarkEntry;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.EmergencyAttendanceV2.Validator
{
    public class SaveStudentEmergencyAttendanceValidator : AbstractValidator<SaveStudentEmergencyAttendanceRequest>
    {
        public SaveStudentEmergencyAttendanceValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.RequestBy).NotEmpty();
            RuleFor(x => x.studentList).NotEmpty();

            RuleForEach(x => x.studentList).NotNull().ChildRules(child =>
            {
                child.RuleFor(x => x.IdStudent).NotEmpty();
            });
        }
    }

}

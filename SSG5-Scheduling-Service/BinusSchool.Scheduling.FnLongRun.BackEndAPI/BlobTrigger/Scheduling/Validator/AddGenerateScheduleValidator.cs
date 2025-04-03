using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnLongRun.BlobTrigger.Scheduling.Validator
{
    public class AddGenerateScheduleValidator : AbstractValidator<AddGenerateScheduleRequest>
    {
        public AddGenerateScheduleValidator()
        {
            RuleFor(x => x.IdAsctimetable).NotEmpty();

            RuleFor(x => x.Grades)
                .NotEmpty()
                .ForEach(gr => gr.ChildRules(grade =>
                 {
                     grade.RuleFor(x => x.GradeId).NotEmpty();

                     grade.RuleFor(x => x.StartPeriod)
                             .NotEmpty()
                             .LessThanOrEqualTo(p=> p.EndPeriod);

                     grade.RuleFor(x => x.EndPeriod)
                         .NotEmpty()
                         .GreaterThanOrEqualTo(x => x.StartPeriod);

                     grade.RuleFor(x => x.Students)
                     .NotEmpty()
                     .ForEach(std => std.ChildRules(student =>
                      {
                          student.RuleFor(x => x.StudentId).NotEmpty();
                          student.RuleFor(x => x.Lessons).NotEmpty()
                                 .ForEach(x => x.ChildRules(lesson =>
                                  {
                                      lesson.RuleFor(x=>x.ClassId).NotEmpty();
                                      lesson.RuleFor(x=>x.WeekId).NotEmpty();
                                      lesson.RuleFor(x=>x.StartPeriode).NotEmpty();
                                      lesson.RuleFor(x=>x.EndPeriode).NotEmpty();
                                      lesson.RuleFor(x=>x.ScheduleDate).NotEmpty();
                                      lesson.RuleFor(x=>x.VenueId).NotEmpty();
                                      lesson.RuleFor(x=>x.VenueName).NotEmpty();
                                      lesson.RuleFor(x=>x.TeacherId).NotEmpty();
                                      lesson.RuleFor(x=>x.TeacherName).NotEmpty();
                                      lesson.RuleFor(x=>x.IdSubject).NotEmpty();
                                      lesson.RuleFor(x=>x.SubjectName).NotEmpty();
                                      lesson.RuleFor(x=>x.IdSession).NotEmpty();
                                      lesson.RuleFor(x=>x.StartTime).NotEmpty();
                                      lesson.RuleFor(x=>x.EndTime).NotEmpty();
                                      lesson.RuleFor(x=>x.IdHomeroom).NotEmpty();
                                      lesson.RuleFor(x=>x.HomeroomName).NotEmpty();
                                  }));

                      }));

                 }));

        }

       
    }
}

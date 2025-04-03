using System.Linq;
using BinusSchool.Common.Exceptions;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using FluentValidation;

namespace BinusSchool.Scheduling.FnLongRun.BlobTrigger.AscTimetable.Validator
{
    public class AddReUploadXmlValidator : AbstractValidator<AddReUploadXmlRequest>
    {
        public AddReUploadXmlValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            // RuleFor(x => x.IdAscTimeTable).NotEmpty();
            RuleFor(x => x.IdGradepathwayforCreateSession).NotEmpty();

            //validate lesson
            When(enrolment => enrolment.Lesson.Any(), () =>
            {
                RuleFor(x => x.Lesson)
                    //.NotEmpty()
                    .ForEach(lesson => lesson.ChildRules(lesson =>
                    {
                        lesson.RuleFor(x => x.IsDataReadyFromMaster)
                              .Must(p => p == true)
                              .WithMessage("Lesson data master must ready from master ");
                    }).OnAnyFailure(request =>
                                throw new BadRequestException("Lesson data " + string.Join(",", request.Where(p => !p.IsDataReadyFromMaster).Select(p => p.ClassId).ToList()) + " this data not ready from master")
                                 ))
                    .OnAnyFailure(request => throw new BadRequestException("Data lesson Not be Empty"));
            });

            //validate home rome
            When(enrolment => enrolment.Class.Any(), () =>
            {
                RuleFor(x => x.Class)
                  //.NotEmpty()
                  .ForEach(clas => clas.ChildRules(clas =>
                  {
                      clas.RuleFor(x => x.IsDataReadyFromMaster)
                            .Must(p => p == true)
                            .WithMessage("class data master must ready from master ");

                      clas.When(x => x.StudentInHomeRoom != null && x.StudentInHomeRoom.Any(), () =>
                      {
                          clas.RuleFor(x => x.StudentInHomeRoom)
                              .ForEach(stdn => stdn.ChildRules(s =>
                              {
                                  s.RuleFor(vs => vs.StudentIsreadyFromMaster)
                                  .Must(p => p == true)
                                  .WithMessage("student not ready from master");
                              }).OnAnyFailure(result =>
                                 throw new BadRequestException("Student data " + string.Join(",", result.Where(p => !p.StudentIsreadyFromMaster).Select(p => p.StudentName).ToList()) + " not found in master")));
                      });

                  }).OnFailure(request => throw new BadRequestException("Class data " + string.Join(",", request.Where(p => !p.IsDataReadyFromMaster).Select(p => p.Class.Code).ToList()) + " this data not ready from master")))
                  .OnAnyFailure(request => throw new BadRequestException("Data Class Not be Empty"));
            });

            //validate schedule
            When(enrolment => enrolment.Schedule.Any(), () =>
            {
                RuleFor(x => x.Schedule)
                   //.NotEmpty()
                   .ForEach(sc => sc.ChildRules(sch =>
                   {
                       sch.RuleFor(z => z.Schedule)
                             .NotEmpty()
                             .ForEach(schc => schc.ChildRules(schcd =>
                             {
                                 schcd.RuleFor(y => y.ListSchedule)
                                 .NotEmpty()
                                  .ForEach(schcdu => schcdu.ChildRules(schcdul =>
                                   {
                                       schcdul.RuleFor(c => c.DataIsUseInMaster)
                                              .Must(c => c == true)
                                              .WithMessage("Schedule data master not ready");
                                   }).OnAnyFailure(result =>
                                                 throw new BadRequestException("data schedule for" + string.Join(",", result.Where(p => !p.DataIsUseInMaster).Select(p => p.ClassID).ToList()) + " data master not ready")))
                                      .OnAnyFailure(result => throw new BadRequestException("Data Schedule not be empty"));
                             }));

                   })
                   .OnAnyFailure(result => throw new BadRequestException("Data Schedule not be empty")));
            });


            //validate enrollment
            When(enrolment => enrolment.Enrollment != null && enrolment.Enrollment.EnrollmentData.Any(), () =>
            {
                RuleFor(x => x.Enrollment)
                   //.NotEmpty()
                   .ChildRules(x => x.RuleFor(p => p.EnrollmentData)
                                    .ForEach(y => y.ChildRules(z =>
                                    {
                                        z.RuleFor(stdn => stdn.Student)
                                         .Must(p => p.DataIsUseInMaster)
                                         .OnFailure(result => throw new BadRequestException("Enrollment for " + result.Student.StudentName + "can't insert because data not reday from master"));

                                        z.RuleFor(a => a.EnrollmentStudent)
                                        .NotEmpty()
                                        .ForEach(b => b.ChildRules(enrol =>
                                        {
                                            enrol.RuleFor(c => c.IsDataReadyFromMaster)
                                                .Must(c => c == true)
                                                .WithMessage("Enrollment data master not ready");

                                        }).OnFailure(result => throw new BadRequestException("data enrolment for " + string.Join(",", result.Where(p => !p.IsDataReadyFromMaster).Select(p => p.ClassId).ToList()) + " not ready from master "))
                                          ).OnAnyFailure(request => throw new BadRequestException("Data Enrolment not be empty"));

                                    })));
            });
        }
    }
}

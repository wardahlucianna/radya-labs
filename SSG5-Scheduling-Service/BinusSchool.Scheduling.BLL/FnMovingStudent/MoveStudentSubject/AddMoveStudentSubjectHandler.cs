using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject;
using BinusSchool.Scheduling.FnMovingStudent.MoveStudentSubject.Validator;
using BinusSchool.Data.Api.Scheduling.FnMovingStudent;
using BinusSchool.Persistence.SchedulingDb.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentSubject
{
    public class AddMoveStudentSubjectHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMoveStudentEnrollment _apiMoveStudentEnrollment;

        public AddMoveStudentSubjectHandler(ISchedulingDbContext schoolDbContext, IMoveStudentEnrollment apiMoveStudentEnrollment)
        {
            _dbContext = schoolDbContext;
            _apiMoveStudentEnrollment = apiMoveStudentEnrollment;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddMoveStudentSubjectRequest, AddMoveStudentSubjectValidator>();

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                  .Where(e => body.IdHomeroomStudents.Contains(e.IdHomeroomStudent)
                                                                && e.IdLesson==body.IdLessonOld
                                                                )
                                                  .Select(e => new
                                                  {
                                                      e.Id,
                                                      e.IdHomeroomStudent,
                                                      e.HomeroomStudent.Homeroom.IdGrade
                                                  })
                                                  .ToListAsync(CancellationToken);


            foreach (var item in listHomeroomStudentEnrollment)
            {
                var param = new AddMoveStudentEnrollmentRequest
                {
                    IdAcademicYear = body.IdAcademicYear,
                    IdHomeroomStudent = item.IdHomeroomStudent,
                    StudentEnrollment = new List<GetMoveStudentEnrollment>()
                    {
                        new GetMoveStudentEnrollment
                        {
                            IdHomeroomStudentEnrollment = item.Id,
                            IdGrade = item.IdGrade,
                            IdLessonOld = body.IdLessonOld,
                            IdSubjectOld = body.IdSubjectOld,
                            IdSubjectLevelOld = body.IdSubjectLevelOld,
                            EffectiveDate = body.EffectiveDate,
                            IdLessonNew = body.IdLesson,
                            IdSubjectNew = body.IdSubject,
                            IdSubjectLevelNew = body.IdSubjectLevel,
                            IsDelete = body.IsDelete,
                            IsSendEmail = body.IsSendEmail,
                            Note = body.Note
                        }
                    }
                };

                await _apiMoveStudentEnrollment.CreateMoveStudentEnrollment(param);
            };
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}

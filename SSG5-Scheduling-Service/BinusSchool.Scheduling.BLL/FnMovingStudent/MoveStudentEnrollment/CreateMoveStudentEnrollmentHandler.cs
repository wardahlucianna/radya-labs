using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment.Validator;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using System.Linq;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.Scheduling.FnMovingStudent;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class CreateMoveStudentEnrollmentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IMoveStudentEnrollment _MoveStudentEnrollmentService;
        public CreateMoveStudentEnrollmentHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime, IMoveStudentEnrollment MoveStudentEnrollmentService)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _MoveStudentEnrollmentService = MoveStudentEnrollmentService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddMoveStudentEnrollmentRequest, AddMoveStudentValidator>();

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                                  .Include(e => e.Grade).ThenInclude(e => e.Level)
                                  .Where(e => e.Grade.Level.AcademicYear.Id == body.IdAcademicYear)
                                  .ToListAsync(CancellationToken);

            var listIdHomeroomStudentEnrollmentBody = body.StudentEnrollment.Select(e => e.IdHomeroomStudentEnrollment).ToList();

            var listStudentHomeroomEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                    .Include(e => e.HomeroomStudent)
                                                    .Where(e => listIdHomeroomStudentEnrollmentBody.Contains(e.Id))
                                                    .ToListAsync(CancellationToken);

            if (!listStudentHomeroomEnrollment.Any())
                throw new BadRequestException("Homeroom student enrollment is not found");

            var semester = listStudentHomeroomEnrollment.Select(e => e.HomeroomStudent.Semester).FirstOrDefault();
            var listSubjectIdNew = body.StudentEnrollment.Select(e => e.IdSubjectNew).ToList();

            var listLessonNew = await _dbContext.Entity<MsLessonPathway>()
                                .Include(e => e.HomeroomPathway)
                                .Include(e => e.Lesson)
                                .Where(e => listSubjectIdNew.Contains(e.Lesson.IdSubject))
                                .Select(e => new
                                {
                                    Id = e.Lesson.Id,
                                    IdHomeroom = e.HomeroomPathway.IdHomeroom,
                                    Semester = e.Lesson.Semester,
                                    IdSubject = e.Lesson.IdSubject,
                                    IdLesson = e.IdLesson
                                })
                                .ToListAsync(CancellationToken);

            if (semester == 1)
            {
                var idStudent = listStudentHomeroomEnrollment.Select(e => e.HomeroomStudent.IdStudent).FirstOrDefault();
                var listIdStuject = listStudentHomeroomEnrollment.Select(e => e.IdSubject).Distinct().ToList();

                var listStudentHomeroomEnrollmentSmt2 = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                            .Include(e => e.HomeroomStudent)
                                                            .Where(e => e.HomeroomStudent.IdStudent == idStudent
                                                                        && e.HomeroomStudent.Homeroom.IdAcademicYear == body.IdAcademicYear
                                                                        && e.HomeroomStudent.Homeroom.Semester == 2
                                                                        && listIdStuject.Contains(e.IdSubject))
                                                            .ToListAsync(CancellationToken);

                listStudentHomeroomEnrollment.AddRange(listStudentHomeroomEnrollmentSmt2);
            }

            var listIdHomeroomStudentEnrollment = listStudentHomeroomEnrollment.Select(e => e.Id).ToList();


            var listTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                                                .Where(e => listIdHomeroomStudentEnrollment.Contains(e.IdHomeroomStudentEnrollment))
                                                .ToListAsync(CancellationToken);

            List<string> ListIdTrHomeroomStudentEnrollment = new List<string>();
            foreach (var studentEnrollment in body.StudentEnrollment)
            {
                var _listStudentHomeroomEnrollment = listStudentHomeroomEnrollment.Where(e => e.IdSubject == studentEnrollment.IdSubjectOld && e.IdSubjectLevel == studentEnrollment.IdSubjectLevelOld).ToList();

                foreach (var item in _listStudentHomeroomEnrollment)
                {
                    var trHomeroomStudentEnrollment = listTrHomeroomStudentEnrollment.Where(e => e.IdHomeroomStudentEnrollment == item.Id).ToList();
                    var studentHomeroomEnrollment = _listStudentHomeroomEnrollment.Where(e => e.Id == item.Id).FirstOrDefault();

                    var apiGetDetailMoveStudentEnrollment = await _MoveStudentEnrollmentService.GetDetailMoveStudentEnrollment(studentHomeroomEnrollment.IdHomeroomStudent);
                    var getDetailMoveStudentEnrollment = apiGetDetailMoveStudentEnrollment.IsSuccess ? apiGetDetailMoveStudentEnrollment.Payload : null;
                    var homeroomStudentEnrollmentOld = getDetailMoveStudentEnrollment.MovingStudentEnrollment.Where(e => e.idhomeroomStudentEnrollment == studentHomeroomEnrollment.Id).FirstOrDefault();

                    TrHomeroomStudentEnrollment newTrHomeroomStudentEnrollment = default;
                    if (!trHomeroomStudentEnrollment.Any())
                    {
                        var effectiveDate = listPeriod
                                     .Where(e => e.IdGrade == studentEnrollment.IdGrade && e.Semester == studentHomeroomEnrollment.HomeroomStudent.Semester)
                                     .Select(e => e.AttendanceStartDate).Min();

                        newTrHomeroomStudentEnrollment = new TrHomeroomStudentEnrollment
                        {
                            IdTrHomeroomStudentEnrollment = Guid.NewGuid().ToString(),
                            IdHomeroomStudent = studentHomeroomEnrollment.IdHomeroomStudent,
                            IdHomeroomStudentEnrollment = studentHomeroomEnrollment.Id,
                            StartDate = effectiveDate,
                            Note = studentEnrollment.Note,
                            IsDelete = false,
                            IsSendEmail = studentEnrollment.IsSendEmail,
                            IdLessonNew = homeroomStudentEnrollmentOld.idLesson,
                            IdSubjectNew = homeroomStudentEnrollmentOld.idSubject,
                            IdSubjectLevelNew = homeroomStudentEnrollmentOld.idSubjectLevel,
                            IdLessonOld = homeroomStudentEnrollmentOld.idLesson,
                            IdSubjectOld = homeroomStudentEnrollmentOld.idSubject,
                            IdSubjectLevelOld = homeroomStudentEnrollmentOld.idSubjectLevel,
                            IsSync = _dateTime.ServerTime.Date >= studentEnrollment.EffectiveDate.Date ? true : (bool?)null,
                            DateSync = _dateTime.ServerTime.Date >= studentEnrollment.EffectiveDate.Date ? _dateTime.ServerTime : (DateTime?)null,
                            IsShowHistory = false
                        };
                        _dbContext.Entity<TrHomeroomStudentEnrollment>().Add(newTrHomeroomStudentEnrollment);

                    }

                    var idLessonNew = listLessonNew.Where(e => e.Semester == studentHomeroomEnrollment.HomeroomStudent.Semester
                                                      && e.IdSubject == studentEnrollment.IdSubjectNew
                                                      && e.IdHomeroom == studentHomeroomEnrollment.HomeroomStudent.IdHomeroom)
                                              .FirstOrDefault();

                    if (idLessonNew == null && !string.IsNullOrEmpty(studentEnrollment.IdSubjectNew))
                        continue;

                    newTrHomeroomStudentEnrollment = new TrHomeroomStudentEnrollment
                    {
                        IdTrHomeroomStudentEnrollment = Guid.NewGuid().ToString(),
                        IdHomeroomStudent = studentHomeroomEnrollment.IdHomeroomStudent,
                        IdHomeroomStudentEnrollment = studentHomeroomEnrollment.Id,
                        StartDate = studentEnrollment.EffectiveDate,
                        Note = studentEnrollment.Note,
                        IsDelete = studentEnrollment.IsDelete,
                        IsSendEmail = studentEnrollment.IsSendEmail,
                        IdLessonNew = studentEnrollment.IsDelete ? homeroomStudentEnrollmentOld.idLesson : semester == 2 ? studentEnrollment.IdLessonNew : idLessonNew.Id,
                        IdSubjectNew = studentEnrollment.IsDelete ? homeroomStudentEnrollmentOld.idSubject : studentEnrollment.IdSubjectNew,
                        IdSubjectLevelNew = studentEnrollment.IsDelete ? homeroomStudentEnrollmentOld.idSubjectLevel : studentEnrollment.IdSubjectLevelNew,
                        IdLessonOld = homeroomStudentEnrollmentOld.idLesson,
                        IdSubjectOld = homeroomStudentEnrollmentOld.idSubject,
                        IdSubjectLevelOld = homeroomStudentEnrollmentOld.idSubjectLevel,
                        IsSync = _dateTime.ServerTime.Date >= studentEnrollment.EffectiveDate.Date ? true : (bool?)null,
                        DateSync = _dateTime.ServerTime.Date >= studentEnrollment.EffectiveDate.Date ? _dateTime.ServerTime : (DateTime?)null,
                        IsShowHistory = true
                    };

                    _dbContext.Entity<TrHomeroomStudentEnrollment>().Add(newTrHomeroomStudentEnrollment);

                    ListIdTrHomeroomStudentEnrollment.Add(newTrHomeroomStudentEnrollment.IdTrHomeroomStudentEnrollment);

                    if (_dateTime.ServerTime.Date >= studentEnrollment.EffectiveDate.Date)
                    {
                        var MsHomeroomStundet = _listStudentHomeroomEnrollment.Where(e => e.Id == studentHomeroomEnrollment.Id).FirstOrDefault();

                        if (studentEnrollment.IsDelete)
                        {
                            MsHomeroomStundet.IsActive = false;
                        }
                        else
                        {
                            MsHomeroomStundet.IdLesson = newTrHomeroomStudentEnrollment.IdLessonNew;
                            MsHomeroomStundet.IdSubject = newTrHomeroomStudentEnrollment.IdSubjectNew;
                            MsHomeroomStundet.IdSubjectLevel = newTrHomeroomStudentEnrollment.IdSubjectLevelNew;
                        }

                        _dbContext.Entity<MsHomeroomStudentEnrollment>().Update(MsHomeroomStundet);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2(ListIdTrHomeroomStudentEnrollment as object);
        }
    }
}

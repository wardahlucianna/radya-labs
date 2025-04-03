using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject;
using BinusSchool.Scheduling.FnMovingStudent.MoveStudentSubject.Validator;
using BinusSchool.Scheduling.FnMovingStudent.MoveStudentHomeroom.Validator;
using Microsoft.Azure.Documents.SystemFunctions;
using BinusSchool.Data.Api.Scheduling.FnMovingStudent;
using NPOI.HPSF;
using NPOI.OpenXmlFormats.Spreadsheet;
using BinusSchool.Common.Abstractions;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentHomeroom
{
    public class AddStudentMoveStudentHomeroomHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMoveStudentEnrollment _apiMoveStudentEnrollment;
        private readonly IMachineDateTime _dateTime;
        private readonly IRolePosition _rolePositionService;
        public AddStudentMoveStudentHomeroomHandler(ISchedulingDbContext schoolDbContext, IMoveStudentEnrollment apiMoveStudentEnrollment, IMachineDateTime dateTime, IRolePosition rolePositionService)
        {
            _dbContext = schoolDbContext;
            _apiMoveStudentEnrollment = apiMoveStudentEnrollment;
            _dateTime = dateTime;
            _rolePositionService = rolePositionService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddStudentMoveStudentHomeroomRequest, AddStudentMoveStudentHomeroomValidator>();

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                                 .Include(e => e.Grade).ThenInclude(e => e.Level)
                                 .Where(e => e.Grade.Level.AcademicYear.Id == body.IdAcademicYear)
                                 .ToListAsync(CancellationToken);

            var listHomeroom = await _dbContext.Entity<MsHomeroom>()
                                .Where(e => e.IdAcademicYear == body.IdAcademicYear)
                                .ToListAsync(CancellationToken);

            var queryHomeroomStudent = _dbContext.Entity<MsHomeroomStudent>()
                                .Include(e=>e.Homeroom).ThenInclude(e=>e.Grade)
                                .Where(e => e.Homeroom.IdAcademicYear == body.IdAcademicYear && body.IdStudents.Contains(e.IdStudent));

            var getHomeroomOld = listHomeroom.Where(e => e.Id == body.IdHomeroomOld).FirstOrDefault();
            var getHomeroomNew = listHomeroom.Where(e => e.Id == body.IdHomeroom).FirstOrDefault();
            var queryHomeroomOld = listHomeroom.Where(e => e.IdGradePathwayClassRoom == getHomeroomOld.IdGradePathwayClassRoom);
            var queryHomeroomNew = listHomeroom.Where(e => e.IdGradePathwayClassRoom == getHomeroomNew.IdGradePathwayClassRoom);
           
            var listIdHomeroomOld = queryHomeroomOld.Where(e => e.Semester == body.Semester).Select(e => e.Id).ToList();
            var listIdHomeroomNew = queryHomeroomNew.Where(e => e.Semester == body.Semester).Select(e => e.Id).ToList();
            var listIdHomeroomOldNew = listIdHomeroomOld.Union(listIdHomeroomNew).ToList();

            var listHomeroomStudent = await queryHomeroomStudent.Where(e => e.Homeroom.Semester == body.Semester).ToListAsync(CancellationToken);
            var listHomeroomStudentAll = await queryHomeroomStudent.ToListAsync(CancellationToken);

            if (body.Semester == 2)
                listHomeroomStudentAll = await queryHomeroomStudent.Where(e => e.Homeroom.Semester == body.Semester).ToListAsync(CancellationToken);

            var listIdHomeroomStudent = listHomeroomStudent.Select(e => e.Id).Distinct().ToList();
            var listIdStudent = listHomeroomStudent.Select(e => e.IdStudent).Distinct().ToList();

            var queryLessonSubjectHomeroom = _dbContext.Entity<MsLessonPathway>()
                                                .Include(e => e.HomeroomPathway)
                                                .Include(e => e.Lesson).ThenInclude(e => e.Subject).ThenInclude(e => e.SubjectMappingSubjectLevels)
                                                .Where(e => e.Lesson.IdAcademicYear == body.IdAcademicYear
                                                            && listIdHomeroomNew.Contains(e.HomeroomPathway.IdHomeroom))
                                                .Where(e => e.Lesson.Semester == body.Semester);

            var queryHomeroomStudentEnrollment = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                                                    .Include(e => e.Lesson)
                                                    .Include(e => e.Subject).ThenInclude(e => e.SubjectMappingSubjectLevels)
                                                    .Where(e => e.Lesson.IdAcademicYear == body.IdAcademicYear
                                                                && listIdStudent.Contains(e.HomeroomStudent.IdStudent))
                                                .Where(e => e.Lesson.Semester == body.Semester);

            var listLessonSubjectHomeroom = await queryLessonSubjectHomeroom
                                                .Select(e => new
                                                {
                                                    e.IdLesson,
                                                    e.Lesson.IdSubject,
                                                    IdSubjectLevel = e.Lesson.Subject.SubjectMappingSubjectLevels
                                                                        .Where(f => f.IdSubject == e.Lesson.IdSubject)
                                                                        .Select(f => f.IdSubjectLevel).FirstOrDefault(),
                                                    Subject = e.Lesson.Subject.Description,
                                                    DateCreate = e.Lesson.DateIn,
                                                    Semester = e.Lesson.Semester
                                                })
                                                .Distinct()
                                                .ToListAsync(CancellationToken);

            var listHomeroomStudentEnrollment = await queryHomeroomStudentEnrollment
                                                    .Select(e => new
                                                    {
                                                        e.Id,
                                                        e.IdHomeroomStudent,
                                                        e.HomeroomStudent.Homeroom.IdGrade,
                                                        e.IdLesson,
                                                        e.IdSubject,
                                                        IdSubjectLevel = e.Subject.SubjectMappingSubjectLevels
                                                                            .Where(f => f.IdSubject == e.IdSubject)
                                                                            .Select(f => f.IdSubjectLevel).FirstOrDefault(),
                                                        Semester = e.Lesson.Semester
                                                    })
                                                    .ToListAsync(CancellationToken);

            var listHTrMoveStudentHomeroom = await _dbContext.Entity<HTrMoveStudentHomeroom>()
                                              .Where(e => listIdStudent.Contains(e.HomeroomStudent.IdStudent) && e.HomeroomStudent.Homeroom.IdAcademicYear==body.IdAcademicYear)
                                              .ToListAsync(CancellationToken);

            List<string> ListIdHTrMoveStudentHomeroom = new List<string>();
            foreach (var homeroomStudent in listHomeroomStudent)
            {
                #region Simpan Moving Homeroom
                List<string> _ListIdHTrMoveStudentHomeroom = new List<string>();
                var listHomeroomStudentAllByStudent = listHomeroomStudentAll.Where(e => e.IdStudent == homeroomStudent.IdStudent).ToList();

                foreach (var item in listHomeroomStudentAllByStudent)
                {
                    var idHomeroomOld = queryHomeroomOld.Where(e => e.Semester == item.Semester).Select(e => e.Id).FirstOrDefault();
                    var idHomeroomNew = queryHomeroomNew.Where(e => e.Semester == item.Semester).Select(e => e.Id).FirstOrDefault();
                    var exsisMoving = listHTrMoveStudentHomeroom.Where(e => e.IdHomeroomStudent == item.Id).Any();


                    if (item.Semester == 2 && idHomeroomNew == null)
                    {
                        throw new BadRequestException($"Homeroom semester 2 is not found");
                    }

                    if (!exsisMoving)
                    {
                        var effectiveDate = listPeriod
                                .Where(e => e.IdGrade == homeroomStudent.Homeroom.IdGrade)
                                .Select(e => e.AttendanceStartDate).Min();

                        var newHistoryMoveFirst = new HTrMoveStudentHomeroom
                        {
                            IdHTrMoveStudentHomeroom = Guid.NewGuid().ToString(),
                            IdHomeroomNew = idHomeroomOld,
                            IdHomeroomOld = idHomeroomOld,
                            IdHomeroomStudent = item.Id,
                            StartDate = effectiveDate,
                            IsSendEmail = body.IsSendEmail,
                            Note = body.Note,
                            IsSync = _dateTime.ServerTime.Date >= body.EffectiveDate.Date ? true : (bool?)null,
                            DateSync = _dateTime.ServerTime.Date >= body.EffectiveDate.Date ? _dateTime.ServerTime : (DateTime?)null,
                            IsShowHistory = false,
                        };

                        _dbContext.Entity<HTrMoveStudentHomeroom>().Add(newHistoryMoveFirst);
                        _ListIdHTrMoveStudentHomeroom.Add(newHistoryMoveFirst.IdHTrMoveStudentHomeroom);
                    }

                    var newHistoryMove = new HTrMoveStudentHomeroom
                    {
                        IdHTrMoveStudentHomeroom = Guid.NewGuid().ToString(),
                        IdHomeroomNew = idHomeroomNew,
                        IdHomeroomOld = idHomeroomOld,
                        IdHomeroomStudent = item.Id,
                        StartDate = body.EffectiveDate,
                        IsSendEmail = body.IsSendEmail,
                        Note = body.Note,
                        IsSync = _dateTime.ServerTime.Date >= body.EffectiveDate.Date ? true : (bool?)null,
                        DateSync = _dateTime.ServerTime.Date >= body.EffectiveDate.Date ? _dateTime.ServerTime : (DateTime?)null,
                        IsShowHistory = true
                    };

                    _dbContext.Entity<HTrMoveStudentHomeroom>().Add(newHistoryMove);

                    if (_dateTime.ServerTime.Date >= body.EffectiveDate.Date)
                    {
                        item.IdHomeroom = newHistoryMove.IdHomeroomNew;
                        _dbContext.Entity<MsHomeroomStudent>().Update(item);
                    }

                    ListIdHTrMoveStudentHomeroom.Add(newHistoryMove.IdHTrMoveStudentHomeroom);
                    _ListIdHTrMoveStudentHomeroom.Add(newHistoryMove.IdHTrMoveStudentHomeroom);
                }
                await _dbContext.SaveChangesAsync();
                #endregion

                var listStudentEnrollment = new List<GetMoveStudentEnrollment>();
                var listHomeroomStudentEnrollmentBySmt = listHomeroomStudentEnrollment.Where(e => e.Semester == homeroomStudent.Semester && e.IdHomeroomStudent==homeroomStudent.Id).ToList();
                foreach (var itemHomeroomStudentEnroll in listHomeroomStudentEnrollmentBySmt)
                {
                    var NewLesson = listLessonSubjectHomeroom
                                        .Where(e => e.IdSubject == itemHomeroomStudentEnroll.IdSubject && e.Semester == itemHomeroomStudentEnroll.Semester)
                                        .OrderBy(e => e.DateCreate)
                                        .LastOrDefault();

                    if (NewLesson == null)
                        continue;

                    GetMoveStudentEnrollment newStudentEnrollment = new GetMoveStudentEnrollment
                    {
                        IdHomeroomStudentEnrollment = itemHomeroomStudentEnroll.Id,
                        IdGrade = itemHomeroomStudentEnroll.IdGrade,
                        IdLessonOld = itemHomeroomStudentEnroll.IdLesson,
                        IdSubjectOld = itemHomeroomStudentEnroll.IdSubject,
                        IdSubjectLevelOld = itemHomeroomStudentEnroll.IdSubjectLevel,
                        EffectiveDate = body.EffectiveDate,
                        IdLessonNew = NewLesson.IdLesson,
                        IdSubjectNew = NewLesson.IdSubject,
                        IdSubjectLevelNew = NewLesson.IdSubjectLevel,
                        IsDelete = false,
                        IsSendEmail = body.IsSendEmail,
                        Note = body.Note,
                    };
                    listStudentEnrollment.Add(newStudentEnrollment);
                }

                var param = new AddMoveStudentEnrollmentRequest
                {
                    IdAcademicYear = body.IdAcademicYear,
                    IdHomeroomStudent = homeroomStudent.Id,
                    StudentEnrollment = listStudentEnrollment
                };

                var apiAddMoveStudent = await _apiMoveStudentEnrollment.CreateMoveStudentEnrollment(param);
                if (!apiAddMoveStudent.IsSuccess)
                {
                    var listHTrMoveStudentHomeroomNew = await _dbContext.Entity<HTrMoveStudentHomeroom>()
                                                              .Where(e => _ListIdHTrMoveStudentHomeroom.Contains(e.IdHTrMoveStudentHomeroom))
                                                              .ToListAsync(CancellationToken);

                    listHTrMoveStudentHomeroomNew.ForEach(e=>e.IsActive = false);
                    _dbContext.Entity<HTrMoveStudentHomeroom>().UpdateRange(listHTrMoveStudentHomeroomNew);

                    foreach (var item in listHomeroomStudentAllByStudent)
                    {
                        var idHomeroomOld = queryHomeroomOld.Where(e => e.Semester == item.Semester).Select(e => e.Id).FirstOrDefault();
                        item.IdHomeroom = idHomeroomOld;
                        _dbContext.Entity<MsHomeroomStudent>().Update(item);
                    }

                    ListIdHTrMoveStudentHomeroom = ListIdHTrMoveStudentHomeroom.Where(e => !_ListIdHTrMoveStudentHomeroom.Contains(e)).ToList();
                    await _dbContext.SaveChangesAsync();
                }
            }


            #region Email
            if (body.IsSendEmail)
            {
                var listMoveStudentEntollment = await _dbContext.Entity<HTrMoveStudentHomeroom>()
                      .Include(e => e.HomeroomOld).ThenInclude(e => e.AcademicYear)
                      .Include(e => e.HomeroomOld).ThenInclude(e => e.Grade)
                      .Include(e => e.HomeroomOld).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                      .Include(e => e.HomeroomNew).ThenInclude(e => e.Grade)
                      .Include(e => e.HomeroomNew).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                      .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                      .Where(e => ListIdHTrMoveStudentHomeroom.Contains(e.IdHTrMoveStudentHomeroom))
                      .Select(e => new
                      {
                          AcademicYear = e.HomeroomOld.AcademicYear.Description,
                          Semester = e.HomeroomOld.Semester.ToString(),
                          OldHomeroom = e.HomeroomOld.Grade.Code + e.HomeroomOld.GradePathwayClassroom.Classroom.Code,
                          NewHomeroom = e.HomeroomNew.Grade.Code + e.HomeroomNew.GradePathwayClassroom.Classroom.Code,
                          IdStudent = e.HomeroomStudent.IdStudent,
                          FirstName = e.HomeroomStudent.Student.FirstName,
                          MiddleName = e.HomeroomStudent.Student.MiddleName,
                          LastName = e.HomeroomStudent.Student.LastName,
                          EffectiveDate = e.StartDate.ToString("dd MMM yyyy"),
                          Note = e.Note,
                          ListIdHomeroom = new List<string>() { e.HomeroomOld.Id, e.HomeroomNew.Id },
                          IdSchool = e.HomeroomOld.AcademicYear.IdSchool
                      })
                      .ToListAsync(CancellationToken);

                var listIdHomeroom = new List<string>(new string[]
                {
                body.IdHomeroomOld, body.IdHomeroom
                });

                var listIdUserHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                                .Where(e => listIdHomeroom.Contains(e.IdHomeroom))
                                                .Select(e => e.IdBinusian)
                                                .ToListAsync(CancellationToken);

                var idSchool = listMoveStudentEntollment.Select(e => e.IdSchool).FirstOrDefault();

                var listEmailRecepient = await _dbContext.Entity<MsEmailRecepient>()
                                                .Include(e => e.Role).ThenInclude(e => e.RoleGroup)
                                                .Where(e => e.Type == TypeEmailRecepient.MovingStudentHomeroom && e.IsCC && e.Role.IdSchool == idSchool)
                                                .ToListAsync(CancellationToken);

                var emailRecepients = listEmailRecepient
                                        .Select(e => new GetUserEmailRecepient
                                        {
                                            IdRole = e.IdRole,
                                            IdTeacherPosition = e.IdTeacherPosition,
                                            IdUser = e.IdBinusian
                                        })
                                        .ToList();

                var getUserSubjectByEmailRecepientService = await _rolePositionService.GetUserSubjectByEmailRecepient(new GetUserSubjectByEmailRecepientRequest
                {
                    IdAcademicYear = body.IdAcademicYear,
                    IdSchool = idSchool,
                    IsShowIdUser = true,
                    EmailRecepients = emailRecepients
                });

                var getUserSubjectByEmailRecepient = getUserSubjectByEmailRecepientService.IsSuccess ? getUserSubjectByEmailRecepientService.Payload : null;

                var listIdUserCC = getUserSubjectByEmailRecepient != null
                                    ? getUserSubjectByEmailRecepient.Select(e => e.IdUser).Distinct().ToList()
                                    : new List<string>();

                if (listEmailRecepient.Where(e => e.Role.RoleGroup.Code == RoleConstant.Student).Any())
                    listIdUserCC.AddRange(listMoveStudentEntollment.Select(e => e.IdStudent).Distinct().ToList());

                var listENS8MoveHomeroom = listMoveStudentEntollment
                                            .GroupBy(e => new
                                            {
                                                AcademicYear = e.AcademicYear,
                                                Semester = e.Semester,
                                                OldHomeroom = e.OldHomeroom,
                                                NewHomeroom = e.NewHomeroom,
                                                IdStudent = e.IdStudent,
                                                FirstName = e.FirstName,
                                                MiddleName = e.MiddleName,
                                                LastName = e.LastName,
                                                EffectiveDate = e.EffectiveDate,
                                                Notes = e.Note,
                                            })
                                            .Select(e => new ENS8MoveHomeroom
                                            {
                                                AcademicYear = e.Key.AcademicYear,
                                                Semester = e.Key.Semester,
                                                OldHomeroom = e.Key.OldHomeroom,
                                                NewHomeroom = e.Key.NewHomeroom,
                                                StudentName = NameUtil.GenerateFullNameWithId(e.Key.IdStudent, e.Key.FirstName, e.Key.MiddleName, e.Key.LastName),
                                                EffectiveDate = e.Key.EffectiveDate,
                                                Notes = e.Key.Notes,
                                            }).ToList();

                var ens8Result = new ENS8Result
                {
                    MoveHomeroom = listENS8MoveHomeroom,
                    IdUserRecepient = listIdUserHomeroomTeacher,
                    IdUserCC = listIdUserCC.Distinct().ToList()
                };

                var Notification = ENS8Notification(KeyValues, AuthInfo, ens8Result);
            }

            #endregion
            return Request.CreateApiResult2();
        }

        public string ENS8Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, ENS8Result ens8Result)
        {
            if (KeyValues.ContainsKey("listMoveHomeroom"))
            {
                KeyValues.Remove("listMoveHomeroom");
            }
            KeyValues.Add("listMoveHomeroom", ens8Result);

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ENS8")
                {
                    IdRecipients = ens8Result.IdUserRecepient,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }

    
}

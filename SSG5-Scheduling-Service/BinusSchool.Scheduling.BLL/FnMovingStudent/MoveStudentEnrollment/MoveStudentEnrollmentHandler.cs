using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Api.Scheduling.FnMovingStudent;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.EmailRecepient;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class MoveStudentEnrollmentHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IEmailRecepient _emailRecepient;
        private readonly IMoveStudentEnrollment _apiMoveStudentEnrollment;

        public MoveStudentEnrollmentHandler(ISchedulingDbContext schedulingDbContext, IMachineDateTime DateTime, IEmailRecepient EmailRecepient, IMoveStudentEnrollment apiMoveStudentEnrollment)
        {
            _dbContext = schedulingDbContext;
            _dateTime = DateTime;
            _emailRecepient = EmailRecepient;
            _apiMoveStudentEnrollment = apiMoveStudentEnrollment;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var HomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                     .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                     .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                     .Include(e => e.Student)
                     .Where(e => e.Id == id)
                     .Select(e => new 
                     {
                         academicYear = e.Homeroom.AcademicYear.Description,
                         semester = e.Homeroom.Semester,
                         studentId = e.IdStudent,
                         studentName = NameUtil.GenerateFullName(e.Student.FirstName, e.Student.MiddleName, e.Student.LastName),
                         homeroom = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code,
                         idHomeroom = e.IdHomeroom,
                         idGrade = e.Homeroom.IdGrade,
                         idAcademicYear = e.Homeroom.IdAcademicYear
                     }).FirstOrDefaultAsync(CancellationToken);

            var registeredDate = await _dbContext.Entity<MsPeriod>()
                    .Where(e => e.IdGrade == HomeroomStudent.idGrade)
                    .Select(e => e.AttendanceStartDate)
                    .MinAsync(CancellationToken);

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                    .Include(e => e.Subject)
                                    .Include(e => e.Lesson).ThenInclude(e => e.LessonTeachers).ThenInclude(e=> e.Staff)
                                    .Include(e => e.SubjectLevel)
                                    .Where(e => e.IdHomeroomStudent == id)
                                    .Select(e => new GetMovingStudentEnrollment
                                    {
                                        idhomeroomStudentEnrollment = e.Id,
                                        idLesson = e.IdLesson,
                                        idSubject = e.IdSubject,
                                        idSubjectLevel = e.IdSubjectLevel,
                                        subjectName = e.Subject.Description + " - " + e.Lesson.LessonTeachers.Where(x=> x.IsPrimary).Select(e => e.Staff.FirstName == null ? e.Staff.LastName.Trim() : e.Staff.FirstName.Trim() + " " + e.Staff.LastName.Trim()).FirstOrDefault() + " - " + e.Lesson.ClassIdGenerated + (e.IdSubjectLevel != null ? $" ({e.SubjectLevel.Code.Trim()})" : ""),
                                        RegisteredDate = registeredDate,
                                    })
                                    .ToListAsync(CancellationToken);

            var listTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                                                        .Include(e => e.SubjectNew)
                                                        .Include(e => e.LessonNew).ThenInclude(e => e.LessonTeachers).ThenInclude(e => e.Staff)
                                                        .Where(e => e.IdHomeroomStudent == id && e.IsShowHistory == true)
                                                        .Select(e => new
                                                        {
                                                            id = e.IdHomeroomStudentEnrollment,
                                                            registeredDate = e.StartDate,
                                                            idSubject = e.IdSubjectNew,
                                                            isDelete = e.IsDelete,
                                                            idLesson = e.IdLessonNew,
                                                            SubjectName = e.SubjectNew.Description + " - " + e.LessonNew.LessonTeachers.Where(x => x.IsPrimary).Select(e => e.Staff.FirstName == null ? e.Staff.LastName.Trim() : e.Staff.FirstName.Trim() + " " + e.Staff.LastName.Trim()).FirstOrDefault() + " - " + e.LessonNew.ClassIdGenerated + (e.IdSubjectLevelNew != null ? $" ({e.SubjectLevelNew.Code.Trim()})" : ""),
                                                            idSubjectLevel = e.IdSubjectLevelNew,
                                                            idHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                                                            Datein = e.DateIn
                                                        })
                                                        .ToListAsync(CancellationToken);

            var listIdStudentEnrollment = listHomeroomStudentEnrollment
                                            .Select(e => new { e.idhomeroomStudentEnrollment,e.idSubjectLevel })
                                            .ToList();

            var listSubjectLevel = await _dbContext.Entity<MsSubjectMappingSubjectLevel>()
                                                      .Include(e => e.SubjectLevel)
                                                      .Where(e => e.Subject.IdGrade == HomeroomStudent.idGrade)
                                                      .ToListAsync(CancellationToken);

            List<GetMovingStudentEnrollment> listHomeroomStudentEnrollmentNew = new List<GetMovingStudentEnrollment>();
            foreach (var StudentEnrollment in listIdStudentEnrollment)
            {
                var homeroomStudentEnrollment = listHomeroomStudentEnrollment.Where(e => e.idhomeroomStudentEnrollment == StudentEnrollment.idhomeroomStudentEnrollment).FirstOrDefault();
                var TrHomeroomStudentEnrollment = listTrHomeroomStudentEnrollment
                                                   .Where(e => e.idHomeroomStudentEnrollment == StudentEnrollment.idhomeroomStudentEnrollment)
                                                   .OrderBy(e => e.registeredDate).OrderBy(e => e.Datein)
                                                   .LastOrDefault();

                var idSubjectLevelFromTr = StudentEnrollment.idSubjectLevel;
                if (TrHomeroomStudentEnrollment != null)
                {
                    if (TrHomeroomStudentEnrollment.isDelete)
                        continue;

                    homeroomStudentEnrollment.idLesson = TrHomeroomStudentEnrollment.idLesson;
                    homeroomStudentEnrollment.idSubject = TrHomeroomStudentEnrollment.idSubject;
                    homeroomStudentEnrollment.idSubjectLevel = TrHomeroomStudentEnrollment.idSubjectLevel;
                    homeroomStudentEnrollment.subjectName = TrHomeroomStudentEnrollment.SubjectName;
                    homeroomStudentEnrollment.RegisteredDate = TrHomeroomStudentEnrollment.registeredDate;
                    idSubjectLevelFromTr = TrHomeroomStudentEnrollment.idSubjectLevel;
                }

                homeroomStudentEnrollment.subjectLevel = listSubjectLevel.Where(e => e.IdSubject == homeroomStudentEnrollment.idSubject)
                                    .Select(e => new ItemValueVm
                                    {
                                        Id = e.SubjectLevel.Id,
                                        Description = e.SubjectLevel.Description,
                                    }).ToList();

                homeroomStudentEnrollment.idSubjectLevel = homeroomStudentEnrollment.subjectLevel.Any() 
                                                            ? !string.IsNullOrEmpty(idSubjectLevelFromTr) ? homeroomStudentEnrollment.subjectLevel.Where(e=> e.Id == idSubjectLevelFromTr).Select(e => e.Id).FirstOrDefault() : homeroomStudentEnrollment.subjectLevel.Select(e => e.Id).FirstOrDefault()
                                                            : null;
                listHomeroomStudentEnrollmentNew.Add(homeroomStudentEnrollment);
            }

            var item = new GetDetailMoveStudentEnrollmentResult
            {
                IdHomeroomStudent = id,
                AcademicYear = HomeroomStudent.academicYear,
                Semester = HomeroomStudent.semester,
                Homeroom = HomeroomStudent.homeroom,
                StudentId = HomeroomStudent.studentId,
                StudentName = HomeroomStudent.studentName,
                IdHomeroom = HomeroomStudent.idHomeroom,
                IsShowSubjectLevel = true,
                IdAcademicYear = HomeroomStudent.idAcademicYear,
                IdGrade = HomeroomStudent.idGrade,
                MovingStudentEnrollment= listHomeroomStudentEnrollmentNew
            };

            return Request.CreateApiResult2(item as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetMoveStudentEnrollmentRequest>();
            var predicate = PredicateBuilder.Create<MsHomeroomStudent>(x => x.Homeroom.IdAcademicYear == param.idAcademicYear
                                                                            && x.Homeroom.Semester == param.semester);
            string[] _columns = { "studentName", "homeroom" };

            var queryHomeroomStudent = _dbContext.Entity<MsHomeroomStudent>()
                        .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                        .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                        .Include(e => e.Student)
                        .Where(predicate);

            if (!string.IsNullOrEmpty(param.idLevel))
                queryHomeroomStudent = queryHomeroomStudent.Where(e => e.Homeroom.Grade.IdLevel == param.idLevel);
            if (!string.IsNullOrEmpty(param.idGrade))
                queryHomeroomStudent = queryHomeroomStudent.Where(e => e.Homeroom.IdGrade == param.idGrade);
            if (!string.IsNullOrEmpty(param.idHomeroom))
                queryHomeroomStudent = queryHomeroomStudent.Where(e => e.Homeroom.Id == param.idHomeroom);

            var listStudent = await queryHomeroomStudent
                                .Select(e => new GetMoveStudentEnrollmentResult
                                {
                                    idHomeroomStudent = e.Id,
                                    studentName = $" {NameUtil.GenerateFullName(e.Student.FirstName, e.Student.MiddleName, e.Student.LastName)} - {e.IdStudent}",
                                    homeroom = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code
                                }).ToListAsync(CancellationToken);

            var query = listStudent.Distinct();

            if (!string.IsNullOrEmpty(param.studentName))
                query = query.Where(e => e.studentName.ToLower().Contains(param.studentName.ToLower()));
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(e => e.studentName.ToLower().Contains(param.Search.ToLower()) || e.homeroom.ToLower().Contains(param.Search.ToLower()));

            //ordering
            switch (param.OrderBy)
            {
                case "studentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.studentName)
                        : query.OrderBy(x => x.studentName);
                    break;

                case "homeroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.homeroom)
                        : query.OrderBy(x => x.homeroom);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();

                items = result.ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddMoveStudentEnrollmentRequest, AddMoveStudentValidator>();

            var apiAddMoving = await _apiMoveStudentEnrollment.CreateMoveStudentEnrollment(body);

            if (apiAddMoving.IsSuccess)
            {
                #region send email
                List<string> listIdSubject = new List<string>();
                List<GetMoveStudentEnrollmentSubjectTeacher> listSubjectTeacher = new List<GetMoveStudentEnrollmentSubjectTeacher>();

                foreach (var item in apiAddMoving.Payload)
                {
                    var listEmail = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.AcademicYear)
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                .Include(e => e.SubjectNew)
                                .Include(e => e.LessonNew)
                                .Include(e => e.SubjectOld)
                                .Include(e => e.LessonOld)
                                .Include(e => e.SubjectLevelNew)
                                .Include(e => e.SubjectLevelOld)
                                .Where(e => e.IdTrHomeroomStudentEnrollment == item && e.IsSendEmail && e.IsShowHistory)
                                //.Where(e => ListIdTrHomeroomStudentEnrollment.Contains(e.IdTrHomeroomStudentEnrollment) && e.IsSendEmail)
                                .ToListAsync(CancellationToken);

                    listIdSubject.AddRange(listEmail.Select(e => e.IdSubjectNew).ToList());
                    listIdSubject.AddRange(listEmail.Select(e => e.IdSubjectOld).ToList());

                    var listSubjectTeacherUser = await _dbContext.Entity<MsLessonTeacher>()
                                    .Where(e => listIdSubject.Contains(e.Lesson.IdSubject))
                                    .GroupBy(e => new
                                    {
                                        e.Lesson.Id,
                                        e.Lesson.IdSubject,
                                        e.IdUser
                                    })
                                    .Select(e => new GetMoveStudentEnrollmentSubjectTeacher
                                    {
                                        idLesson = e.Key.Id,
                                        idSubject = e.Key.IdSubject,
                                        idUserTeacher = e.Key.IdUser
                                    })
                                    .ToListAsync(CancellationToken);

                    //var getEmailRecepient = await _emailRecepient.GetEmailBccAndTos(new GetEmailBccAndTosRequest
                    //{
                    //    Type = TypeEmailRecepient.MovingStudentEnrollment
                    //});
                    var idSchool = listEmail.FirstOrDefault()?.HomeroomStudent.Student.IdSchool;

                    var EmailRecepient = new GetEmailBccAndTosResult();

                    if (listEmail.Count > 0)
                        EmailRecepient = GetEmailUser(TypeEmailRecepient.MovingStudentEnrollment, _dbContext, idSchool);

                    //var EmailRecepient = getEmailRecepient.Payload;

                    #region move student enrollment
                    var listEmailMoveStudent = listEmail.Where(e => !e.IsDelete).ToList();

                    foreach (var moveStudent in listEmailMoveStudent)
                    {
                        List<string> listIdUser = new List<string>();
                        listIdUser.AddRange(listSubjectTeacherUser.Where(e => e.idSubject == moveStudent.IdSubjectOld && e.idLesson == moveStudent.IdLessonOld).Select(e => e.idUserTeacher).Distinct().ToList());
                        listIdUser.AddRange(listSubjectTeacherUser.Where(e => e.idSubject == moveStudent.IdSubjectNew && e.idLesson == moveStudent.IdLessonNew).Select(e => e.idUserTeacher).Distinct().ToList());

                        var EmailMoveStudentEnrollment = new EmailMoveStudentEnrollmentRequest
                        {
                            academicYear = moveStudent.HomeroomStudent.Homeroom.AcademicYear.Description,
                            semester = moveStudent.HomeroomStudent.Homeroom.Semester,
                            homeRoom = $"{moveStudent.HomeroomStudent.Homeroom.Grade.Code}{moveStudent.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code}",
                            studentId = moveStudent.HomeroomStudent.IdStudent,
                            studentName = NameUtil.GenerateFullName(moveStudent.HomeroomStudent.Student.FirstName, moveStudent.HomeroomStudent.Student.MiddleName, moveStudent.HomeroomStudent.Student.LastName),
                            newSubject = moveStudent.SubjectNew == null ? null : $"{moveStudent.SubjectNew.Description}-{moveStudent.LessonNew.ClassIdGenerated}",
                            oldSubject = moveStudent.SubjectOld == null ? null : $"{moveStudent.SubjectOld.Description}-{moveStudent.LessonOld.ClassIdGenerated}",
                            newSubjectLevel = moveStudent.SubjectLevelNew == null ? null : moveStudent.SubjectLevelNew.Description,
                            oldSubjectLevel = moveStudent.SubjectLevelOld == null ? null : moveStudent.SubjectLevelOld.Description,
                            effectiveDate = Convert.ToDateTime(moveStudent.StartDate).ToString("dd MMM yyyy"),
                            note = moveStudent.Note,
                            IdUserTeacher = listIdUser,
                            IdUserCc = EmailRecepient.Bcc
                        };

                        if (KeyValues.ContainsKey("EmailMoveStudentEnrollment"))
                        {
                            KeyValues.Remove("EmailMoveStudentEnrollment");
                        }
                        KeyValues.Add("EmailMoveStudentEnrollment", EmailMoveStudentEnrollment);
                        var Notification = EMS1Notification(KeyValues, AuthInfo);

                    }
                    #endregion

                    #region delete student enrollment
                    var listEmailMoveStudentDelete = listEmail.Where(e => e.IsDelete).ToList();

                    foreach (var moveStudent in listEmailMoveStudentDelete)
                    {
                        List<string> listIdUser = new List<string>();
                        listIdUser.AddRange(listSubjectTeacherUser.Where(e => e.idSubject == moveStudent.IdSubjectOld && e.idLesson == moveStudent.IdLessonOld).Select(e => e.idUserTeacher).ToList());
                        //listIdUser.AddRange(listSubjectTeacherUser.Where(e => e.idSubject == moveStudent.IdSubjectNew).Select(e => e.idUserTeacher).ToList());

                        var EmailMoveStudentEnrollment = new EmailMoveStudentEnrollmentRequest
                        {
                            academicYear = moveStudent.HomeroomStudent.Homeroom.AcademicYear.Description,
                            semester = moveStudent.HomeroomStudent.Homeroom.Semester,
                            homeRoom = $"{moveStudent.HomeroomStudent.Homeroom.Grade.Code}{moveStudent.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code}",
                            studentId = moveStudent.HomeroomStudent.IdStudent,
                            studentName = NameUtil.GenerateFullName(moveStudent.HomeroomStudent.Student.FirstName, moveStudent.HomeroomStudent.Student.MiddleName, moveStudent.HomeroomStudent.Student.LastName),
                            oldSubject = moveStudent.SubjectOld == null ? null : $"{moveStudent.SubjectOld.Description}-{moveStudent.LessonOld.ClassIdGenerated}",
                            oldSubjectLevel = moveStudent.SubjectLevelOld == null ? null : moveStudent.SubjectLevelOld.Description,
                            effectiveDate = Convert.ToDateTime(moveStudent.StartDate).ToString("dd MMM yyyy"),
                            note = moveStudent.Note,
                            IdUserTeacher = listIdUser,
                            IdUserCc = EmailRecepient.Bcc
                        };

                        if (KeyValues.ContainsKey("EmailMoveStudentEnrollment"))
                        {
                            KeyValues.Remove("EmailMoveStudentEnrollment");
                        }
                        KeyValues.Add("EmailMoveStudentEnrollment", EmailMoveStudentEnrollment);
                        var Notification = EMS2Notification(KeyValues, AuthInfo);
                    }
                    #endregion
                }
                #endregion
            }

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<MoveStudentEnrollmentSyncRequest, GetMoveStudentEnrollmentSyncValidator>();

            var idAcademicYear = await _dbContext.Entity<MsPeriod>()
                                    .Include(e => e.Grade).ThenInclude(e => e.Level)
                                    .Where(e => e.Grade.Level.AcademicYear.IdSchool == body.idSchool
                                            && (e.StartDate.Date<=body.Date))
                                    .OrderBy(e=>e.StartDate)
                                    .Select(e => e.Grade.Level.IdAcademicYear)
                                    .LastOrDefaultAsync(CancellationToken);

            var listTrHomeroomStudent = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                                                .Where(e => e.HomeroomStudent.Homeroom.IdAcademicYear == idAcademicYear
                                                        && body.Date >= e.StartDate.Date
                                                        && e.IsShowHistory==true
                                                        )
                                                 .OrderBy(e => e.StartDate).ThenBy(e => e.DateIn)
                                                .ToListAsync(CancellationToken);

            var listTrIdHomeroomStudentEnrollment = listTrHomeroomStudent.Select(e => e.IdTrHomeroomStudentEnrollment).Distinct().ToList();
            var listIdHomeroomStudentEnrollment = listTrHomeroomStudent.Select(e => e.IdHomeroomStudentEnrollment).Distinct().ToList();

            var listMsHomeroomStudent = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                        .Where(e => e.HomeroomStudent.Homeroom.IdAcademicYear == idAcademicYear
                                                && listIdHomeroomStudentEnrollment.Contains(e.Id))
                                        .ToListAsync(CancellationToken);

            listIdHomeroomStudentEnrollment = listMsHomeroomStudent.Select(e => e.Id).ToList();

            foreach (var IdHomeroomStudentEnrollment in listIdHomeroomStudentEnrollment)
            {
                var item = listTrHomeroomStudent.Where(e => e.IdHomeroomStudentEnrollment == IdHomeroomStudentEnrollment)
                            .OrderBy(e => e.StartDate).ThenBy(e => e.DateIn).LastOrDefault();

                if (item.IsSync==true || item==null)
                    continue;

                var MsHomeroomStundet = listMsHomeroomStudent.Where(e => e.Id == IdHomeroomStudentEnrollment).FirstOrDefault();

                if (MsHomeroomStundet == null)
                    continue;

                if (item.IsDelete)
                {
                    MsHomeroomStundet.IsActive = false;
                }
                else
                {
                    MsHomeroomStundet.IdLesson = item.IdLessonNew;
                    MsHomeroomStundet.IdSubject = item.IdSubjectNew;
                    MsHomeroomStundet.IdSubjectLevel = item.IdSubjectLevelNew;
                }

                _dbContext.Entity<MsHomeroomStudentEnrollment>().Update(MsHomeroomStundet);

                item.IsSync = true;
                item.DateSync = _dateTime.ServerTime;
                _dbContext.Entity<TrHomeroomStudentEnrollment>().Update(item);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        public static string EMS1Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailMoveStudentEnrollment").Value;
            var EmailMoveStudentEnrollment = JsonConvert.DeserializeObject<EmailMoveStudentEnrollmentRequest>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "EMS1")
                {
                    IdRecipients = EmailMoveStudentEnrollment.IdUserTeacher,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string EMS2Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailMoveStudentEnrollment").Value;
            var EmailMoveStudentEnrollment = JsonConvert.DeserializeObject<EmailMoveStudentEnrollmentRequest>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "EMS2")
                {
                    IdRecipients = EmailMoveStudentEnrollment.IdUserTeacher,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static GetEmailBccAndTosResult GetEmailUser(TypeEmailRecepient type, ISchedulingDbContext _dbContext, string idSchool)
        {
            var currentAy = _dbContext.Entity<MsAcademicYear>().OrderByDescending(x => x.Code).FirstOrDefault(x => x.IdSchool == idSchool).Id;

            var getEmailRecepient = _dbContext.Entity<MsEmailRecepient>()
                                        .Where(e => e.Type == type)
                                        .ToList();

            var userSchool = _dbContext.Entity<MsUser>()
                .Include(x => x.UserSchools)
                .Where(x => getEmailRecepient.Select(x => x.IdBinusian).Contains(x.Id) && x.UserSchools.Any(a => a.IdSchool == idSchool))
                .Select(x => x.Id)
                .ToList();

            getEmailRecepient = getEmailRecepient.Where(x => userSchool.Contains(x.IdBinusian)).ToList();

            var listIdTecaherPosition = getEmailRecepient.Where(e => e.IdTeacherPosition != null).Select(e => e.IdTeacherPosition).ToList();
            var listIdRole = getEmailRecepient.Where(e => e.IdTeacherPosition != null).ToList();

            var getNonTeachingLoad = _dbContext.Entity<TrNonTeachingLoad>()
                                        .Include(e => e.MsNonTeachingLoad)
                                        .Where(e => listIdTecaherPosition.Contains(e.MsNonTeachingLoad.IdTeacherPosition) && e.MsNonTeachingLoad.IdAcademicYear == currentAy)
                                        .ToList();

            var msUser = _dbContext.Entity<MsUser>()
                                .Include(x => x.UserSchools)
                                .Include(x => x.UserRoles)
                                    .ThenInclude(x => x.Role)
                                .Where(x => x.UserRoles.Any(y => y.Role.IdSchool == idSchool))
                                .ToList();

            var msTeacherPosition = _dbContext.Entity<MsTeacherPosition>()
                                .Include(x => x.Position)
                                .Where(x => x.IdSchool == idSchool)
                                .ToList();

            var msHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
                                    .Where(x => x.TeacherPosition.School.Id == idSchool)
                                    .ToList();

            var msLessonTeacher = _dbContext.Entity<MsLessonTeacher>().ToList();

            #region Tos
            var getEmailRecepientTos = getEmailRecepient.Where(e => !e.IsCC).ToList();
            var listIdTecaherPositionTos = getEmailRecepientTos.Where(e => e.IdTeacherPosition != null).Select(e => e.IdTeacherPosition).ToList();
            var getNonTeachingLoadTos = getNonTeachingLoad.Where(e => listIdTecaherPositionTos.Contains(e.MsNonTeachingLoad.IdTeacherPosition)).ToList();
            var idUserTos = GetIdUser(idSchool, getEmailRecepientTos, getNonTeachingLoadTos, msUser, msTeacherPosition, msHomeroomTeacher, msLessonTeacher);
            #endregion

            #region BCC
            var getEmailRecepientBcc = getEmailRecepient.Where(e => e.IsCC).ToList();
            var listIdTecaherPositionBcc = getEmailRecepientBcc.Where(e => e.IdTeacherPosition != null).Select(e => e.IdTeacherPosition).ToList();
            var getNonTeachingLoadBcc = getNonTeachingLoad.Where(e => listIdTecaherPositionBcc.Contains(e.MsNonTeachingLoad.IdTeacherPosition)).ToList();
            var idUserBcc = GetIdUser(idSchool, getEmailRecepientBcc, getNonTeachingLoadBcc, msUser, msTeacherPosition, msHomeroomTeacher, msLessonTeacher);
            #endregion

            var result = new GetEmailBccAndTosResult
            {
                Tos = idUserTos,
                Bcc = idUserBcc
            };

            return result;
        }
        public static List<string> GetIdUser(string idSchool, List<MsEmailRecepient> emailRecepient, List<TrNonTeachingLoad> nonTeachingLoads, List<MsUser> users, List<MsTeacherPosition> teacherPositions, List<MsHomeroomTeacher> homeroomTeacher, List<MsLessonTeacher> lessonTeachers)
        {
            var listIdUser = new List<string>();

            foreach (var item in emailRecepient)
            {
                if (item.IdTeacherPosition != null)
                {
                    //checking from idRole 
                    var user = users.Where(x => x.UserRoles.Any(x => x.Role.Id == item.IdRole)).ToList();

                    if (user.Where(x => x.UserRoles.Any(y => y.Role.IdRoleGroup == "STF")).Any() || user.Where(x => x.UserRoles.Any(y => y.Role.IdRoleGroup == "ADM")).Any() || user.Where(x => x.UserRoles.Any(y => y.Role.IdRoleGroup == "STD")).Any())
                    {
                        var userSTF = user.Where(x => x.UserRoles.Any(x => x.Role.IdRoleGroup == "STF")).Any() ?
                        user.Where(x => x.UserRoles.Any(x => x.Role.IdRoleGroup == "STF")).ToList() :
                        user.Where(x => x.UserRoles.Any(x => x.Role.IdRoleGroup == "ADM")).Any() ?
                        user.Where(x => x.UserRoles.Any(x => x.Role.IdRoleGroup == "ADM")).ToList() :
                        user.Where(x => x.UserRoles.Any(x => x.Role.IdRoleGroup == "STD")).ToList();

                        listIdUser.AddRange(userSTF.Select(x => x.Id));
                    }
                    else
                    {
                        var teacherPosition = teacherPositions.Where(x => x.IdSchool == idSchool && x.Id == item.IdTeacherPosition).ToList();
                        foreach (var itemTeacherPosition in teacherPosition)
                        {
                            //if checking idRoleGroup = ST
                            if (itemTeacherPosition.Position.Code == "ST")
                            {
                                var listUserST = user.Where(x => x.UserRoles.Any(x => x.Role.IdRoleGroup.Contains("ST") && x.Role.Id == item.IdRole)).Select(x => x.Id).ToList();
                                var listUserLesson = lessonTeachers.Where(x => listUserST.Contains(x.IdUser)).Select(x => x.IdUser).ToList();
                                listIdUser.AddRange(listUserLesson);

                            }
                            //checking idRoleGroup = CA || COT
                            else if (itemTeacherPosition.Position.Code == "CA" || itemTeacherPosition.Position.Code == "COT")
                            {
                                listIdUser.AddRange(homeroomTeacher.Where(x => x.IdTeacherPosition == item.IdTeacherPosition).Select(x => x.IdBinusian).ToList());
                            }
                            else if (itemTeacherPosition.Position.Code == "SH")
                            {
                                listIdUser.AddRange(users.Select(x => x.Id).ToList());
                            }
                            else
                            {
                                listIdUser.AddRange(nonTeachingLoads.Where(e => e.MsNonTeachingLoad.IdTeacherPosition == item.IdTeacherPosition).Select(e => e.IdUser).ToList());
                            }
                        }
                    }
                }

                if (item.IdBinusian != null)
                {
                    listIdUser.Add(item.IdBinusian);
                }
            }

            return listIdUser;
        }
    }
}

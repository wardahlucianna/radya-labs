using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnMovingStudent;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.EmailRecepient;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Scheduling.FnMovingStudent.StudentProgramme.Validator;
using FluentEmail.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.Scheduling.FnMovingStudent.StudentProgramme
{
    public class StudentProgrammeHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IEmailRecepient _emailRecepient;
        public StudentProgrammeHandler(ISchedulingDbContext schedulingDbContext, IMachineDateTime DateTime, IEmailRecepient EmailRecepient)
        {
            _dbContext = schedulingDbContext;
            _dateTime = DateTime;
            _emailRecepient = EmailRecepient;
        }
        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var studentProgramme = await _dbContext.Entity<TrStudentProgramme>()
                                    .Include(e=>e.Student)
                                      .Where(e => e.Id == id)
                                      .FirstOrDefaultAsync(CancellationToken);

            if (studentProgramme == null)
                throw new BadRequestException("Student programme is not found");

            var item = new GetDetailStudentProgrammeResult
            {
                id = studentProgramme.Id,
                idStudent = studentProgramme.IdStudent,
                studentName = NameUtil.GenerateFullName(studentProgramme.Student.FirstName, studentProgramme.Student.LastName),
                programme = studentProgramme.Programme,
            };

            return Request.CreateApiResult2(item as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetStudentProgrammeRequest>();
            var predicate = PredicateBuilder.Create<TrStudentProgramme>(x => true);
            string[] _columns = { "studentId", "studentName", "homeroom", "programme", "lastSaved" };

            var listStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                .Where(e => e.Homeroom.Grade.Level.IdAcademicYear == param.idAcademicYear
                                        && (e.Homeroom.Grade.Code.Contains("11") || e.Homeroom.Grade.Code.Contains("12")))
                                .OrderBy(e => e.Semester)
                                .Select(e => new
                                {
                                    idStudent = e.IdStudent,
                                    homeroom = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code
                                })
                                .ToListAsync(CancellationToken);

            var idSchool = await _dbContext.Entity<MsAcademicYear>()
                               .Where(e => e.Id == param.idAcademicYear)
                               .Select(e => e.IdSchool)
                               .FirstOrDefaultAsync(CancellationToken);

            if (idSchool == null)
                throw new BadRequestException("school is not found");

            var listIdStudent = listStudent.Select(e => e.idStudent).ToList();

            var listProgramme = await _dbContext.Entity<TrStudentProgramme>()
                        .Include(e => e.Student)
                        .Where(e => listIdStudent.Contains(e.IdStudent))
                        .Select(e => new GetStudentProgrammeResult
                        {
                            Id = e.Id,
                            idStudent = e.IdStudent,
                            studentName = NameUtil.GenerateFullName(e.Student.FirstName, e.Student.MiddleName, e.Student.LastName),
                            lastSaved = e.DateUp == null ? e.DateIn : e.DateUp,
                            programme = e.Programme.GetDescription()
                        })
                        .ToListAsync(CancellationToken);

            listProgramme.ForEach(e => e.homeroom = listStudent.Where(f => f.idStudent == e.idStudent).Select(e => e.homeroom).LastOrDefault());

            var query = listProgramme.Distinct();

            if (!string.IsNullOrEmpty(param.programme))
                query = query.Where(x => x.programme == param.programme);
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.studentName.ToLower().Contains(param.Search.ToLower()) || x.programme.ToLower().Contains(param.Search.ToLower()));

            //ordering
            switch (param.OrderBy)
            {
                case "studentId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.idStudent)
                        : query.OrderBy(x => x.idStudent);
                    break;

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
                case "programme":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.programme)
                        : query.OrderBy(x => x.programme);
                    break;
                case "lastSaved":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.lastSaved)
                        : query.OrderBy(x => x.lastSaved);
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
            var body = await Request.ValidateBody<AddStudentProgrammeRequest, AddStudentProgrammeValidator>();

            var exsisStudentProgramme = await _dbContext.Entity<TrStudentProgramme>()
                                        .Where(e => body.idstudent.Contains(e.IdStudent))
                                        .AnyAsync(CancellationToken);

            if (exsisStudentProgramme)
                throw new BadRequestException("id student exsis in student programe");

            foreach (var idstudent in body.idstudent)
            {
                var addStudentProgramme = new TrStudentProgramme
                {
                    Id = Guid.NewGuid().ToString(),
                    IdSchool = body.idSchool,
                    Programme = body.Programme,
                    IdStudent = idstudent,
                };
                _dbContext.Entity<TrStudentProgramme>().Add(addStudentProgramme);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateStudentProgrammeRequest, UpdateStudentProgrammeValidator>();

            var studentProgramme = await _dbContext.Entity<TrStudentProgramme>()
                                        .Where(e => e.Id == body.id)
                                        .FirstOrDefaultAsync(CancellationToken);
            
            if (studentProgramme==null)
                throw new BadRequestException("Student program is not found");

            var addHistoryStudentProgramme = new HTrStudentProgramme
            {
                IdHTrStudentProgramme = Guid.NewGuid().ToString(),
                IdStudentProgramme = studentProgramme.Id,
                IdSchool = studentProgramme.IdSchool,
                ProgrammeOld = studentProgramme.Programme,
                ProgrammeNew = body.Programme,
                IdStudent = studentProgramme.IdStudent,
                IsSendEmail = body.isSendEmail,
                StartDate = body.effectiveDate,
            };
            _dbContext.Entity<HTrStudentProgramme>().Add(addHistoryStudentProgramme);
            
            studentProgramme.StartDate = body.effectiveDate;
            studentProgramme.Programme = body.Programme;
            studentProgramme.IsSendEmail = body.isSendEmail;
            _dbContext.Entity<TrStudentProgramme>().Update(studentProgramme);

            await _dbContext.SaveChangesAsync(CancellationToken);

            if (body.isSendEmail)
            {
                #region email

                var getEmailRecepient = GetEmailUser(TypeEmailRecepient.StudentProgram, _dbContext, studentProgramme.IdSchool, 
                    body.IdAcademicYear, studentProgramme.IdStudent,body.effectiveDate);

                //var getEmailRecepient = await _emailRecepient.GetEmailBccAndTos(new GetEmailBccAndTosRequest
                //{
                //    Type = TypeEmailRecepient.StudentProgram
                //});

                //var EmailRecepient = getEmailRecepient.Payload;

                if (getEmailRecepient.Tos.Any() == false && getEmailRecepient.Bcc.Any() == false)
                    return Request.CreateApiResult2();

                var emailStudentProgramme = await _dbContext.Entity<HTrStudentProgramme>()
                                           .Include(e => e.Student)
                                           .Where(e => e.IdHTrStudentProgramme == addHistoryStudentProgramme.IdHTrStudentProgramme)
                                           .Select(e => new EmailStudentProgrammeResult
                                           {
                                               studentId = e.IdStudent,
                                               studentName = NameUtil.GenerateFullName(e.Student.FirstName, e.Student.MiddleName, e.Student.LastName),
                                               homeroom = body.homeroom,
                                               newProgramme = e.ProgrammeNew.GetDescription(),
                                               oldProgramme = e.ProgrammeOld.GetDescription(),
                                               effectiveDate = Convert.ToDateTime(e.StartDate).ToString("dd MMM yyyy"),
                                           })
                                           .FirstOrDefaultAsync(CancellationToken);

                if (emailStudentProgramme != null)
                {
                    emailStudentProgramme.idUserBcc = getEmailRecepient.Bcc;
                    emailStudentProgramme.idUserTo = getEmailRecepient.Tos;

                    if (KeyValues.ContainsKey("emailStudentProgramme"))
                    {
                        KeyValues.Remove("emailStudentProgramme");
                    }
                    KeyValues.Add("emailStudentProgramme", emailStudentProgramme);
                    var Notification = ESP1Notification(KeyValues, AuthInfo);
                }
                #endregion
            }

            return Request.CreateApiResult2();
        }

        public static string ESP1Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "emailStudentProgramme").Value;
            var emailStudentProgramme = JsonConvert.DeserializeObject<EmailStudentProgrammeResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ESP1")
                {
                    IdRecipients = emailStudentProgramme.idUserTo,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string ESP2Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "emailStudentProgramme").Value;
            var emailStudentProgramme = JsonConvert.DeserializeObject<EmailStudentProgrammeResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ESP2")
                {
                    IdRecipients = emailStudentProgramme.idUserTo,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static GetEmailBccAndTosResult GetEmailUser(TypeEmailRecepient type, ISchedulingDbContext _dbContext, string idSchool, 
            string idAcademicYear, string idStudent, DateTime _date)
        {

            var getEmailRecepient = _dbContext.Entity<MsEmailRecepient>().Include(x => x.Role).ThenInclude(x=> x.RoleGroup)
                                        .Where(e => e.Type == type && e.Role.IdSchool == idSchool)
                                        .ToList();

            if (getEmailRecepient.Any() == false)
                return new GetEmailBccAndTosResult();

            var listIdTecaherPosition = getEmailRecepient.Where(e => e.IdTeacherPosition != null).Select(e => e.IdTeacherPosition).ToList();

            var getNonTeachingLoad = _dbContext.Entity<TrNonTeachingLoad>()
                                        .Include(e => e.MsNonTeachingLoad)
                                        .Where(e => listIdTecaherPosition.Contains(e.MsNonTeachingLoad.IdTeacherPosition) && e.MsNonTeachingLoad.IdAcademicYear == idAcademicYear)
                                        .ToList();

            var msUser = _dbContext.Entity<MsUser>()
                                .Include(x => x.UserRoles)
                                .Where(x => x.UserSchools.Any(y => y.IdSchool == idSchool))
                                .ToList();            

            var msTeacherPosition = _dbContext.Entity<MsTeacherPosition>()
                                .Include(x => x.Position)
                                .Where(x => x.IdSchool == idSchool)
                                .ToList();

            var homeroom =  _dbContext.Entity<MsHomeroomStudent>()
                    .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
                    .Where(e => e.IdStudent == idStudent && e.Homeroom.IdAcademicYear == idAcademicYear)
                    .OrderBy(e => e.Semester)
                    .Select(e => new
                    {
                        idHomeroom = e.Homeroom.Id,
                        idGrade = e.Homeroom.Grade.Id, 
                        idLevel = e.Homeroom.Grade.IdLevel
                    })
                    .ToList();

            var idHomeroom = homeroom.Select(x => x.idHomeroom).ToList();
            var idGrade = homeroom.Select(x => x.idGrade).FirstOrDefault();
            var idLevel = homeroom.Select(x => x.idLevel).FirstOrDefault();

            var periods = _dbContext.Entity<MsPeriod>()
                .Where(x => x.IdGrade == idGrade)
                .ToList();

            #region Tos
            var getEmailRecepientTos = getEmailRecepient.Where(e => !e.IsCC).ToList();
            var listIdTecaherPositionTos = getEmailRecepientTos.Where(e => e.IdTeacherPosition != null).Select(e => e.IdTeacherPosition).ToList();
            var getNonTeachingLoadTos = getNonTeachingLoad.Where(e => listIdTecaherPositionTos.Contains(e.MsNonTeachingLoad.IdTeacherPosition)).ToList();
            var idUserTos = GetIdUser(_dbContext, idSchool, getEmailRecepientTos, getNonTeachingLoadTos, msUser, msTeacherPosition,periods,idHomeroom,
                _date,idStudent,idGrade, idLevel, idAcademicYear);
            #endregion

            #region BCC
            var getEmailRecepientBcc = getEmailRecepient.Where(e => e.IsCC).ToList();
            var listIdTecaherPositionBcc = getEmailRecepientBcc.Where(e => e.IdTeacherPosition != null).Select(e => e.IdTeacherPosition).ToList();
            var getNonTeachingLoadBcc = getNonTeachingLoad.Where(e => listIdTecaherPositionBcc.Contains(e.MsNonTeachingLoad.IdTeacherPosition)).ToList();
            var idUserBcc = GetIdUser(_dbContext, idSchool, getEmailRecepientBcc, getNonTeachingLoadBcc, msUser, msTeacherPosition, periods, idHomeroom,
                _date, idStudent, idGrade, idLevel, idAcademicYear);
            #endregion

            var result = new GetEmailBccAndTosResult
            {
                Tos = idUserTos,
                Bcc = idUserBcc
            };

            return result;
        }
        public static List<string> GetIdUser(ISchedulingDbContext _dbContext, string idSchool, List<MsEmailRecepient> emailRecepient, List<TrNonTeachingLoad> nonTeachingLoads, 
            List<MsUser> users, List<MsTeacherPosition> teacherPositions,List<MsPeriod> periods, List<string> idHomeroom, DateTime _date, 
            string idStudent, string idGrade, string idLevel, string idAcademicYear)
        {
            var listIdUser = new List<string>();

            foreach (var item in emailRecepient)
            {
                if (item.IdTeacherPosition != null)
                {
                    var teacherPosition = teacherPositions.Where(x => x.IdSchool == idSchool && x.Id == item.IdTeacherPosition).FirstOrDefault();

                    if (teacherPosition == null)
                        continue;
                    //checking ST
                    if (teacherPosition.Position.Code == PositionConstant.SubjectTeacher)
                    {
                        var semester = periods.Where(x => x.StartDate <= _date && x.EndDate >= _date).Select(x=> x.Semester).FirstOrDefault();

                        var idlesson = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                        .Where(x => x.HomeroomStudent.IdStudent == idStudent && x.HomeroomStudent.Homeroom.Semester == semester &&
                                        x.HomeroomStudent.Homeroom.IdAcademicYear == idAcademicYear)
                                        .Select(x => x.IdLesson).ToList();

                        var listUserST = _dbContext.Entity<MsLessonTeacher>()
                                        .Where(x => idlesson.Contains(x.IdLesson))
                                        .Select(x=> x.IdUser)
                                        .Distinct()
                                        .ToList();

                        listUserST = listUserST.Where(idUser => users.Select(y => y.Id).Contains(idUser)).ToList();
                        if (listUserST.Any())
                            listIdUser.AddRange(listUserST);
                    }else if (teacherPosition.Position.Code == PositionConstant.ClassAdvisor || teacherPosition.Position.Code == PositionConstant.CoTeacher) //checking CA/Homeroom Teacher
                    {
                        var semester = periods.Where(x => x.StartDate <= _date && x.EndDate >= _date).Select(x => x.Semester).FirstOrDefault();
                        var listUserCA = _dbContext.Entity<MsHomeroomTeacher>()
                        .Where(x => x.Homeroom.IdAcademicYear == idAcademicYear
                        && idHomeroom.Contains(x.IdHomeroom) && x.Homeroom.Semester == semester
                        && x.IdTeacherPosition == item.IdTeacherPosition)
                        .Select(x=> x.IdBinusian)
                        .Distinct()
                        .ToList();

                        listUserCA = listUserCA.Where(idUser => users.Select(y => y.Id).Contains(idUser)).ToList();
                        if (listUserCA.Any())
                            listIdUser.AddRange(listUserCA);
                    }else if (teacherPosition.Position.Code == PositionConstant.SubjectHead) //checking Subject Head
                    {
                        var semester = periods.Where(x => x.StartDate <= _date && x.EndDate >= _date).Select(x => x.Semester).FirstOrDefault();

                        var idSubject = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                        .Where(x => x.HomeroomStudent.IdStudent == idStudent && x.HomeroomStudent.Homeroom.Semester == semester &&
                                        x.HomeroomStudent.Homeroom.IdAcademicYear == idAcademicYear)
                                        .Select(x => x.IdSubject).ToList();

                        var SH = nonTeachingLoads.Where(x => x.MsNonTeachingLoad.IdTeacherPosition == item.IdTeacherPosition).ToList();
                        foreach (var data in SH)
                        {
                            var _dataNewGrade = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(data.Data);

                            _dataNewGrade.TryGetValue("Grade", out var _gradeSH);
                            if (_gradeSH.Id != null)
                            {
                                if (_gradeSH.Id == idGrade)
                                {
                                    var _dataNewSubject = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(data.Data);

                                    _dataNewSubject.TryGetValue("Subject", out var subjectSH);

                                    if (subjectSH.Id != null)
                                    {
                                        if (idSubject.Contains(subjectSH.Id))
                                            if (users.Select(x=> x.Id).ToList().Contains(data.IdUser))
                                                listIdUser.Add(data.IdUser);
                                    }
                                }
                            }
                        }
                    }else if (teacherPosition.Position.Code == PositionConstant.LevelHead)//checking level Head
                    {
                        var LH = nonTeachingLoads.Where(x => x.MsNonTeachingLoad.IdTeacherPosition == item.IdTeacherPosition).ToList();
                        foreach (var data in LH)
                        {
                            var _dataNewGrade = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(data.Data);

                            _dataNewGrade.TryGetValue("Grade", out var _gradeLH);

                            if (_gradeLH.Id != null)
                            {
                                if (_gradeLH.Id == idGrade)
                                {
                                    if (users.Select(x => x.Id).ToList().Contains(data.IdUser))
                                        listIdUser.Add(data.IdUser);
                                }
                            }
                        }
                    }
                    else if (teacherPosition.Position.Code == PositionConstant.Principal || 
                        teacherPosition.Position.Code == PositionConstant.VicePrincipal ||
                        teacherPosition.Position.Code == PositionConstant.AffectiveCoordinator)//checking VP or P or Cordinator
                    {
                        var VP = nonTeachingLoads.Where(x => x.MsNonTeachingLoad.IdTeacherPosition == item.IdTeacherPosition).ToList();
                        foreach (var data in VP)
                        {
                            var _dataNewLevel = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(data.Data);

                            _dataNewLevel.TryGetValue("Level", out var _levelVP);

                            if (_levelVP.Id != null)
                            {
                                if (_levelVP.Id == idLevel)
                                {
                                    if (users.Select(x => x.Id).ToList().Contains(data.IdUser))
                                        listIdUser.Add(data.IdUser);
                                }
                            }
                        }
                    }
                    else // other position
                    {
                        if (item.Role.RoleGroup.Id == "STF" || item.Role.RoleGroup.Id == "SADM")
                        {
                            var listUserSTF = users.Where(x => x.UserRoles.Any(y => y.IdRole == item.IdRole)).Select(x=> x.Id).ToList();
                            listIdUser.AddRange(listUserSTF);
                        }
                        else
                        {
                            var listUserOtherPostion = nonTeachingLoads.Where(x => x.MsNonTeachingLoad.IdTeacherPosition == item.IdTeacherPosition).Select(x => x.IdUser).Distinct().ToList();
                            listUserOtherPostion = listUserOtherPostion.Where(idUser => users.Select(y => y.Id).Contains(idUser)).ToList();
                            if (listUserOtherPostion.Any())
                                listIdUser.AddRange(listUserOtherPostion);
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

using System;
using System.Collections.Generic;
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
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitForm;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.StudentExitForm.Validator;
using FluentEmail.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using NPOI.XWPF.UserModel;
using StackExchange.Redis;

namespace BinusSchool.Student.FnStudent.StudentExitForm
{
    public class StudentExitFormHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IParent _serviceParent;
        private readonly IPeriod _serviceperiod;
        private readonly IMachineDateTime _datetime;
        private readonly IRolePosition _rolePosition;
        public StudentExitFormHandler(IStudentDbContext schoolDbContext, IParent serviceParent, IPeriod serviceperiod, IMachineDateTime datetime, IRolePosition rolePosition)
        {
            _dbContext = schoolDbContext;
            _serviceParent = serviceParent;
            _serviceperiod = serviceperiod;
            _datetime = datetime;
            _rolePosition = rolePosition;
        }
        protected async override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<TrStudentExit>()
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                data.IsActive = false;
                _dbContext.Entity<TrStudentExit>().Update(data);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            //send notification email

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected async override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<TrStudentExit>()
                .Include(x => x.HomeroomStudent).ThenInclude(x => x.Student)
                .Include(x => x.AcademicYear)
                .Include(x => x.UserFather)
                .Include(x => x.UserMother)
                .Include(x => x.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.Classroom)
                .Include(x => x.StudentExitStatuses)
                .Select(x => new GetStudentExitFormDetailResult
                {
                    Id = x.Id,
                    AcademicYear = new ItemValueVm(x.AcademicYear.Id, x.AcademicYear.Description),
                    IdHomeroomStudent = x.IdHomeroomStudent,
                    IdStudent = x.HomeroomStudent.IdStudent,
                    StudentName = NameUtil.GenerateFullName(x.HomeroomStudent.Student.FirstName, x.HomeroomStudent.Student.MiddleName, x.HomeroomStudent.Student.LastName),
                    Level = new ItemValueVm(x.Homeroom.Grade.MsLevel.Id, x.Homeroom.Grade.MsLevel.Description),
                    Grade = new ItemValueVm(x.Homeroom.Grade.Id, x.Homeroom.Grade.Code),
                    IdUserFather = x.IdUserFather,
                    UserFatherName = NameUtil.GenerateFullName(x.UserFather.FirstName, x.UserFather.MiddleName, x.UserFather.LastName),
                    FatherEmail = x.FatherEmail,
                    FatherPhone = x.FatherPhone,
                    IdUserMother = x.IdUserMother,
                    UserMotherName = NameUtil.GenerateFullName(x.UserMother.FirstName, x.UserMother.MiddleName, x.UserMother.LastName),
                    MotherEmail = x.MotherEmail,
                    MotherPhone = x.MotherPhone,
                    StartExit = x.StartExit,
                    ReasonExitStudents = new List<ReasonExitStudent>(),
                    Explain = string.IsNullOrEmpty(x.Explain) ? string.Empty : x.Explain,
                    IsMeetSchoolTeams = x.IsMeetSchoolTeams,
                    NewSchoolName = x.NewSchoolName,
                    NewSchoolCity = x.NewSchoolCity,
                    NewSchoolCountry = x.NewSchoolCountry,
                    Status = x.Status,
                    Homeroom = new ItemValueVm
                    {
                        Id = x.IdHomeroom,
                        Description = $"{x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code}{x.Homeroom.MsGradePathwayClassroom.Classroom.Code}"
                    },
                    ExitStudentStatuses = x.StudentExitStatuses
                        .Where(t => t.Status == x.Status)
                        .OrderByDescending(t => t.DateIn)
                        .Select(t => new ExitStudentStatus { Id = t.Id, Note = t.Note, Status = t.Status.GetDescription()})
                        .FirstOrDefault(),
                    IdUserin = x.UserIn
                })
                .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            var isParent = _dbContext.Entity<MsUserRole>().Include(x => x.Role).Any(x => x.IdUser == data.IdUserin && x.Role.IdRoleGroup == "PRT");

            data.IsParent = isParent;

            var dataReasonExitStudents = _dbContext.Entity<TrStudentExitReason>().Include(x => x.StudentExitReason)
                .Where(x => x.IdStudentExit == data.Id).ToList();
            
            if(dataReasonExitStudents.Any())
            {
                foreach(var itemReason in dataReasonExitStudents)
                {
                    data.ReasonExitStudents.Add(new ReasonExitStudent { Id = itemReason.Id, Reason = itemReason.StudentExitReason.Description });
                }
            }

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetStudentExitFormRequest>(nameof(GetStudentExitFormRequest.IdUser));

            var columns = new[] { "Id", "IdSchool","SchoolName", "IdAcademicYear", "AcademicYear", "Status", "IdStudent", "StudentName", "SubmissionDate", "LastDateOfAttendance"};
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "Id" },
                { columns[1], "IdSchool" },
                { columns[2], "SchoolName"},
                { columns[3], "IdAcademicYear"},
                { columns[4], "AcademicYear"},
                { columns[5], "Status"},
                { columns[6], "IdStudent"},
                { columns[7], "SubmissionDate"},
                { columns[8], "LastDateOfAttendance"}
            };

            var apiGetcurrentAcademicyear = _serviceperiod.GetCurrenctAcademicYear(new CurrentAcademicYearRequest
            {
                IdSchool = param.IdSchool.First()
            });

            var currentAcademicyear = apiGetcurrentAcademicyear.Result.IsSuccess ? apiGetcurrentAcademicyear.Result.Payload : null;

            var apiGetChild = _serviceParent.GetChildrens(new GetChildRequest
            {
                IdParent = param.IdUser,
                IdAcademicYear = currentAcademicyear.Id,
            });

            var listChild = apiGetChild.Result.IsSuccess ? apiGetChild.Result.Payload : null;

            listChild.Where(e => e.Role == RoleConstant.Parent).ForEach(e => e.Id = param.IdUser);

            var listFamilyId = listChild.Select(e => e.Id).ToList();

            var predicate = PredicateBuilder.Create<TrStudentExit>(x => true);
                predicate = predicate.And(x => listFamilyId.Any(t => t == x.HomeroomStudent.IdStudent));

            var query = _dbContext.Entity<TrStudentExit>()
                .Include(x => x.AcademicYear)
                .Include(x => x.Homeroom)
                .Include(x => x.HomeroomStudent).ThenInclude(x => x.Student)
                .Include(x => x.StudentExitStatuses)
                .Where(predicate)
                .OrderByDynamic(param, aliasColumns);

            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear.Description)
                        : query.OrderBy(x => x.AcademicYear.Description);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
                case "IdStudent":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.HomeroomStudent.IdStudent)
                        : query.OrderBy(x => x.HomeroomStudent.IdStudent);
                    break;
                case "StudentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.HomeroomStudent.Student.FirstName)
                        : query.OrderBy(x => x.HomeroomStudent.Student.FirstName);
                    break;
                case "SubmissionDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.DateIn)
                        : query.OrderBy(x => x.DateIn);
                    break;
                case "LastDateOfAttendance":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StartExit)
                        : query.OrderBy(x => x.StartExit);
                    break;
                default:
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderBy(x => x.DateIn)
                        : query.OrderByDescending(x => x.DateIn);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetStudentExitFormResult()
                    {
                        Id = x.Id,
                        IdSchool = param.IdSchool.First(),
                        SchoolName = _dbContext.Entity<MsSchool>().Where(t => t.Id == param.IdSchool.First()).Select(x => x.Name).FirstOrDefault(),
                        AcademicYear = new ItemValueVm(x.AcademicYear.Id, x.AcademicYear.Description),
                        Status = x.Status,
                        IdHomeroomStudent = x.IdHomeroomStudent,
                        IdStudent = x.HomeroomStudent.IdStudent,
                        StudentName = NameUtil.GenerateFullName(
                            x.HomeroomStudent.Student.FirstName,
                            x.HomeroomStudent.Student.MiddleName,
                            x.HomeroomStudent.Student.LastName),
                        SubmissionDate = x.DateIn.Value,
                        LastDateOfAttendance = x.StartExit,
                        ApproveNote = x.StudentExitStatuses
                            .Where(t => t.Status == x.Status)
                            .OrderByDescending(t => t.DateIn)
                            .First().Note
                    })
                    .ToListAsync(CancellationToken);
                    
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected async override Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddStudentExitFormRequest, AddStudentExitFormValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var StatusCancel = new List<StatusExitStudent> { StatusExitStudent.CancelledBySchool, StatusExitStudent.CancelledBySchool };

            var isStudentExitIsExist = await _dbContext.Entity<TrStudentExit>()
                .Where(x => x.IdHomeroomStudent == body.IdHomeroomStudent && x.IdAcademicYear == body.IdAcademicYear && (x.Status != StatusExitStudent.CancelledBySchool && x.Status != StatusExitStudent.CancelledByParent))
                .FirstOrDefaultAsync(CancellationToken);

            if (isStudentExitIsExist != null)
                throw new BadRequestException("Your previous request is being processed");

            var newStudentExit = new TrStudentExit
            {
                Id = Guid.NewGuid().ToString(),
                IdAcademicYear = body.IdAcademicYear,
                IdHomeroomStudent = body.IdHomeroomStudent,
                IdUserFather = body.IdUserFather,
                FatherEmail = body.FatherEmail,
                FatherPhone = body.FatherPhone,
                IdUserMother = body.IdUserMother,
                MotherEmail = body.MotherEmail,
                MotherPhone = body.MotherPhone,
                StartExit = body.StartExit,
                Explain = (string.IsNullOrEmpty(body.Explain)) ? string.Empty : body.Explain,
                IsMeetSchoolTeams = body.IsMeetSchoolTeams,
                NewSchoolName = body.NewSchoolName,
                NewSchoolCity = body.NewSchoolCity,
                NewSchoolCountry = body.NewSchoolCountry,
                //Status = StatusExitStudent.WaitingApproval,
                Status = body.IsParent ? StatusExitStudent.WaitingApproval : StatusExitStudent.Approved,
                IdHomeroom = body.IdHomeroom,
            };

            _dbContext.Entity<TrStudentExit>().Add(newStudentExit);


            foreach (var data in body.ReasonExitStudent.Where(x => !string.IsNullOrEmpty(x)))
            {
                var ReasonExit = new TrStudentExitReason
                {
                    Id = Guid.NewGuid().ToString(),
                    IdMsStudentExitReason = data,
                    IdStudentExit = newStudentExit.Id
                };

                _dbContext.Entity<TrStudentExitReason>().Add(ReasonExit);
            }

            var newStudentExitStatus = new TrStudentExitStatus
            {
                Id = Guid.NewGuid().ToString(),
                IdStudentExit = newStudentExit.Id,
                Status = body.IsParent ? StatusExitStudent.WaitingApproval : StatusExitStudent.Approved,
                Note = string.Empty,
            };

            _dbContext.Entity<TrStudentExitStatus>().Add(newStudentExitStatus);

            var IdStudent = _dbContext.Entity<MsHomeroomStudent>().Where(x => x.Id == body.IdHomeroomStudent).Select(x => x.IdStudent).FirstOrDefault();
            if (IdStudent == null)
                throw new BadRequestException($"{body.IdHomeroomStudent} not exists");

            var dataStudentStatus = _dbContext.Entity<TrStudentStatus>()
                .Where(x => x.IdStudent == IdStudent)
                .OrderByDescending(e => e.DateIn)
                .FirstOrDefault();

            if (dataStudentStatus != null)
            {
                dataStudentStatus.CurrentStatus = "H";
                dataStudentStatus.EndDate = body.StartExit;
                _dbContext.Entity<TrStudentStatus>().Update(dataStudentStatus);
            }

            var getDataReasons = _dbContext.Entity<MsStudentExitReason>().Where(x => body.ReasonExitStudent.Any(t => t == x.Id)).Select(x => x.Description).ToList();
            var dataReasons = (getDataReasons.Any()) ? string.Join(";", getDataReasons) : string.Empty;

            TrStudentStatus newStudentStatus = new TrStudentStatus
            {
                IdTrStudentStatus = Guid.NewGuid().ToString(),
                IdAcademicYear = body.IdAcademicYear,
                IdStudent = dataStudentStatus.IdStudent,
                IdStudentStatus = body.IsParent ? 16 : 5, //OWP (16) : Resign (5)
                StartDate = body.StartExit.AddDays(1),
                CurrentStatus = "A",
                ActiveStatus = false,
                Remarks = dataReasons
            };
            _dbContext.Entity<TrStudentStatus>().Add(newStudentStatus);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            #region send email
            if (body.IsParent)
            {
                var ListRole = new List<GetUserEmailRecepient>();

                //get user acop by school
                var school = await _dbContext.Entity<MsAcademicYear>().FirstOrDefaultAsync(x => x.Id == body.IdAcademicYear, CancellationToken);

                var requestAcopUser = new GetUserSubjectByEmailRecepientRequest()
                {
                    IdAcademicYear = body.IdAcademicYear,
                    IdSchool = school.IdSchool,
                    IsShowIdUser = true
                };

                var listIdRole = await _dbContext.Entity<LtRole>()
                                     .Where(x => x.Code.Contains("acop"))
                                     .Select(x => x.Id)
                                     .ToListAsync(CancellationToken);

                foreach (var item in listIdRole)
                {
                    var idRole = new GetUserEmailRecepient
                    {
                        IdRole = item
                    };

                    ListRole.Add(idRole);
                }

                requestAcopUser.EmailRecepients = ListRole;

                var getDataUserAcop = await _rolePosition.GetUserSubjectByEmailRecepient(requestAcopUser);
                var userAcop = getDataUserAcop.Payload;

                //collecting data
                var queryStudentExit = await _dbContext.Entity<TrStudentExit>()
                                        .Include(x => x.HomeroomStudent)
                                            .ThenInclude(x => x.Student)
                                        .Include(x => x.Homeroom)
                                            .ThenInclude(x => x.Grade)
                                                .ThenInclude(x => x.MsLevel)
                                                    .ThenInclude(x => x.MsAcademicYear)
                                        .Include(x => x.Homeroom)
                                            .ThenInclude(x => x.MsGradePathwayClassroom)
                                                .ThenInclude(x => x.Classroom)
                                        .Where(x => x.Id == newStudentExit.Id)
                                        .FirstOrDefaultAsync(CancellationToken);

                var studentParent = await _dbContext.Entity<MsUser>()
                                        .Include(x => x.UserRoles)
                                            .ThenInclude(x => x.Role)
                                        .Where(x => x.UserRoles.Any(a => a.Role.Code == "PRT") && x.Id.Contains(queryStudentExit.HomeroomStudent.Student.Id))
                                        .Select(x => x.DisplayName)
                                        .ToListAsync(CancellationToken);


                var dataEmail = new StudentExitNotificationResult
                {
                    IdStudentExit = newStudentExit.Id,
                    Status = queryStudentExit.Status.GetDescription(),
                    AcademicYear = queryStudentExit.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                    Semester = queryStudentExit.Homeroom.Semester,
                    Level = queryStudentExit.Homeroom.Grade.MsLevel.Code,
                    Grade = queryStudentExit.Homeroom.Grade.Description,
                    Homeroom = $"{queryStudentExit.Homeroom.Grade.Description}{queryStudentExit.Homeroom.MsGradePathwayClassroom.Classroom.Description}",
                    BinusianId = queryStudentExit.HomeroomStudent.IdStudent,
                    StudentName = NameUtil.GenerateFullName(queryStudentExit.HomeroomStudent.Student.FirstName, queryStudentExit.HomeroomStudent.Student.MiddleName, queryStudentExit.HomeroomStudent.Student.LastName),
                    ParentName = studentParent.FirstOrDefault()
                };


                //send email to acop
                if (KeyValues.ContainsKey("studentExitResult"))
                {
                    KeyValues.Remove("studentExitResult");
                }
                KeyValues.Add("studentExitResult", dataEmail);

                await SXT1Notification(KeyValues, AuthInfo, userAcop.Select(a => a.IdUser).ToList());

            }
            #endregion
            return Request.CreateApiResult2();
        }

        private async Task SXT1Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, List<string> IdUserRecipient)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "studentExitResult").Value;
            var emailDownload = JsonConvert.DeserializeObject<StudentExitNotificationResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SXT1")
                {
                    IdRecipients = IdUserRecipient,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
        }

        protected async override Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateStudentExitFormRequest, UpdateStudentExitFormValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<TrStudentExit>()
                .Where(x => x.Id == body.Id)
                .FirstOrDefaultAsync(CancellationToken);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Id"], "Id", body.Id));

            if (data.IdHomeroomStudent != body.IdHomeroomStudent)
            {
                UpdateLastStudentToActive(data.IdHomeroomStudent, data.IdAcademicYear);
                
                InsertNewStudentStatus(body.IdHomeroomStudent, body.IdAcademicYear, body.Id);
            }

            data.IdAcademicYear = body.IdAcademicYear;
            data.IdHomeroomStudent = body.IdHomeroomStudent;
            data.IdUserFather = body.IdUserFather;
            data.FatherEmail = body.FatherEmail;
            data.FatherPhone = body.FatherPhone;
            data.IdUserMother = body.IdUserMother;
            data.MotherEmail = body.MotherEmail;
            data.MotherPhone = body.MotherPhone;
            data.StartExit = body.StartExit;
            data.Explain = (string.IsNullOrEmpty(body.Explain)) ? string.Empty : body.Explain;
            data.IsMeetSchoolTeams = body.IsMeetSchoolTeams;
            data.NewSchoolName = body.NewSchoolName;
            data.NewSchoolCity = body.NewSchoolCity;
            data.NewSchoolCountry = body.NewSchoolCountry;
            data.Status = body.Status;
            data.IdHomeroom = body.IdHomeroom;

            _dbContext.Entity<TrStudentExit>().Update(data);

            var dataStudentExitReason = _dbContext.Entity<TrStudentExitReason>()
                .Where(x => x.IdStudentExit == body.Id)
                .ToList();

            if(dataStudentExitReason.Any())
            {
                foreach (var itemReason in dataStudentExitReason)
                {
                    itemReason.IsActive = false;
                    _dbContext.Entity<TrStudentExitReason>().Update(itemReason);
                }
            }

            foreach (var dataReason in body.ReasonExitStudent.Where(x => !string.IsNullOrEmpty(x)))
            {
                var ReasonExit = new TrStudentExitReason
                {
                    Id = Guid.NewGuid().ToString(),
                    IdMsStudentExitReason = dataReason,
                    IdStudentExit = body.Id
                };

                _dbContext.Entity<TrStudentExitReason>().Add(ReasonExit);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        private void UpdateLastStudentToActive(string idHomeroomStudent, string idAcademicYear)
        {
            var IdStudent = _dbContext.Entity<MsHomeroomStudent>().Where(x => x.Id == idHomeroomStudent).Select(x => x.IdStudent).FirstOrDefault();
            if (IdStudent == null)
                throw new BadRequestException($"{idHomeroomStudent} not exists");

            var dataStudentStatus = _dbContext.Entity<TrStudentStatus>()
                .Where(x => x.IdStudent == IdStudent)
                .OrderByDescending(e => e.DateIn)
                .FirstOrDefault();

            if (dataStudentStatus != null)
            {
                dataStudentStatus.CurrentStatus = "H";
                dataStudentStatus.EndDate = _datetime.ServerTime;

                _dbContext.Entity<TrStudentStatus>().Update(dataStudentStatus);
            }

            TrStudentStatus newStudentStatus = new TrStudentStatus
            {
                IdTrStudentStatus = Guid.NewGuid().ToString(),
                IdAcademicYear = idAcademicYear,
                IdStudent = dataStudentStatus.IdStudent,
                IdStudentStatus = 1,
                StartDate = _datetime.ServerTime,
                CurrentStatus = "A",
                ActiveStatus = true
            };
            _dbContext.Entity<TrStudentStatus>().Add(newStudentStatus);
        }

        private void InsertNewStudentStatus(string idHomeroomStudent, string idAcademicYear, string idExitStudent)
        {
            var IdStudent = _dbContext.Entity<MsHomeroomStudent>().Where(x => x.Id == idHomeroomStudent).Select(x => x.IdStudent).FirstOrDefault();
            if (IdStudent == null)
                throw new BadRequestException($"{idHomeroomStudent} not exists");

            var dataStudentStatus = _dbContext.Entity<TrStudentStatus>()
                .Where(x => x.IdStudent == IdStudent)
                .OrderByDescending(e => e.DateIn)
                .FirstOrDefault();

            if (dataStudentStatus != null)
            {
                dataStudentStatus.CurrentStatus = "H";
                dataStudentStatus.EndDate = _datetime.ServerTime;

                _dbContext.Entity<TrStudentStatus>().Update(dataStudentStatus);
            }

            var getDataReasons = _dbContext.Entity<TrStudentExitReason>()
                        .Include(x => x.StudentExitReason)
                        .Where(x => x.IdStudentExit == idExitStudent)
                        .Select(x => x.StudentExitReason.Description)
                        .ToList();

            var dataReasons = (getDataReasons.Any()) ? string.Join(";", getDataReasons) : string.Empty;

            TrStudentStatus newStudentStatus = new TrStudentStatus
            {
                IdTrStudentStatus = Guid.NewGuid().ToString(),
                IdAcademicYear = idAcademicYear,
                IdStudent = dataStudentStatus.IdStudent,
                IdStudentStatus = 16,
                StartDate = _datetime.ServerTime,
                CurrentStatus = "A",
                ActiveStatus = false,
                Remarks = dataReasons
            };

            _dbContext.Entity<TrStudentStatus>().Add(newStudentStatus);
        }
    }
}

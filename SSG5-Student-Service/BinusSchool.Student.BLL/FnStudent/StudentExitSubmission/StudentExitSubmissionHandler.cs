using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitSubmission;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Student.FnStudent.StudentExitSubmission.Validator;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using NPOI.OpenXmlFormats.Spreadsheet;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitForm;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using BinusSchool.Auth.Authentications.Jwt;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using FluentEmail.Core;
using NPOI.SS.Formula.Functions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Api.Scheduling.FnSchedule;

namespace BinusSchool.Student.FnStudent.StudentExitSubmission
{
    public class StudentExitSubmissionHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IRolePosition _rolePosition;
        public StudentExitSubmissionHandler(IStudentDbContext schoolDbContext, IMachineDateTime datetime, IRolePosition rolePosition)
        {
            _dbContext = schoolDbContext;
            _datetime = datetime;
            _rolePosition = rolePosition;
        }
        protected async override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            return Request.CreateApiResult2();
        }

        protected async override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            return Request.CreateApiResult2(null as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetStudentExitSubmissionRequest>(nameof(GetStudentExitSubmissionRequest.IdAcademicYear));

            var columns = new[] { "Id", "AcademicYear", "Semester", "Level", "Grade", "Homeroom", "IdStudent", "StudentName", "Status" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "Id" },
                { columns[1], "AcademicYear" },
                { columns[2], "Semester"},
                { columns[3], "Level"},
                { columns[4], "Grade"},
                { columns[5], "Homeroom"},
                { columns[6], "IdStudent"},
                { columns[7], "StudentName"},
                { columns[8], "Status"}
            };

            var predicate = PredicateBuilder.Create<TrStudentExit>(x => x.IdAcademicYear == param.IdAcademicYear);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.HomeroomStudent.Student.FirstName, param.SearchPattern())
                    || EF.Functions.Like(x.HomeroomStudent.Student.MiddleName, param.SearchPattern())
                    || EF.Functions.Like(x.HomeroomStudent.Student.LastName, param.SearchPattern())
                    || EF.Functions.Like(x.HomeroomStudent.IdStudent, param.SearchPattern()));

            if (param.Semester != null)
                predicate = predicate.And(x => x.HomeroomStudent.Semester == param.Semester);

            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);

            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.Homeroom.Grade.Id == param.IdGrade);

            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.Homeroom.Id == param.IdHomeroom);

            if (param.Status != null)
                predicate = predicate.And(x => x.Status == (StatusExitStudent)Enum.Parse(typeof(StatusExitStudent), param.Status));

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
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;

                case "Semester":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom.Semester)
                        : query.OrderBy(x => x.Homeroom.Semester);
                    break;
                case "Level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom.Grade.MsLevel.Code)
                        : query.OrderBy(x => x.Homeroom.Grade.MsLevel.Code);
                    break;
                case "Grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom.Grade.Code)
                        : query.OrderBy(x => x.Homeroom.Grade.Code);
                    break;
                case "Homeroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code)
                        : query.OrderBy(x => x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code);
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
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
                default:
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom.Grade.MsLevel.Code)
                        : query.OrderBy(x => x.Homeroom.Grade.MsLevel.Code);
                    break;

            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                     .Select(x => new ItemValueVm(x.Id, x.IdHomeroomStudent))
                     .ToListAsync(CancellationToken);
            }
            else
            {
                var itemData = await query
                    .SetPagination(param)
                    .Select(x => new GetStudentExitSubmissionResult()
                    {
                        Id = x.Id,
                        AcademicYear = new ItemValueVm(x.AcademicYear.Id, x.AcademicYear.Description),
                        Semester = x.Homeroom.Semester,
                        Level = new ItemValueVm(x.Homeroom.Grade.MsLevel.Id, x.Homeroom.Grade.MsLevel.Description),
                        Grade = new ItemValueVm(x.Homeroom.Grade.Id, x.Homeroom.Grade.Code),
                        Homeroom = new ItemValueVm(x.IdHomeroom, $"{x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code}{x.Homeroom.MsGradePathwayClassroom.Classroom.Code}"),
                        IdHomeroomStudent = x.IdHomeroomStudent,
                        IdStudent = x.HomeroomStudent.IdStudent,
                        StudentName = NameUtil.GenerateFullName(
                            x.HomeroomStudent.Student.FirstName,
                            x.HomeroomStudent.Student.MiddleName,
                            x.HomeroomStudent.Student.LastName),
                        Status = x.Status,
                        IdUserIn = x.UserIn
                    })
                    .ToListAsync(CancellationToken);

                foreach (var item in itemData)
                {
                    var msUser = await _dbContext.Entity<MsUser>().FirstOrDefaultAsync(x => x.Id == item.IdUserIn, CancellationToken);

                    item.CreatedBy = msUser.DisplayName;
                }

                items = itemData;

            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected async override Task<ApiErrorResult<object>> PostHandler()
        {
            return Request.CreateApiResult2();
        }

        protected async override Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateStudentExitSubmissionRequest, UpdateStudentExitSubmissionValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<TrStudentExit>()
                .Where(x => x.Id == body.Id)
                .FirstOrDefaultAsync(CancellationToken);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Id"], "Id", body.Id));

            data.Status = body.Status;

            if (body.Status == StatusExitStudent.Approved)
            {
                data.EffectiveDate = body.EffectiveDate;
            }

            _dbContext.Entity<TrStudentExit>().Update(data);

            var dataStudentExitStatus = _dbContext.Entity<TrStudentExitStatus>()
                .Where(x => x.IdStudentExit == body.Id && x.Status == body.Status)
                .FirstOrDefault();

            if (dataStudentExitStatus != null)
            {
                dataStudentExitStatus.Note = body.Note;
                _dbContext.Entity<TrStudentExitStatus>().Update(dataStudentExitStatus);
            }
            else
            {
                var newStudentExitStatus = new TrStudentExitStatus
                {
                    Id = Guid.NewGuid().ToString(),
                    IdStudentExit = data.Id,
                    Status = body.Status,
                    Note = body.Note,
                };

                _dbContext.Entity<TrStudentExitStatus>().Add(newStudentExitStatus);
            }


            if (body.Status != StatusExitStudent.ApproveWithNote && body.Status != StatusExitStudent.DeleteRequest)
            {
                var IdStudent = _dbContext.Entity<MsHomeroomStudent>().Where(x => x.Id == data.IdHomeroomStudent).Select(x => x.IdStudent).FirstOrDefault();
                if (IdStudent == null)
                    throw new BadRequestException($"{data.IdHomeroomStudent} not exists");

                //update previous data
                var trStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                    .Where(x => x.IdStudent == IdStudent && x.IdAcademicYear == data.IdAcademicYear)
                    .ToListAsync(CancellationToken);

                if (body.Status == StatusExitStudent.Approved)
                {
                    var checkDataLastExit = trStudentStatus.OrderByDescending(x => x.DateIn).Select(x => x.StartDate.Date).FirstOrDefault();

                    if (checkDataLastExit >= body.EffectiveDate.Value.Date) //update OWP and Active
                    {
                        var updateStatusOWP = trStudentStatus.Where(x => x.IdStudentStatus == 16).OrderByDescending(x => x.DateIn).FirstOrDefault();
                        updateStatusOWP.IsActive = false;
                        updateStatusOWP.CurrentStatus = "H";
                        updateStatusOWP.EndDate = body.EffectiveDate.Value.AddDays(-1);

                        var updateActiveDate = trStudentStatus.Where(x => x.IdStudentStatus == 1).OrderByDescending(x => x.DateIn).FirstOrDefault();
                        updateActiveDate.EndDate = body.EffectiveDate.Value.AddDays(-1);
                        updateActiveDate.CurrentStatus = "H";

                        _dbContext.Entity<TrStudentStatus>().Update(updateStatusOWP);
                        _dbContext.Entity<TrStudentStatus>().Update(updateActiveDate);
                    }
                    else //update OWP
                    {
                        var updateStatusOWP = trStudentStatus.Where(x => x.IdStudentStatus == 16).OrderByDescending(x => x.DateIn).FirstOrDefault();
                        updateStatusOWP.CurrentStatus = "H";
                        updateStatusOWP.EndDate = body.EffectiveDate.Value.AddDays(-1);

                        _dbContext.Entity<TrStudentStatus>().Update(updateStatusOWP);
                    }

                    var getDataReasons = _dbContext.Entity<TrStudentExitReason>()
                        .Include(x => x.StudentExitReason)
                        .Where(x => x.IdStudentExit == body.Id)
                        .Select(x => x.StudentExitReason.Description)
                        .ToList();

                    var dataReasons = (getDataReasons.Any()) ? string.Join(";", getDataReasons) : string.Empty;

                    TrStudentStatus newStudentStatus = new TrStudentStatus
                    {
                        IdTrStudentStatus = Guid.NewGuid().ToString(),
                        IdAcademicYear = data.IdAcademicYear,
                        IdStudent = IdStudent,
                        IdStudentStatus = 5,
                        StartDate = body.EffectiveDate.Value,
                        CurrentStatus = "A",
                        ActiveStatus = false,
                        Remarks = dataReasons
                    };

                    _dbContext.Entity<TrStudentStatus>().Add(newStudentStatus);
                }
                else if (body.Status == StatusExitStudent.CancelledByParent || body.Status == StatusExitStudent.CancelledBySchool)
                {
                    var checkDataLastExit = trStudentStatus.OrderByDescending(x => x.DateIn).FirstOrDefault();

                    if (checkDataLastExit.IdStudentStatus == 5) // cancel after resign
                    {
                        if (_datetime.ServerTime.Date > checkDataLastExit.StartDate.Date) // check if date now more than last data start date resign
                        {
                            var updateStatusResign = trStudentStatus.Where(x => x.IdStudentStatus == 5).OrderByDescending(x => x.DateIn).FirstOrDefault();
                            updateStatusResign.CurrentStatus = "H";
                            updateStatusResign.EndDate = _datetime.ServerTime.AddDays(-1);

                            _dbContext.Entity<TrStudentStatus>().Update(updateStatusResign);
                        }
                        else if (_datetime.ServerTime.Date == checkDataLastExit.StartDate.Date)
                        {
                            var updateStatusResign = trStudentStatus.Where(x => x.IdStudentStatus == 5).OrderByDescending(x => x.DateIn).FirstOrDefault();
                            updateStatusResign.CurrentStatus = "H";
                            updateStatusResign.EndDate = _datetime.ServerTime.AddDays(-1);

                            _dbContext.Entity<TrStudentStatus>().Update(updateStatusResign);
                        }
                        else // cancel before resign
                        {
                            var updateStatusResign = trStudentStatus.Where(x => x.IdStudentStatus == 5).OrderByDescending(x => x.DateIn).FirstOrDefault();
                            updateStatusResign.CurrentStatus = "H";
                            updateStatusResign.IsActive = false;
                            updateStatusResign.EndDate = _datetime.ServerTime.AddDays(-1);

                            var updateStatusOWP = trStudentStatus.Where(x => x.IdStudentStatus == 16).OrderByDescending(x => x.DateIn).FirstOrDefault();
                            updateStatusOWP.CurrentStatus = "H";
                            updateStatusOWP.IsActive = false;
                            updateStatusOWP.EndDate = _datetime.ServerTime.AddDays(-1);

                            var updateActiveDate = trStudentStatus.Where(x => x.IdStudentStatus == 1).OrderByDescending(x => x.DateIn).FirstOrDefault();
                            updateActiveDate.EndDate = _datetime.ServerTime.AddDays(-1);
                            updateActiveDate.CurrentStatus = "H";

                            _dbContext.Entity<TrStudentStatus>().Update(updateActiveDate);
                            _dbContext.Entity<TrStudentStatus>().Update(updateStatusResign);
                            _dbContext.Entity<TrStudentStatus>().Update(updateStatusOWP);
                        }

                    }
                    else if (checkDataLastExit.IdStudentStatus == 16) // cancel before resign
                    {
                        if (_datetime.ServerTime.Date >= checkDataLastExit.StartDate.Date) // cancel after owp date
                        {
                            var updateStatusOWP = trStudentStatus.Where(x => x.IdStudentStatus == 16).OrderByDescending(x => x.DateIn).FirstOrDefault();
                            updateStatusOWP.CurrentStatus = "H";
                            updateStatusOWP.EndDate = _datetime.ServerTime.AddDays(-1);

                            _dbContext.Entity<TrStudentStatus>().Update(updateStatusOWP);
                        }
                        else // cancel before owp date
                        {
                            var updateStatusOWP = trStudentStatus.Where(x => x.IdStudentStatus == 16).OrderByDescending(x => x.DateIn).FirstOrDefault();
                            updateStatusOWP.CurrentStatus = "H";
                            updateStatusOWP.IsActive = false;
                            updateStatusOWP.EndDate = _datetime.ServerTime.AddDays(-1);

                            var updateActiveDate = trStudentStatus.Where(x => x.IdStudentStatus == 1).OrderByDescending(x => x.DateIn).FirstOrDefault();
                            updateActiveDate.EndDate = _datetime.ServerTime.AddDays(-1);
                            updateActiveDate.CurrentStatus = "H";

                            _dbContext.Entity<TrStudentStatus>().Update(updateStatusOWP);
                            _dbContext.Entity<TrStudentStatus>().Update(updateActiveDate);
                        }
                    }

                    TrStudentStatus newStudentStatus = new TrStudentStatus
                    {
                        IdTrStudentStatus = Guid.NewGuid().ToString(),
                        IdAcademicYear = data.IdAcademicYear,
                        IdStudent = IdStudent,
                        IdStudentStatus = 1,
                        CurrentStatus = "A",
                        StartDate = _datetime.ServerTime.Date,
                        ActiveStatus = true,
                        Remarks = body.Status.GetDescription()
                    };

                    _dbContext.Entity<TrStudentStatus>().Add(newStudentStatus);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);
            #region send email
            //collecting data
            var queryEmail = await _dbContext.Entity<TrStudentExit>()
                                    .Include(x => x.HomeroomStudent)
                                        .ThenInclude(x => x.Student)
                                    .Include(x => x.Homeroom)
                                        .ThenInclude(x => x.Grade)
                                            .ThenInclude(x => x.MsLevel)
                                                .ThenInclude(x => x.MsAcademicYear)
                                    .Include(x => x.Homeroom)
                                        .ThenInclude(x => x.MsGradePathwayClassroom)
                                            .ThenInclude(x => x.Classroom)
                                    .Where(x => x.Id == body.Id)
                                    .FirstOrDefaultAsync(CancellationToken);

            var studentParent = await _dbContext.Entity<MsUser>()
                                        .Include(x => x.UserRoles)
                                            .ThenInclude(x => x.Role)
                                        .Where(x => x.UserRoles.Any(a => a.Role.Code == "PRT") && x.Id.Contains(queryEmail.HomeroomStudent.Student.Id))
                                        .ToListAsync(CancellationToken);

            var dataEmail = new StudentExitNotificationResult
            {
                IdStudentExit = body.Id,
                Status = queryEmail.Status.GetDescription(),
                AcademicYear = queryEmail.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                Semester = queryEmail.Homeroom.Semester,
                Level = queryEmail.Homeroom.Grade.MsLevel.Code,
                Grade = queryEmail.Homeroom.Grade.Description,
                Homeroom = $"{queryEmail.Homeroom.Grade.Description}{queryEmail.Homeroom.MsGradePathwayClassroom.Classroom.Description}",
                BinusianId = queryEmail.HomeroomStudent.IdStudent,
                StudentName = NameUtil.GenerateFullName(queryEmail.HomeroomStudent.Student.FirstName, queryEmail.HomeroomStudent.Student.MiddleName, queryEmail.HomeroomStudent.Student.LastName),
                Note = body.Note,
                EffectiveDate = body.Status == StatusExitStudent.Approved ? body.EffectiveDate.Value.ToString("dd MMMM yyyy") : "-",
                ParentName = studentParent.FirstOrDefault().DisplayName
            };

            if (body.Status == StatusExitStudent.CancelledByParent)
            {
                var ListRole = new List<GetUserEmailRecepient>();
                //get user acop by school
                var school = await _dbContext.Entity<MsAcademicYear>().FirstOrDefaultAsync(x => x.Id == queryEmail.Homeroom.Grade.MsLevel.IdAcademicYear, CancellationToken);

                var requestAcopUser = new GetUserSubjectByEmailRecepientRequest()
                {
                    IdAcademicYear = queryEmail.Homeroom.Grade.MsLevel.IdAcademicYear,
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

                //send email to acop
                if (KeyValues.ContainsKey("studentExitResult"))
                {
                    KeyValues.Remove("studentExitResult");
                }
                KeyValues.Add("studentExitResult", dataEmail);

                await SXT2Notification(KeyValues, AuthInfo, userAcop.Select(x => x.IdUser).ToList());
            }

            if (body.Status == StatusExitStudent.CancelledBySchool)
            {

                // send email to parent
                if (KeyValues.ContainsKey("studentExitResult"))
                {
                    KeyValues.Remove("studentExitResult");
                }
                KeyValues.Add("studentExitResult", dataEmail);

                await SXT5Notification(KeyValues, AuthInfo, studentParent.Select(x => x.Id).ToList());

            }

            if (body.Status == StatusExitStudent.Approved || body.Status == StatusExitStudent.ApproveWithNote)
            {
                // send email to parent
                if (KeyValues.ContainsKey("studentExitResult"))
                {
                    KeyValues.Remove("studentExitResult");
                }
                KeyValues.Add("studentExitResult", dataEmail);

                if (body.Status == StatusExitStudent.ApproveWithNote)
                {
                    await SXT3Notification(KeyValues, AuthInfo, studentParent.Select(x => x.Id).ToList());
                }

                if (body.Status == StatusExitStudent.Approved)
                {
                    await SXT4Notification(KeyValues, AuthInfo, studentParent.Select(x => x.Id).ToList());
                }

            }

            #endregion

            return Request.CreateApiResult2();
        }

        private async Task SXT2Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, List<string> IdUserRecipient)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "studentExitResult").Value;
            var emailDownload = JsonConvert.DeserializeObject<StudentExitNotificationResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SXT2")
                {
                    IdRecipients = IdUserRecipient,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
        }
        private async Task SXT3Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, List<string> IdUserRecipient)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "studentExitResult").Value;
            var emailDownload = JsonConvert.DeserializeObject<StudentExitNotificationResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SXT3")
                {
                    IdRecipients = IdUserRecipient,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
        }
        private async Task SXT4Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, List<string> IdUserRecipient)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "studentExitResult").Value;
            var emailDownload = JsonConvert.DeserializeObject<StudentExitNotificationResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SXT4")
                {
                    IdRecipients = IdUserRecipient,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
        }
        private async Task SXT5Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, List<string> IdUserRecipient)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "studentExitResult").Value;
            var emailDownload = JsonConvert.DeserializeObject<StudentExitNotificationResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SXT5")
                {
                    IdRecipients = IdUserRecipient,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
        }



    }
}

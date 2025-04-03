using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class DeleteEntryMeritHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public DeleteEntryMeritHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteEntryMeritDemeritTeacherRequest, DeleteEntryMeritDemeritTeacherValidator>();

            var GetEntryMeritStudent = await _dbContext.Entity<TrEntryMeritStudent>()
                .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.MsLevel)
             .Where(x => x.Id==body.Id)
             .SingleOrDefaultAsync(CancellationToken);

            if (GetEntryMeritStudent == null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["EntryMeritStudent"], "Id", body.Id));

            var GetDemeritApproval = _dbContext.Entity<TrEntryDemeritStudent>()
                   .Any(e => e.IdHomeroomStudent == GetEntryMeritStudent.IdHomeroomStudent && e.IsHasBeenApproved == true && e.Status.Contains("Waiting Approval"));

            var GetMeritApproval = _dbContext.Entity<TrEntryMeritStudent>()
                   .Any(e => e.IdHomeroomStudent == GetEntryMeritStudent.IdHomeroomStudent && e.IsHasBeenApproved == true && e.Status.Contains("Waiting Approval"));
            ;
            if (GetDemeritApproval || GetMeritApproval)
                throw new BadRequestException("Sorry, you can't delete Merit for student(s) above because panding approval status still exsis");

            var GetApprovalSetting = await _dbContext.Entity<MsMeritDemeritApprovalSetting>()
               .Include(e => e.Level)
                 .Where(e => e.IdLevel == GetEntryMeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel && e.Level.IdAcademicYear == body.IdAcademicYear)
                 .SingleOrDefaultAsync(CancellationToken);

            if (GetApprovalSetting == null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ApprovalSetting"], "Id", GetEntryMeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel));

            var getPositionByUser = await _dbContext.Entity<TrNonTeachingLoad>()
                                          .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                          .Include(e => e.User)
                                          .Where(e =>
                                                  e.MsNonTeachingLoad.IdAcademicYear == body.IdAcademicYear &&
                                                  !string.IsNullOrEmpty(e.Data) &&
                                                  (e.MsNonTeachingLoad.TeacherPosition.Id == GetApprovalSetting.Approval1 ||
                                                      e.MsNonTeachingLoad.TeacherPosition.Id == GetApprovalSetting.Approval2 ||
                                                      e.MsNonTeachingLoad.TeacherPosition.Id == GetApprovalSetting.Approval3)
                                                  )
                                          .ToListAsync(CancellationToken);

            List<GetUserGrade> idGrades = new List<GetUserGrade>();
            if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal))
            {
                if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal).ToList() != null && getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal).Count() > 0)
                {
                    var Principal = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal).ToList();
                    List<string> IdLevels = new List<string>();
                    foreach (var item in Principal)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        if (_levelLH.Id == GetEntryMeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
                        {
                            idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == GetEntryMeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
                            .Select(x => new GetUserGrade
                            {
                                IdGrade = x.Id,
                                Grade = x.Description,
                                IdUser = item.User.Id,
                                Fullname = item.User.DisplayName,
                                codePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                            }).ToListAsync(CancellationToken));
                        }
                    }
                }
            }

            if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal))
            {
                if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).ToList() != null && getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).Count() > 0)
                {
                    var VicePrincipal = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).ToList();
                    List<string> IdLevels = new List<string>();
                    foreach (var item in VicePrincipal)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        if (_levelLH.Id == GetEntryMeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
                        {
                            idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == GetEntryMeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
                            .Select(x => new GetUserGrade
                            {
                                IdGrade = x.Id,
                                Grade = x.Description,
                                IdUser = item.User.Id,
                                Fullname = item.User.DisplayName,
                                codePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                            }).ToListAsync(CancellationToken));
                        }
                    }
                }
            }

            if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator))
            {
                if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator).ToList() != null && getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator).Count() > 0)
                {
                    var AffectiveCoordinator = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator).ToList();
                    List<string> IdLevels = new List<string>();
                    foreach (var item in AffectiveCoordinator)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        if (_levelLH.Id == GetEntryMeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
                        {
                            idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == GetEntryMeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
                            .Select(x => new GetUserGrade
                            {
                                IdGrade = x.Id,
                                Grade = x.Description,
                                IdUser = item.User.Id,
                                Fullname = item.User.DisplayName,
                                codePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                            }).ToListAsync(CancellationToken));
                        }
                    }
                }
            }

            if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.LevelHead).ToList() != null)
            {
                var LevelHead = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.LevelHead).ToList();
                foreach (var item in LevelHead)
                {
                    var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                    _dataNewLH.TryGetValue("Grade", out var _GradeLH);
                    if (_GradeLH.Id == GetEntryMeritStudent.HomeroomStudent.Homeroom.Grade.Id)
                    {
                        idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == GetEntryMeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
                        .Select(x => new GetUserGrade
                        {
                            IdGrade = x.Id,
                            Grade = x.Description,
                            IdUser = item.User.Id,
                            Fullname = item.User.DisplayName,
                            codePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                            IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                        }).ToListAsync(CancellationToken));
                    }
                }
            }

            var approvalUserByGrade = idGrades.Where(e => e.IdGrade == GetEntryMeritStudent.HomeroomStudent.Homeroom.IdGrade).Distinct().ToList();

            if (!string.IsNullOrEmpty(GetApprovalSetting.Approval1))
            {
                if (!approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval1))
                    throw new BadRequestException(string.Format("Approval1 dont have user"));
            }

            if (!string.IsNullOrEmpty(GetApprovalSetting.Approval2))
            {
                if (!approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval2))
                    throw new BadRequestException(string.Format("Approval2 dont have user"));
            }

            if (!string.IsNullOrEmpty(GetApprovalSetting.Approval3))
            {
                if (!approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval3))
                    throw new BadRequestException(string.Format("Approval3 dont have user"));
            }

            if (GetEntryMeritStudent != null)
            {
                GetEntryMeritStudent.IsDeleted = true;
                GetEntryMeritStudent.RequestReason = body.Note;
                GetEntryMeritStudent.RequestType = RequestType.Delete;
                GetEntryMeritStudent.Status = "Waiting Approval (1)";
                GetEntryMeritStudent.IsHasBeenApproved = true;
                _dbContext.Entity<TrEntryMeritStudent>().Update(GetEntryMeritStudent);

                var NewHsApproval = new TrStudentMeritApprovalHs
                {
                    Id = Guid.NewGuid().ToString(),
                    IdUserApproved1 = approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval1) ? approvalUserByGrade.FirstOrDefault(e => e.IdTeacherPosition == GetApprovalSetting.Approval1).IdUser : null,
                    IdUserApproved2 = approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval2) ? approvalUserByGrade.FirstOrDefault(e => e.IdTeacherPosition == GetApprovalSetting.Approval2).IdUser : null,
                    IdUserApproved3 = approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval3) ? approvalUserByGrade.FirstOrDefault(e => e.IdTeacherPosition == GetApprovalSetting.Approval3).IdUser : null,
                    RequestReason = body.Note,
                    RequestType = RequestType.Delete,
                    Status = "Waiting Approval (1)",
                    IdTrEntryMeritStudent = GetEntryMeritStudent.Id,
                };
                _dbContext.Entity<TrStudentMeritApprovalHs>().Add(NewHsApproval);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            var GetMeritStudent = await (from EntMeritStudent in _dbContext.Entity<TrEntryMeritStudent>()
                                           join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntMeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                                           join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                                           join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                                           join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                                           join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                                           join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                                           join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                                           join School in _dbContext.Entity<MsSchool>() on AcademicYear.IdSchool equals School.Id
                                           join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                                           join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntMeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                                           join LevelOfInfraction in _dbContext.Entity<MsLevelOfInteraction>() on MeritDemeritMapping.IdLevelOfInteraction equals LevelOfInfraction.Id into JoinedLevelOfInfraction
                                           from LevelOfInfraction in JoinedLevelOfInfraction.DefaultIfEmpty()
                                           join User in _dbContext.Entity<MsUser>() on EntMeritStudent.UserIn equals User.Id
                                           where EntMeritStudent.Id== GetEntryMeritStudent.Id
                                         select new
                                           {
                                               Id = EntMeritStudent.Id,
                                               IdStudent = Student.IdBinusian,
                                               StudentName = (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                               Category = "Merit",
                                               LevelOfInfracton = LevelOfInfraction.IdParentLevelOfInteraction == null ? LevelOfInfraction.NameLevelOfInteraction : LevelOfInfraction.Parent.NameLevelOfInteraction + LevelOfInfraction.NameLevelOfInteraction,
                                               DisciplineName = MeritDemeritMapping.DisciplineName,
                                               Point = EntMeritStudent.Point,
                                               Note = EntMeritStudent.Note,
                                               TeachderName = User.DisplayName,
                                               TeachderId = User.Id,
                                               CreateDate = Convert.ToDateTime(EntMeritStudent.DateMerit).ToString("dd MMM yyyy"),
                                               RequestType = EntMeritStudent.RequestType,
                                               Status = EntMeritStudent.Status,
                                               SchoolName = School.Name,
                                           }).SingleOrDefaultAsync(CancellationToken);

            var data = "<tr>" +
                                "<td>" + GetMeritStudent.IdStudent + "</td>" +
                                "<td>" + GetMeritStudent.StudentName + "</td>" +
                                "<td>" + GetMeritStudent.LevelOfInfracton + "</td>" +
                                "<td>" + GetMeritStudent.DisciplineName + "</td>" +
                                "<td>" + GetMeritStudent.Point + "</td>" +
                                "<td>" + GetMeritStudent.Note + "</td>" +
                                "<td>" + GetMeritStudent.RequestType.ToString() + "</td>" +
                                "<td>" + GetMeritStudent.Status + "</td>" +
                                "<td> Link </td>" +
                                "<td>" + GetMeritStudent.TeachderName + "</td>" +
                            "</tr>";

            var dictionary = new Dictionary<string, object>
                        {
                            { "Receivername", GetMeritStudent.TeachderName },
                            { "SchoolName", GetMeritStudent.SchoolName.ToUpper() },
                            { "Data", data },
                        };

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "DS7")
                {
                    IdRecipients = new[] { GetMeritStudent.TeachderId },
                    KeyValues = dictionary
                });
                collector.Add(message);
            }

            return Request.CreateApiResult2();
        }
    }
}

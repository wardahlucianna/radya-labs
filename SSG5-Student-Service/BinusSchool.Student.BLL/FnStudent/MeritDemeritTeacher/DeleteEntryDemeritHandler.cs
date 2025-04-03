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
    public class DeleteEntryDemeritHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public DeleteEntryDemeritHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteEntryMeritDemeritTeacherRequest, DeleteEntryMeritDemeritTeacherValidator>();

            var GetEntryDemeritStudent = await _dbContext.Entity<TrEntryDemeritStudent>()
              .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
             .Where(x => x.Id == body.Id)
             .SingleOrDefaultAsync(CancellationToken);

            if (GetEntryDemeritStudent == null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["EntryDemeritStudent"], "Id", body.Id));

            var GetDemeritApproval = _dbContext.Entity<TrEntryDemeritStudent>()
                   .Any(e => e.IdHomeroomStudent == GetEntryDemeritStudent.IdHomeroomStudent && e.IsHasBeenApproved == true && e.Status.Contains("Waiting Approval"));

            var GetMeritApproval = _dbContext.Entity<TrEntryMeritStudent>()
                   .Any(e => e.IdHomeroomStudent == GetEntryDemeritStudent.IdHomeroomStudent && e.IsHasBeenApproved == true && e.Status.Contains("Waiting Approval"));
            ;
            if (GetDemeritApproval || GetMeritApproval)
                throw new BadRequestException("Sorry, you can't delete Demerit for student(s) above because panding approval status still exsis");

            var GetApprovalSetting = await _dbContext.Entity<MsMeritDemeritApprovalSetting>()
              .Include(e => e.Level)
                .Where(e => e.IdLevel == GetEntryDemeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel && e.Level.IdAcademicYear == body.IdAcademicYear)
                .SingleOrDefaultAsync(CancellationToken);

            if (GetApprovalSetting == null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ApprovalSetting"], "Id", GetEntryDemeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel));

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
                        if (_levelLH.Id == GetEntryDemeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
                        {
                            idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == GetEntryDemeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
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
                        if (_levelLH.Id == GetEntryDemeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
                        {
                            idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == GetEntryDemeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
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
                        if (_levelLH.Id == GetEntryDemeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
                        {
                            idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == GetEntryDemeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
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
                    if (_GradeLH.Id == GetEntryDemeritStudent.HomeroomStudent.Homeroom.Grade.Id)
                    {
                        idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == GetEntryDemeritStudent.HomeroomStudent.Homeroom.Grade.IdLevel)
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

            var approvalUserByGrade = idGrades.Where(e => e.IdGrade == GetEntryDemeritStudent.HomeroomStudent.Homeroom.IdGrade).ToList();

            if (GetEntryDemeritStudent != null)
            {
                GetEntryDemeritStudent.IsDeleted = true;
                GetEntryDemeritStudent.RequestReason = body.Note;
                GetEntryDemeritStudent.RequestType = (RequestType)2;
                GetEntryDemeritStudent.Status = "Waiting Approval (1)";
                GetEntryDemeritStudent.IsHasBeenApproved = true;
                _dbContext.Entity<TrEntryDemeritStudent>().Update(GetEntryDemeritStudent);

                var NewHsApproval = new TrStudentDemeritApprovalHs
                {
                    Id = Guid.NewGuid().ToString(),
                    IdUserApproved1 = approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval1) ? approvalUserByGrade.FirstOrDefault(e => e.IdTeacherPosition == GetApprovalSetting.Approval1).IdUser : null,
                    IdUserApproved2 = approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval2) ? approvalUserByGrade.FirstOrDefault(e => e.IdTeacherPosition == GetApprovalSetting.Approval2).IdUser : null,
                    IdUserApproved3 = approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval3) ? approvalUserByGrade.FirstOrDefault(e => e.IdTeacherPosition == GetApprovalSetting.Approval3).IdUser : null,
                    RequestReason = GetEntryDemeritStudent.RequestReason,
                    IdTrEntryDemeritStudent = GetEntryDemeritStudent.Id,
                    Status = GetEntryDemeritStudent.Status,
                    RequestType = RequestType.Delete,
                };
                _dbContext.Entity<TrStudentDemeritApprovalHs>().Add(NewHsApproval);
                await _dbContext.SaveChangesAsync(CancellationToken);
            }

            return Request.CreateApiResult2();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator;
using BinusSchool.Auth.Authentications.Jwt;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{

    public class CreateInvitationBookingSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public CreateInvitationBookingSettingHandler(
            ISchedulingDbContext dbContext,
            IMachineDateTime dateTime
        )
        {
            _dateTime = dateTime;
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CreateInvitationBookingSettingRequest, CreateInvitationBookingSettingValidator>();

            var GetPeriod = await _dbContext.Entity<MsPeriod>()
                .Include(e => e.Grade).ThenInclude(e => e.Level)
              .Where(e => e.Grade.Level.IdAcademicYear == body.GeneralInfo.IdAcademicYear && (body.GeneralInfo.InvitationStartDate >= e.StartDate.Date && body.GeneralInfo.InvitationStartDate <= e.EndDate.Date))
              .FirstOrDefaultAsync(CancellationToken);

            if (body.StepWizard == 1)
            {
                if (GetPeriod is null)
                    throw new BadRequestException($"Period not found");
            }

            #region Save General Info
            if (body.StepWizard == 1)
            {
                if (body.GeneralInfo.IdInvitationBookingSetting == null)
                {
                    var isNameExist = await _dbContext.Entity<TrInvitationBookingSetting>()
                        .Where(x => body.GeneralInfo.InvitationStartDate >= x.InvitationStartDate && body.GeneralInfo.InvitationEndDate <= x.InvitationEndDate && x.InvitationName.ToLower() == body.GeneralInfo.InvitationName.ToLower())
                        .FirstOrDefaultAsync(CancellationToken);
                    if (isNameExist != null)
                        throw new BadRequestException($"{body.GeneralInfo.InvitationName} already exists in this invitation date");

                    var idInvitationBookingSetting = Guid.NewGuid().ToString();
                    var newGeneralInfo = new TrInvitationBookingSetting
                    {
                        Id = idInvitationBookingSetting,
                        IdAcademicYear = body.GeneralInfo.IdAcademicYear,
                        InvitationName = body.GeneralInfo.InvitationName,
                        InvitationStartDate = body.GeneralInfo.InvitationStartDate,
                        InvitationEndDate = body.GeneralInfo.InvitationEndDate,
                        InvitationType = body.GeneralInfo.InvitationType,
                        ParentBookingStartDate = body.GeneralInfo.ParentBookingStartDate,
                        ParentBookingEndDate = body.GeneralInfo.ParentBookingEndDate,
                        StaffBookingStartDate = body.GeneralInfo.StaffBookingStartDate != null ? body.GeneralInfo.StaffBookingStartDate : null,
                        StaffBookingEndDate = body.GeneralInfo.StaffBookingEndDate != null ? body.GeneralInfo.StaffBookingEndDate : null,
                        SchedulingSiblingSameTime = body.GeneralInfo.SchedulingSiblingSameTime,
                        FootNote = body.GeneralInfo.FootNote,
                        StepWizard = 2,
                        Status = StatusInvitationBookingSetting.Draft
                    };

                    _dbContext.Entity<TrInvitationBookingSetting>().Add(newGeneralInfo);

                    var trInvBookingSettingRoleParticipant = new List<TrInvBookingSettingRoleParticipant>();

                    foreach (var roleParticipants in body.GeneralInfo.RoleParticipants)
                    {
                        var checkDataRole = _dbContext.Entity<LtRole>()
                        .Where(x => x.Id == roleParticipants.IdRole).FirstOrDefault();

                        if (roleParticipants.IdRole != "ALL" && checkDataRole == null)
                            throw new BadRequestException("Role with Id " + roleParticipants.IdRole + " is not found");

                        var checkDataTeacherPosition = _dbContext.Entity<MsTeacherPosition>()
                        .Where(x => x.Id == roleParticipants.IdTeacherPosition).FirstOrDefault();

                        if (roleParticipants.IdTeacherPosition != "ALL" && checkDataTeacherPosition == null)
                            throw new BadRequestException("Teacher Position with Id " + roleParticipants.IdTeacherPosition + " is not found");

                        var newRoleParticipant = new TrInvBookingSettingRoleParticipant
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdRole = roleParticipants.IdRole == "ALL" ? null : roleParticipants.IdRole,
                            IdTeacherPosition = roleParticipants.IdTeacherPosition == "ALL" ? null : roleParticipants.IdTeacherPosition,
                            IdInvitationBookingSetting = newGeneralInfo.Id
                        };

                        trInvBookingSettingRoleParticipant.Add(newRoleParticipant);
                    }

                    var trInvBookingSettingExcludeSubject = new List<TrInvitationBookingSettingExcludeSub>();

                    foreach (var excludeSubjects in body.GeneralInfo.ExcludeSubjects)
                    {
                        var newExcludeSubject = new TrInvitationBookingSettingExcludeSub
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdGrade = excludeSubjects.IdGrade,
                            IdSubject = excludeSubjects.IdSubject,
                            IdInvitationBookingSetting = newGeneralInfo.Id
                        };

                        trInvBookingSettingExcludeSubject.Add(newExcludeSubject);
                    }

                    var generalInfoDetail = new List<TrInvitationBookingSettingDetail>();
                    var settingUser = new List<TrInvitationBookingSettingUser>();

                    if (body.GeneralInfo.InvitationType == InvitationType.Personal)
                    {
                        if (body.GeneralInfo.PersonalIdStudent != null)
                        {
                            foreach (var personal in body.GeneralInfo.PersonalIdStudent)
                            {
                                var dataStudent = _dbContext.Entity<MsHomeroomStudent>()
                                .Include(x => x.Student)
                                .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level)
                                .Where(x => x.IdStudent == personal && x.Semester == GetPeriod.Semester && x.Homeroom.IdAcademicYear == body.GeneralInfo.IdAcademicYear).First();

                                var newGeneralInfoDetail = new TrInvitationBookingSettingDetail
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdLevel = dataStudent.Homeroom.Grade.Level.Id,
                                    IdGrade = dataStudent.Homeroom.Grade.Id,
                                    IdHomeroom = dataStudent.IdHomeroom,
                                    IdInvitationBookingSetting = newGeneralInfo.Id
                                };
                                generalInfoDetail.Add(newGeneralInfoDetail);

                                var newSettingUser = new TrInvitationBookingSettingUser
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdHomeroomStudent = dataStudent.Id,
                                    IdInvitationBookingSetting = newGeneralInfo.Id
                                };

                                settingUser.Add(newSettingUser);
                            }
                            _dbContext.Entity<TrInvitationBookingSettingDetail>().AddRange(generalInfoDetail);
                            _dbContext.Entity<TrInvitationBookingSettingUser>().AddRange(settingUser);
                            if (trInvBookingSettingRoleParticipant.Any())
                                _dbContext.Entity<TrInvBookingSettingRoleParticipant>().AddRange(trInvBookingSettingRoleParticipant);
                        }

                        _dbContext.Entity<TrInvitationBookingSettingExcludeSub>().AddRange(trInvBookingSettingExcludeSubject);

                        await _dbContext.SaveChangesAsync(CancellationToken);
                        return Request.CreateApiResult2(new { IdInvitationBookingSetting = idInvitationBookingSetting } as object);
                    }

                    if (body.GeneralInfo.InvitationType == InvitationType.Event)
                    {
                        if (body.GeneralInfo.PersonalIdStudent != null)
                        {
                            foreach (var personal in body.GeneralInfo.PersonalIdStudent)
                            {
                                var dataStudent = _dbContext.Entity<MsHomeroomStudent>()
                                .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level)
                                .Where(x => x.IdStudent == personal & x.Semester == GetPeriod.Semester && x.Homeroom.IdAcademicYear == body.GeneralInfo.IdAcademicYear).First();

                                var newGeneralInfoDetail = new TrInvitationBookingSettingDetail
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdLevel = dataStudent.Homeroom.Grade.Level.Id,
                                    IdGrade = dataStudent.Homeroom.Grade.Id,
                                    IdHomeroom = dataStudent.IdHomeroom,
                                    IdInvitationBookingSetting = newGeneralInfo.Id
                                };
                                generalInfoDetail.Add(newGeneralInfoDetail);

                                var newSettingUser = new TrInvitationBookingSettingUser
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdHomeroomStudent = dataStudent.Id,
                                    IdInvitationBookingSetting = newGeneralInfo.Id
                                };

                                settingUser.Add(newSettingUser);
                            }
                            _dbContext.Entity<TrInvitationBookingSettingDetail>().AddRange(generalInfoDetail);
                            _dbContext.Entity<TrInvitationBookingSettingUser>().AddRange(settingUser);
                            if (trInvBookingSettingRoleParticipant.Any())
                                _dbContext.Entity<TrInvBookingSettingRoleParticipant>().AddRange(trInvBookingSettingRoleParticipant);
                        }

                        if (body.GeneralInfo.IdHomeroom != null)
                        {
                            foreach (var homeroomId in body.GeneralInfo.IdHomeroom)
                            {
                                var dataHomeroom = _dbContext.Entity<MsHomeroom>()
                                .Include(x => x.Grade).ThenInclude(x => x.Level)
                                .Where(x => x.Id == homeroomId).First();

                                var dataHomeroomStudent = _dbContext.Entity<MsHomeroomStudent>()
                                .Where(x => x.IdHomeroom == homeroomId)
                                // .GroupBy(x => x.IdStudent)
                                .ToList();

                                var newGeneralInfoDetail = new TrInvitationBookingSettingDetail
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdLevel = dataHomeroom.Grade.Level.Id,
                                    IdGrade = dataHomeroom.Grade.Id,
                                    IdHomeroom = dataHomeroom.Id,
                                    IdInvitationBookingSetting = newGeneralInfo.Id
                                };
                                generalInfoDetail.Add(newGeneralInfoDetail);

                                if (dataHomeroomStudent != null)
                                {
                                    foreach (var homeroomStudentId in dataHomeroomStudent)
                                    {
                                        var newSettingUser = new TrInvitationBookingSettingUser
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            IdHomeroomStudent = homeroomStudentId.Id,
                                            IdInvitationBookingSetting = newGeneralInfo.Id
                                        };

                                        settingUser.Add(newSettingUser);
                                    }
                                    _dbContext.Entity<TrInvitationBookingSettingUser>().AddRange(settingUser);
                                }
                            }
                            _dbContext.Entity<TrInvitationBookingSettingDetail>().AddRange(generalInfoDetail);
                        }

                        _dbContext.Entity<TrInvitationBookingSettingExcludeSub>().AddRange(trInvBookingSettingExcludeSubject);

                        await _dbContext.SaveChangesAsync(CancellationToken);
                        return Request.CreateApiResult2(new { IdInvitationBookingSetting = idInvitationBookingSetting } as object);

                    }

                    if (body.GeneralInfo.InvitationType == InvitationType.Grade)
                    {
                        if (body.GeneralInfo.IdGrade != null)
                        {
                            foreach (var grade in body.GeneralInfo.IdGrade)
                            {
                                var dataGrade = _dbContext.Entity<MsGrade>().Where(x => x.Id == grade).First();

                                var newGeneralInfoDetail = new TrInvitationBookingSettingDetail
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdLevel = dataGrade.IdLevel,
                                    IdGrade = grade,
                                    IdInvitationBookingSetting = newGeneralInfo.Id
                                };
                                generalInfoDetail.Add(newGeneralInfoDetail);
                            }
                            _dbContext.Entity<TrInvitationBookingSettingDetail>().AddRange(generalInfoDetail);

                            // var currentSemester = await _dbContext.Entity<MsPeriod>()
                            // .Where(x => x.IdGrade == body.GeneralInfo.IdGrade.FirstOrDefault())
                            // .Where(x => _dateTime.ServerTime >= x.StartDate)
                            // .Where(x => _dateTime.ServerTime <= x.EndDate)
                            // .Select(x => new
                            // {
                            //     Semester = x.Semester
                            // }).FirstOrDefaultAsync();

                            var dataHomeroomStudent = _dbContext.Entity<MsHomeroomStudent>()
                                                .Include(x => x.Homeroom)
                                                .Where(x => x.Semester == GetPeriod.Semester && body.GeneralInfo.IdGrade.Contains(x.Homeroom.IdGrade))
                                                .ToList();

                            var bookingSettingUser = new List<TrInvitationBookingSettingUser>();

                            foreach (var homeroomStudent in dataHomeroomStudent)
                            {
                                var newBookingSettingUser = new TrInvitationBookingSettingUser
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdHomeroomStudent = homeroomStudent.Id,
                                    IdInvitationBookingSetting = newGeneralInfo.Id
                                };
                                bookingSettingUser.Add(newBookingSettingUser);
                            }
                            _dbContext.Entity<TrInvitationBookingSettingUser>().AddRange(bookingSettingUser);
                        }

                        _dbContext.Entity<TrInvBookingSettingRoleParticipant>().AddRange(trInvBookingSettingRoleParticipant);
                        _dbContext.Entity<TrInvitationBookingSettingExcludeSub>().AddRange(trInvBookingSettingExcludeSubject);

                        await _dbContext.SaveChangesAsync(CancellationToken);
                        return Request.CreateApiResult2(new { IdInvitationBookingSetting = idInvitationBookingSetting } as object);
                    }
                }
                else
                {
                    var data = await _dbContext.Entity<TrInvitationBookingSetting>().FindAsync(body.GeneralInfo.IdInvitationBookingSetting);

                    if (data is null)
                        throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["InvitationBookingSetting"], "Id", body.GeneralInfo.IdInvitationBookingSetting));

                    data.IdAcademicYear = body.GeneralInfo.IdAcademicYear;
                    data.InvitationName = body.GeneralInfo.InvitationName;
                    data.InvitationStartDate = body.GeneralInfo.InvitationStartDate;
                    data.InvitationEndDate = body.GeneralInfo.InvitationEndDate;
                    data.InvitationType = body.GeneralInfo.InvitationType;
                    data.ParentBookingStartDate = body.GeneralInfo.ParentBookingStartDate;
                    data.ParentBookingEndDate = body.GeneralInfo.ParentBookingEndDate;
                    data.StaffBookingStartDate = body.GeneralInfo.StaffBookingStartDate;
                    data.StaffBookingEndDate = body.GeneralInfo.StaffBookingEndDate;
                    data.SchedulingSiblingSameTime = body.GeneralInfo.SchedulingSiblingSameTime;
                    data.FootNote = body.GeneralInfo.FootNote;
                    data.StepWizard = body.StepWizard;
                    data.Status = data.Status;

                    _dbContext.Entity<TrInvitationBookingSetting>().Update(data);

                    var settingUser = new List<TrInvitationBookingSettingUser>();

                    if (body.GeneralInfo.InvitationType == InvitationType.Personal)
                    {
                        if (body.GeneralInfo.PersonalIdStudent != null)
                        {
                            foreach (var personal in body.GeneralInfo.PersonalIdStudent)
                            {
                                var dataStudent = _dbContext.Entity<MsHomeroomStudent>()
                                .Include(x => x.Student)
                                .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level)
                                .Where(x => x.IdStudent == personal && x.Semester == GetPeriod.Semester && x.Homeroom.IdAcademicYear == body.GeneralInfo.IdAcademicYear).First();

                                var checkUserStudent = _dbContext.Entity<TrInvitationBookingSettingUser>().Where(x => x.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting && x.HomeroomStudent.IdStudent == dataStudent.IdStudent).FirstOrDefault();

                                if (checkUserStudent == null)
                                {
                                    var newSettingUser = new TrInvitationBookingSettingUser
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdHomeroomStudent = dataStudent.Id,
                                        IdInvitationBookingSetting = data.Id
                                    };

                                    settingUser.Add(newSettingUser);
                                }
                            }
                            _dbContext.Entity<TrInvitationBookingSettingUser>().AddRange(settingUser);
                        }
                    }

                    var generalInfoDetail = new List<TrInvitationBookingSettingDetail>();

                    if (body.GeneralInfo.InvitationType == InvitationType.Grade)
                    {

                        if (body.GeneralInfo.IdGrade != null)
                        {
                            var dataDetailOld = _dbContext.Entity<TrInvitationBookingSettingDetail>().Where(x => x.IdInvitationBookingSetting == data.Id).ToList();
                            dataDetailOld.ForEach(e => e.IsActive = false);
                            _dbContext.Entity<TrInvitationBookingSettingDetail>().UpdateRange(dataDetailOld);

                            var dataUserlOld = _dbContext.Entity<TrInvitationBookingSettingUser>().Where(x => x.IdInvitationBookingSetting == data.Id).ToList();
                            dataUserlOld.ForEach(e => e.IsActive = false);
                            _dbContext.Entity<TrInvitationBookingSettingUser>().UpdateRange(dataUserlOld);

                            foreach (var grade in body.GeneralInfo.IdGrade)
                            {
                                var dataGrade = _dbContext.Entity<MsGrade>().Where(x => x.Id == grade).First();

                                var newGeneralInfoDetail = new TrInvitationBookingSettingDetail
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdLevel = dataGrade.IdLevel,
                                    IdGrade = grade,
                                    IdInvitationBookingSetting = data.Id
                                };
                                generalInfoDetail.Add(newGeneralInfoDetail);
                            }
                            _dbContext.Entity<TrInvitationBookingSettingDetail>().AddRange(generalInfoDetail);

                            var dataHomeroomStudent = _dbContext.Entity<MsHomeroomStudent>()
                                                .Include(x => x.Homeroom)
                                                .Where(x => x.Semester == GetPeriod.Semester && body.GeneralInfo.IdGrade.Contains(x.Homeroom.IdGrade))
                                                .ToList();

                            var bookingSettingUser = new List<TrInvitationBookingSettingUser>();

                            foreach (var homeroomStudent in dataHomeroomStudent)
                            {
                                var newBookingSettingUser = new TrInvitationBookingSettingUser
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdHomeroomStudent = homeroomStudent.Id,
                                    IdInvitationBookingSetting = data.Id
                                };
                                bookingSettingUser.Add(newBookingSettingUser);
                            }
                            _dbContext.Entity<TrInvitationBookingSettingUser>().AddRange(bookingSettingUser);
                        }
                    }

                    var trInvBookingSettingRoleParticipant = new List<TrInvBookingSettingRoleParticipant>();

                    #region Delete Data Role Participant Before Create
                    var getRoleParticipants = await _dbContext.Entity<TrInvBookingSettingRoleParticipant>()
                                              .Where(e => e.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting)
                                              .ToListAsync(CancellationToken);
                    getRoleParticipants.ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrInvBookingSettingRoleParticipant>().UpdateRange(getRoleParticipants);
                    #endregion

                    foreach (var roleParticipants in body.GeneralInfo.RoleParticipants)
                    {
                        var checkDataRole = _dbContext.Entity<LtRole>()
                        .Where(x => x.Id == roleParticipants.IdRole).FirstOrDefault();

                        if (roleParticipants.IdRole != "ALL" && checkDataRole == null)
                            throw new BadRequestException("Role with Id " + roleParticipants.IdRole + " is not found");

                        var checkDataTeacherPosition = _dbContext.Entity<MsTeacherPosition>()
                        .Where(x => x.Id == roleParticipants.IdTeacherPosition).FirstOrDefault();

                        if (roleParticipants.IdTeacherPosition != "ALL" && checkDataTeacherPosition == null)
                            throw new BadRequestException("Teacher Position with Id " + roleParticipants.IdTeacherPosition + " is not found");

                        var newRoleParticipant = new TrInvBookingSettingRoleParticipant
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdRole = roleParticipants.IdRole == "ALL" ? null : roleParticipants.IdRole,
                            IdTeacherPosition = roleParticipants.IdTeacherPosition == "ALL" ? null : roleParticipants.IdTeacherPosition,
                            IdInvitationBookingSetting = data.Id
                        };

                        trInvBookingSettingRoleParticipant.Add(newRoleParticipant);
                    }

                    var trInvBookingSettingExcludeSubject = new List<TrInvitationBookingSettingExcludeSub>();

                    #region Delete Data Exclude Subject Before Create
                    var getExcludeSubjects = await _dbContext.Entity<TrInvitationBookingSettingExcludeSub>()
                                              .Where(e => e.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting)
                                              .ToListAsync(CancellationToken);
                    getExcludeSubjects.ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrInvitationBookingSettingExcludeSub>().UpdateRange(getExcludeSubjects);
                    #endregion

                    foreach (var excludeSubjects in body.GeneralInfo.ExcludeSubjects)
                    {
                        var newExcludeSubject = new TrInvitationBookingSettingExcludeSub
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdGrade = excludeSubjects.IdGrade,
                            IdSubject = excludeSubjects.IdSubject,
                            IdInvitationBookingSetting = data.Id
                        };

                        trInvBookingSettingExcludeSubject.Add(newExcludeSubject);
                    }

                    _dbContext.Entity<TrInvBookingSettingRoleParticipant>().AddRange(trInvBookingSettingRoleParticipant);
                    _dbContext.Entity<TrInvitationBookingSettingExcludeSub>().AddRange(trInvBookingSettingExcludeSubject);

                    await _dbContext.SaveChangesAsync(CancellationToken);
                    return Request.CreateApiResult2(new { IdInvitationBookingSetting = body.GeneralInfo.IdInvitationBookingSetting } as object);
                }
            }
            #endregion

            #region Save User Venue Mapping
            if (body.StepWizard == 2)
            {
                var userVenueMapping = new List<TrInvitationBookingSettingVenueDate>();
                var userVenueMappingDetail = new List<TrInvitationBookingSettingVenueDtl>();
                if (body.UserVenueMapping.Count > 0)
                {
                    var dataVenue = await _dbContext.Entity<TrInvitationBookingSettingVenueDate>().Where(x => x.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting).ToListAsync();
                    if (dataVenue != null)
                    {
                        foreach (var deleted in dataVenue)
                        {
                            var dataVenueDtl = await _dbContext.Entity<TrInvitationBookingSettingVenueDtl>().Where(x => x.IdInvitationBookingSettingVenueDate == deleted.Id).ToListAsync();
                            if (dataVenueDtl != null)
                            {
                                foreach (var deletedVenueDtl in dataVenueDtl)
                                {
                                    deletedVenueDtl.IsActive = false;
                                    _dbContext.Entity<TrInvitationBookingSettingVenueDtl>().Update(deletedVenueDtl);
                                }
                            }
                            deleted.IsActive = false;
                            _dbContext.Entity<TrInvitationBookingSettingVenueDate>().Update(deleted);
                        }
                    }
                    foreach (var VenueMapping in body.UserVenueMapping)
                    {
                        // if(VenueMapping.IdInvitationBookingSettingVenueDate == null)
                        // {

                        // foreach (DateTime dateInvitation in VenueMapping.InvitationDate)
                        // {
                        var IdVenueMapping = Guid.NewGuid().ToString();
                        var newVenueMapping = new TrInvitationBookingSettingVenueDate
                        {
                            Id = IdVenueMapping,
                            IdInvitationBookingSetting = VenueMapping.IdInvitationBookingSetting,
                            DateInvitationExact = VenueMapping.InvitationDate.Min(),
                            DateInvitation = String.Join("||", VenueMapping.InvitationDate.Select(x => x.ToString("yyyy-MM-dd")).ToList())
                        };

                        userVenueMapping.Add(newVenueMapping);

                        foreach (var venueMappingDetail in VenueMapping.DataUserVenue)
                        {
                            var newVenueMappingDetail = new TrInvitationBookingSettingVenueDtl
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdUserTeacher = venueMappingDetail.IdUserTeacher,
                                IdInvitationBookingSettingVenueDate = IdVenueMapping,
                                IdVenue = venueMappingDetail.IdVenue,
                                IdRole = venueMappingDetail.IdRole,
                                IdTeacherPosition = venueMappingDetail.IdTeacherPosition,
                            };

                            userVenueMappingDetail.Add(newVenueMappingDetail);
                        }
                        // }
                        _dbContext.Entity<TrInvitationBookingSettingVenueDate>().AddRange(userVenueMapping);
                        _dbContext.Entity<TrInvitationBookingSettingVenueDtl>().AddRange(userVenueMappingDetail);
                        // }
                        // else
                        // {
                        //     var dataVenue = await _dbContext.Entity<TrInvitationBookingSettingVenueDate>().Where(x => x.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting).ToListAsync();
                        //     if (dataVenue != null)
                        //     {
                        //         foreach (var deleted in dataVenue)
                        //         {
                        //             var dataVenueDtl = await _dbContext.Entity<TrInvitationBookingSettingVenueDtl>().Where(x => x.IdInvitationBookingSettingVenueDate == deleted.Id).ToListAsync();
                        //             if (dataVenueDtl != null)
                        //             {
                        //                 foreach (var deletedVenueDtl in dataVenueDtl)
                        //                 {
                        //                     deletedVenueDtl.IsActive = false;
                        //                     _dbContext.Entity<TrInvitationBookingSettingVenueDtl>().Update(deletedVenueDtl);
                        //                 }
                        //             }
                        //             deleted.IsActive = false;
                        //             _dbContext.Entity<TrInvitationBookingSettingVenueDate>().Update(deleted);
                        //         }
                        //     }

                        //     foreach (DateTime dateInvitation in VenueMapping.InvitationDate)
                        //     {
                        //         var IdVenueMapping = Guid.NewGuid().ToString();
                        //         var newVenueMapping = new TrInvitationBookingSettingVenueDate
                        //         {
                        //             Id = IdVenueMapping,
                        //             IdInvitationBookingSetting = VenueMapping.IdInvitationBookingSetting,
                        //             DateInvitationExact = dateInvitation,
                        //             DateInvitation = String.Join("||", VenueMapping.InvitationDate.Select(x => x.ToString("yyyy-MM-dd")).ToList())
                        //         };

                        //         userVenueMapping.Add(newVenueMapping);

                        //         foreach (var VenueMappingDetail in userVenueMapping)
                        //         {
                        //             var newVenueMappingDetail = new TrInvitationBookingSettingVenueDtl
                        //             {
                        //                 Id = Guid.NewGuid().ToString(),
                        //                 IdUserTeacher = VenueMapping.IdUserTeacher,
                        //                 IdInvitationBookingSettingVenueDate = IdVenueMapping,
                        //                 IdVenue = VenueMapping.IdVenue

                        //             };

                        //             userVenueMappingDetail.Add(newVenueMappingDetail);
                        //         }
                        //     }
                        //     _dbContext.Entity<TrInvitationBookingSettingVenueDate>().AddRange(userVenueMapping);
                        //     _dbContext.Entity<TrInvitationBookingSettingVenueDtl>().AddRange(userVenueMappingDetail);
                        // }

                    }
                }

                var data = _dbContext.Entity<TrInvitationBookingSetting>().Where(x => x.Id == body.GeneralInfo.IdInvitationBookingSetting).First();
                if (data is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["InvitationBookingSetting"], "Id", body.GeneralInfo.IdInvitationBookingSetting));

                if (data.StepWizard > 2)
                {
                    data.StepWizard = data.StepWizard;
                }
                else
                {
                    data.StepWizard = 3;
                }

                _dbContext.Entity<TrInvitationBookingSetting>().Update(data);
            }
            #endregion

            #region Save Setting Quota & Duration
            if (body.StepWizard == 3)
            {
                var settingQuotaDuration = new List<TrInvitationBookingSettingQuota>();

                if (body.SettingQuotaDuration.Count > 0)
                {
                    if (body.SettingQuotaDuration.Any(x => x.IdInvitationBookingSettingQuota == null))
                    {
                        var dataQuotaDurationDelete = await _dbContext.Entity<TrInvitationBookingSettingQuota>().Where(x => x.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting).ToListAsync(CancellationToken);
                        if (dataQuotaDurationDelete.Count() > 0)
                        {
                            foreach (var deleteQuotaDuration in dataQuotaDurationDelete)
                            {
                                deleteQuotaDuration.IsActive = false;
                                _dbContext.Entity<TrInvitationBookingSettingQuota>().Update(deleteQuotaDuration);
                            }
                        }
                    }

                    foreach (var QuotaDuration in body.SettingQuotaDuration)
                    {

                        if (QuotaDuration.StartTime == QuotaDuration.EndTime)
                            throw new BadRequestException($"Start Time : {QuotaDuration.StartTime} same with End Time : {QuotaDuration.EndTime}");

                        if (QuotaDuration.StartTime > QuotaDuration.EndTime)
                            throw new BadRequestException($"Start Time : {QuotaDuration.StartTime} is more than End Time : {QuotaDuration.EndTime}");

                        if (QuotaDuration.IdInvitationBookingSettingQuota == null)
                        {
                            var IdQuotaDuration = Guid.NewGuid().ToString();
                            var newQuotaDuration = new TrInvitationBookingSettingQuota
                            {
                                Id = IdQuotaDuration,
                                IdInvitationBookingSetting = QuotaDuration.IdInvitationBookingSetting,
                                SettingType = QuotaDuration.SettingType,
                                IdGrade = QuotaDuration.IdGrade != null ? QuotaDuration.IdGrade : null,
                                DateInvitation = QuotaDuration.DateInvitation,
                                StartTime = QuotaDuration.StartTime,
                                EndTime = QuotaDuration.EndTime,
                                BreakBetweenSession = QuotaDuration.BreakBetweenSession != null ? QuotaDuration.BreakBetweenSession : null,
                                QuotaSlot = QuotaDuration.QuotaSlot,
                                Duration = QuotaDuration.Duration
                            };

                            settingQuotaDuration.Add(newQuotaDuration);

                            _dbContext.Entity<TrInvitationBookingSettingQuota>().AddRange(settingQuotaDuration);
                        }
                        else
                        {
                            var dataBreakSettingDelete = await _dbContext.Entity<TrInvitationBookingSettingBreak>().Where(x => x.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting).ToListAsync(CancellationToken);
                            if (dataBreakSettingDelete.Count() > 0)
                            {
                                foreach (var deleteBreakSetting in dataBreakSettingDelete)
                                {
                                    deleteBreakSetting.IsActive = false;
                                    _dbContext.Entity<TrInvitationBookingSettingBreak>().Update(deleteBreakSetting);
                                }
                            }

                            var dataQuotaDuration = await _dbContext.Entity<TrInvitationBookingSettingQuota>().Where(x => x.Id == QuotaDuration.IdInvitationBookingSettingQuota).FirstOrDefaultAsync(CancellationToken);

                            if (dataQuotaDuration is null)
                                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["InvitationBookingSettingQuota"], "Id", body.GeneralInfo.IdInvitationBookingSetting));

                            dataQuotaDuration.Id = QuotaDuration.IdInvitationBookingSettingQuota;
                            dataQuotaDuration.IdInvitationBookingSetting = QuotaDuration.IdInvitationBookingSetting;
                            dataQuotaDuration.SettingType = QuotaDuration.SettingType;
                            dataQuotaDuration.IdGrade = QuotaDuration.IdGrade != null ? QuotaDuration.IdGrade : null;
                            dataQuotaDuration.DateInvitation = QuotaDuration.DateInvitation;
                            dataQuotaDuration.StartTime = QuotaDuration.StartTime;
                            dataQuotaDuration.EndTime = QuotaDuration.EndTime;
                            dataQuotaDuration.BreakBetweenSession = QuotaDuration.BreakBetweenSession != null ? QuotaDuration.BreakBetweenSession : null;
                            dataQuotaDuration.QuotaSlot = QuotaDuration.QuotaSlot;
                            dataQuotaDuration.Duration = QuotaDuration.Duration;

                            _dbContext.Entity<TrInvitationBookingSettingQuota>().UpdateRange(dataQuotaDuration);
                        }
                    }
                }

                var data = _dbContext.Entity<TrInvitationBookingSetting>().Where(x => x.Id == body.GeneralInfo.IdInvitationBookingSetting).First();
                if (data is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["InvitationBookingSetting"], "Id", body.GeneralInfo.IdInvitationBookingSetting));

                if (data.StepWizard > 3)
                {
                    data.StepWizard = data.StepWizard;
                }
                else
                {
                    data.StepWizard = 4;
                }

                _dbContext.Entity<TrInvitationBookingSetting>().Update(data);
            }
            #endregion

            #region Save Break Setting
            if (body.StepWizard == 4)
            {
                var dataUserVenueDtl = await _dbContext.Entity<TrInvitationBookingSettingVenueDtl>()
                    .Include(x => x.InvitationBookingSettingVenueDate).ThenInclude(x => x.InvitationBookingSetting).ThenInclude(x => x.InvitationBookingSettingQuotas)
                    .Where(x => x.InvitationBookingSettingVenueDate.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting)
                    .ToListAsync(CancellationToken);

                var data = _dbContext.Entity<TrInvitationBookingSetting>().Where(x => x.Id == body.GeneralInfo.IdInvitationBookingSetting).First();
                if (data is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["InvitationBookingSetting"], "Id", body.GeneralInfo.IdInvitationBookingSetting));

                if (data.StepWizard > 4)
                {
                    data.StepWizard = data.StepWizard;
                }
                else
                {
                    data.StepWizard = 4;
                }

                _dbContext.Entity<TrInvitationBookingSetting>().Update(data);

                #region Remove Generate
                var UpdateSchedule = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                                                    .Where(e => e.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting)
                                                    .ToListAsync(CancellationToken);

                if (UpdateSchedule.Any())
                {
                    UpdateSchedule.ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrInvitationBookingSettingSchedule>().UpdateRange(UpdateSchedule);
                }
                #endregion

                #region Generade Schedule
                var GetInvitationBookingSetting = await _dbContext.Entity<TrInvitationBookingSetting>()
                                                    .Where(e => e.Id == body.GeneralInfo.IdInvitationBookingSetting)
                                                    .FirstOrDefaultAsync(CancellationToken);
                if (GetInvitationBookingSetting == null)
                    throw new BadRequestException($"Id invitation booking setting : {body.GeneralInfo.IdInvitationBookingSetting} is not found");

                var GetInvitationBookingSettingQuota = await _dbContext.Entity<TrInvitationBookingSettingQuota>()
                                                    .Where(e => e.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting)
                                                    .ToListAsync(CancellationToken);

                if (!GetInvitationBookingSettingQuota.Any())
                    throw new BadRequestException("Invitation booking setting quota is not found");

                var GetInvitationBookingSettingBreak = await _dbContext.Entity<TrInvitationBookingSettingBreak>()
                                                    .Where(e => e.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting)
                                                    .ToListAsync(CancellationToken);

                var GetInvitationBookingSettingVenueDate = await _dbContext.Entity<TrInvitationBookingSettingVenueDate>()
                                    .Where(e => e.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting)
                                    .ToListAsync(CancellationToken);

                var dataPersonalInvitation = await _dbContext.Entity<TrPersonalInvitation>()
                                    .Include(e => e.Student)
                                    .Include(e => e.Venue)
                                     .Where(e => e.InvitationDate.Date >= GetInvitationBookingSetting.InvitationStartDate && e.InvitationDate.Date <= GetInvitationBookingSetting.InvitationEndDate
                                        && e.IdAcademicYear == GetInvitationBookingSetting.IdAcademicYear
                                        && (e.Status == PersonalInvitationStatus.OnRequest || e.Status == PersonalInvitationStatus.Approved))
                                     .ToListAsync(CancellationToken);

                if (!GetInvitationBookingSettingVenueDate.Any())
                    throw new BadRequestException("Invitation booking setting venue is not found");

                var GetSchedule = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                                                    .Where(e => e.DateInvitation.Date >= GetInvitationBookingSetting.InvitationStartDate.Date
                                                        && e.DateInvitation.Date <= GetInvitationBookingSetting.InvitationEndDate.Date
                                                        && e.IdInvitationBookingSetting != body.GeneralInfo.IdInvitationBookingSetting
                                                        && (e.IsPriority == true || e.IsFlexibleBreak == true || e.IsFixedBreak == true)
                                                    )
                                                    .ToListAsync(CancellationToken); ;


                var StartDateInvitaiton = GetInvitationBookingSetting.InvitationStartDate;
                var EndDateInvitaiton = GetInvitationBookingSetting.InvitationEndDate;

                List<GetGenerateSchedule> GetGenerateSchedule = new List<GetGenerateSchedule>();
                for (var day = StartDateInvitaiton; day.Date <= EndDateInvitaiton; day = day.AddDays(1))
                {
                    TrInvitationBookingSettingQuota GetInvitationBookingSettingQuotaByDate = default;
                    if (GetInvitationBookingSettingQuota.FirstOrDefault().SettingType == 2)
                    {
                        GetInvitationBookingSettingQuotaByDate = GetInvitationBookingSettingQuota.FirstOrDefault();
                    }
                    else
                    {
                        GetInvitationBookingSettingQuotaByDate = GetInvitationBookingSettingQuota.Where(e => e.DateInvitation.Date == day.Date).FirstOrDefault();

                        if (GetInvitationBookingSettingQuotaByDate == null)
                            continue;
                    }

                    var GetFixbreakByDate = GetInvitationBookingSettingBreak.Where(e => e.DateInvitation.Date == day.Date && e.BreakType == BreakType.Fixed).ToList();
                    var GetFlexiblebreakByDate = GetInvitationBookingSettingBreak.Where(e => e.DateInvitation.Date == day.Date && e.BreakType == BreakType.Flexible).ToList();

                    var StartTime = GetInvitationBookingSettingQuotaByDate.StartTime;
                    var EndTime = GetInvitationBookingSettingQuotaByDate.StartTime;
                    var Index = 0;
                    var IndexSession = 1;
                    do
                    {
                        bool isBreakFix = false;

                        if (Index != 0)
                            StartTime = EndTime;

                        #region FixBreak
                        var GetFixBerakByDateTime = GetFixbreakByDate.Where(e => (StartTime >= e.StartTime && StartTime < e.EndTime)).FirstOrDefault();

                        if (GetFixBerakByDateTime != null)
                        {
                            var EndTimeBreak = GetFixBerakByDateTime.EndTime;
                            EndTime = GetFixBerakByDateTime.EndTime.Add(TimeSpan.FromMinutes(Convert.ToInt32(GetInvitationBookingSettingQuotaByDate.BreakBetweenSession)));
                            var Duration = ((EndTime - StartTime).Hours * 60) + ((EndTime - StartTime).Minutes);
                            GetGenerateSchedule.Add(new GetGenerateSchedule
                            {
                                Date = day.Date,
                                StartTime = StartTime,
                                EndTime = EndTimeBreak,
                                Description = GetFixBerakByDateTime.BreakName,
                                IsFixBreak = true,
                                IsFlexibleBreak = false,
                                Quota = 0,
                                Duration = Duration
                            });
                            Index++;
                            continue;
                        }
                        #endregion

                        #region Session
                        EndTime = StartTime.Add(TimeSpan.FromMinutes(GetInvitationBookingSettingQuotaByDate.Duration));

                        if (EndTime > GetInvitationBookingSettingQuotaByDate.EndTime)
                            break;

                        var GetFlexiblebreakByDateTime = GetFlexiblebreakByDate.Where(e => (StartTime >= e.StartTime && StartTime < e.EndTime)).FirstOrDefault();


                        GetGenerateSchedule.Add(new GetGenerateSchedule
                        {
                            Date = day.Date,
                            StartTime = StartTime,
                            EndTime = EndTime,
                            Description = "Session " + IndexSession,
                            IsFixBreak = false,
                            IsFlexibleBreak = GetFlexiblebreakByDateTime == null ? false : true,
                            Quota = GetInvitationBookingSettingQuotaByDate.QuotaSlot,
                            Duration = GetInvitationBookingSettingQuotaByDate.Duration,
                            IdBreakFlexible = GetFlexiblebreakByDateTime == null ? null : GetFlexiblebreakByDateTime.Id,
                            NameBreakFlexible = GetFlexiblebreakByDateTime == null ? null : GetFlexiblebreakByDateTime.BreakName,
                        });
                        #endregion

                        #region Break Session
                        if (GetInvitationBookingSettingQuotaByDate.BreakBetweenSession > 0)
                        {
                            StartTime = EndTime;
                            EndTime = StartTime.Add(TimeSpan.FromMinutes(Convert.ToInt32(GetInvitationBookingSettingQuotaByDate.BreakBetweenSession)));

                            if (EndTime >= GetInvitationBookingSettingQuotaByDate.EndTime)
                                break;

                            GetGenerateSchedule.Add(new GetGenerateSchedule
                            {
                                Date = day.Date,
                                StartTime = StartTime,
                                EndTime = EndTime,
                                Description = "Break Between Session",
                                IsFixBreak = false,
                                IsFlexibleBreak = GetFlexiblebreakByDateTime == null ? false : true,
                                Quota = 0,
                                Duration = Convert.ToInt32(GetInvitationBookingSettingQuotaByDate.BreakBetweenSession),
                            });
                        }
                        #endregion

                        if (!isBreakFix)
                        {
                            IndexSession++;
                        }
                        Index++;
                    } while (EndTime < GetInvitationBookingSettingQuotaByDate.EndTime);

                }

                foreach (var itemDate in GetInvitationBookingSettingVenueDate)
                {
                    List<DateTime> DayTeacher = new List<DateTime>();
                    var DateVenueTeacher = itemDate.DateInvitation;
                    var SpitDateVanueTeacher = DateVenueTeacher.Split("||");

                    if (SpitDateVanueTeacher.Count() > 0)
                    {
                        foreach (var ItemDate in SpitDateVanueTeacher)
                        {
                            DayTeacher.Add(Convert.ToDateTime(ItemDate));
                        }
                    }

                    var GetIdUserTeacher = itemDate.InvitationBookingSettingVenueDtl.ToList();

                    foreach (var ItemDateInvitatin in DayTeacher)
                    {
                        foreach (var ItemTeacher in GetIdUserTeacher)
                        {
                            foreach (var ItemSchedule in GetGenerateSchedule.Where(e => e.Date.Date == ItemDateInvitatin.Date).ToList())
                            {
                                var ExsisDisabled = GetSchedule
                                                .Where(e => e.IdUserTeacher == ItemTeacher.IdUserTeacher
                                                && ((ItemSchedule.StartTime >= e.StartTime && ItemSchedule.StartTime < e.EndTime) || (ItemSchedule.EndTime > e.StartTime && ItemSchedule.EndTime < e.EndTime))
                                                ).Any();

                                var exsistPersonalInvitation = dataPersonalInvitation.Where(e => e.InvitationDate.Date == ItemDateInvitatin.Date
                                                            && ((ItemSchedule.StartTime >= e.InvitationStartTime && ItemSchedule.StartTime < e.InvitationEndTime) || (ItemSchedule.EndTime > e.InvitationStartTime && ItemSchedule.EndTime < e.InvitationEndTime))
                                                            && e.IdUserTeacher == ItemTeacher.IdUserTeacher).Any();


                                var NewInvitationBookingSettingSchedule = new TrInvitationBookingSettingSchedule
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    StartTime = ItemSchedule.StartTime,
                                    EndTime = ItemSchedule.EndTime,
                                    Description = ItemSchedule.Description,
                                    QuotaSlot = ItemSchedule.Quota,
                                    Duration = ItemSchedule.Duration,
                                    IsPriority = null,
                                    IsFlexibleBreak = null,
                                    IsFixedBreak = ItemSchedule.IsFixBreak,
                                    DateInvitation = ItemSchedule.Date,
                                    IsAvailable = exsistPersonalInvitation != true,
                                    IdInvitationBookingSetting = body.GeneralInfo.IdInvitationBookingSetting,
                                    IdUserTeacher = ItemTeacher.IdUserTeacher,
                                    IdInvitationBookingSettingBreak = ItemSchedule.IdBreakFlexible == null ? null : ItemSchedule.IdBreakFlexible,
                                    BreakName = ItemSchedule.NameBreakFlexible == null ? null : ItemSchedule.NameBreakFlexible,
                                    IdVenue = ItemTeacher.IdVenue,
                                    IsDisabledAvailable = exsistPersonalInvitation == true ? true : ExsisDisabled,
                                    IsDisabledPriority = exsistPersonalInvitation == true ? true : ExsisDisabled,
                                    IsDisabledFlexible = ExsisDisabled
                                };

                                _dbContext.Entity<TrInvitationBookingSettingSchedule>().Add(NewInvitationBookingSettingSchedule);


                                var updateExistSetting = GetSchedule
                                                    .Where(e => e.IdUserTeacher == ItemTeacher.IdUserTeacher
                                                    && ((ItemSchedule.StartTime >= e.StartTime && ItemSchedule.StartTime < e.EndTime) || (ItemSchedule.EndTime > e.StartTime && ItemSchedule.EndTime < e.EndTime))
                                                    ).ToList();

                                foreach (var item in updateExistSetting)
                                {
                                    item.IsDisabledPriority = false;
                                    _dbContext.Entity<TrInvitationBookingSettingSchedule>().Update(item);
                                }

                            }

                        }
                    }

                }




                #endregion
            }
            #endregion

            #region Save Last
            if (body.StepWizard == 5)
            {
                var data = _dbContext.Entity<TrInvitationBookingSetting>()
                    .Include(e => e.AcademicYears)
                    .Include(e => e.InvitationBookingSettingDetails)
                    .Where(x => x.Id == body.GeneralInfo.IdInvitationBookingSetting).First();

                if (data is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["InvitationBookingSetting"], "Id", body.GeneralInfo.IdInvitationBookingSetting));

                if (data.StepWizard > 5)
                {
                    data.StepWizard = data.StepWizard;
                }
                else
                {
                    data.StepWizard = 5;
                }

                if (data.Status != StatusInvitationBookingSetting.Published)
                {
                    var dataVenueMapping = _dbContext.Entity<TrInvitationBookingSettingVenueDtl>()
                    .Include(e => e.InvitationBookingSettingVenueDate)
                        .ThenInclude(e => e.InvitationBookingSetting)
                    .Where(x => x.InvitationBookingSettingVenueDate.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting).ToList();

                    var EmailInvitatinBookingParent = new EmailInvitationBookingSettingResult
                    {
                        IdInvitationBookingSetting = data.Id,
                        AcademicYear = data.AcademicYears.Description,
                        InvitationName = data.InvitationName,
                        InvitationStartDate = Convert.ToDateTime(data.InvitationStartDate).ToString("dd MMM yyyy"),
                        InvitationEndDate = Convert.ToDateTime(data.InvitationEndDate).ToString("dd MMM yyyy"),
                        ParentBookingStartDate = Convert.ToDateTime(data.ParentBookingStartDate).ToString("dd MMM yyyy HH:mm"),
                        ParentBookingEndDate = Convert.ToDateTime(data.ParentBookingEndDate).ToString("dd MMM yyyy HH:mm"),
                        StaffBookingStartDate = Convert.ToDateTime(data.StaffBookingStartDate).ToString("dd MMM yyyy HH:mm"),
                        StaffBookingEndDate = Convert.ToDateTime(data.StaffBookingEndDate).ToString("dd MMM yyyy HH:mm"),
                        IdListTeacher = dataVenueMapping.Select(e => e.IdUserTeacher).ToList(),
                        IdSchool = data.AcademicYears.IdSchool
                    };

                    var studentRecipients = new List<string>();

                    // Personal type, get specific user only
                    if (data.InvitationType == InvitationType.Personal)
                    {
                        var dataSettingUsers = await _dbContext.Entity<TrInvitationBookingSettingUser>()
                            .Where(x => x.IdInvitationBookingSetting == body.GeneralInfo.IdInvitationBookingSetting)
                            .Select(x => x.HomeroomStudent.IdStudent)
                            .ToListAsync(CancellationToken);

                        if (data.SchedulingSiblingSameTime)
                        {
                            var siblingGroupIds = await _dbContext.Entity<MsSiblingGroup>()
                                .Where(x => dataSettingUsers.Contains(x.IdStudent)).Select(x => x.Id).ToListAsync(CancellationToken);

                            var siblingStudentIds = await _dbContext.Entity<MsSiblingGroup>()
                                .Where(x => siblingGroupIds.Contains(x.Id)).Select(x => x.IdStudent).ToListAsync(CancellationToken);

                            dataSettingUsers = siblingStudentIds;
                        }

                        dataSettingUsers = dataSettingUsers.Distinct().ToList();

                        studentRecipients.AddRange(dataSettingUsers);
                    }
                    // Get all student data by selected date
                    else
                    {
                        if (!data.InvitationBookingSettingDetails.Any() || data.InvitationBookingSettingDetails == null)
                            throw new NotFoundException("Invitation booking setting details not found");

                        var idGrades = data.InvitationBookingSettingDetails.Select(x => x.IdGrade).ToList();
                        var studentGrades = await _dbContext.Entity<MsStudentGrade>()
                            .Where(x => idGrades.Contains(x.IdGrade))
                            .Select(x => x.IdStudent)
                            .ToListAsync(CancellationToken);

                        if (data.SchedulingSiblingSameTime)
                        {
                            var siblingGroupIds = await _dbContext.Entity<MsSiblingGroup>()
                                .Where(x => studentGrades.Contains(x.IdStudent)).Select(x => x.Id).ToListAsync(CancellationToken);

                            var siblingStudentIds = await _dbContext.Entity<MsSiblingGroup>()
                                .Where(x => siblingGroupIds.Contains(x.Id)).Select(x => x.IdStudent).ToListAsync(CancellationToken);

                            studentGrades = siblingStudentIds;
                        }

                        studentGrades = studentGrades.Distinct().ToList();

                        studentRecipients.AddRange(studentGrades);
                    }

                    if (KeyValues.ContainsKey("GetInvitationBookingSettingEmail"))
                        KeyValues.Remove("GetInvitationBookingSettingEmail");

                    KeyValues.Add("GetInvitationBookingSettingEmail", EmailInvitatinBookingParent);
                    APP1Notification(KeyValues, AuthInfo);

                    if (KeyValues.ContainsKey("GetInvitationBookingSettingParentEmail"))
                        KeyValues.Remove("GetInvitationBookingSettingParentEmail");

                    KeyValues.Add("GetInvitationBookingSettingParentEmail", EmailInvitatinBookingParent);
                    APP2Notification(KeyValues, AuthInfo, studentRecipients);
                }

                data.Status = StatusInvitationBookingSetting.Published;

                _dbContext.Entity<TrInvitationBookingSetting>().Update(data);
            }
            #endregion

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        public static bool APP1Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetInvitationBookingSettingEmail").Value;
            var EmailInvitation = JsonConvert.DeserializeObject<EmailInvitationBookingSettingResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP1")
                {
                    IdRecipients = new List<string>()
                    {
                        EmailInvitation.IdListTeacher.First(),
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return true;
        }

        public static bool APP2Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, List<string> studentRecipients)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetInvitationBookingSettingParentEmail").Value;
            var EmailInvitation = JsonConvert.DeserializeObject<EmailInvitationBookingSettingResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP2")
                {
                    IdRecipients = studentRecipients,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return true;
        }

    }
}

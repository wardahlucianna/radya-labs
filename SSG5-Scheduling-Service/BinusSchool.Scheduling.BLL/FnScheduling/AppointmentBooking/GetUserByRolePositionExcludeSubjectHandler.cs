using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using Newtonsoft.Json;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.HODAndSH;
using FluentEmail.Core;
using BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator;
using Microsoft.Azure.Documents.SystemFunctions;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetUserByRolePositionExcludeSubjectHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetUserByRolePositionExcludeSubjectHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetUserByRolePositionExcludeSubjectRequest, GetUserByRolePositionExcludeSubjectRequestValidator>();

            var CheckRole = await (from a in _dbContext.Entity<MsUser>()
                                   join us in _dbContext.Entity<MsUserSchool>() on a.Id equals us.IdUser
                                   join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                   join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                   where r.IdRole == body.IdRole
                                    
                                  select new
                                  {
                                      IdSchool = us.IdSchool,
                                      IdRole = rg.Id,
                                      IdRoleGroup = rg.IdRoleGroup
                                  }).FirstOrDefaultAsync(CancellationToken);

            if(CheckRole == null)
                throw new BadRequestException($"User in this role not found");

            if(CheckRole.IdRoleGroup == "STD")
            {
                var dataUser = await (from a in _dbContext.Entity<MsUser>()
                                   join us in _dbContext.Entity<MsUserSchool>() on a.Id equals us.IdUser
                                   join s in _dbContext.Entity<MsStudent>() on a.Id equals s.Id
                                   where us.IdSchool == CheckRole.IdSchool
                                  select new GetUserByRolePositionExcludeSubjectResult
                                  {
                                      Id = a.Id,
                                      Fullname = s.FirstName + (!string.IsNullOrEmpty(s.MiddleName)?" "+ s.MiddleName : "") + (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                      BinusianID = s.Id,
                                      Username = a.Username,
                                      Email = a.Email
                                  }).ToListAsync(CancellationToken);

                return Request.CreateApiResult2(dataUser as object);
            }
            else if(CheckRole.IdRoleGroup == "STF")
            {
                var dataUser = await (from a in _dbContext.Entity<MsUser>()
                                   join ur in _dbContext.Entity<MsUserRole>() on a.Id equals ur.IdUser
                                   join us in _dbContext.Entity<MsUserSchool>() on a.Id equals us.IdUser
                                   join s in _dbContext.Entity<MsStaff>() on a.Id equals s.IdBinusian
                                   where ur.IdRole == CheckRole.IdRole && us.IdSchool == CheckRole.IdSchool
                                  select new GetUserByRolePositionExcludeSubjectResult
                                  {
                                      Id = a.Id,
                                      Fullname = s.FirstName + (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                      BinusianID = s.IdBinusian,
                                      Username = a.Username,
                                      Email = a.Email,
                                      Contact = s.MobilePhoneNumber1
                                  }).ToListAsync(CancellationToken);

                return Request.CreateApiResult2(dataUser as object);
            }
            else
            {
                if(body.CodePosition == null)
                {
                    var dataUser = await (from a in _dbContext.Entity<MsUser>()
                                          join ur in _dbContext.Entity<MsUserRole>() on a.Id equals ur.IdUser
                                          join s in _dbContext.Entity<MsStaff>() on a.Id equals s.IdBinusian
                                          where ur.IdRole == CheckRole.IdRole
                                          select new GetUserByRolePositionExcludeSubjectResult
                                          {
                                              Id = a.Id,
                                              Fullname = s.FirstName + (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                              BinusianID = s.IdBinusian,
                                              Username = a.Username,
                                              Email = a.Email,
                                              Contact = s.MobilePhoneNumber1
                                          }).Distinct().ToListAsync(CancellationToken);

                    return Request.CreateApiResult2(dataUser as object);
                }

                if(string.IsNullOrEmpty(body.CodePosition))
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Code Position"], "Id", body.CodePosition));

                var checkPosition = await _dbContext.Entity<LtPosition>()
                .Where(x => x.Id == body.CodePosition)
                .FirstOrDefaultAsync(CancellationToken);
                if (checkPosition == null)
                    throw new BadRequestException($"Position code {body.CodePosition} not found");

                if(checkPosition.Code == "CA" || checkPosition.Code == "COT"){

                    var dataUser = await (from ht in _dbContext.Entity<MsHomeroomTeacher>()
                                    join h in _dbContext.Entity<MsHomeroom>() on ht.IdHomeroom equals h.Id
                                    join tp in _dbContext.Entity<MsTeacherPosition>() on ht.IdTeacherPosition equals tp.Id
                                    join p in _dbContext.Entity<LtPosition>() on tp.IdPosition equals p.Id
                                    join u in _dbContext.Entity<MsUser>() on ht.IdBinusian equals u.Id
                                    join ur in _dbContext.Entity<MsUserRole>() on u.Id equals ur.IdUser
                                    join s in _dbContext.Entity<MsStaff>() on ht.IdBinusian equals s.IdBinusian
                                    where
                                        h.IdAcademicYear == body.IdAcademicYear && p.Code == checkPosition.Code && ur.IdRole == body.IdRole
                                    select new GetUserByRolePositionExcludeSubjectResult
                                    {
                                        Id = u.Id,
                                        Fullname = s.FirstName +  (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                        BinusianID = s.IdBinusian,
                                        Username = u.Username,
                                        Email = u.Email,
                                        Contact = s.MobilePhoneNumber1
                                    }).Distinct().ToListAsync(CancellationToken);

                    return Request.CreateApiResult2(dataUser as object);
                }
                else if(checkPosition.Code == "ST"){

                    var dataUser = await (from lt in _dbContext.Entity<MsLessonTeacher>()
                                    join l in _dbContext.Entity<MsLesson>() on lt.IdLesson equals l.Id
                                    join u in _dbContext.Entity<MsUser>() on lt.IdUser equals u.Id
                                    join ur in _dbContext.Entity<MsUserRole>() on u.Id equals ur.IdUser
                                    join s in _dbContext.Entity<MsStaff>() on lt.IdUser equals s.IdBinusian
                                    where
                                        l.IdAcademicYear == body.IdAcademicYear && ur.IdRole == body.IdRole
                                    select new
                                    {
                                        Id = u.Id,
                                        Fullname = u.DisplayName,
                                        BinusianID = u.Id,
                                        Username = u.Username,
                                        Email = u.Email,
                                        Contact = s.MobilePhoneNumber1,
                                        IdGrade = l.IdGrade,
                                        IdSubject = l.IdSubject
                                    })
                                    .ToListAsync(CancellationToken);

                    if(body.IdGradeParticipants != null && body.IdGradeParticipants.Count() != 0)
                        dataUser = dataUser.Where(x => body.IdGradeParticipants.Contains(x.IdGrade)).ToList();

                    if(body.ExcludeIdSubject != null && body.ExcludeIdSubject.Count() != 0)
                        dataUser = dataUser.Where(x => !body.ExcludeIdSubject.Contains(x.IdSubject)).ToList();

                    dataUser = dataUser.GroupBy(e => e.Id).Select(x => x.First()).ToList();

                    return Request.CreateApiResult2(dataUser as object);
                } else if (checkPosition.Code == "SH")
                {
                    List<GetUserByRolePositionExcludeSubjectResult> dataUser = new List<GetUserByRolePositionExcludeSubjectResult>();

                    var dataUserSH = await (from ntl in _dbContext.Entity<TrNonTeachingLoad>()
                                          join mntl in _dbContext.Entity<MsNonTeachingLoad>() on ntl.IdMsNonTeachingLoad equals mntl.Id
                                          join tp in _dbContext.Entity<MsTeacherPosition>() on mntl.IdTeacherPosition equals tp.Id
                                          join p in _dbContext.Entity<LtPosition>() on tp.IdPosition equals p.Id
                                          join u in _dbContext.Entity<MsUser>() on ntl.IdUser equals u.Id
                                          join ur in _dbContext.Entity<MsUserRole>() on u.Id equals ur.IdUser
                                          join s in _dbContext.Entity<MsStaff>() on u.Id equals s.IdBinusian
                                          where
                                              mntl.IdAcademicYear == body.IdAcademicYear && p.Code == checkPosition.Code && ur.IdRole == body.IdRole
                                          select new
                                          {
                                              Id = u.Id,
                                              Fullname = u.DisplayName,
                                              BinusianID = u.Id,
                                              Username = u.Username,
                                              Email = u.Email,
                                              Contact = s.MobilePhoneNumber1,
                                              DataNTL = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(ntl.Data)
                                          }).Distinct().ToListAsync(CancellationToken);

                    if(body.ExcludeIdSubject != null)
                    {
                        foreach (var item in dataUserSH)
                        {
                            item.DataNTL.TryGetValue("Grade", out var _GradeSH);
                            item.DataNTL.TryGetValue("Subject", out var _SubjectSH);

                            if (body.IdGradeParticipants != null && body.IdGradeParticipants.Count() != 0)
                            {
                                if (body.IdGradeParticipants.Contains(_GradeSH.Id) && !body.ExcludeIdSubject.Contains(_SubjectSH.Id))
                                {
                                    dataUser.Add(new GetUserByRolePositionExcludeSubjectResult
                                    {
                                        Id = item.Id,
                                        Fullname = item.Fullname,
                                        BinusianID = item.Id,
                                        Username = item.Username,
                                        Email = item.Email,
                                        Contact = item.Contact
                                    });
                                }
                            }
                            else
                            {
                                dataUser.Add(new GetUserByRolePositionExcludeSubjectResult
                                {
                                    Id = item.Id,
                                    Fullname = item.Fullname,
                                    BinusianID = item.Id,
                                    Username = item.Username,
                                    Email = item.Email,
                                    Contact = item.Contact
                                });
                            }
                        }

                        dataUser = dataUser.GroupBy(x => x.Id).Select(y => y.First()).ToList();
                    }
                    else
                    {
                        dataUser = dataUserSH.Select(x => new GetUserByRolePositionExcludeSubjectResult
                        {
                            Id = x.Id,
                            Fullname = x.Fullname,
                            BinusianID = x.Id,
                            Username = x.Username,
                            Email = x.Email,
                            Contact = x.Contact
                        }).GroupBy(x => x.Id).Select(y => y.First()).ToList();
                    }

                    return Request.CreateApiResult2(dataUser as object);
                }
                else{

                    if(body.CodePosition != null)
                    {
                        if (checkPosition.Code != "ADM")
                        {
                            var dataUser = await (from ntl in _dbContext.Entity<TrNonTeachingLoad>()
                                                  join mntl in _dbContext.Entity<MsNonTeachingLoad>() on ntl.IdMsNonTeachingLoad equals mntl.Id
                                                  join tp in _dbContext.Entity<MsTeacherPosition>() on mntl.IdTeacherPosition equals tp.Id
                                                  join p in _dbContext.Entity<LtPosition>() on tp.IdPosition equals p.Id
                                                  join u in _dbContext.Entity<MsUser>() on ntl.IdUser equals u.Id
                                                  join ur in _dbContext.Entity<MsUserRole>() on u.Id equals ur.IdUser
                                                  join s in _dbContext.Entity<MsStaff>() on u.Id equals s.IdBinusian
                                                  where
                                                      mntl.IdAcademicYear == body.IdAcademicYear && p.Code == checkPosition.Code && ur.IdRole == body.IdRole
                                                  select new GetUserByRolePositionExcludeSubjectResult
                                                  {
                                                      Id = u.Id,
                                                      Fullname = u.DisplayName,
                                                      BinusianID = u.Id,
                                                      Username = u.Username,
                                                      Email = u.Email,
                                                      Contact = s.MobilePhoneNumber1
                                                  }).Distinct().ToListAsync(CancellationToken);

                            return Request.CreateApiResult2(dataUser as object);
                        }
                        else
                        {
                            var dataUser = await (from a in _dbContext.Entity<MsUser>()
                                                  join ur in _dbContext.Entity<MsUserRole>() on a.Id equals ur.IdUser
                                                  join s in _dbContext.Entity<MsStaff>() on a.Id equals s.IdBinusian
                                                  where ur.IdRole == CheckRole.IdRole
                                                  select new GetUserByRolePositionExcludeSubjectResult
                                                  {
                                                      Id = a.Id,
                                                      Fullname = s.FirstName + (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                                      BinusianID = s.IdBinusian,
                                                      Username = a.Username,
                                                      Email = a.Email,
                                                      Contact = s.MobilePhoneNumber1
                                                  }).Distinct().ToListAsync(CancellationToken);

                            return Request.CreateApiResult2(dataUser as object);
                        }
                    }
                    else
                    {
                        var dataUser = await (from a in _dbContext.Entity<MsUser>()
                                   join ur in _dbContext.Entity<MsUserRole>() on a.Id equals ur.IdUser
                                   join s in _dbContext.Entity<MsStaff>() on a.Id equals s.IdBinusian
                                   where ur.IdRole == CheckRole.IdRole
                                  select new GetUserByRolePositionExcludeSubjectResult
                                  {
                                      Id = a.Id,
                                      Fullname = s.FirstName + (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                      BinusianID = s.IdBinusian,
                                      Username = a.Username,
                                      Email = a.Email,
                                      Contact = s.MobilePhoneNumber1
                                  }).Distinct().ToListAsync(CancellationToken);

                        return Request.CreateApiResult2(dataUser as object);
                    }
                }
                
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Employee.FnStaff;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSubject;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.HODAndSH;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment.HODAndSH
{
    public class AssignHODAndSHDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _teachingDbContext;
        private readonly IStringLocalizer _localizer;
        private readonly IServiceProvider _provider;

        public AssignHODAndSHDetailHandler(ITeachingDbContext teachingDbContext,
        IStringLocalizer localizer,
        IServiceProvider provider)
        {
            _teachingDbContext = teachingDbContext;
            _localizer = localizer;
            _provider = provider;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var result = new GetAssignHODAndSHDetailResult();
            var param = Request.ValidateParams<GetAssignHODAndSHDetailRequest>(nameof(GetAssignHODAndSHDetailRequest.IdSchoolDepartment), nameof(GetAssignHODAndSHDetailRequest.IdSchoolAcadYear));
            var getDetails = await _teachingDbContext.Entity<TrNonTeachingLoad>()
                    .Include(x => x.MsNonTeachingLoad)
                        .ThenInclude(x => x.TeacherPosition)
                            .ThenInclude(x => x.Position)
                    .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdSchoolAcadYear)
                    .Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.HeadOfDepartment)
                    .ToListAsync(CancellationToken);
            var data = new List<TrNonTeachingLoad>();
            foreach (var item in getDetails)
            {
                var itemData = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                if (itemData.TryGetValue("Department", out var department) && department.Id == param.IdSchoolDepartment)
                    data.Add(item);
            }
            var subjectHeads = _teachingDbContext.Entity<TrNonTeachingLoad>()
               .Include(x => x.MsNonTeachingLoad)
                        .ThenInclude(x => x.TeacherPosition)
                            .ThenInclude(x => x.Position)
                 .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdSchoolAcadYear)
                 .Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.SubjectHead).ToList();

            var subjectHeadAssistant = _teachingDbContext.Entity<TrNonTeachingLoad>()
               .Include(x => x.MsNonTeachingLoad)
                        .ThenInclude(x => x.TeacherPosition)
                            .ThenInclude(x => x.Position)
                 .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdSchoolAcadYear)
                 .Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.SubjectHeadAssitant).ToList();

            if (data != null && data.Count > 0)
            {
                result = data.Select(x => new GetAssignHODAndSHDetailResult()
                {
                    Id = x.Id,
                    SchoolUser = new ItemValueVm
                    {
                        Id = x.IdUser,
                        Description = ""
                    },
                    HOD = _teachingDbContext.Entity<MsNonTeachingLoad>()
                            .Include(x => x.TeacherPosition)
                                .ThenInclude(x => x.Position)
                            .Where(x => x.TeacherPosition.Position.Code == PositionConstant.HeadOfDepartment)
                            .Where(x => x.IdAcademicYear == param.IdSchoolAcadYear)
                            .Select(x => new SchoolNonTeachingLoadVm
                            {
                                Id = x.Id,
                                Load = x.Load
                            }).FirstOrDefault(),
                    SubjectHead = _teachingDbContext.Entity<MsNonTeachingLoad>()
                            .Include(x => x.TeacherPosition)
                                .ThenInclude(x => x.Position)
                            .Where(x => x.TeacherPosition.Position.Code == PositionConstant.SubjectHead)
                            .Where(x => x.IdAcademicYear == param.IdSchoolAcadYear)
                            .Select(x => new SchoolNonTeachingLoadVm
                            {
                                Id = x.Id,
                                Load = x.Load
                            }).FirstOrDefault(),
                    SubjectHeadAssistance = _teachingDbContext.Entity<MsNonTeachingLoad>()
                            .Include(x => x.TeacherPosition)
                                .ThenInclude(x => x.Position)
                            .Where(x => x.TeacherPosition.Position.Code == PositionConstant.SubjectHeadAssitant)
                            .Where(x => x.IdAcademicYear == param.IdSchoolAcadYear)
                            .Select(x => new SchoolNonTeachingLoadVm
                            {
                                Id = x.Id,
                                Load = x.Load
                            }).FirstOrDefault(),
                    AcademicYear = new ItemValueVm
                    {
                        Id = x.MsNonTeachingLoad.IdAcademicYear,
                        Description = "",
                    },
                    //Department = _dbContext.Entity<SchoolDepartment>().Where(x => x.Id == param.IdSchoolDepartment)
                    //    .Select(x => new ItemValueVm
                    //    {
                    //        Id = x.Id,
                    //        Description = x.Description
                    //    }).FirstOrDefault(),
                    Department = new ItemValueVm
                    {
                        Id = param.IdSchoolDepartment
                    },
                    //Subjects = _dbContext.Entity<Subject2>()
                    //    .Include(x => x.SchoolDepartment)
                    //    .Include(x => x.SchoolAcadyearLevelGrade)
                    //        .ThenInclude(x => x.SchoolAcadyearLevel)
                    //            .ThenInclude(x => x.SchoolLevel)
                    //    .Include(x => x.SchoolAcadyearLevelGrade)
                    //        .ThenInclude(x => x.SchoolGrade)
                    //    .Where(x => x.IdDepartment == param.IdSchoolDepartment)
                    //    .Select(x => new ItemValueVm
                    //    {
                    //        Id = x.Id,
                    //        //Text = string.Format("{0}-{1} {2} ({3})", x.SchoolAcadyearLevelGrade.SchoolAcadyearLevel.SchoolLevel.Description,
                    //        //x.SchoolAcadyearLevelGrade.SchoolGrade.Description,
                    //        //x.Description,
                    //        //x.Code)
                    //        Description = x.Description
                    //    }).ToList(),
                    Subjects = new List<SubjectVm>()
                }).FirstOrDefault();
            }
            else
            {
                result = new GetAssignHODAndSHDetailResult()
                {
                    Id = null,
                    SchoolUser = null,
                    HOD = _teachingDbContext.Entity<MsNonTeachingLoad>()
                            .Include(x => x.TeacherPosition)
                                .ThenInclude(x => x.Position)
                            .Where(x => x.TeacherPosition.Position.Code == PositionConstant.HeadOfDepartment)
                            .Where(x => x.IdAcademicYear == param.IdSchoolAcadYear)
                            .Select(x => new SchoolNonTeachingLoadVm
                            {
                                Id = x.Id,
                                Load = x.Load
                            }).FirstOrDefault(),
                    SubjectHead = _teachingDbContext.Entity<MsNonTeachingLoad>()
                            .Include(x => x.TeacherPosition)
                                .ThenInclude(x => x.Position)
                            .Where(x => x.TeacherPosition.Position.Code == PositionConstant.SubjectHead)
                            .Where(x => x.IdAcademicYear == param.IdSchoolAcadYear).Select(x => new SchoolNonTeachingLoadVm
                            {
                                Id = x.Id,
                                Load = x.Load
                            }).FirstOrDefault(),
                    SubjectHeadAssistance = _teachingDbContext.Entity<MsNonTeachingLoad>()
                            .Include(x => x.TeacherPosition)
                                .ThenInclude(x => x.Position)
                            .Where(x => x.TeacherPosition.Position.Code == PositionConstant.SubjectHeadAssitant)
                            .Where(x => x.IdAcademicYear == param.IdSchoolAcadYear).Select(x => new SchoolNonTeachingLoadVm
                            {
                                Id = x.Id,
                                Load = x.Load
                            }).FirstOrDefault(),
                    AcademicYear = new ItemValueVm
                    {
                        Id = param.IdSchoolAcadYear,
                        Description = "",
                    },
                    //Department = _dbContext.Entity<SchoolDepartment>().Where(x => x.Id == param.IdSchoolDepartment)
                    //    .Select(x => new ItemValueVm
                    //    {
                    //        Id = x.Id,
                    //        Description = x.Description
                    //    }).FirstOrDefault(),
                    Department = new ItemValueVm
                    {
                        Id = param.IdSchoolDepartment
                    },
                    //Subjects = _dbContext.Entity<Subject2>()
                    //    .Include(x => x.SchoolDepartment)
                    //    .Include(x => x.SchoolAcadyearLevelGrade)
                    //        .ThenInclude(x => x.SchoolAcadyearLevel)
                    //            .ThenInclude(x => x.SchoolLevel)
                    //    .Include(x => x.SchoolAcadyearLevelGrade)
                    //        .ThenInclude(x => x.SchoolGrade)
                    //    .Where(x => x.IdDepartment == param.IdSchoolDepartment)
                    //    .Select(x => new ItemValueVm
                    //    {
                    //        Id = x.Id,
                    //        //Text = string.Format("{0}-{1} {2} ({3})", x.SchoolAcadyearLevelGrade.SchoolAcadyearLevel.SchoolLevel.Description,
                    //        //x.SchoolAcadyearLevelGrade.SchoolGrade.Description,
                    //        //x.Description,
                    //        //x.Code)
                    //        Description = x.Description
                    //    }).ToList()
                    Subjects = new List<SubjectVm>()
                };
            }
            if (result.SchoolUser != null)
            {
                var userHod = result.SchoolUser != null ? new List<string>() { result.SchoolUser.Id } : new List<string>() { "dummy" };
                var dataUser = await (from user in _teachingDbContext.Entity<MsUser>()
                                      join userSchool in _teachingDbContext.Entity<MsUserSchool>() on user.Id equals userSchool.IdUser
                                      join staff in _teachingDbContext.Entity<MsStaff>() on user.Id equals staff.IdBinusian
                                      where
                                      userSchool.IdSchool == param.IdSchool
                                      && userHod.Any(p => p == staff.IdBinusian)
                                      select new CheckTeacherForAscTimetableResult
                                      {
                                          IdTeacher = staff.IdBinusian,
                                          TeacherBinusianId = staff.IdBinusian,
                                          TeacherName = staff.FirstName,
                                          TeacherShortName = staff.ShortName,
                                          TeacherInitialName = staff.ShortName,
                                      }
                                ).ToListAsync();
                if (dataUser != null)
                {
                    result.SchoolUser = new ItemValueVm
                    {
                        Id = dataUser.Where(x => x.TeacherBinusianId == result.SchoolUser.Id).Select(x => x.IdTeacher).FirstOrDefault(),
                        Description = dataUser.Where(x => x.TeacherBinusianId == result.SchoolUser.Id).Select(x => x.TeacherName).FirstOrDefault()
                    };
                }
            }


            var dataDepartment = await _teachingDbContext.Entity<MsDepartment>().Where(x => x.Id == param.IdSchoolDepartment).FirstOrDefaultAsync();
            if (dataDepartment != null)
            {
                result.Department = new ItemValueVm
                {
                    Id = dataDepartment.Id,
                    Description = dataDepartment.Description
                };
            }

            result.Subjects = await _teachingDbContext.Entity<MsSubject>()
                .Include(x => x.Grade).ThenInclude(x => x.Level)
                .Include(x => x.Department).ThenInclude(x => x.AcademicYear).ThenInclude(x => x.School)
                .Where(x => x.Department.AcademicYear.IdSchool == param.IdSchool)
                .Where(x => x.IdDepartment == param.IdSchoolDepartment)
                .Select(x => new SubjectVm
                {
                    Id = x.Id,
                    SubjectId = x.SubjectID,
                    Description = x.Description,
                    Grade = new CodeWithIdVm
                    {
                        Id = x.Grade.Id,
                        Code = x.Grade.Code,
                        Description = x.Grade.Description
                    },
                    Level = new CodeWithIdVm
                    {
                        Id = x.Grade.Level.Id,
                        Code = x.Grade.Level.Code,
                        Description = x.Grade.Level.Description
                    }
                }).ToListAsync();

            var dataForSubjectHead = new List<SubjectHeadVm>();
            foreach (var subject in result.Subjects)
            {
                foreach (var sh in subjectHeads)
                {
                    var shVm = new SubjectHeadVm();
                    var shData = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(sh.Data);

                    if (shData.TryGetValue("Department", out var department) && department.Id == param.IdSchoolDepartment)
                    {
                        if (shData.TryGetValue("Subject", out var sbj) && sbj.Id == subject.Id)
                        {
                            shVm.User = new ItemValueVm
                            {
                                Id = sh.IdUser,
                                Description = "",//sh.SchoolUser.User.Fullname
                            };
                            shVm.IdSubject = subject.Id;
                            shVm.IdUserNonTeachingAcademic = sh.Id;

                            foreach (var sha in subjectHeadAssistant)
                            {
                                var shaData = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(sha.Data);
                                if (shaData.TryGetValue("Department", out var dpt2) && dpt2.Id == param.IdSchoolDepartment && shaData.TryGetValue("Subject", out var sbj2) && sbj2.Id == subject.Id)
                                {
                                    shVm.SubjectHeadAssistance = new SubjectHeadAssistanceVm
                                    {
                                        User = new ItemValueVm
                                        {
                                            Id = sha.IdUser,
                                            Description = "",//sh.SchoolUser.User.Fullname
                                        },
                                        IdSubject = subject.Id,
                                        IdUserNonTeachingAcademic = sha.Id
                                    };
                                }
                            }
                        }
                        dataForSubjectHead.Add(shVm);
                    }
                }
            }
            if (dataForSubjectHead != null && dataForSubjectHead.Count > 0)
            {
                var userSubjectHead = dataForSubjectHead.Select(x => x.User?.Id).Distinct().ToList();
                var userSubjectHeadAssistance = dataForSubjectHead.Select(x => x.SubjectHeadAssistance?.User?.Id).Distinct().ToList();

                var user = userSubjectHead.Union(userSubjectHeadAssistance).Distinct().ToList();
                if (user != null && user.Count > 0)
                {
                    var dataUserShAndSha =
                                          await (from usershandsha in _teachingDbContext.Entity<MsUser>()
                                                 join userSchool in _teachingDbContext.Entity<MsUserSchool>() on usershandsha.Id equals userSchool.IdUser
                                                 join staff in _teachingDbContext.Entity<MsStaff>() on usershandsha.Id equals staff.IdBinusian
                                                 where
                                                 userSchool.IdSchool == param.IdSchool
                                                 && user.Any(p => p == staff.IdBinusian)
                                                 select new CheckTeacherForAscTimetableResult
                                                 {
                                                     IdTeacher = staff.IdBinusian,
                                                     TeacherBinusianId = staff.IdBinusian,
                                                     TeacherName = staff.FirstName,
                                                     TeacherShortName = staff.ShortName,
                                                     TeacherInitialName = staff.ShortName,
                                                 }
                                ).ToListAsync();
                    dataForSubjectHead =
                    (
                        from _dataForSubjectHead in dataForSubjectHead
                        join _dataUserShAndSha in dataUserShAndSha on _dataForSubjectHead.User?.Id equals _dataUserShAndSha.TeacherBinusianId
                        select new SubjectHeadVm
                        {
                            IdSubject = _dataForSubjectHead.IdSubject,
                            IdUserNonTeachingAcademic = _dataForSubjectHead.IdUserNonTeachingAcademic,
                            User = new ItemValueVm
                            {
                                Id = _dataForSubjectHead.User?.Id,
                                Description = _dataUserShAndSha.TeacherName
                            },
                            SubjectHeadAssistance = new SubjectHeadAssistanceVm
                            {
                                IdSubject = _dataForSubjectHead.SubjectHeadAssistance?.IdSubject,
                                IdUserNonTeachingAcademic = _dataForSubjectHead.SubjectHeadAssistance?.IdUserNonTeachingAcademic,
                                User = new ItemValueVm
                                {
                                    Id = _dataForSubjectHead.SubjectHeadAssistance?.User?.Id,
                                    Description = dataUserShAndSha.Where(x => x.TeacherBinusianId == _dataForSubjectHead.SubjectHeadAssistance?.User?.Id).Select(x => x.TeacherName).FirstOrDefault()
                                }
                            }
                        }
                    ).ToList();
                }
                else
                {
                    dataForSubjectHead =
                    (
                        from _dataForSubjectHead in dataForSubjectHead
                        select new SubjectHeadVm
                        {
                            IdSubject = _dataForSubjectHead.IdSubject,
                            IdUserNonTeachingAcademic = _dataForSubjectHead.IdUserNonTeachingAcademic,
                            User = new ItemValueVm(),
                            SubjectHeadAssistance = new SubjectHeadAssistanceVm
                            {
                                IdSubject = _dataForSubjectHead.SubjectHeadAssistance?.IdSubject,
                                IdUserNonTeachingAcademic = _dataForSubjectHead.SubjectHeadAssistance?.IdUserNonTeachingAcademic,
                                User = new ItemValueVm()
                            }
                        }
                    ).ToList();
                }
            }
            result.SubjectHeads = dataForSubjectHead;

            return Request.CreateApiResult2(result as object, null);
        }
    }
}

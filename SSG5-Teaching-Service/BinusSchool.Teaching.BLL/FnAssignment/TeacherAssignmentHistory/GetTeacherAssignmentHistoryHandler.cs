using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentCertification;
using BinusSchool.Data.Model.School.FnSchool.Venue;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignmentHistory;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using BinusSchool.Teaching.FnAssignment.TeacherAssignmentHistory.Validator;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignmentHistory
{
    public class GetTeacherAssignmentHistoryHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public GetTeacherAssignmentHistoryHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetTeacherAssignmentHistoryRequest, GetTeacherAssignmentHistoryValidator>();

            var teacherData = await _dbContext.Entity<MsUserSchool>()
                .Include(x => x.User)
                    .ThenInclude(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RoleGroup)
                .Where(x => (param.IdUser == null || !param.IdUser.Any()) ? true : param.IdUser.Contains(x.IdUser) &&
                            x.User.UserRoles.Any(y => y.Role.RoleGroup.Code == RoleConstant.Teacher))
                .Select(x => new
                {
                    IdUser = x.IdUser,
                    TeacherName = x.User.DisplayName
                }).ToListAsync(CancellationToken);

            var nonTeachingLoadData = await _dbContext.Entity<TrNonTeachingLoad>()
                .Include(x => x.MsNonTeachingLoad)
                .Where(x => teacherData.Select(x => x.IdUser).Contains(x.IdUser) && x.Data != null)
                .Select(x => new GetTeacherAssignmentHistory_TeacherAssignment
                {
                    IdUser = x.IdUser,
                    Data = JsonConvert.DeserializeObject<GetTeacherAssignmentHistory_Data>(x.Data),
                    IdAcademicYear = x.MsNonTeachingLoad.IdAcademicYear,
                    IdTeacherPosition = x.MsNonTeachingLoad.IdTeacherPosition
                })
                .ToListAsync(CancellationToken);

            var lessonTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                .Where(x => x.Code == "ST").FirstOrDefaultAsync(CancellationToken);

            var lessonTeacherData = await _dbContext.Entity<MsLessonTeacher>()
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Grade)
                    .ThenInclude(x => x.Level)
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Subject)
                    .ThenInclude(x => x.Department)
                .Where(x => teacherData.Select(x => x.IdUser).Contains(x.IdUser))
                .Select(x => new GetTeacherAssignmentHistory_TeacherAssignment
                {
                    IdUser = x.IdUser,
                    Data = new GetTeacherAssignmentHistory_Data
                    {
                        Level = new ItemValueVm
                        {
                            Id = x.Lesson.Grade.Level.Id,
                            Description = x.Lesson.Grade.Level.Description
                        },
                        Grade = new ItemValueVm
                        {
                            Id = x.Lesson.Grade.Id,
                            Description = x.Lesson.Grade.Description
                        },
                        Department = new ItemValueVm
                        {
                            Id = x.Lesson.Subject.Department.Id,
                            Description = x.Lesson.Subject.Department.Description
                        },
                        Subject = new ItemValueVm
                        {
                            Id = x.Lesson.Subject.Id,
                            Description = x.Lesson.Subject.Description
                        }
                    },
                    IdAcademicYear = x.Lesson.Grade.Level.IdAcademicYear,

                    IdTeacherPosition = lessonTeacherPosition.Id
                    //_dbContext.Entity<MsUserRole>()
                    //    .Include(y => y.Role)
                    //        .ThenInclude(y => y.RolePositions)
                    //        .ThenInclude(y => y.TeacherPosition)
                    //        .ThenInclude(y => y.Position)
                    //    .Where(y => y.IdUser == x.IdUser && y.Role.RolePositions.Select(z => z.TeacherPosition.Position.Code).Contains("ST"))
                    //    .Select(y => y.Role.RolePositions.Select(z => z.IdTeacherPosition).FirstOrDefault()).FirstOrDefault()

                }).ToListAsync(CancellationToken);

            var homeroomTeacherData = await _dbContext.Entity<MsHomeroomTeacher>()
                .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level)
                .Where(x => teacherData.Select(x => x.IdUser).Contains(x.IdBinusian))
                .Select(x => new GetTeacherAssignmentHistory_TeacherAssignment
                {
                    IdUser = x.IdBinusian,
                    IdAcademicYear = x.Homeroom.Grade.Level.IdAcademicYear,
                    IdTeacherPosition = x.IdTeacherPosition,
                    Data = new GetTeacherAssignmentHistory_Data
                    {
                        Grade = new ItemValueVm
                        {
                            Id = x.Homeroom.Grade.Id,
                            Description = x.Homeroom.Grade.Description
                        },
                        Level = new ItemValueVm
                        {
                            Id = x.Homeroom.Grade.Level.Id,
                            Description = x.Homeroom.Grade.Level.Description
                        },
                        Subject = null,
                        Department = null
                    }
                }).ToListAsync(CancellationToken);

            var unionTeacherAssignment = nonTeachingLoadData.Union(lessonTeacherData);
            unionTeacherAssignment = unionTeacherAssignment.Union(homeroomTeacherData);

            var getAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                .Include(x => x.School)
                .Where(x => unionTeacherAssignment.Select(x => x.IdAcademicYear).Contains(x.Id))
                .ToListAsync(CancellationToken);

            var getTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                .Where(x => unionTeacherAssignment.Select(x => x.IdTeacherPosition).Contains(x.Id))
                .ToListAsync(CancellationToken);

            var resultList = new List<GetTeacherAssignmentHistoryResult>();
            foreach(var teacher in teacherData)
            {
                var teacherAssignment = unionTeacherAssignment.Where(x => x.IdUser == teacher.IdUser).ToList();

                var resultData = new GetTeacherAssignmentHistoryResult
                {
                    IdUser = teacher.IdUser,
                    TeacherName = teacher.TeacherName,
                    Assignments = teacherAssignment.Select(x => new
                    {
                        TeacherPosition = getTeacherPosition.Where(y => y.Id == x.IdTeacherPosition).Select(y => y.Description).FirstOrDefault(),
                        AcademicYear = getAcademicYear.Where(y => y.Id == x.IdAcademicYear).Select(y => y.Description).FirstOrDefault(),
                        Grade = x.Data.Grade?.Description,
                        Level = x.Data.Level?.Description,
                        Department = x.Data.Department?.Description,
                        Subject = x.Data.Subject?.Description,
                        SchoolName = getAcademicYear.Where(y => y.Id == x.IdAcademicYear).Select(y => y.School.Description).FirstOrDefault(),
                    })
                    .Distinct()
                    .Select(x => new GetTeacherAssignmentHistoryResult_Assignment
                    {
                        TeacherPosition = x.TeacherPosition,
                        AcademicYear = x.AcademicYear,
                        Grade = x.Grade,
                        Level = x.Level,
                        Department = x.Department,
                        Subject = x.Subject,
                        SchoolName = x.SchoolName
                    })
                    .OrderByDescending(x => x.AcademicYear).ThenByDescending(x => x.Level).ThenByDescending(x => x.Grade).ToList()
                };

                resultList.Add(resultData);
            }


            var data = resultList.SetPagination(param).ToList();

            var count = param.CanCountWithoutFetchDb(data.Count)
                ? data.Count
                : resultList.Select(x => x).Count();

            return Request.CreateApiResult2(data as object);
        }

        private class GetTeacherAssignmentHistory_Data
        {
            public ItemValueVm Level { get; set; }
            public ItemValueVm Grade { get; set; }
            public ItemValueVm Department { get; set; }
            public ItemValueVm Subject { get; set; }
        }
        private class GetTeacherAssignmentHistory_TeacherAssignment
        {
            public string IdUser { get; set; }
            public GetTeacherAssignmentHistory_Data Data { get; set; }
            public string IdAcademicYear { get; set; }
            public string IdTeacherPosition { get; set; }
        }
    }
}

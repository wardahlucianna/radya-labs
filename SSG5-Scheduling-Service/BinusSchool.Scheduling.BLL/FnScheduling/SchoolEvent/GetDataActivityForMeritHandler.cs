using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetDataActivityForMeritHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetDataActivityForMeritRequest.IdEvent),
        };
        private static readonly string[] _columns = { "Fullname", "BinusianID", "Level", "Grade", "Class", "Activity", "Award"};
        private readonly ISchedulingDbContext _dbContext;

        public GetDataActivityForMeritHandler(ISchedulingDbContext SchoolEventDbContext)
        {
            _dbContext = SchoolEventDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDataActivityForMeritRequest>(_requiredParams);

            var query = await _dbContext.Entity<TrEventActivityAward>()
                                .Include(x => x.EventActivity).ThenInclude(x => x.Event)
                                .Include(x => x.EventActivity).ThenInclude(x => x.Activity)
                                .Include(x => x.Award)
                                .Include(x => x.HomeroomStudent)
                                    .ThenInclude(x => x.Student)
                                        .ThenInclude(x => x.StudentGrades)
                                            .ThenInclude(x => x.Grade)
                                                .ThenInclude(x => x.Level)
                                                    .ThenInclude(x => x.AcademicYear)
                                .Include(x => x.HomeroomStudent)
                                    .ThenInclude(x => x.Homeroom)
                                        .ThenInclude(x => x.GradePathwayClassroom)
                                            .ThenInclude(x => x.Classroom)
                                .Select(x => new GetDataActivityForMeritResult
                                  {
                                      Id = x.Id,
                                      IdEvent = x.EventActivity.Event.Id,
                                      Fullname = x.HomeroomStudent.Student.FirstName + " " + x.HomeroomStudent.Student.LastName,
                                      BinusianID = x.HomeroomStudent.Student.Id,
                                      IdLevel = x.HomeroomStudent.Student.StudentGrades.FirstOrDefault().Grade.Level.Id,
                                      Level = x.HomeroomStudent.Student.StudentGrades.FirstOrDefault().Grade.Level.Description,
                                      IdGrade = x.HomeroomStudent.Student.StudentGrades.FirstOrDefault().Grade.Id,
                                      Grade = x.HomeroomStudent.Student.StudentGrades.FirstOrDefault().Grade.Description,
                                      IdClassroom = x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Id,
                                      Classroom = x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description,
                                      IdHomeroom = x.HomeroomStudent.Homeroom.Id,
                                      IdHomeroomStudent = x.HomeroomStudent.Id,
                                      Semester = x.HomeroomStudent.Semester,
                                      IdActivity = x.EventActivity.Activity.Id,
                                      Activity = x.EventActivity.Activity.Description,
                                      IdAward = x.Award.Id,
                                      Award = x.Award.Description
                                  })
                                  .Where(x => x.IdEvent == param.IdEvent)
                                  .Distinct()
                                  .ToListAsync(CancellationToken);

            if (!string.IsNullOrEmpty(param.IdLevel))
                    query = query.Where(x => x.IdLevel == param.IdLevel).ToList();

            if (!string.IsNullOrEmpty(param.IdGrade))
                    query = query.Where(x => x.IdGrade == param.IdGrade).ToList();

            if (!string.IsNullOrEmpty(param.IdClassroom))
                    query = query.Where(x => x.IdClassroom == param.IdClassroom).ToList();
                
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                    query = query.Where(x => x.IdHomeroom == param.IdHomeroom).ToList();

            if (!string.IsNullOrEmpty(param.IdActivity))
                    query = query.Where(x => x.IdActivity == param.IdActivity).ToList();

            if (!string.IsNullOrEmpty(param.IdAward))
                    query = query.Where(x => x.IdAward == param.IdAward).ToList();
            
            //ordering
            switch (param.OrderBy)
            {
                case "Fullname":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Fullname).ToList()
                        : query.OrderBy(x => x.Fullname).ToList();
                    break;
                case "BinusianID":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BinusianID).ToList()
                        : query.OrderBy(x => x.BinusianID).ToList();
                    break;
                case "Level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Level).ToList()
                        : query.OrderBy(x => x.Level).ToList();
                    break;
                case "Grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade).ToList()
                        : query.OrderBy(x => x.Grade).ToList();
                    break;
                case "Homeroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Classroom).ToList()
                        : query.OrderBy(x => x.Classroom).ToList();
                    break;
                case "Classroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Classroom).ToList()
                        : query.OrderBy(x => x.Classroom).ToList();
                    break;
                case "Activity":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Activity).ToList()
                        : query.OrderBy(x => x.Activity).ToList();
                    break;
                case "Award":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Award).ToList()
                        : query.OrderBy(x => x.Award).ToList();
                    break;
            };

            var dataListPagination = query
                .SetPagination(param)
                .Select(x => new GetDataActivityForMeritResult
                {
                    Id = x.Id,
                    IdEvent = x.IdEvent,
                    Fullname = x.Fullname,
                    BinusianID = x.BinusianID,
                    IdLevel = x.IdLevel,
                    Level = x.Level,
                    IdGrade = x.IdGrade,
                    Grade = x.Grade,
                    IdHomeroom = x.IdHomeroom,
                    IdClassroom = x.IdClassroom,
                    Classroom = x.Classroom,
                    IdHomeroomStudent = x.IdHomeroomStudent,
                    Semester = x.Semester,
                    IdActivity = x.IdActivity,
                    Activity = x.Activity,
                    IdAward = x.IdAward,
                    Award = x.Award
                }).ToList();

            var count = param.CanCountWithoutFetchDb(dataListPagination.Count) 
                ? dataListPagination.Count 
                : query.Count;

            return Request.CreateApiResult2(dataListPagination as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}

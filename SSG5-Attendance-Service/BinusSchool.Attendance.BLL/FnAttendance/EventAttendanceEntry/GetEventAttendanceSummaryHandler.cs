using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Common.Exceptions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Common.Constants;
using BinusSchool.Data.Api.Student.FnStudent;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry
{
    public class GetEventAttendanceSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetEventAttendanceSummaryHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEventAttendanceSummaryRequest>(nameof(GetEventAttendanceSummaryRequest.IdEventCheck),
                                                                                 nameof(GetEventAttendanceSummaryRequest.IdLevel));

            var eventCheck = await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
                                             .Where(x => x.Id == param.IdEventCheck)
                                             .SingleOrDefaultAsync();
            if (eventCheck is null)
                throw new BadRequestException("Event check is not found");

            var level = await _dbContext.Entity<MsLevel>()
                                        .Where(x => x.Id == param.IdLevel)
                                        .Select(x => new CodeWithIdVm
                                        {
                                            Id = x.Id,
                                            Code = x.Code,
                                            Description = x.Description
                                        })
                                        .SingleOrDefaultAsync();
            if (level is null)
                throw new BadRequestException("Level is not found");

            // get filtered student id
            var students = await _dbContext.Entity<MsStudent>()
                                             .Include(x => x.HomeroomStudents).ThenInclude(x => x.Homeroom).ThenInclude(x => x.GradePathwayClassroom)
                                                .ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade)
                                             .Include(x => x.HomeroomStudents).ThenInclude(x => x.Homeroom).ThenInclude(x => x.GradePathwayClassroom)
                                                .ThenInclude(x => x.GradePathway).ThenInclude(x => x.GradePathwayDetails).ThenInclude(x => x.Pathway)
                                             .Include(x => x.HomeroomStudents).ThenInclude(x => x.Homeroom).ThenInclude(x => x.HomeroomPathways)
                                                .ThenInclude(x => x.LessonPathways).ThenInclude(x => x.Lesson)
                                             .Where(x => x.HomeroomStudents.Any(y => y.Homeroom.GradePathwayClassroom.GradePathway.Grade.IdLevel == param.IdLevel)
                                                         && (!string.IsNullOrEmpty(param.IdGrade) ? x.HomeroomStudents.Any(y => y.Homeroom.GradePathwayClassroom.GradePathway.IdGrade == param.IdGrade) : true)
                                                         && (!string.IsNullOrEmpty(param.IdHomeroom) ? x.HomeroomStudents.Any(y => y.IdHomeroom == param.IdHomeroom) : true)
                                                         && (!string.IsNullOrEmpty(param.IdSubject) ? x.HomeroomStudents.Any(y => y.Homeroom.HomeroomPathways.Any(z => z.LessonPathways.Any(za => za.Lesson.IdSubject == param.IdSubject))) : true))
                                             .ToListAsync();

            var studentIds = students.Select(x => x.Id).ToList();

            var userEvents = await _dbContext.Entity<TrUserEvent>()
                .Include(x => x.User)
                .Include(x => x.UserEventAttendance2s).ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                    .Where(x
                        => studentIds.Contains(x.IdUser)
                        && x.EventDetail.Event.EventIntendedFor
                            .Where(y => y.IntendedFor == RoleConstant.Student)
                            .Any(y => y.EventIntendedForAttendanceStudents.Any(z => z.EventIntendedForAtdCheckStudents.Any(a => a.Id == param.IdEventCheck)))).ToListAsync(CancellationToken);

            var entries = userEvents.Where(x => x.UserEventAttendance2s.Any(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck))?
                               .Select(x => new EventAttendanceSummaryStudent
                               {
                                   Id = x.IdUser,
                                   Name = x.User.DisplayName,
                                   IdUserEvent = x.Id,
                                   StartTime = eventCheck.StartTime,
                                   EndTime = eventCheck.EndTime,
                                   Grade = students.Where(y => y.Id == x.IdUser)
                                                   .Select(y => new CodeWithIdVm
                                                   {
                                                       Id = y.HomeroomStudents.First().Homeroom.GradePathwayClassroom.GradePathway.Grade.Id,
                                                       Code = y.HomeroomStudents.First().Homeroom.GradePathwayClassroom.GradePathway.Grade.Code,
                                                       Description = y.HomeroomStudents.First().Homeroom.GradePathwayClassroom.GradePathway.Grade.Description
                                                   }).First(),
                                   Pathway = students.Where(y => y.Id == x.IdUser)
                                                     .Select(y => new CodeWithIdVm
                                                     {
                                                         Id = y.HomeroomStudents.First().Homeroom.GradePathwayClassroom.GradePathway.GradePathwayDetails.First().Pathway.Id,
                                                         Code = y.HomeroomStudents.First().Homeroom.GradePathwayClassroom.GradePathway.GradePathwayDetails.First().Pathway.Code,
                                                         Description = y.HomeroomStudents.First().Homeroom.GradePathwayClassroom.GradePathway.GradePathwayDetails.First().Pathway.Description,
                                                     }).First(),
                                   Attendance = new AttendanceEntryItem
                                   {
                                       IdAttendanceMapAttendance = x.UserEventAttendance2s.First(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck).IdAttendanceMappingAttendance,
                                       Id = x.UserEventAttendance2s.First(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck).AttendanceMappingAttendance.IdAttendance,
                                       Code = x.UserEventAttendance2s.First(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck).AttendanceMappingAttendance.Attendance.Code,
                                       Description = x.UserEventAttendance2s.First(y => y.IdEventIntendedForAtdCheckStudent == param.IdEventCheck).AttendanceMappingAttendance.Attendance.Description
                                   }
                               }).ToList();
            var result = new EventAttendanceSummaryResult
            {
                EventCheck = new ItemValueVm
                {
                    Id = eventCheck.Id,
                    Description = eventCheck.CheckName
                },
                Level = level,
                Summary = new EventAttendanceEntrySummary
                {
                    TotalStudent = userEvents.Count(),
                    Submitted = entries.Count()
                },
                Entries = entries
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}

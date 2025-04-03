using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnAssignment.TimetablePreferences
{
    public class GetTimeTablePreferencesHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetListTimeTableRequest.IdAcademicyears)
        });
        private static readonly Lazy<string[]> _columns = new Lazy<string[]>(new[]
        {
            "subjectName", "totalSession", "class", "teacerName", "totalLoad",
            "venue", "count", "lenght", "division", "week", "term", "department", "level", "grade", "streaming"
        });
        // private static readonly Lazy<IDictionary<string, string>> _aliasColumns = new Lazy<IDictionary<string, string>>(new Dictionary<string, string>
        // {
        //     { _columns.Value[0], "subjectComb.GradeSubject.Description" },
        //     { _columns.Value[1], "subjectComb.GradeSubject.MaxSession" },
        //     { _columns.Value[2], "subjectComb.GradePathwayClassroom.SchoolAcadYearLevelGradePathway.SchoolAcadyearLevelGrade.SchoolGrade.Code" },
        //     { _columns.Value[5], "TimetablePreferencesDetails.SchoolBuild.Code" },
        //     { _columns.Value[6], "TimetablePreferencesDetails.Count" },
        //     { _columns.Value[7], "TimetablePreferencesDetails.Length" },
        //     { _columns.Value[8], "TimetablePreferencesDetails.SchoolDivision.Description" },
        //     { _columns.Value[9], "TimetablePreferencesDetails.Week" },
        //     { _columns.Value[10], "TimetablePreferencesDetails.SchoolPeriode.Description" },
        //     { _columns.Value[11], "subjectComb.GradeSubject.SchoolDepartment.Description" },
        //     { _columns.Value[12], "subjectComb.GradePathwayClassroom.SchoolAcadYearLevelGradePathway.SchoolAcadyearLevelGrade.SchoolAcadyearLevel.SchoolLevel.Code" },
        //     { _columns.Value[13], "subjectComb.GradePathwayClassroom.SchoolAcadYearLevelGradePathway.SchoolAcadyearLevelGrade.SchoolGrade.Code" },
        //     { _columns.Value[14], "subjectComb.GradeSubject.SubjectPathways.SchoolAcadYearLevelGradePathwayDetail.SchoolPathway.Description" },
        // });

        private readonly ITeachingDbContext _dbContext;

        public GetTimeTablePreferencesHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListTimeTableRequest>(_requiredParams.Value);

            var timeTable = await _dbContext.Entity<MsSubjectCombination>()
                .Include(x => x.Subject).ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear)
                .Include(x => x.Subject).ThenInclude(x => x.Department)
                .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.GradePathwayDetails).ThenInclude(x => x.Pathway)
                .Include(x => x.TimeTablePrefHeader).ThenInclude(x => x.Childs).ThenInclude(x => x.SubjectCombination).ThenInclude(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                .Include(x => x.TimeTablePrefHeader).ThenInclude(x => x.TimetablePrefDetails).ThenInclude(x => x.TeachingLoads).ThenInclude(x => x.User)
                .Include(x => x.TimeTablePrefHeader).ThenInclude(x => x.TimetablePrefDetails).ThenInclude(x => x.Venue).ThenInclude(x => x.Building)
                .Include(x => x.TimeTablePrefHeader).ThenInclude(x => x.TimetablePrefDetails).ThenInclude(x => x.Period)
                .Include(x => x.TimeTablePrefHeader).ThenInclude(x => x.TimetablePrefDetails).ThenInclude(x => x.Division)
                .Where(x
                    => x.Subject.Grade.Level.IdAcademicYear == param.IdAcademicyears
                    && x.TimeTablePrefHeader != null
                    && x.TimeTablePrefHeader.IdParent == null)
                .OrderBy(x => x.Subject.SubjectID)
                .ToListAsync(CancellationToken);
            var idUsers = timeTable
                .SelectMany(x => x.TimeTablePrefHeader.TimetablePrefDetails
                    .Where(y => y.TeachingLoads != null)
                    .SelectMany(y => y.TeachingLoads.Select(z => z.IdUser)));

            var reqUserProfile = await _dbContext.Entity<MsStaff>()
                .Where(x => idUsers.Contains(x.IdBinusian))
                .ToListAsync(CancellationToken);
            var teacherLoads = await _dbContext.Entity<TrTeachingLoad>()
                .Include(x => x.TimetablePrefDetail)
                .ThenInclude(x => x.TimetablePrefHeader)
                .GroupBy(group => group.IdUser)
                .Select(x => new
                {
                    IdUser = x.Key,
                    Load = x.Sum(y => y.Load)
                })
                .ToListAsync(CancellationToken);

            var counter = 1;
            var data =
                (
                    from _timeTablePref in timeTable
                    select new GetListTimeTableResult
                    {
                        No = counter++,
                        Id = _timeTablePref.TimeTablePrefHeader.Id,
                        IsMarge = _timeTablePref.TimeTablePrefHeader.IsMerge,
                        AcadYear = new CodeView
                        {
                            Id = _timeTablePref.Subject.Grade.Level.IdAcademicYear,
                            Code = _timeTablePref.Subject.Grade.Level.AcademicYear.Code,
                            Description = _timeTablePref.Subject.Grade.Level.AcademicYear.Description
                        },
                        Level = new CodeView
                        {
                            Id = _timeTablePref.Subject.Grade.IdLevel,
                            Code = _timeTablePref.Subject.Grade.Level.Code,
                            Description = _timeTablePref.Subject.Grade.Level.Description
                        },
                        Grade = new CodeView
                        {
                            Id = _timeTablePref.Subject.IdGrade,
                            Code = _timeTablePref.Subject.Grade.Code,
                            Description = _timeTablePref.Subject.Grade.Description
                        },
                        Class = new CodeView
                        {
                            Id = _timeTablePref.GradePathwayClassroom.Id,
                            Code =
                            _timeTablePref.TimeTablePrefHeader.SubjectCombination.GradePathwayClassroom.Classroom.Description + (_timeTablePref.TimeTablePrefHeader.Childs.Count > 0 ? "," + string.Join(" , ", _timeTablePref.TimeTablePrefHeader.Childs.Select(x => x.SubjectCombination.GradePathwayClassroom.Classroom.Description).Distinct()) : null),
                            Description = null
                        },
                        IdChilds = _timeTablePref.TimeTablePrefHeader.Childs.Count > 0 ? _timeTablePref.TimeTablePrefHeader.Childs.Select(p => p.Id).ToList() : new List<string>(),
                        Status = _timeTablePref.TimeTablePrefHeader.Status,
                        Subject = new SubjectVm
                        {
                            Id = _timeTablePref.Subject.Id,
                            SubjectId = _timeTablePref.Subject.SubjectID,
                            SubjectName = _timeTablePref.Subject.Code,
                            Description = _timeTablePref.Subject.Description
                        },
                        TotalSession = _timeTablePref.Subject.MaxSession,
                        Department = new CodeView
                        {
                            Id = _timeTablePref.Subject.IdDepartment,
                            Code = _timeTablePref.Subject.Department.Code,
                            Description = _timeTablePref.Subject.Department.Description
                        },
                        Streaming = new CodeView
                        {
                            Id = string.Join(",", _timeTablePref.GradePathwayClassroom.GradePathway.GradePathwayDetails.Select(x => x.Id).Distinct()),
                            Code = string.Join(" & ", _timeTablePref.GradePathwayClassroom.GradePathway.GradePathwayDetails.Select(x => x.Pathway.Code).Distinct()),
                            Description = string.Join(" & ", _timeTablePref.GradePathwayClassroom.GradePathway.GradePathwayDetails.Select(x => x.Pathway.Description).Distinct())
                        },
                        IsParent = _timeTablePref.TimeTablePrefHeader.IsParent,
                        TimeTableDetail = _timeTablePref.TimeTablePrefHeader.TimetablePrefDetails.Select(x => new TimeTableDetailVm
                        {
                            Id = x.Id,
                            Week = x.Week,
                            Count = x.Count,
                            Lenght = x.Length,
                            Department = _timeTablePref.Subject.Department.Description,
                            Streaming = string.Join(" & ", _timeTablePref.GradePathwayClassroom.GradePathway.GradePathwayDetails.Select(x => x.Pathway.Description).Distinct()),
                            BuildingVanue = new BuildingVenueVm
                            {
                                Id = x.IdVenue,
                                Code = x.Venue?.Building?.Code,
                                Description = string.Format("{0} - {1}", x.Venue?.Building?.Code, x.Venue?.Code),
                                BuildingCode = x.Venue?.Building?.Code,
                                BuildingDesc = x.Venue?.Building?.Description
                            },
                            Division = new CodeView
                            {
                                Id = x.IdDivision,
                                Code = x.Division?.Code,
                                Description = x.Division?.Description
                            },
                            Term = new CodeView
                            {
                                Id = x.IdPeriod,
                                Code = x.Period?.Code,
                                Description = x.Period?.Description
                            },
                            Level = _timeTablePref.Subject.Grade.IdLevel,
                            TotalLoad = teacherLoads.Where(y => y.IdUser == x.TeachingLoads.FirstOrDefault(y => y.IdTimetablePrefDetail == x.Id)?.IdUser).Select
                            (x => x.Load).FirstOrDefault(),
                            Teacher = new TeacherVm
                            {
                                BinusianId = reqUserProfile.FirstOrDefault(e => e.IdBinusian == (x.TeachingLoads.FirstOrDefault(y => y.IdTimetablePrefDetail == x.Id)?.IdUser))?.IdBinusian,
                                Code = reqUserProfile.FirstOrDefault(e => e.IdBinusian == (x.TeachingLoads.FirstOrDefault(y => y.IdTimetablePrefDetail == x.Id)?.IdUser))?.ShortName,
                                Description = reqUserProfile.FirstOrDefault(e => e.IdBinusian == (x.TeachingLoads.FirstOrDefault(y => y.IdTimetablePrefDetail == x.Id)?.IdUser))?.FirstName,
                                Id = x.TeachingLoads.FirstOrDefault(y => y.IdTimetablePrefDetail == x.Id)?.IdUser,
                                TotalLoad = teacherLoads.Where(y => y.IdUser == x.TeachingLoads.FirstOrDefault(y => y.IdTimetablePrefDetail == x.Id)?.IdUser).Select
                            (x => x.Load).FirstOrDefault()
                            }
                        }).ToList()
                    }).ToList();

            return Request.CreateApiResult2(data as object, param.CreatePaginationProperty(0).AddColumnProperty(_columns.Value));
        }
    }
}

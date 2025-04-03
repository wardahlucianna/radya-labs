using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.LHAndCA;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment.LHAndCA
{
    public class GetAssignLHAndCADetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _teachingDbContext;
        private readonly IStringLocalizer _localizer;

        public GetAssignLHAndCADetailHandler(ITeachingDbContext teachingDbContext, IStringLocalizer localizer)
        {
            _teachingDbContext = teachingDbContext;
            _localizer = localizer;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAssignLHAndCADetailRequest>(nameof(GetAssignLHAndCADetailRequest.Id), nameof(GetAssignLHAndCADetailRequest.IdSchool), nameof(GetAssignLHAndCADetailRequest.IdAcademicYear));
            var dataGrade = await _teachingDbContext.Entity<MsGrade>()
                .Include(x => x.Level)
                    .ThenInclude(x => x.AcademicYear)
                        .ThenInclude(x => x.School)
                .Where(x => x.Id == param.Id).FirstOrDefaultAsync();
            if (dataGrade == null)
                throw new BadRequestException(string.Format(_localizer["ExNotExist"], _localizer["Grade"], "Id", param.Id));
            var grade = dataGrade;
            var dataMaxTeacherLoad = _teachingDbContext.Entity<MsMaxTeacherLoad>().Where(x => x.IdAcademicYear == grade.Level.AcademicYear.Id).FirstOrDefault();
            if (dataMaxTeacherLoad == null)
                throw new BadRequestException("Max teacher load not been set for this Academic Year");
            var teacherAssignments = await _teachingDbContext.Entity<TrNonTeachingLoad>()
               .Include(x => x.MsNonTeachingLoad).ThenInclude(x => x.TrNonTeachingLoads)
               .Include(x => x.MsNonTeachingLoad).ThenInclude(x => x.TeacherPosition).ThenInclude(x => x.Position)
               .Where(x => new[] { PositionConstant.LevelHead, PositionConstant.ClassAdvisor }.Contains(x.MsNonTeachingLoad.TeacherPosition.Position.Code)
                        && x.MsNonTeachingLoad.IdAcademicYear == grade.Level.AcademicYear.Id)
               .ToListAsync(CancellationToken);

            var levelHeads = teacherAssignments.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.LevelHead);
            var currentLevelHead = levelHeads
                .Where(x => x.Data != null)
                .Select(x => new { usrLoad = x, lvlHead = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(x.Data) })
                .FirstOrDefault(x => x.lvlHead.TryGetValue("Grade", out var g) && g.Id == grade.Id)?.usrLoad;

            var classAdvisors = teacherAssignments
                .Where(x => x.Data != null && x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.ClassAdvisor)
                .Select(x => new { usrLoad = x, classAdvisor = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(x.Data) });

            var currentClassAdvisors = classAdvisors
                .Where(x => x.classAdvisor.TryGetValue("Classroom", out _) && x.classAdvisor.TryGetValue("Grade", out var g) && g.Id == grade.Id)
                .ToList();

            GetListGradePathwayClassRoomRequest req = new GetListGradePathwayClassRoomRequest
            {
                Ids = new List<string> { param.Id },
                Search = ""
            };
            var dataClassroomByGrade = await _teachingDbContext.Entity<MsGradePathwayClassroom>()
                .Include(x => x.Classroom)
                .Include(x => x.GradePathway).ThenInclude(x => x.GradePathwayDetails)
                .Where(x => x.GradePathway.IdGrade == param.Id)
                .Select(x => new GetClassroomMapByGradeResult
                {
                    Id = x.Id,
                    Code = x.Classroom.Code,
                    Description = x.Classroom.Description,
                    Formatted = $"{x.GradePathway.Grade.Code}{x.Classroom.Code}",
                    Grade = new CodeWithIdVm
                    {
                        Id = x.GradePathway.IdGrade,
                        Code = x.GradePathway.Grade.Code,
                        Description = x.GradePathway.Grade.Description
                    },
                    Pathway = new ClassroomMapPathway
                    {
                        Id = x.GradePathway.Id,
                        PathwayDetails = x.GradePathway.GradePathwayDetails.Select(y => new CodeWithIdVm
                        {
                            Id = y.Id,
                            Code = y.Pathway.Code,
                            Description = y.Pathway.Description
                        })
                    },
                    Class = new CodeWithIdVm
                    {
                        Id = x.Classroom.Id,
                        Code = x.Classroom.Code,
                        Description = x.Classroom.Description
                    }
                })
                .ToListAsync();


            var schoolUserIds = teacherAssignments.Select(y => y.IdUser).ToList();
            var reqUserProfile = await _teachingDbContext.Entity<MsStaff>()
                 .Where(p => schoolUserIds.Any(x => x == p.ShortName) ||
                            schoolUserIds.Any(x => x == p.IdBinusian))
                 .ToListAsync();
            //var timeTable = await _teachingDbContext.Entity<TrTimeTablePrefHeader>()
            //.Include(x => x.Childs)
            //.Include(x => x.TimetablePrefDetails)
            //    .ThenInclude(x => x.TeachingLoads)
            //.Include(x => x.SubjectCombination)
            //    .ThenInclude(x => x.Subject)
            //        .ThenInclude(x => x.Grade)
            //            .ThenInclude(x => x.Level)
            //.Where(x => x.IsActive && x.IdParent == null)
            //.Where(x => x.SubjectCombination.Subject.Grade.Level.AcademicYear.Id == param.IdAcademicYear)
            //.ToListAsync();

            //var idSubjectCombination = new List<string>();
            //var getChildSubjectCombination = timeTable.Where(p => p.Childs.Count > 0).SelectMany(p => p.Childs.Select(x => x.Id)).ToList();
            //if (getChildSubjectCombination.Count > 0)
            //{
            //    idSubjectCombination.AddRange(getChildSubjectCombination);
            //}

            //if (timeTable.Count > 0)
            //{
            //    idSubjectCombination.AddRange(timeTable.Select(p => p.Id).ToList());
            //}

            //var subjectCombs = await _teachingDbContext.Entity<MsSubjectCombination>()
            //    .Where(x => x.Subject.Grade.Level.IdAcademicYear == param.IdAcademicYear)
            //    .Select(x => new ItemValueVm(x.Id))
            //    .ToListAsync(CancellationToken);

            var schoolUsers =
            (
                from _users in reqUserProfile
                join _teachingLoad in
                (
                    from b in _teachingDbContext.Entity<TrTimeTablePrefHeader>()
                    join c in _teachingDbContext.Entity<TrTimetablePrefDetail>() on b.Id equals c.IdTimetablePrefHeader
                    join d in _teachingDbContext.Entity<TrTeachingLoad>() on c.Id equals d.IdTimetablePrefDetail
                    join e in _teachingDbContext.Entity<MsSubjectCombination>() on b.Id equals e.Id
                    join f in _teachingDbContext.Entity<MsSubject>() on e.IdSubject equals f.Id
                    join g in _teachingDbContext.Entity<MsGrade>() on f.IdGrade equals g.Id
                    join h in _teachingDbContext.Entity<MsLevel>() on g.IdLevel equals h.Id
                    where
                    1 == 1
                    && h.IdAcademicYear == param.IdAcademicYear
                    group d by d.IdUser into g
                    select new
                    {
                        IdUser = g.Key,
                        Load = g.Sum(x => x.Load)
                    }
                ) on _users.IdBinusian equals _teachingLoad.IdUser into joinedTeaching
                from _teachingLoad in joinedTeaching.DefaultIfEmpty(new
                {
                    IdUser = _users.IdBinusian,
                    Load = 0
                })
                join _nonTeachingLoad in
                (
                    from a in _teachingDbContext.Entity<TrNonTeachingLoad>()
                    join b in _teachingDbContext.Entity<MsNonTeachingLoad>() on a.IdMsNonTeachingLoad equals b.Id
                    where b.IdAcademicYear == param.IdAcademicYear
                    group a by a.IdUser into g
                    select new
                    {
                        IdUser = g.Key,
                        Load = g.Sum(x => x.Load)
                    }
                ) on _users.IdBinusian equals _nonTeachingLoad.IdUser into joinedNonTeaching
                from _nonTeachingLoad in joinedNonTeaching.DefaultIfEmpty(new
                {
                    IdUser = _users.IdBinusian,
                    Load = 0
                })
                select new
                {
                    User = _users,
                    Description = string.Format("{0} - {1} - {2} - {3}/{4}/{5}",
                    _users?.FirstName,
                    _users?.ShortName,
                    _users?.IdBinusian,
                    _teachingLoad?.Load,
                    _nonTeachingLoad?.Load,
                    (_teachingLoad?.Load + _nonTeachingLoad?.Load)),
                    Load = (_teachingLoad?.Load + _nonTeachingLoad?.Load)
                }
            ).ToList();
            var result = new GetAssignLHAndCADetailResult
            {
                MaxLoad = dataMaxTeacherLoad.Max,
                Acadyear = new CodeWithIdVm
                {
                    Id = grade.Level.IdAcademicYear,
                    Code = grade.Level.AcademicYear.Code,
                    Description = grade.Level.AcademicYear.Description
                },
                Level = new CodeWithIdVm
                {
                    Id = grade.IdLevel,
                    Code = grade.Level.Code,
                    Description = grade.Level.Description
                },
                Id = grade.Id,
                Code = grade.Code,
                Description = grade.Description,
                LevelHead = GetLevelHead(),
                ClassAdvisors = dataClassroomByGrade.OrderBy(x => x.Code).Select(x => CreateClassAdvisorDetail(x)),//new List<ClassAdvisorDetail>(), //.Payload.Select(x => CreateClassAdvisorDetail(x)),
                School = new GetSchoolResult
                {
                    Id = grade.Level.AcademicYear.School.Id,
                    Name = grade.Level.AcademicYear.School.Name,
                    Description = grade.Level.AcademicYear.School.Description,
                    Address = ""
                }
            };
            var LHAndCALoad = await _teachingDbContext.Entity<MsNonTeachingLoad>()
               .Include(x => x.TeacherPosition).ThenInclude(x => x.Position)
               .Where(x => new[] { PositionConstant.LevelHead, PositionConstant.ClassAdvisor }.Contains(x.TeacherPosition.Position.Code))
               .Where(x => x.IdAcademicYear == param.IdAcademicYear)
               .Select(x => new NonTeachingLoadChecking
               {
                   Code = x.TeacherPosition.Position.Code,
                   Id = x.Id,
                   Load = x.Load,
                   IdAcademicYear = x.IdAcademicYear
               })
               .ToListAsync(CancellationToken);

            var (lh, ca) = GetLHAndCALoad(LHAndCALoad, param.IdAcademicYear);
            result.LevelHeadLoad = lh;
            result.ClassAdvisorLoad = ca;

            return Request.CreateApiResult2(result as object);
            #region Internal Method

            ClassAdvisorDetail CreateClassAdvisorDetail(GetClassroomMapByGradeResult classroom)
            {
                var caLoad = GetClassAdvisorAndTotalLoad(classroom);
                var caDetail = new ClassAdvisorDetail
                {
                    Id = classroom.Id,
                    Code = classroom.Code,
                    Description = classroom.Description,
                    Status = Enumerable.Empty<string>(),
                    Pathway = new ItemValueVm(
                        classroom.Pathway.Id,
                        string.Join(", ", classroom.Pathway.PathwayDetails.Select(y => y.Code)))
                };
                caDetail.LoadAfterAssigned = caLoad.totalLoad;
                caDetail.ClassAdvisor = caLoad.ca;

                // check load above standard
                if (caLoad.totalLoad > dataMaxTeacherLoad.Max)
                    caDetail.Status = caDetail.Status.Concat(new[] { ColorStatusConstant.Red });
                // check already assigned to another class
                if (!string.IsNullOrEmpty(caLoad.status))
                    caDetail.Status = caDetail.Status.Concat(new[] { ColorStatusConstant.Blue });
                // check no teaching assignment
                if (caLoad.ca == null)
                    caDetail.Status = caDetail.Status.Concat(new[] { ColorStatusConstant.Green });
                return caDetail;
            }

            (ItemValueVm ca, int totalLoad, string status) GetClassAdvisorAndTotalLoad(GetClassroomMapByGradeResult classroom)
            {
                string statusConstant = "";
                var currentClassAdvisor = currentClassAdvisors.Find(x => x.classAdvisor["Classroom"].Id == classroom.Id);
                if (currentClassAdvisor is null)
                    return (null, 0, null);
                if (schoolUsers.Count > 0)
                {
                    var schoolUser = schoolUsers.Find(x => x.User.IdBinusian == currentClassAdvisor.usrLoad.IdUser);
                    var totalLoad = schoolUser != null ? schoolUser.Load : 0;
                    foreach (var ca in classAdvisors)
                    {
                        if (ca.classAdvisor.TryGetValue("Classroom", out var _classroom))
                        {
                            if (_classroom.Id != classroom.Id)
                            {
                                if (ca.usrLoad.IdUser == currentClassAdvisor.usrLoad.IdUser)
                                {
                                    statusConstant = ColorStatusConstant.Blue;
                                    break;
                                }
                            }
                            else
                            {
                                statusConstant = "";
                            }
                        }
                    }

                    return (
                        new ItemValueVm(schoolUser.User.IdBinusian, schoolUser.Description),
                        totalLoad.Value, statusConstant);
                }
                else
                {
                    return (null, 0, null);
                }
            }

            LevelHeadDetail GetLevelHead()
            {
                if (currentLevelHead is null)
                    return null;

                var schoolUser = schoolUsers.Find(x => x.User.IdBinusian == currentLevelHead.IdUser);
                return new LevelHeadDetail
                {
                    Id = schoolUser.User.IdBinusian,
                    Description = schoolUser.Description,
                    LoadAfterAssigned = schoolUser.Load.Value
                };
            }
            #endregion
        }

        private (NonTeachingLoadVm lh, NonTeachingLoadVm ca) GetLHAndCALoad(List<NonTeachingLoadChecking> result, string IdAcademicYear)
        {

            var lhLoad = result.FirstOrDefault(x => x.Code == PositionConstant.LevelHead && x.IdAcademicYear == IdAcademicYear);
            var caLoad = result.FirstOrDefault(x => x.Code == PositionConstant.ClassAdvisor && x.IdAcademicYear == IdAcademicYear);
            return (
                lhLoad is null ? null : new NonTeachingLoadVm { Id = lhLoad.Id, Load = lhLoad.Load },
                caLoad is null ? null : new NonTeachingLoadVm { Id = caLoad.Id, Load = caLoad.Load });
        }
    }

    public class NonTeachingLoadChecking
    {
        public string Code { get; set; }
        public string Id { get; set; }
        public int Load { get; set; }
        public string IdAcademicYear { get; set; }
    }

}

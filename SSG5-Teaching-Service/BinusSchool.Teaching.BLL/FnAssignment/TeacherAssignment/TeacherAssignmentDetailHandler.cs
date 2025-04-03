using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using BinusSchool.Persistence.TeachingDb;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using CodeView = BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination.CodeView;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment
{
    public class TeacherAssignmentDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        private readonly IStringLocalizer _localizer;

        public TeacherAssignmentDetailHandler(
            ITeachingDbContext dbContext,
            IStringLocalizer localizer)
        {
            _dbContext = dbContext;
            _localizer = localizer;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<TeacherAssignmentDetailRequest>(new string[] { nameof(TeacherAssignmentDetailRequest.IdSchoolUser), nameof(TeacherAssignmentDetailRequest.IdSchoolAcademicYear) });
            FillConfiguration();

            var dataTeacher = await _dbContext.Entity<MsStaff>()
                            .Where(p => p.IdBinusian == param.IdSchoolUser)
                            .Select(p => new CheckTeacherForAscTimetableResult
                            {
                                TeacherBinusianId = p.IdBinusian,
                                TeacherName = NameUtil.GenerateFullName(p.FirstName, p.LastName),
                            }).FirstOrDefaultAsync(CancellationToken);

            var dataAcadYear = await _dbContext.Entity<MsAcademicYear>()
                    .Where(x => x.Id == param.IdSchoolAcademicYear)
                    .FirstOrDefaultAsync(CancellationToken);

            #region data
            var listGrade = await _dbContext.Entity<MsGrade>()
                                    .Include(e => e.Level)
                                    .Where(e => e.Level.IdAcademicYear==param.IdSchoolAcademicYear)
                                    .ToListAsync(CancellationToken);

            var listDepartment = await _dbContext.Entity<MsDepartment>()
                                    .Where(e => e.IdAcademicYear == param.IdSchoolAcademicYear)
                                    .ToListAsync(CancellationToken);

            var listSubject = await _dbContext.Entity<MsSubject>()
                                    .Include(e=>e.Grade)
                                    .Where(e => e.Grade.Level.IdAcademicYear == param.IdSchoolAcademicYear)
                                    .ToListAsync(CancellationToken);

            var listStreaming = await _dbContext.Entity<MsGradePathwayDetail>()
                                    .Include(e => e.GradePathway).ThenInclude(e => e.Grade)
                                    .Include(e => e.Pathway)
                                    .Where(e => e.GradePathway.Grade.Level.IdAcademicYear == param.IdSchoolAcademicYear)
                                    .ToListAsync(CancellationToken);

            var listClassroom = await _dbContext.Entity<MsGradePathwayClassroom>()
                                    .Include(e => e.Classroom)
                                    .Include(e => e.GradePathway).ThenInclude(e=>e.Grade)
                                    .Where(e => e.GradePathway.Grade.Level.IdAcademicYear == param.IdSchoolAcademicYear)
                                    .ToListAsync(CancellationToken);
            #endregion

            #region non teaching load
            var listMsNonTeachingLoad = await _dbContext.Entity<MsNonTeachingLoad>()
                                        .Include(x => x.TeacherPosition)
                                        .Where(x => x.IdAcademicYear == param.IdSchoolAcademicYear)
                                        .Select(x => new
                                        {
                                            Id = x.Id,
                                            TeacherPositionCode = x.TeacherPosition.Code,
                                            TeacherPosition = x.TeacherPosition.Description,
                                            Parameter = x.Parameter,
                                            Load = x.Load,
                                            Category = x.Category
                                        })
                                        .ToListAsync(CancellationToken);

            var listTrNonTeachingLoad = await _dbContext.Entity<TrNonTeachingLoad>()
                                        .Include(x => x.MsNonTeachingLoad)
                                        .Where(x => x.IdUser == param.IdSchoolUser)
                                        .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdSchoolAcademicYear)
                                        .Select(e => new NonTeachingLoad
                                        {
                                            Load = e.Load,
                                            Category = e.MsNonTeachingLoad.Category,
                                            Id = e.Id,
                                            IdUser = e.IdUser,
                                            IdMsNonTeachingLoad = e.IdMsNonTeachingLoad,
                                            Data = e.Data
                                        })
                                        .ToListAsync(CancellationToken);

            List<NonTeachingLoad> listNonTeachingLoad = new List<NonTeachingLoad>();
            foreach (var nonTeachingLoad in listTrNonTeachingLoad)
            {
                if (nonTeachingLoad.Category == AcademicType.Academic)
                {
                    if (nonTeachingLoad.Data == null)
                        continue;

                    var paramData = new GetNonTeachingLoadRequest
                    {
                        IdAcademicYear = param.IdSchoolAcademicYear,
                        Data = nonTeachingLoad.Data,
                        Grade = listGrade,
                        Department = listDepartment,
                        Subject = listSubject,
                        Streaming = listStreaming,
                        Classroom = listClassroom,
                    };

                    var dataObject = GetDataNonTeachingLoad(paramData);

                    if (!dataObject)
                        continue;

                    listNonTeachingLoad.Add(nonTeachingLoad);
                }
                else
                {
                    listNonTeachingLoad.Add(nonTeachingLoad);
                }
            }
            #endregion
            

            var getDetails =
                new TeacherAssignmentGetDetailResult
                {
                    Id = dataTeacher.TeacherBinusianId,
                    IdSchoolUser = dataTeacher.TeacherBinusianId,
                    IdSchoolAcademicYears = param.IdSchoolAcademicYear,
                    Academicyears = dataAcadYear.Description,
                    MaxLoadinSchool = _dbContext.Entity<MsMaxTeacherLoad>().Where(x => x.IdAcademicYear == param.IdSchoolAcademicYear).Select(x => x.Max).FirstOrDefault(),
                    TeacherName = dataTeacher.TeacherName,
                    //TeachingLoad = teachingLoad != null ? teachingLoad.Load : 0,
                    NonTeacingLoadAcademic = listNonTeachingLoad
                                              .Where(x => x.Category == AcademicType.Academic)
                                              .Sum(x => x.Load),
                    NonTeacingLoadNonAcademic = listNonTeachingLoad
                                      .Where(x => x.Category == AcademicType.NonAcademic)
                                      .Sum(x => x.Load),
                    TotalLoad = listNonTeachingLoad
                                      .Sum(x => x.Load),
                    NonTeachingAssignmentAcademic = listNonTeachingLoad
                                      .Where(x => x.Category == AcademicType.Academic)
                                      .Select(x => new NonteacingLoadAcademic()
                                      {
                                          Id = x.Id,
                                          IdSchoolUser = x.IdUser,
                                          IdSchoolNonTeachingLoad = x.IdMsNonTeachingLoad,
                                          Load = x.Load,
                                          Data = x.Data,
                                      }).ToList(),
                    NonTeachingAssignmentNonAcademic = listNonTeachingLoad
                                      .Where(x => x.Category == AcademicType.NonAcademic)
                                      .Select(x => new NonteacingLoadNonAcademic()
                                      {
                                          Id = x.Id,
                                          IdSchoolUser = x.IdUser,
                                          IdSchoolNonTeachingLoad = x.IdMsNonTeachingLoad,
                                          Load = x.Load,
                                          Data = x.Data,
                                      }).ToList(),
                    ListNonTeachingAcademics = listMsNonTeachingLoad
                                      .Where(x => x.Category == AcademicType.Academic)
                                      .Select(x => new ListNonTeachingAcademic
                                      {
                                          Id = x.Id,
                                          Name = x.TeacherPositionCode,
                                          Description = x.TeacherPosition,
                                          Data = x.Parameter,
                                          Load = x.Load

                                      }).ToList(),
                    ListNonTeachingNonAcademics = listMsNonTeachingLoad
                                      .Where(x => x.Category == AcademicType.NonAcademic)
                                      .Select(x => new ListNonTeachingAcademic
                                      {
                                          Id = x.Id,
                                          Name = x.TeacherPositionCode,
                                          Description = x.TeacherPosition,
                                          Data = x.Parameter,
                                          Load = x.Load
                                      }).ToList()
                };

            return Request.CreateApiResult2(getDetails as object);
        }

        public bool GetDataNonTeachingLoad(GetNonTeachingLoadRequest param)
        {
            string idLevel = null;
            string idDepartment = null;
            string idGrade = null;
            string idSubject = null;
            string idStreaming = null;
            string idClassroom = null;

            var _dataPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(param.Data);
            _dataPosition.TryGetValue("Level", out var _LevelPosition);
            _dataPosition.TryGetValue("Department", out var _DepartemenPosition);
            _dataPosition.TryGetValue("Grade", out var _GradePosition);
            _dataPosition.TryGetValue("Subject", out var _SubjectPosition);
            _dataPosition.TryGetValue("Streaming", out var _StreamingPosition);
            _dataPosition.TryGetValue("Classroom", out var _ClassroomPosition);

            if (_LevelPosition != null)
            {
                idLevel = _LevelPosition.Id;
                var listLevel = param.Grade.Where(e => e.IdLevel == idLevel).ToList();

                if (!listLevel.Any())
                    return false;
            }

            if (_DepartemenPosition != null)
            {
                idDepartment = _DepartemenPosition.Id;
                var listDepartemen = param.Department.Where(e => e.Id == idDepartment).ToList();

                if (!listDepartemen.Any())
                    return false;
            }

            if (_GradePosition != null)
            {
                idGrade = _GradePosition.Id;
                var listGrade = param.Grade.Where(e => e.Id == idGrade && e.IdLevel==idLevel).ToList();

                if (!listGrade.Any())
                    return false;
            }

            if (_SubjectPosition != null)
            {
                idSubject = _SubjectPosition.Id;

                var listSubject = new List<MsSubject>();

                listSubject = string.IsNullOrEmpty(idGrade)
                    ? param.Subject.Where(e => e.Id == idSubject && e.IdDepartment == idDepartment && e.Grade.IdLevel == idLevel).ToList()
                    : param.Subject.Where(e => e.Id == idSubject && e.IdGrade == idGrade && e.IdDepartment == idDepartment && e.Grade.IdLevel == idLevel).ToList();

                if (!listSubject.Any())
                    return false;
            }

            if (_StreamingPosition != null)
            {
                idStreaming = _StreamingPosition.Id;
                var Streaming = _StreamingPosition.Description;

                var listStreaming = param.Streaming
                                    .Where(e => e.IdGradePathway == idStreaming 
                                            && e.Pathway.Description == Streaming 
                                            && e.GradePathway.IdGrade==idGrade 
                                            && e.GradePathway.Grade.IdLevel==idLevel)
                                    .ToList();

                if (!listStreaming.Any())
                    return false;
            }

            if (_ClassroomPosition != null)
            {
                idClassroom = _ClassroomPosition.Id;
                var listClassroom = param.Classroom
                                    .Where(e => e.Id == idClassroom && e.GradePathway.IdGrade==idGrade && e.GradePathway.Grade.IdLevel==idLevel)
                                    .ToList();

                if (!listClassroom.Any())
                    return false;
            }

            return true;
        }

        //protected override async Task<ApiErrorResult<object>> HandlerOld()
        //{
        //    var param = Request.ValidateParams<TeacherAssignmentDetailRequest>(new string[] { nameof(TeacherAssignmentDetailRequest.IdSchoolUser), nameof(TeacherAssignmentDetailRequest.IdSchoolAcademicYear) });
        //    FillConfiguration();

        //    var ListShortName = new List<string>() { param.IdSchoolUser };
        //    var ListAcadyears = new List<string>() { param.IdSchoolAcademicYear };

        //    var dataTeacher = await _dbContext.Entity<MsStaff>()
        //                    .Where(p => ListShortName.Any(x => x == p.ShortName) ||
        //                                ListShortName.Any(x => x == p.IdBinusian) )
        //                    .Select(p => new CheckTeacherForAscTimetableResult
        //                    {
        //                        TeacherBinusianId = p.IdBinusian,
        //                        TeacherName = p.FirstName,
        //                    }).ToListAsync();

        //    var dataAcadYear = _dbContext.Entity<MsAcademicYear>()
        //            .Where(x => ListAcadyears.Contains(x.Id))
        //            .ToList();

        //    var dataTeachingLoad = _dbContext.Entity<TrTeachingLoad>().Where(x => x.IdUser == param.IdSchoolUser).Select(x => x.IdSubjectCombination).ToList();

        //    var ListIds = dataTeachingLoad.Count > 0 ? dataTeachingLoad : new List<string>() { "dummy" };
        //    var dataSubjectCombination = await _dbContext.Entity<MsSubjectCombination>()
        //        .Include(x => x.Subject)
        //        .Include(x => x.Subject)
        //            .ThenInclude(x => x.Department)
        //        .Include(x => x.Subject)
        //            .ThenInclude(x => x.Grade)
        //                .ThenInclude(x => x.Level)
        //                    .ThenInclude(x => x.AcademicYear)
        //        .Include(x => x.GradePathwayClassroom)
        //            .ThenInclude(x => x.Classroom)
        //        .Include(x => x.GradePathwayClassroom)
        //            .ThenInclude(x => x.GradePathway)
        //                .ThenInclude(x => x.Grade)
        //        .Where(x => ListIds.Contains(x.Id))
        //        .Select(x => new GetListSubjectCombinationTimetableResult
        //        {
        //            Id = x.Id,
        //            AcadYear = new CodeView
        //            {
        //                Id = x.Subject.Grade.Level.IdAcademicYear,
        //                Code = x.Subject.Grade.Level.AcademicYear.Code,
        //                Description = x.Subject.Grade.Level.AcademicYear.Description,
        //                IdMapping = x.GradePathwayClassroom.GradePathway.Grade.Level.AcademicYear.Id
        //            },
        //        }).ToListAsync();

        //    var teachingLoad = 
        //     (from a in dataSubjectCombination
        //        join b in _dbContext.Entity<TrTimeTablePrefHeader>() on a.Id equals b.Id
        //        join c in _dbContext.Entity<TrTimetablePrefDetail>() on b.Id equals c.IdTimetablePrefHeader
        //        join d in _dbContext.Entity<TrTeachingLoad>() on c.Id equals d.IdTimetablePrefDetail
        //        where
        //        d.IdUser == param.IdSchoolUser
        //        group d by d.IdUser into g
        //        select new
        //        {
        //            Load =g.Sum(x=>x.Load)
        //        }).FirstOrDefault();

        //    var getDetails =
        //        (
        //            from _dataTeacher in dataTeacher
        //            where _dataTeacher.TeacherBinusianId == param.IdSchoolUser
        //            select new TeacherAssignmentGetDetailResult
        //            {
        //                Id = _dataTeacher.TeacherBinusianId,
        //                IdSchoolUser = _dataTeacher.TeacherBinusianId,
        //                IdSchoolAcademicYears = param.IdSchoolAcademicYear,
        //                Academicyears = dataAcadYear.Where(x=>x.Id == param.IdSchoolAcademicYear).Select(x=>x.Description).FirstOrDefault(),
        //                MaxLoadinSchool = _dbContext.Entity<MsMaxTeacherLoad>().Where(x => x.IdAcademicYear == param.IdSchoolAcademicYear).Select(x=>x.Max).FirstOrDefault(),
        //                TeacherName = _dataTeacher.TeacherName,
        //                TeachingLoad = teachingLoad != null ? teachingLoad.Load : 0,
        //                NonTeacingLoadAcademic = _dbContext.Entity<TrNonTeachingLoad>()
        //                              .Include(x => x.MsNonTeachingLoad)
        //                              .Where(x => x.IdUser == param.IdSchoolUser)
        //                              .Where(x => x.MsNonTeachingLoad.Category == AcademicType.Academic)
        //                              .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdSchoolAcademicYear)
        //                              .Sum(x => x.Load),
        //                NonTeacingLoadNonAcademic = _dbContext.Entity<TrNonTeachingLoad>()
        //                              .Include(x => x.MsNonTeachingLoad)
        //                              .Where(x => x.IdUser == param.IdSchoolUser)
        //                              .Where(x => x.MsNonTeachingLoad.Category == AcademicType.NonAcademic)
        //                              .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdSchoolAcademicYear)
        //                              .Sum(x => x.Load),
        //                TotalLoad = (teachingLoad != null ? teachingLoad.Load : 0) +
        //                              _dbContext.Entity<TrNonTeachingLoad>()
        //                              .Include(x => x.MsNonTeachingLoad)
        //                              .Where(x => x.IdUser == param.IdSchoolUser)
        //                              .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdSchoolAcademicYear)
        //                              .Sum(x => x.Load),
        //                NonTeachingAssignmentAcademic = _dbContext.Entity<TrNonTeachingLoad>()
        //                              .Include(x => x.MsNonTeachingLoad)
        //                                .ThenInclude(x => x.TeacherPosition)
        //                              .Where(x => x.IdUser == param.IdSchoolUser)
        //                              .Where(x => x.MsNonTeachingLoad.Category == AcademicType.Academic)
        //                              .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdSchoolAcademicYear)
        //                              .Select(x => new NonteacingLoadAcademic()
        //                              {
        //                                  Id = x.Id,
        //                                  IdSchoolUser = x.IdUser,
        //                                  IdSchoolNonTeachingLoad = x.IdMsNonTeachingLoad,
        //                                  Load = x.Load,
        //                                  Data = x.Data,
        //                              }).ToList(),
        //                NonTeachingAssignmentNonAcademic = _dbContext.Entity<TrNonTeachingLoad>()
        //                              .Include(x => x.MsNonTeachingLoad)
        //                                .ThenInclude(x => x.TeacherPosition)
        //                              .Where(x => x.IdUser == param.IdSchoolUser)
        //                              .Where(x => x.MsNonTeachingLoad.Category == AcademicType.NonAcademic)
        //                              .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdSchoolAcademicYear)
        //                              .Select(x => new NonteacingLoadNonAcademic()
        //                              {
        //                                  Id = x.Id,
        //                                  IdSchoolUser = x.IdUser,
        //                                  IdSchoolNonTeachingLoad = x.IdMsNonTeachingLoad,
        //                                  Load = x.Load,
        //                                  Data = x.Data,
        //                              }).ToList(),
        //                ListNonTeachingAcademics = _dbContext.Entity<MsNonTeachingLoad>()
        //                              .Include(x => x.TeacherPosition)
        //                              .Where(x => x.IdAcademicYear == param.IdSchoolAcademicYear)
        //                              .Where(x => x.Category == AcademicType.Academic)
        //                              .Select(x => new ListNonTeachingAcademic
        //                              {
        //                                  Id = x.Id,
        //                                  Name = x.TeacherPosition.Code,
        //                                  Description = x.TeacherPosition.Description,
        //                                  Data = x.Parameter,
        //                                  Load = x.Load

        //                              }).ToList(),
        //                ListNonTeachingNonAcademics = _dbContext.Entity<MsNonTeachingLoad>()
        //                              .Include(x => x.TeacherPosition)
        //                              .Where(x => x.IdAcademicYear == param.IdSchoolAcademicYear)
        //                              .Where(x => x.Category == AcademicType.NonAcademic)
        //                              .Select(x => new ListNonTeachingAcademic
        //                              {
        //                                  Id = x.Id,
        //                                  Name = x.TeacherPosition.Code,
        //                                  Description = x.TeacherPosition.Description,
        //                                  Data = x.Parameter,
        //                                  Load = x.Load
        //                              }).ToList()
        //            }
        //        ).FirstOrDefault();

        //    return Request.CreateApiResult2(getDetails as object);
        //}
    }


    public class NonTeachingLoad
    {
        public int Load { get; set; }
        public AcademicType Category { get; set; }
        public string Id { get; set; }
        public string IdUser { get; set; }
        public string IdMsNonTeachingLoad { get; set; }
        public string Data { get; set; }
    }

    public class GetNonTeachingLoadRequest
    {
        public string IdAcademicYear { get; set; }
        public string Data { get; set; }
        public List<MsGrade> Grade { get; set; }
        public List<MsDepartment> Department { get; set; }
        public List<MsSubject> Subject { get; set; }
        public List<MsGradePathwayDetail> Streaming { get; set; }
        public List<MsGradePathwayClassroom> Classroom { get; set; }
    }
}

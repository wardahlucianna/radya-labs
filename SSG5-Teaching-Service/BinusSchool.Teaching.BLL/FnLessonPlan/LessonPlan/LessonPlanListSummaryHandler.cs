using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Constants;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanListSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public LessonPlanListSummaryHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLessonPlanSummaryRequest>(nameof(GetLessonPlanSummaryRequest.IdUser));

            var listLessonByUser = await GetLessonByUser(_dbContext, CancellationToken, param.IdUser, param.IdAcademicYear, param.PositionCode);

            var dataApprovers = await _dbContext.Entity<MsLessonApprovalState>()
                .Include(x => x.LessonApproval)
                .Where(x => x.IdUser == param.IdUser)
                .Select(x => x.IdUser)
                .ToListAsync(CancellationToken);
            var isApprover = dataApprovers.Contains(param.IdUser);

            var dataQuery = (
                from tlp in _dbContext.Entity<TrLessonPlan>()
                join mlt in _dbContext.Entity<MsLessonTeacher>() on tlp.IdLessonTeacher equals mlt.Id
                join ml in _dbContext.Entity<MsLesson>() on mlt.IdLesson equals ml.Id
                join ms in _dbContext.Entity<MsSubject>() on ml.IdSubject equals ms.Id
                join mg in _dbContext.Entity<MsGrade>() on ms.IdGrade equals mg.Id
                join ml2 in _dbContext.Entity<MsLevel>() on mg.IdLevel equals ml2.Id
                join may in _dbContext.Entity<MsAcademicYear>() on ml2.IdAcademicYear equals may.Id
                join ms2 in _dbContext.Entity<MsStaff>() on mlt.IdUser equals ms2.IdBinusian
                join mwsd in _dbContext.Entity<MsWeekSettingDetail>() on tlp.IdWeekSettingDetail equals mwsd.Id
                join mws in _dbContext.Entity<MsWeekSetting>() on mwsd.IdWeekSetting equals mws.Id
                join mp in _dbContext.Entity<MsPeriod>() on new { A = mg.Id, B = mws.IdPeriod } equals new { A = mp.IdGrade, B = mp.Id }
                // where
                //     isApprover == false ? mlt.IdUser == param.IdUser : 1 == 1
                group new { tlp, mlt, ms, mg, ml2, may, mwsd, mws, mp } by new
                {
                    IdUser = mlt.IdUser,
                    IdAcademicYear = may.Id,
                    AcademicYear = may.Code,
                    IdLevel = ml2.Id,
                    Level = ml2.Description,
                    IdGrade = mg.Id,
                    Grade = mg.Description,
                    Subject = ms.Code,
                    IdTerm = mp.Id,
                    Term = mp.Code,
                    IdSubject = ms.Id,
                    IdWeekSettingDetail = mwsd.Id,
                    mws.IdPeriod,
                    mlt.IdLesson
                } into gb

                select new
                {
                    gb.Key.IdUser,
                    gb.Key.IdAcademicYear,
                    gb.Key.AcademicYear,
                    gb.Key.IdLevel,
                    gb.Key.Level,
                    gb.Key.IdGrade,
                    gb.Key.Grade,
                    gb.Key.Subject,
                    gb.Key.IdTerm,
                    gb.Key.Term,
                    gb.Key.IdSubject,
                    gb.Key.IdPeriod,
                    gb.Key.IdLesson,
                }
            )
            .Distinct()
            .AsQueryable();

            // Filter

            var listIdlesson = listLessonByUser.Select(e => e.IdLesson).Distinct().ToList();
            if (listLessonByUser.Any())
            {
                dataQuery = dataQuery.Where(x => listIdlesson.Contains(x.IdLesson));
            }

            if (param.IdAcademicYear != null)
                dataQuery = dataQuery.Where(x => x.IdAcademicYear == param.IdAcademicYear);

            if (param.IdLevel != null)
                dataQuery = dataQuery.Where(x => x.IdLevel == param.IdLevel);

            if (param.IdGrade != null)
                dataQuery = dataQuery.Where(x => x.IdGrade == param.IdGrade);

            if (param.IdPeriod != null)
                dataQuery = dataQuery.Where(x => x.IdTerm == param.IdPeriod);

            if (param.IdSubject != null)
                dataQuery = dataQuery.Where(x => x.IdSubject == param.IdSubject);

            switch (param.OrderBy)
            {
                case "academicYear":
                    dataQuery = param.OrderType == OrderType.Desc
                        ? dataQuery.OrderByDescending(x => x.AcademicYear)
                        : dataQuery.OrderBy(x => x.AcademicYear);
                    break;
                case "level":
                    dataQuery = param.OrderType == OrderType.Desc
                        ? dataQuery.OrderByDescending(x => x.Level)
                        : dataQuery.OrderBy(x => x.Level);
                    break;
                case "grade":
                    dataQuery = param.OrderType == OrderType.Desc
                        ? dataQuery.OrderByDescending(x => x.Grade.Length).ThenByDescending(x => x.Grade)
                        : dataQuery.OrderBy(x => x.Grade.Length).ThenBy(x => x.Grade);
                    break;
                case "term":
                    dataQuery = param.OrderType == OrderType.Desc
                        ? dataQuery.OrderByDescending(x => x.Term)
                        : dataQuery.OrderBy(x => x.Term);
                    break;
                case "subject":
                    dataQuery = param.OrderType == OrderType.Desc
                        ? dataQuery.OrderByDescending(x => x.Subject)
                        : dataQuery.OrderBy(x => x.Subject);
                    break;
            };

            var dataListQuery_ = dataQuery.ToList();

            var dataListQuery = dataListQuery_.GroupBy(x => new
            {
                x.IdUser,
                x.IdAcademicYear,
                x.AcademicYear,
                x.IdLevel,
                x.Level,
                x.IdGrade,
                x.Grade,
                x.Subject,
                x.IdTerm,
                x.Term,
                x.IdSubject,
                x.IdPeriod,
            }).Select(y => new
            {
                y.Key.IdUser,
                y.Key.IdAcademicYear,
                y.Key.AcademicYear,
                y.Key.IdLevel,
                y.Key.Level,
                y.Key.IdGrade,
                y.Key.Grade,
                y.Key.Subject,
                y.Key.IdTerm,
                y.Key.Term,
                y.Key.IdSubject,
                y.Key.IdPeriod,
            }).ToList();

            var result = new List<GetLessonPlanSummaryResult>();

            foreach (var d in dataListQuery)
            {
                var dataWeekQuery = new List<GetWeekQueryResult>();
                var lessonPlan = _dbContext.Entity<TrLessonPlan>()
                    .Include(x => x.WeekSettingDetail)
                        .ThenInclude(x => x.WeekSetting)
                    .Include(x => x.LessonTeacher)
                        .ThenInclude(x => x.Lesson)
                            .ThenInclude(x => x.Subject)
                    .Where(x => x.WeekSettingDetail.WeekSetting.IdPeriod == d.IdPeriod && x.LessonTeacher.Lesson.IdSubject == d.IdSubject && listIdlesson.Contains(x.LessonTeacher.IdLesson))
                    .ToList();


                var IdLessonPlan = "";
                foreach (var lp in lessonPlan)
                {
                    var PositionCode = listLessonByUser.Where(e => e.IdLesson == lp.LessonTeacher.IdLesson).Select(e => e.PositionCode).Distinct().FirstOrDefault();
                    var isLessonPlanSubjectTeacher = listLessonByUser.Where(e => e.PositionCode == PositionConstant.SubjectTeacher.ToString() && e.IdLesson == lp.LessonTeacher.IdLesson).Any();
                    if (isLessonPlanSubjectTeacher)
                    {
                        var lessonPlanByST = lessonPlan.Where(e => e.LessonTeacher.IdUser == param.IdUser && e.Id == lp.Id).FirstOrDefault();

                        var detailedWeek = new GetWeekQueryResult()
                        {
                            IdWeekSettingDetail = lp.IdWeekSettingDetail,
                            WeekNumber = lp.WeekSettingDetail.WeekNumber,
                            PositionCode = PositionConstant.SubjectTeacher
                        };

                        if (lessonPlanByST == null)
                        {
                            detailedWeek.Uploaded = 0;
                            detailedWeek.Total = 0;
                        }
                        else
                        {
                            detailedWeek.Uploaded = lp.Status == "Approved" || lp.Status == "Created" || lp.Status == "Late" ? 1 : 0;
                            detailedWeek.Total++;
                        }
                        dataWeekQuery.Add(detailedWeek);
                        continue;
                    }

                    var uploaded = dataWeekQuery.Where(x => x.IdWeekSettingDetail == lp.IdWeekSettingDetail && x.WeekNumber == lp.WeekSettingDetail.WeekNumber).FirstOrDefault();
                    if (uploaded == null)
                    {
                        var detailedWeek = new GetWeekQueryResult()
                        {
                            IdWeekSettingDetail = lp.IdWeekSettingDetail,
                            WeekNumber = lp.WeekSettingDetail.WeekNumber,
                            Uploaded = lp.Status == "Approved" || lp.Status == "Created" || lp.Status == "Late" ? 1 : 0,
                            Total = 1,
                            PositionCode = PositionCode
                        };
                        dataWeekQuery.Add(detailedWeek);
                    }
                    else
                    {
                        uploaded.Uploaded = lp.Status == "Approved" || lp.Status == "Created" || lp.Status == "Late" ? (uploaded.Uploaded + 1) : uploaded.Uploaded;
                        uploaded.Total++;
                    }
                    IdLessonPlan = lp.Id;
                }

                var idLP = IdLessonPlan;
                var dataWeek = new List<GetWeekResult>();

                var listWeekNumber = dataWeekQuery.Select(e=>e.WeekNumber).OrderBy(x => x).Distinct().ToList();

                foreach(var weekNumber in listWeekNumber)
                {
                    var listWeekNumberByWeek = dataWeekQuery.Where(e => e.WeekNumber == weekNumber).ToList();
                    var getWeekNumberByWeek = listWeekNumberByWeek.FirstOrDefault();

                    if (getWeekNumberByWeek != null)
                    {
                        var total = listWeekNumberByWeek.Select(e => e.Total).Sum();
                        var upload = listWeekNumberByWeek.Select(e => e.Uploaded).Sum();
                        dataWeek.Add(new GetWeekResult
                        {
                            IdWeekSettingDetail = getWeekNumberByWeek.IdWeekSettingDetail,
                            WeekNumber = getWeekNumberByWeek.WeekNumber,
                            Text = upload.ToString() + " / " + total.ToString(),
                            PositionCode = getWeekNumberByWeek.PositionCode
                        });
                    }
                }

                //foreach (var dwq in dataWeekQuery.OrderBy(x => x.WeekNumber).ToList())
                //{
                //    dataWeek.Add(new GetWeekResult
                //    {
                //        IdWeekSettingDetail = dwq.IdWeekSettingDetail,
                //        WeekNumber = dwq.WeekNumber,
                //        Text = dwq.Uploaded.ToString() + " / " + dwq.Total.ToString(),
                //        PositionCode = dwq.PositionCode
                //    });
                //}

                if (string.IsNullOrEmpty(idLP))
                {
                    result.Add(new GetLessonPlanSummaryResult
                    {
                        IdAcademicYear = d.IdAcademicYear,
                        AcademicYear = d.AcademicYear,
                        IdLevel = d.IdLevel,
                        Level = d.Level,
                        IdGrade = d.IdGrade,
                        Grade = d.Grade,
                        IdTerm = d.IdTerm,
                        Term = d.Term,
                        Subject = d.Subject,
                        IdSubject = d.IdSubject,
                        IdLessonPlan = idLP,
                        IdLessonTeacher = null,
                        Week = dataWeek
                        //ClassId = d.classId
                    });
                }
                else
                {
                    if (!result.Any(x => x.IdLessonPlan == idLP))
                    {
                        result.Add(new GetLessonPlanSummaryResult
                        {
                            IdAcademicYear = d.IdAcademicYear,
                            AcademicYear = d.AcademicYear,
                            IdLevel = d.IdLevel,
                            Level = d.Level,
                            IdGrade = d.IdGrade,
                            Grade = d.Grade,
                            IdTerm = d.IdTerm,
                            Term = d.Term,
                            Subject = d.Subject,
                            IdSubject = d.IdSubject,
                            IdLessonPlan = idLP,
                            IdLessonTeacher = null,
                            Week = dataWeek
                            //ClassId = d.classId
                        });
                    }
                }             
            }

            var count = param.CanCountWithoutFetchDb(result.Count)
            ? result.Count
            : result.Select(x => x.AcademicYear).Count();

            return Request.CreateApiResult2(result as object, param.CreatePaginationProperty(count));
        }

        public static async Task<List<LessonByUser>> GetLessonByUser(ITeachingDbContext _dbContext, System.Threading.CancellationToken CancellationToken, string IdUser, string IdAcademicYear, string PositionCode)
        {
            List<LessonByUser> listLessonByUser = new List<LessonByUser>();

            var idSchool = await _dbContext.Entity<MsAcademicYear>()
                                   .Where(x => x.Id == IdAcademicYear)
                                   .Select(e => e.IdSchool)
                                   .FirstOrDefaultAsync(CancellationToken);

            var listTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                                       .Include(e => e.Position)
                                       .Where(x => x.IdSchool == idSchool)
                                       .Select(e => new
                                       {
                                           Id = e.Id,
                                           PositionCode = e.Position.Code,
                                       })
                                       .ToListAsync(CancellationToken);

            #region CA
            var listHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
               .Include(e => e.TeacherPosition).ThenInclude(e => e.Position)
               .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
               .Where(x => x.IdBinusian == IdUser && x.Homeroom.IdAcademicYear == IdAcademicYear)
               .Select(e => new
               {
                   e.IdHomeroom,
                   PositionCode = e.TeacherPosition.Position.Code
               })
               .Distinct().ToListAsync(CancellationToken);

            var listIdHomeroom = listHomeroomTeacher.Select(e => e.IdHomeroom).ToList();

            var listLessonByCa = await _dbContext.Entity<MsLessonPathway>()
               .Include(e => e.Lesson).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
               .Include(e => e.HomeroomPathway)
               .Where(x => listIdHomeroom.Contains(x.HomeroomPathway.IdHomeroom))
               .Select(e => new
               {
                   IdLevel = e.Lesson.Grade.IdLevel,
                   IdGrade = e.Lesson.IdGrade,
                   IdLesson = e.IdLesson,
                   IdHomeroom = e.HomeroomPathway.IdHomeroom
               })
               .Distinct().ToListAsync(CancellationToken);

            foreach (var itemHomeroomTeacher in listHomeroomTeacher)
            {
                var listLessonCaByHomeroom = listLessonByCa
                    .Where(e => e.IdHomeroom == itemHomeroomTeacher.IdHomeroom)
                    .Select(e => new LessonByUser
                    {
                        IdLevel = e.IdLevel,
                        IdGrade = e.IdGrade,
                        IdLesson = e.IdLesson,
                        PositionCode = itemHomeroomTeacher.PositionCode
                    })
                    .ToList();
                listLessonByUser.AddRange(listLessonCaByHomeroom);
            }
            #endregion

            #region ST
            var positionCodeBySubjectTeacher = listTeacherPosition
                                                    .Where(e => e.PositionCode == PositionConstant.SubjectTeacher)
                                                    .Select(e => e.PositionCode)
                                                    .ToList();

            var listLessonBySt = await _dbContext.Entity<MsLessonTeacher>()
                                    .Include(e => e.Lesson).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                   .Where(x => x.IdUser == IdUser && x.Lesson.IdAcademicYear == IdAcademicYear)
                                   .Select(e => new LessonByUser
                                   {
                                       IdGrade = e.Lesson.IdGrade,
                                       IdLevel = e.Lesson.Grade.IdLevel,
                                       IdLesson = e.IdLesson,
                                   })
                                   .Distinct()
                                   .ToListAsync(CancellationToken);

            foreach (var itemPositionCode in positionCodeBySubjectTeacher)
            {
                listLessonBySt.ForEach(d => d.PositionCode = itemPositionCode);
                listLessonByUser.AddRange(listLessonBySt);
            }

            #endregion

            #region non teaching load
            var listTeacherNonTeaching = await _dbContext.Entity<TrNonTeachingLoad>()
                                .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                 .Where(x => x.IdUser == IdUser && x.MsNonTeachingLoad.IdAcademicYear == IdAcademicYear)
                                 .ToListAsync(CancellationToken);

            var listLesson = await _dbContext.Entity<MsLesson>()
                                .Include(e => e.Grade).ThenInclude(e => e.Level)
                                .Where(x => x.Grade.Level.IdAcademicYear == IdAcademicYear)
                                .Select(e => new
                                {
                                    IdLevel = e.Grade.IdLevel,
                                    IdGrade = e.IdGrade,
                                    IdLesson = e.Id,
                                    IdSubject = e.IdSubject
                                })
                                .ToListAsync(CancellationToken);

            var listDepartmentLevel = await _dbContext.Entity<MsDepartmentLevel>()
                                .Include(e => e.Level).ThenInclude(e => e.Grades)
                                 .Where(x => x.Level.IdAcademicYear == IdAcademicYear)
                                 .ToListAsync(CancellationToken);

            foreach (var item in listTeacherNonTeaching)
            {
                var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                _dataNewPosition.TryGetValue("Department", out var _DepartemenPosition);
                _dataNewPosition.TryGetValue("Grade", out var _GradePosition);
                _dataNewPosition.TryGetValue("Level", out var _LevelPosition);
                _dataNewPosition.TryGetValue("Subject", out var _SubjectPosition);
                if (_SubjectPosition == null && _GradePosition == null && _LevelPosition == null && _DepartemenPosition != null)
                {
                    var getDepartmentLevelbyIdLevel = listDepartmentLevel.Where(e => e.IdDepartment == _DepartemenPosition.Id).ToList();

                    foreach (var itemDepartement in getDepartmentLevelbyIdLevel)
                    {
                        var listGrade = itemDepartement.Level.Grades.ToList();

                        foreach (var itemGrade in listGrade)
                        {
                            var listLessonByIdGarde = listLesson.Where(e => e.IdGrade == itemGrade.Id).ToList();

                            foreach (var itemLesson in listLessonByIdGarde)
                            {
                                LessonByUser newSubjectTeacher = new LessonByUser
                                {
                                    IdGrade = itemLesson.IdGrade,
                                    IdLevel = itemLesson.IdLevel,
                                    IdLesson = itemLesson.IdLesson,
                                    PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                                };

                                listLessonByUser.Add(newSubjectTeacher);
                            }
                        }
                    }

                }
                else if (_SubjectPosition != null && _GradePosition != null && _LevelPosition != null && _DepartemenPosition != null)
                {
                    var listLessonByIdSubject = listLesson.Where(e => e.IdSubject == _SubjectPosition.Id).ToList();

                    foreach (var itemSubject in listLessonByIdSubject)
                    {
                        LessonByUser newSubjectTeacher = new LessonByUser
                        {
                            IdGrade = itemSubject.IdGrade,
                            IdLevel = itemSubject.IdLevel,
                            IdLesson = itemSubject.IdLesson,
                            PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                        };

                        listLessonByUser.Add(newSubjectTeacher);
                    }

                }
                else if (_SubjectPosition == null && _GradePosition != null && _LevelPosition != null)
                {
                    var listLessonByIdGrade = listLesson.Where(e => e.IdGrade == _GradePosition.Id).ToList();

                    foreach (var itemSubject in listLessonByIdGrade)
                    {
                        LessonByUser newSubjectTeacher = new LessonByUser
                        {
                            IdGrade = itemSubject.IdGrade,
                            IdLevel = itemSubject.IdLevel,
                            IdLesson = itemSubject.IdLesson,
                            PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                        };

                        listLessonByUser.Add(newSubjectTeacher);
                    }
                }
                else if (_SubjectPosition == null && _GradePosition == null && _LevelPosition != null)
                {
                    var listLessonByIdLevel = listLesson.Where(e => e.IdLevel == _LevelPosition.Id).ToList();

                    foreach (var itemSubject in listLessonByIdLevel)
                    {
                        LessonByUser newSubjectTeacher = new LessonByUser
                        {
                            IdGrade = itemSubject.IdGrade,
                            IdLevel = itemSubject.IdLevel,
                            IdLesson = itemSubject.IdLesson,
                            PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                        };

                        listLessonByUser.Add(newSubjectTeacher);
                    }
                }
            }
            #endregion
            var listLessonByPositionCode = listLessonByUser.ToList();
            if (!string.IsNullOrEmpty(PositionCode))
            {
                listLessonByPositionCode = listLessonByUser.Where(e => e.PositionCode == PositionCode).ToList();
            }

            return listLessonByPositionCode;
        }

        public class LessonByUser
        {
            public string IdLevel { get; set; }
            public string IdGrade { get; set; }
            public string IdLesson { get; set; }
            public string PositionCode { get; set; }
        }
    }
}

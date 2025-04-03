using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.StudentExtracurricular;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.StudentExtracurricular.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.StudentExtracurricular
{
    public class GetStudentExtracurricularHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetStudentExtracurricularHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetStudentExtracurricularRequest, GetStudentExtracurricularValidator>();

            var idHomeroomList = new List<string>();

            // get all id Homeroom from respective grade
            //if (param.IdHomeroom == "all")
            //{
            //    idHomeroomList = _dbContext.Entity<MsHomeroom>()
            //                        .Where(x => x.IdGrade == param.IdGrade &&
            //                                    x.Semester == param.Semester)
            //                        .Select(x => x.Id)
            //                        .ToList();
            //}
            //else
            //{
            //    idHomeroomList.Add(param.IdHomeroom);
            //}

            //plan B : search by StudentID/StudentName
            //var predicate = PredicateBuilder.True<MsHomeroomStudent>();
            //if (!string.IsNullOrWhiteSpace(param.Search))
            //    predicate = predicate.And(x
            //        =>
            //         //x.ExemplaryStudents.Any(a => EF.Functions.Like(a.IdStudent, param.SearchPattern()))
            //         //|| x.ExemplaryStudents.Any(a => EF.Functions.Like((a.Student.FirstName != null ? a.Student.FirstName + " " : ""), param.SearchPattern()))
            //         //|| EF.Functions.Like(x.Title, param.SearchPattern())
            //         EF.Functions.Like((x.Student.FirstName != null ? x.Student.FirstName + " " : ""), param.SearchPattern())
            //         || EF.Functions.Like(x.IdStudent, param.SearchPattern())
            //        );


            // get all students from homeroom
            var studentHomeroomList = _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(hs => hs.Homeroom)
                                        .ThenInclude(h => h.GradePathwayClassroom)
                                        .ThenInclude(gpc => gpc.Classroom)
                                        .Include(hs => hs.Homeroom)
                                        .ThenInclude(h => h.Grade)
                                        .ThenInclude(gpc => gpc.Level)
                                        .Include(ep => ep.Student)
                                        // param.IdHomeroom == "all" -> all homeroom
                                        //.Where(predicate)
                                        .Where(x => /*idHomeroomList.Contains(x.IdHomeroom) &&*/
                                                    x.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear &&
                                                    x.Semester == param.Semester &&
                                                    x.Homeroom.Grade.IdLevel == (param.IdLevel != null ? param.IdLevel : x.Homeroom.Grade.IdLevel) &&
                                                    x.Homeroom.IdGrade == (param.IdGrade != null ? param.IdGrade : x.Homeroom.IdGrade) &&                                                
                                                    x.IdHomeroom == (param.IdHomeroom != null ? param.IdHomeroom : x.IdHomeroom)
                                                    ) 
                                        .Select(x => new
                                        {
                                            IdLevel = x.Homeroom.Grade.IdLevel,
                                            IdGrade = x.Homeroom.IdGrade,
                                            Semester = x.Semester,
                                            Homeroom = new NameValueVm
                                            {
                                                Id = x.Homeroom.Id,
                                                Name = string.Format("{0}{1}", x.Homeroom.Grade.Code, x.Homeroom.GradePathwayClassroom.Classroom.Code)
                                            },
                                            Student = new NameValueVm
                                            {
                                                Id = x.Student.Id,
                                                Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.LastName)
                                            }
                                        })
                                        .Distinct()
                                        .ToList();
                       
            //var extracurricularIdGradeList = _dbContext.Entity<TrExtracurricularGradeMapping>()
            //                                .Include(egm => egm.Extracurricular)
            //                                .Include(egm => egm.Grade)
            //                                .ThenInclude(g => g.Level)
            //                                .ThenInclude(l => l.AcademicYear)
            //                                .Where(x => x.Grade.Id == param.IdGrade &&
            //                                            x.Grade.Level.Id == param.IdLevel &&
            //                                            x.Grade.Level.AcademicYear.Id == param.IdAcademicYear &&
            //                                            x.Extracurricular.Semester == param.Semester)
            //                                .Select(x => x.Extracurricular.Id)
            //                                .Distinct()
            //                                .ToList();

            var studentExtracurricularQuery = studentHomeroomList.GroupJoin(
                                                _dbContext.Entity<MsExtracurricularParticipant>()
                                                .Include(ep => ep.Extracurricular)
                                                .Where(x =>
                                                            //extracurricularIdGradeList.Contains(x.IdExtracurricular) &&
                                                            x.Status == true &&
                                                            x.Extracurricular.Status == true &&
                                                            string.IsNullOrEmpty(param.IdGrade) ? true : param.IdGrade == x.IdGrade),
                                                sh => (sh.Student.Id, sh.IdGrade, sh.Semester),
                                                ex => (ex.IdStudent, ex.IdGrade, ex.Extracurricular.Semester),
                                                (sh, ex) => new { sh, ex }
                                                ).SelectMany(x => x.ex.DefaultIfEmpty(),
                                                (studentHomeroom, extracurricular) => new
                                                {
                                                    IdLevel = studentHomeroom.sh.IdLevel,
                                                    IdGrade = studentHomeroom.sh.IdGrade,
                                                    homeroom = new NameValueVm
                                                    {
                                                        Id = studentHomeroom.sh.Homeroom.Id,
                                                        Name = studentHomeroom.sh.Homeroom.Name
                                                    },
                                                    student = new NameValueVm
                                                    {
                                                        Id = studentHomeroom.sh.Student.Id,
                                                        Name = studentHomeroom.sh.Student.Name
                                                    },
                                                    extracurricular = new NameValueVm
                                                    {
                                                        Id = extracurricular?.Extracurricular.Id,
                                                        Name = extracurricular?.Extracurricular.Name
                                                    },
                                                    //priority = extracurricular?.Priority,
                                                    isPrimary = extracurricular?.IsPrimary,
                                                    isNoExtracurricular = extracurricular == null ? true : false
                                                }
                                                ).ToList();


            //var studentExtracurricularQuery = _dbContext.Entity<MsExtracurricularParticipant>()
            //                        .Include(ep => ep.Student)
            //                        .Include(ep => ep.Extracurricular)
            //                        .Where(x => extracurricularIdGradeList.Contains(x.IdExtracurricular) &&
            //                                    x.Status == true &&
            //                                    x.IdGrade == param.IdGrade)
            //                        .Join(_dbContext.Entity<MsHomeroomStudent>()
            //                            .Include(hs => hs.Homeroom)
            //                            .ThenInclude(h => h.GradePathwayClassroom)
            //                            .ThenInclude(gpc => gpc.Classroom)
            //                            .Include(hs => hs.Homeroom)
            //                            .ThenInclude(h => h.Grade)
            //                            // param.IdHomeroom == "all" -> all homeroom
            //                            .Where(x => param.IdHomeroom == "all" ? true == true : x.Homeroom.Id == param.IdHomeroom),
            //                            extracurricular => new { p1 = extracurricular.Student.Id, p2 = extracurricular.IdGrade, p3 = extracurricular.Extracurricular.Semester },
            //                            homeroom => new { p1 = homeroom.IdStudent, p2 = homeroom.Homeroom.IdGrade, p3 = homeroom.Homeroom.Semester },
            //                            (extracurricular, homeroom) => new
            //                            {
            //                                homeroom = new NameValueVm
            //                                {
            //                                    Id = homeroom.Homeroom.Id,
            //                                    Name = string.Format("{0}{1}", homeroom.Homeroom.Grade.Code, homeroom.Homeroom.GradePathwayClassroom.Classroom.Code)
            //                                },
            //                                student = new NameValueVm
            //                                {
            //                                    Id = extracurricular.Student.Id,
            //                                    Name = (string.IsNullOrEmpty(extracurricular.Student.FirstName.Trim()) ? "" : extracurricular.Student.FirstName.Trim()) + (string.IsNullOrEmpty(extracurricular.Student.LastName.Trim()) ? "" : (" " + extracurricular.Student.LastName.Trim()))
            //                                },
            //                                extracurricular = new NameValueVm
            //                                {
            //                                    Id = extracurricular.Extracurricular.Id,
            //                                    Name = extracurricular.Extracurricular.Name
            //                                },
            //                                priority = extracurricular.Priority
            //                            })
            //                        .Select(x => x)
            //                        .ToList();

            var studentExtracurricularGroupWithTotalList = studentExtracurricularQuery
                                                        .GroupBy(x => new
                                                        {
                                                            gradeId = x.IdGrade,
                                                            levelId = x.IdLevel,
                                                            homeroomId = x.homeroom.Id,
                                                            homeroomName = x.homeroom.Name,
                                                            studentId = x.student.Id,
                                                            studentName = x.student.Name,
                                                            isNoExtracurricular = x.isNoExtracurricular
                                                        })
                                                        .Select(x => new
                                                        {
                                                            IdLevel = x.Key.levelId,
                                                            IdGrade = x.Key.gradeId,
                                                            homeroom = new NameValueVm
                                                            {
                                                                Id = x.Key.homeroomId,
                                                                Name = x.Key.homeroomName
                                                            },
                                                            student = new NameValueVm
                                                            {
                                                                Id = x.Key.studentId,
                                                                Name = x.Key.studentName
                                                            },
                                                            isNoExtracurricular = x.Key.isNoExtracurricular,
                                                            totalExtracurricular = x.Key.isNoExtracurricular == true ? 0 : x.Count()
                                                        })
                                                        .Distinct()
                                                        .OrderBy(x => x.homeroom.Name)
                                                        .ThenBy(x => x.student.Name)
                                                        .ToList();



            var resultList = new List<GetStudentExtracurricularResult>();

            foreach (var studentExtracurricular in studentExtracurricularGroupWithTotalList)
            {
                var result = new GetStudentExtracurricularResult
                {
                    IdLevel = studentExtracurricular.IdLevel,
                    IdGrade = studentExtracurricular.IdGrade,
                    Homeroom = studentExtracurricular.homeroom,
                    Student = studentExtracurricular.student,
                    PrimaryExtracurricular = studentExtracurricular.isNoExtracurricular == true ?
                                                null
                                                :
                                                studentExtracurricularQuery
                                                .Where(x => x.student.Id == studentExtracurricular.student.Id &&
                                                            x.homeroom.Id == studentExtracurricular.homeroom.Id &&
                                                            x.isPrimary == true)
                                                .Select(x => new NameValueVm
                                                {
                                                    Id = x.extracurricular.Id,
                                                    Name = x.extracurricular.Name
                                                })
                                                .ToList(),
                    TotalExtracurricular = studentExtracurricular.totalExtracurricular
                };
                resultList.Add(result);

            }
            
            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                resultList = resultList.Where(a => (a.PrimaryExtracurricular != null ? (a.PrimaryExtracurricular.Select(b => b.Name.ToLower()).Contains(param.Search.ToLower())) : false)
                                       || a.Student.Name.ToLower().Contains(param.Search.ToLower())
                                       || a.Student.Id.ToLower().Contains(param.Search.ToLower())                                  
                                       ).ToList();
           
            }

            var resultItems = resultList.AsQueryable().OrderByDynamic(param).SetPagination(param).ToList();
            var resultCount = resultList.Count();

            return Request.CreateApiResult2(resultItems as object, param.CreatePaginationProperty(resultCount));
        }
    }
}

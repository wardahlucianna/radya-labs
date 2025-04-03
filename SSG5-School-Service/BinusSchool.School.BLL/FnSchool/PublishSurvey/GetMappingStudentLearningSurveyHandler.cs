using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.PublishSurvey;
using BinusSchool.Data.Model.School.FnSchool.PublishSurvey;

namespace BinusSchool.Attendance.FnAttendance.PublishSurvey
{
    public class GetMappingStudentLearningSurveyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetMappingStudentLearningSurveyHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetResetMappingStudentLearningSurveyRequest>();

            var listResetMappingStudentLearning = await GetResetMappingStudentLearningSurveyHandler.GetResetMappingStudentLearning(param,_dbContext,CancellationToken);

            var predicate = PredicateBuilder.Create<TrPublishSurveyMapping>(x => x.IsActive && x.IsMapping);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                predicate = predicate.And(x => x.HomeroomStudent.Homeroom.Semester == param.Semester);

            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.HomeroomStudent.Homeroom.Grade.IdLevel == param.IdLevel);

            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.HomeroomStudent.Homeroom.IdGrade == param.IdGrade);

            if (!string.IsNullOrEmpty(param.IdPublishSurvey))
                predicate = predicate.And(x => x.IdPublishSurvey == param.IdPublishSurvey);

            if (!string.IsNullOrEmpty(param.IdStudent))
                predicate = predicate.And(x => x.HomeroomStudent.IdStudent == param.IdStudent);

            var listSurveyTemplateMapping = await _dbContext.Entity<TrPublishSurveyMapping>()
                                .Include(e=>e.Lesson)
                                .Where(predicate)
                                .Select(e => new
                                {
                                    IdStudent = e.HomeroomStudent.IdStudent,
                                    e.IdLesson,
                                    IdUserTeacher = e.IdBinusian,
                                    ClassId = e.Lesson.ClassIdGenerated,
                                    e.IdHomeroomStudent
                                }).Distinct()
                                .ToListAsync(CancellationToken);

            List<IDictionary<string, object>> header = new List<IDictionary<string, object>>();
            foreach (var item in listResetMappingStudentLearning.Student)
            {
                IDictionary<string, object> newHeader = new Dictionary<string, object>();

                newHeader.Add("StudentName", item.StudentName);
                newHeader.Add("IdStudent", item.IdStudent);
                newHeader.Add("IdHomeroom", item.IdHomeroom);
                newHeader.Add("IdHomeroomStudent", item.IdHomeroomStudent);
                newHeader.Add("Religion", item.Religion);

                var listMapping = item.Mapping.OrderBy(e => e.IsReligion ? 0 : 1).ToList();

                foreach (var itemMappingClass in listMapping)
                {
                    var listSurveyTemplateMappingByStudent = listSurveyTemplateMapping
                                                .Where(e => e.IdStudent == item.IdStudent && e.IdLesson == itemMappingClass.IdLesson && e.IdUserTeacher == itemMappingClass.IdUserTeacher)
                                                .FirstOrDefault();

                    if (newHeader.ContainsKey(itemMappingClass.Id))
                        continue;

                    if ( listSurveyTemplateMappingByStudent == null)
                    {
                        var value = new MappingStudentLearningValueResult
                        {
                            IdLesson = itemMappingClass.IdLesson,
                            IsChecked = false,
                            IsReligion = itemMappingClass.IsReligion,
                            IdUserTeacher = itemMappingClass.IdUserTeacher
                        };

                        newHeader.Add(itemMappingClass.Id, value);
                    }
                    else
                    {
                        var value = new MappingStudentLearningValueResult
                        {
                            IdLesson = itemMappingClass.IdLesson,
                            IsChecked = true,
                            IsReligion = itemMappingClass.IsReligion,
                            IdUserTeacher = itemMappingClass.IdUserTeacher
                        };

                        newHeader.Add($"{listSurveyTemplateMappingByStudent.ClassId}-{listSurveyTemplateMappingByStudent.IdUserTeacher}", value);
                    }
                }

                header.Add(newHeader);
            }

            GetResetMappingStudentLearningSurveyResult items = new GetResetMappingStudentLearningSurveyResult
            {
                Header = listResetMappingStudentLearning.Teacher.OrderBy(e => e.IsReligion ? 0 : 1).ToList(),
                MappingStudentLearningSurvey = header
            };

            return Request.CreateApiResult2(items as object);
        }
    }
}

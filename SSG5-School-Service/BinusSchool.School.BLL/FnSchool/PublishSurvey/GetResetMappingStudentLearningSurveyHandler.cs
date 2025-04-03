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
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using BinusSchool.Data.Model.School.FnSchool.PublishSurvey;

namespace BinusSchool.School.FnSchool.PublishSurvey
{
    public class GetResetMappingStudentLearningSurveyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetResetMappingStudentLearningSurveyHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetResetMappingStudentLearningSurveyRequest>();

            var listResetMappingStudentLearning = await GetResetMappingStudentLearning(param,_dbContext,CancellationToken);

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
                    var value = new MappingStudentLearningValueResult
                    {
                        IdLesson = itemMappingClass.IdLesson,
                        IsChecked = itemMappingClass.IsChecked,
                        IsReligion = itemMappingClass.IsReligion,
                        IdUserTeacher = itemMappingClass.IdUserTeacher
                    };

                    if (newHeader.ContainsKey(itemMappingClass.Id))
                        continue;

                    newHeader.Add(itemMappingClass.Id, value);
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

        public static async Task<GetResetMappingStudentLearning> GetResetMappingStudentLearning(GetResetMappingStudentLearningSurveyRequest param, ISchoolDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {
            var predicate = PredicateBuilder.Create<MsHomeroomStudentEnrollment>(e => e.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear
                                            && e.HomeroomStudent.Homeroom.Semester == param.Semester
                                            && e.HomeroomStudent.Homeroom.Grade.IdLevel == param.IdLevel
                                            && e.HomeroomStudent.Homeroom.IdGrade == param.IdGrade
                                            && e.HomeroomStudent.IdHomeroom == param.IdHomeroom);

            if (!string.IsNullOrEmpty(param.IdStudent))
                predicate = predicate.And(x => x.HomeroomStudent.IdStudent==param.IdStudent);

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student).ThenInclude(e => e.Religion)
                                .Where(predicate)
                                .OrderBy(e=>e.DateIn)
                                .Select(e => new
                                {
                                    IdStudent = e.HomeroomStudent.IdStudent,
                                    StudentName = NameUtil.GenerateFullName(e.HomeroomStudent.Student.FirstName, e.HomeroomStudent.Student.MiddleName, e.HomeroomStudent.Student.LastName),
                                    e.IdLesson,
                                    Religion = e.HomeroomStudent.Student.Religion.ReligionName,
                                    e.IdHomeroomStudent,
                                    e.HomeroomStudent.IdHomeroom,
                                }).Distinct()
                                .ToListAsync(CancellationToken);

            var listIdLesson = listHomeroomStudentEnrollment.Select(e => e.IdLesson).Distinct().ToList();

            var listScheduleRealization = await _dbContext.Entity<TrScheduleRealization2>()
                                                .Include(e => e.Lesson).ThenInclude(e=>e.Subject).ThenInclude(e=>e.SubjectGroup)
                                                .Where(e => e.IdAcademicYear == param.IdAcademicYear
                                                            && e.Lesson.Semester == param.Semester
                                                            && e.IdLevel == param.IdLevel
                                                            && e.IdGrade == param.IdGrade
                                                            && listIdLesson.Contains(e.IdLesson)
                                                        )
                                                .Select(e => new GetSurveyTeacher
                                                {
                                                    IdUser = e.IdBinusianSubtitute,
                                                    TeacherName = e.TeacherNameSubtitute,
                                                    Code = $"{e.Lesson.ClassIdGenerated}-{e.IdBinusianSubtitute}",
                                                    ClassId = e.Lesson.ClassIdGenerated,
                                                    IdHomeroom = param.IdHomeroom,
                                                    IdLesson = e.IdLesson,
                                                    IsReligion = e.Lesson.Subject.SubjectGroup.Code=="Religion"?true:false,
                                                    Subject = e.Lesson.Subject.Description
                                                })
                                                .Distinct()
                                                .ToListAsync(CancellationToken);

            var listLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                                .Include(e => e.Staff)
                                                .Include(e => e.Lesson).ThenInclude(e => e.Subject).ThenInclude(e => e.SubjectGroup)
                                                .Where(e => listIdLesson.Contains(e.IdLesson))
                                                .Select(e => new GetSurveyTeacher
                                                {
                                                    IdUser = e.IdUser,
                                                    TeacherName = NameUtil.GenerateFullName(e.Staff.FirstName, e.Staff.LastName),
                                                    Code = $"{e.Lesson.ClassIdGenerated}-{e.IdUser}",
                                                    ClassId = e.Lesson.ClassIdGenerated,
                                                    IdHomeroom = param.IdHomeroom,
                                                    IdLesson = e.IdLesson,
                                                    IsReligion = e.Lesson.Subject.SubjectGroup.Code == "Religion" ? true : false,
                                                    Subject = e.Lesson.Subject.Description
                                                })
                                                .Distinct()
                                                .ToListAsync(CancellationToken);

            var listTeacherLesson = listScheduleRealization.Union(listLessonTeacher)
                                .GroupBy(e => new
                                {
                                    IdUser = e.IdUser,
                                    TeacherName = e.TeacherName,
                                    Code = e.Code,
                                    ClassId = e.ClassId,
                                    IdHomeroom = e.IdHomeroom,
                                    IdLesson = e.IdLesson,
                                    IsReligion = e.IsReligion
                                })
                                .Select(e => new GetSurveyTeacher
                                {
                                    IdUser = e.Key.IdUser,
                                    TeacherName = e.Key.TeacherName,
                                    Code = e.Key.Code,
                                    ClassId = e.Key.ClassId,
                                    IdHomeroom = e.Key.IdHomeroom,
                                    IdLesson = e.Key.IdLesson,
                                    IsReligion = e.Key.IsReligion
                                }).OrderBy(e => e.Code).ToList();

            var listTeacher = listScheduleRealization.Union(listLessonTeacher)
                                .GroupBy(e => new
                                {
                                    IdUser = e.IdUser,
                                    TeacherName = e.TeacherName,
                                    Code = e.Code,
                                    ClassId = e.ClassId,
                                    IdHomeroom = e.IdHomeroom,
                                    Subject = e.Subject,
                                    IsReligion = e.IsReligion
                                })
                                .Select(e => new GetSurveyTeacher
                                {
                                    IdUser = e.Key.IdUser,
                                    TeacherName = e.Key.TeacherName,
                                    Code = e.Key.Code,
                                    ClassId = e.Key.ClassId,
                                    IdHomeroom = e.Key.IdHomeroom,
                                    Subject = e.Key.Subject,
                                    IsReligion = e.Key.IsReligion
                                }).OrderBy(e => e.Code).ToList();


            var liststudent = listHomeroomStudentEnrollment.Select(e => new
            {
                IdStudent = e.IdStudent,
                StudentName = e.StudentName,
                Religion = e.Religion,
                IdHomeroomStudent = e.IdHomeroomStudent,
                IdHomeroom = e.IdHomeroom,
            }).Distinct().ToList();

            List <GetStudentLearning> listStudentLearning = new List<GetStudentLearning>();
            foreach (var student in liststudent)
            {
                List <GetMappingStudent> listMappingClass = new List<GetMappingStudent>();

                foreach (var itemTeacher in listTeacherLesson)
                {
                    var exsis = listHomeroomStudentEnrollment.Where(e => e.IdLesson == itemTeacher.IdLesson && e.IdStudent == student.IdStudent).Any();

                    listMappingClass.Add(new GetMappingStudent
                    {
                        Id = itemTeacher.Code,
                        Description = $"{itemTeacher.IdUser}-{itemTeacher.ClassId}",
                        IdLesson = itemTeacher.IdLesson,
                        IdUserTeacher = itemTeacher.IdUser,
                        IsChecked = exsis,
                        IsReligion = itemTeacher.IsReligion
                    });
                }

                listStudentLearning.Add(new GetStudentLearning
                {
                    IdStudent = student.IdStudent,
                    StudentName = student.StudentName,
                    IdHomeroom = student.IdHomeroom,
                    IdHomeroomStudent = student.IdHomeroomStudent,
                    Religion = student.Religion,
                    Mapping = listMappingClass
                });

            }

            var listResetMappingStudentLearning = new GetResetMappingStudentLearning
            {
                Teacher = listTeacher,
                Student = listStudentLearning
            };

            return listResetMappingStudentLearning;
        }
    }
}

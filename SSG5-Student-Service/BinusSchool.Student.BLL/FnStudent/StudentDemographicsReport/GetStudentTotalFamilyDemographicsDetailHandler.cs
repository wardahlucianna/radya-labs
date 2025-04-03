using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentTotalFamilyDemographicsDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentTotalFamilyDemographicsDetailHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<GetStudentTotalFamilyDemographicsDetailRequest>();

            var result = await GetStudentTotalFamilyDemographicsData(new GetStudentTotalFamilyDemographicsDetailRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester,
                ViewCategoryType = param.ViewCategoryType,
                Grade = param.Grade,
                Homeroom = param.Homeroom
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<List<GetStudentTotalFamilyDemographicsDetailResult>> GetStudentTotalFamilyDemographicsData(GetStudentTotalFamilyDemographicsDetailRequest param)
        {
            var retVal = new List<GetStudentTotalFamilyDemographicsDetailResult>();

            var homeroomStudentDataQuery = _dbContext.Entity<MsHomeroomStudent>()
                .Include(a => a.Homeroom.Grade.MsLevel)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Include(a => a.Student)
                .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                            && a.Semester == param.Semester
                            && a.Homeroom.Semester == param.Semester);

            if (param.ViewCategoryType == "grade")
            {
                homeroomStudentDataQuery = homeroomStudentDataQuery.Where(a => param.Grade == null ? true : param.Grade.Any(b => b == a.Homeroom.IdGrade));
            }
            else if (param.ViewCategoryType == "homeroom")
            {
                homeroomStudentDataQuery = homeroomStudentDataQuery.Where(a => param.Homeroom == null ? true : param.Homeroom.Any(b => b == a.IdHomeroom));
            }

            var homeroomStudentData = homeroomStudentDataQuery.ToList();

            var siblingIds = homeroomStudentData.Select(h => h.IdStudent).ToList();

            var siblings = _dbContext.Entity<MsSiblingGroup>()
                .Where(s => siblingIds.Any(b => b == s.IdStudent))
                .ToList();

            var fatherParents = _dbContext.Entity<MsStudentParent>()
                .Include(a => a.Parent)
                .Where(a => siblingIds.Any(b => b == a.IdStudent) && a.Parent.IdParentRole == "F")
                .ToList();

            var motherParents = _dbContext.Entity<MsStudentParent>()
                .Include(a => a.Parent)
                .Where(a => siblingIds.Any(b => b == a.IdStudent) && a.Parent.IdParentRole == "M")
                .ToList();

            var joinedData = from homeroomStudent in homeroomStudentData
                             join sibling in siblings on homeroomStudent.IdStudent equals sibling.IdStudent
                             join father in fatherParents on sibling.IdStudent equals father.IdStudent
                             join mother in motherParents on sibling.IdStudent equals mother.IdStudent
                             select new
                             {
                                 HomeroomStudent = homeroomStudent,
                                 Sibling = sibling,
                                 Father = father,
                                 Mother = mother
                             };

            var groupedResult = joinedData.GroupBy(a => new
            {
                a.Sibling.Id,
                MotherName = NameUtil.GenerateFullName(a.Mother.Parent.FirstName, a.Mother.Parent.LastName),
                MotherPhone = a.Mother.Parent.MobilePhoneNumber1 ?? "-",
                MotherEmail = a.Mother.Parent.PersonalEmailAddress ?? "-",
                FatherName = NameUtil.GenerateFullName(a.Father.Parent.FirstName, a.Father.Parent.LastName),
                FatherPhone = a.Father.Parent.MobilePhoneNumber1 ?? "-",
                FatherEmail = a.Father.Parent.PersonalEmailAddress ?? "-"
            })
            .Select(group => new GetStudentTotalFamilyDemographicsDetailResult
            {
                IdSiblingGroup = group.Key.Id,
                MotherName = group.Key.MotherName,
                MotherPhone = group.Key.MotherPhone,
                MotherEmail = group.Key.MotherEmail,
                FatherName = group.Key.FatherName,
                FatherPhone = group.Key.FatherPhone,
                FatherEmail = group.Key.FatherEmail,
                SiblingData = group.Select(a => new GetStudentTotalFamilyDemographicsDetailResult_Student
                {
                    Student = new NameValueVm
                    {
                        Id = a.Sibling.IdStudent,
                        Name = NameUtil.GenerateFullName(a.HomeroomStudent.Student.FirstName, a.HomeroomStudent.Student.LastName)
                    },
                    Grade = new ItemValueVm
                    {
                        Id = a.HomeroomStudent.Homeroom.IdGrade,
                        Description = a.HomeroomStudent.Homeroom.Grade.Description
                    },
                    Homeroom = new ItemValueVm
                    {
                        Id = a.HomeroomStudent.IdHomeroom,
                        Description = a.HomeroomStudent.Homeroom.Grade.Code + a.HomeroomStudent.Homeroom.MsGradePathwayClassroom.Classroom.Code
                    }
                }).OrderBy(a => a.Homeroom.Description.Length)
                .ThenBy(a => a.Homeroom.Description)
                .ThenBy(a => a.Student.Name)
                .ToList()
            }).ToList();

            retVal.AddRange(groupedResult);

            return retVal;
        }
    }
}

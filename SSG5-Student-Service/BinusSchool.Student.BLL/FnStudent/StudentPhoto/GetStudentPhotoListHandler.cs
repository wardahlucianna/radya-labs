using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.StudentPhoto;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentPhoto
{
    public class GetStudentPhotoListHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { 
            nameof(GetStudentPhotoListRequest.IdSchool), 
            nameof(GetStudentPhotoListRequest.IdAcademicYear),
            nameof(GetStudentPhotoListRequest.StudentType),
            //nameof(GetStudentPhotoListRequest.IdLevel),
            //nameof(GetStudentPhotoListRequest.IdGrade)
        };

        private static readonly string[] _columns = new[] { "AcademicYear", "BinusianID", "StudentID", "StudentName", "Grade", "FileName" };

        private readonly IStudentDbContext _dbContext;

        public GetStudentPhotoListHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentPhotoListRequest>(_requiredParams);

            var results = new List<GetStudentPhotoListResult>();

            var predicate = PredicateBuilder.True<MsStudentGrade>();
            //var predicateNewStudent = PredicateBuilder.True<MsAdmissionData>();

            if (!string.IsNullOrEmpty(param.IdSchool))
                predicate = predicate.And(x => x.Student.IdSchool == param.IdSchool);
                //predicateNewStudent = predicateNewStudent.And(x => x.Student.IdSchool == param.IdSchool);
            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x =>
                    EF.Functions.Like(x.Student.Id, param.SearchPattern())
                    || EF.Functions.Like(x.Student.IdBinusian, param.SearchPattern())
                    || EF.Functions.Like(
                        (string.IsNullOrWhiteSpace(x.Student.FirstName) ? "" : x.Student.FirstName.Trim() + " ") +
                        (string.IsNullOrWhiteSpace(x.Student.MiddleName) ? "" : x.Student.MiddleName.Trim() + " ") +
                        (string.IsNullOrWhiteSpace(x.Student.LastName) ? "" : x.Student.LastName.Trim()), param.SearchPattern())
                    || EF.Functions.Like(x.Grade.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Grade.MsLevel.MsAcademicYear.Description, param.SearchPattern())
                    );

            if (param.StudentType != true)
            {
                var query = _dbContext.Entity<MsStudentGrade>()
                    .Include(x => x.Student)
                    .Include(x => x.Grade)
                        .ThenInclude(x => x.MsLevel)
                        .ThenInclude(x => x.MsAcademicYear)
                    //.SearchByIds(param)
                    .Where(predicate);

                var getStudentList = query.Select(x => x.IdStudent).ToList();

                var getStudentPhoto = _dbContext.Entity<TrStudentPhoto>()
                        .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                        .Where(x => getStudentList.Distinct().Any(y => y == x.IdStudent))
                        .ToList();

                var getStudentPhotoData = query.ToList()
                        .GroupJoin(
                              getStudentPhoto,
                              std => new { std.IdStudent, std.Grade.MsLevel.IdAcademicYear },
                              pth => new { pth.IdStudent, pth.IdAcademicYear },
                              (std, pth) => new { std, pth }
                        )
                        .SelectMany(
                              x => x.pth.DefaultIfEmpty(),
                              (Student, Photo) => new 
                              {
                                  IdBinusian = Student.std.Student.IdBinusian,
                                  Student = Student.std.IdStudent,
                                  StudentName = NameUtil.GenerateFullName(Student.std.Student.FirstName, Student.std.Student.LastName),
                                  AcademicYear = Student.std.Grade.MsLevel.IdAcademicYear,
                                  AcademicYearDesc = Student.std.Grade.MsLevel.MsAcademicYear.Description,
                                  Grade = Student.std.IdGrade,
                                  GradeDesc = Student.std.Grade.Description,
                                  IdPhoto = Photo == null ? null : Photo.Id,
                                  FileName = Photo == null ? null : Photo.FileName,
                                  FilePath = Photo == null ? null : Photo.FilePath,
                              }
                        ).Distinct().ToList();

                foreach (var data in getStudentPhotoData)
                {
                    var result = new GetStudentPhotoListResult
                    {
                        IdStudentPhoto = data.IdPhoto,
                        AcademicYear = new ItemValueVm
                        {
                            Id = data.AcademicYear,
                            Description = data.AcademicYearDesc
                        },
                        Student = new GetStudentPhotoResult_Student
                        {
                            Student = new NameValueVm
                            {
                                Id = data.Student,
                                Name = data.StudentName
                            },
                            IdBinusian = data.IdBinusian
                        },
                        Grade = new ItemValueVm
                        {
                            Id = data.Grade,
                            Description = data.GradeDesc
                        },
                        FileName = data.FileName,
                        FilePath = data.FilePath
                    };
                    results.Add(result);
                }
            }
            else
            {
                var getGrade = "";

                if (!string.IsNullOrEmpty(param.IdGrade))
                {
                    getGrade = _dbContext.Entity<MsGrade>()
                        .Where(x => x.Id == param.IdGrade)
                        .Select(x => x.Code)
                        .FirstOrDefault();
                }

                var getAdmissionData = _dbContext.Entity<MsAdmissionData>()
                            .Where(x => x.IdYearLevel == getGrade)
                            .Where(x => x.IdSchoolLevel == param.IdLevel)
                            .Where(x => x.IdSchool == param.IdSchool)
                            .Include(x => x.Student)
                            .ToList();

                var getStudentList = getAdmissionData.Select(x => x.IdStudent).ToList();

                var getStudentPhoto = _dbContext.Entity<TrStudentPhoto>()
                        .Where(x => getStudentList.Distinct().Any(y => y == x.IdStudent))
                        .ToList();

                var getStudentPhotoData = getAdmissionData
                        .GroupJoin(
                              getStudentPhoto,
                              std => new { std.IdStudent},
                              pth => new { pth.IdStudent},
                              (std, pth) => new { std, pth }
                        )
                        .SelectMany(
                              x => x.pth.DefaultIfEmpty(),
                              (Student, Photo) => new
                              {
                                  IdBinusian = Student.std.Student.IdBinusian,
                                  Student = Student.std.IdStudent,
                                  StudentName = NameUtil.GenerateFullName(Student.std.Student.FirstName, Student.std.Student.LastName),
                                  IdPhoto = Photo == null ? null : Photo.Id,
                                  FileName = Photo == null ? null : Photo.FileName,
                                  FilePath = Photo == null ? null : Photo.FilePath,
                              }
                        ).Distinct().ToList();

                foreach (var data in getStudentPhotoData)
                {
                    var result = new GetStudentPhotoListResult
                    {
                        IdStudentPhoto = data.IdPhoto,
                        Student = new GetStudentPhotoResult_Student
                        {
                            Student = new NameValueVm
                            {
                                Id = data.Student,
                                Name = data.StudentName
                            },
                            IdBinusian = data.IdBinusian
                        },
                        FileName = data.FileName,
                        FilePath = data.FilePath
                    };
                    results.Add(result);
                }
            }
            
            return Request.CreateApiResult2(results as object);

        }
    }
}


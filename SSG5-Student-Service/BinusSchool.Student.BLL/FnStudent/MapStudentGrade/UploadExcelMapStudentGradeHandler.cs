using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentGrade;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Student.FnStudent.MapStudentGrade
{
    public class UploadExcelMapStudentGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public UploadExcelMapStudentGradeHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var file = Request.Form.Files.FirstOrDefault();
            var param = Request.ValidateParams<UploadExcelMapStudentGradeRequest>(nameof(UploadExcelMapStudentGradeRequest.IdAcademicYear));

            if (file == null || file.Length == 0)
                throw new BadRequestException("Excel file not provided");

            var fileInfo = new FileInfo(file.FileName);
            if (fileInfo.Extension != ".xlsx")
                throw new BadRequestException("File extension not supported");

            using var fs = file.OpenReadStream();
            var workbook = new XSSFWorkbook(fs);
            var sheet = workbook.GetSheetAt(0);

            var studentIds = new List<string>();

            bool isLastRow = false;
            int rowNumber = 1;

            while (!isLastRow)
            {
                if (sheet.IsCellNullOrEmpty(rowNumber, 0))
                {
                    isLastRow = true;
                    continue;
                }

                var currentRow = sheet.GetRow(rowNumber++);
                var cellValue = currentRow.CellValue(0);

                studentIds.Add(cellValue);
            }

            var successStudents = new List<ListMapStudentGradeResult>();
            var failedStudents = new List<string>();

            var studentGradeQuery = await _dbContext.Entity<MsStudentGrade>()
                .Include(x => x.Grade)
                    .ThenInclude(x => x.MsLevel)
                        .ThenInclude(x => x.MsAcademicYear)
                .Include(x => x.Student)
                .Where(x => studentIds.Contains(x.IdStudent) 
                && x.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                && x.Student.IdSchool == param.IdSchool)
                .Select(x => new ListMapStudentGradeResult
                {
                    IdStudent = x.IdStudent,
                    StudentName = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                    Gender = x.Student.Gender.ToString(),
                    Level = new ItemValueVm
                    {
                        Id = x.Grade.IdLevel,
                        Description = x.Grade.MsLevel.Description
                    },
                    Grade = new ItemValueVm
                    {
                        Id = x.IdGrade,
                        Description = x.Grade.Description
                    }
                })
                .ToListAsync();

            if (studentGradeQuery.Any())
            {
                successStudents.AddRange(studentGradeQuery);

                var newStudent = studentIds.Where(x => !studentGradeQuery.Select(y => y.IdStudent).Any(y => y == x)).ToList();

                if (newStudent.Any())
                {
                    var studentQuery = await _dbContext.Entity<MsStudent>()
                        .Where(x => newStudent.Contains(x.Id) && x.IdSchool == param.IdSchool)
                        .Select(x => new ListMapStudentGradeResult
                        {
                            IdStudent = x.Id,
                            StudentName = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName),
                            Gender = x.Gender.ToString(),
                        }).ToListAsync();

                    if (studentQuery.Any())
                        successStudents.AddRange(studentQuery);

                    var notFoundStudents = newStudent.Where(x => !studentQuery.Select(y => y.IdStudent).Any(y => y == x)).ToList();
                    if (notFoundStudents.Any())
                        failedStudents.AddRange(notFoundStudents);
                }
            }
            else
            {
                var studentQuery = await _dbContext.Entity<MsStudent>()
                        .Where(x => studentIds.Contains(x.Id) && x.IdSchool == param.IdSchool)
                        .Select(x => new ListMapStudentGradeResult
                        {
                            IdStudent = x.Id,
                            StudentName = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName),
                            Gender = x.Gender.ToString(),
                        }).ToListAsync();

                if (studentQuery.Any())
                    successStudents.AddRange(studentQuery);

                var notFoundStudents = studentIds.Where(x => !studentQuery.Select(y => y.IdStudent).Any(y => y == x)).ToList();
                if (notFoundStudents.Any())
                    failedStudents.AddRange(notFoundStudents);
            }

            var result = new UploadExcelMapStudentGradeResult
            {
                IdAcademicYear = param.IdAcademicYear,
                SuccessStudents = successStudents,
                FailedStudents = failedStudents
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}

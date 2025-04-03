using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway;
using NPOI.XSSF.UserModel;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.StudentDb.Entities.School;

namespace BinusSchool.Student.FnStudent.MapStudentPathway
{
    public class ImportMapStudentPathwayHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public ImportMapStudentPathwayHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<ImportMapStudentPathwayRequest>(
                nameof(ImportMapStudentPathwayRequest.IdSchool), 
                nameof(ImportMapStudentPathwayRequest.IdAcadyear));

            var file = Request.Form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
                throw new BadRequestException("Excel file not provided");
            
            var fileInfo = new FileInfo(file.FileName);
            if (fileInfo.Extension != ".xlsx")
                throw new BadRequestException("Excel file not provided");
            
            using var fs = file.OpenReadStream();
            var workbook = new XSSFWorkbook(fs);
            var sheet = workbook.GetSheetAt(0);

            var mapStudentPathways = new List<MapStudentPathway>();
            for (var row = 1; row <= sheet.LastRowNum; row++)
            {
                var rowVal = sheet.GetRow(row);
                if (rowVal != null) // check if row not empty
                {
                    mapStudentPathways.Add(new MapStudentPathway
                    {
                        Level = rowVal.GetCell(0).ToString(),
                        Grade = rowVal.GetCell(1).ToString(),
                        StudentId = rowVal.GetCell(2).ToString(),
                        StudentName = rowVal.GetCell(3).ToString(),
                        Pathway = rowVal.GetCell(4).ToString(),
                        PathwayNextAcademicYear = rowVal.GetCell(5).ToString()
                    });
                }
            }
            
            var levels = await _dbContext.Entity<MsLevel>()
                .Include(x => x.MsGrades)
                .Where(x => x.IdAcademicYear == param.IdAcadyear)
                .ToListAsync(CancellationToken);

            // make sure uploaded level is exist
            var uploadedLevels = mapStudentPathways.Select(x => x.Level).Distinct();
            var notFoundLevels = uploadedLevels.Except(levels.Select(x => x.Code));
            if (notFoundLevels.Any())
                throw new BadRequestException(string.Format(Localizer["ExNotFound"], string.Join(", ", notFoundLevels)));

            // make sure uploaded grades is exist
            var uploadedGrades = mapStudentPathways.Select(x => x.Grade).Distinct();
            var notFoundGrades = uploadedGrades.Except(levels.SelectMany(x => x.MsGrades.Select(y => y.Code)));
            if (notFoundGrades.Any())
                throw new BadRequestException(string.Format(Localizer["ExNotFound"], string.Join(", ", notFoundGrades)));

            // get master pathways
            var pathways = await _dbContext.Entity<MsPathway>()
                .Where(x => x.IdAcademicYear == param.IdAcadyear)
                .ToListAsync(CancellationToken);

            // make sure uploaded pathways is exist
            var uploadedPathways = mapStudentPathways.Select(x => x.Pathway).Distinct();
            var notFoundPathways = uploadedPathways.Except(pathways.Select(x => x.Description));
            if (notFoundPathways.Any())
                throw new BadRequestException(string.Format(Localizer["ExNotFound"], string.Join(", ", notFoundPathways)));

            var levelAndGrades = mapStudentPathways.Select(x => new { x.Level, x.Grade }).Distinct();
            var students = new List<GetMapStudentPathwayResult>();
            foreach (var lg in levelAndGrades)
            {
                // get existing students per grade
                var studentsPerGrade = mapStudentPathways.Where(x => x.Level == lg.Level && x.Grade == lg.Grade);
                if (studentsPerGrade.Any())
                {
                    // get all students by grade
                    var idGrade = levels.First(x => x.Code == lg.Level).MsGrades.First(x => x.Code == lg.Grade).Id;
                    var existStudents = await _dbContext.Entity<MsStudentGrade>()
                        .Include(x => x.Student)
                        .Where(x => x.IdGrade == idGrade)
                        .Select(x => new
                        {
                            x.IdStudent,
                            x.Student.FirstName,
                            x.Student.MiddleName,
                            x.Student.LastName,
                            x.IsActive
                        })
                        .ToListAsync(CancellationToken);

                    // filter uploaded students that not exist in database
                    var filteredStudents = studentsPerGrade
                        .Join(existStudents, 
                            upStudent => upStudent.StudentId, 
                            msStudent => msStudent.IdStudent, 
                            (upStudent, msStudent) => new GetMapStudentPathwayResult
                            {
                                Id = msStudent.IdStudent,
                                Code = lg.Grade,
                                Description = NameUtil.GenerateFullNameWithId(
                                    msStudent.IdStudent,
                                    msStudent.FirstName, msStudent.MiddleName, msStudent.LastName),
                                //Gender = msStudent.Student.Gender,
                                IsActive = msStudent.IsActive,
                                Pathway = fillPathway(upStudent.Pathway),
                                PathwayNextAcademicYear = fillPathway(upStudent.PathwayNextAcademicYear)
                            });

                    students.AddRange(filteredStudents);
                }
            }

            ItemValueVm fillPathway(string uploadPathway)
            {
                var pathway = pathways?.FirstOrDefault(x => x.Description == uploadPathway);
                return new ItemValueVm(pathway?.Id, pathway?.Description);
            }

            return Request.CreateApiResult2(students as object);
        }
    }

    internal class MapStudentPathway
    {
        public string Level { get; set; }
        public string Grade { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string Pathway { get; set; }
        public string PathwayNextAcademicYear { get; set; }
    }
}

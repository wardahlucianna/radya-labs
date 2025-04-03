using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using NPOI.XSSF.UserModel;


namespace BinusSchool.User.FnBlocking.StudentBlocking
{
    public class UploadExcelColumnNameStudentBlockingValidationHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public UploadExcelColumnNameStudentBlockingValidationHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
                throw new BadRequestException("Excel file not provided");

            var fileInfo = new FileInfo(file.FileName);
            if (fileInfo.Extension != ".xlsx")
                throw new BadRequestException("File extension not supported");

            using var fs = file.OpenReadStream();
            var workbook = new XSSFWorkbook(fs);
            var sheet = workbook.GetSheetAt(0);

            var firstRowVal = sheet.GetRow(0);
            var maxCell = firstRowVal.LastCellNum;

            if (firstRowVal.GetCell(0).ToString().ToLower() != "student")
                throw new BadRequestException("Wrong format excel");

            if (sheet.LastRowNum == 0)
                throw new BadRequestException("No data is imported");

            if (maxCell < 3)
                throw new BadRequestException("No data is imported");

            var dataSource = new List<UploadExcelContentDataStudentBlockingValidationQueryResult>();
            var _columns1 = new List<string>();
            var _columns2 = new List<string>();

            for (var column = 0; column <= maxCell; column++)
            {
                var rowVal = sheet.GetRow(0);
                if (rowVal is null)
                    continue;

                if (rowVal.GetCell(column) is null)
                    continue;
                var columnNameUpload = rowVal.GetCell(column).ToString().TrimStart().TrimEnd();
                if (string.IsNullOrEmpty(columnNameUpload))
                    break;

                if (column < 2)
                {
                    _columns1.Add(columnNameUpload);
                }
                else
                {
                    var splitColumnNameUpload = columnNameUpload.Split("|");
                    if (splitColumnNameUpload.Length == 2)
                    {
                        var blockingCategory = splitColumnNameUpload[0].ToString();
                        var blockingType = splitColumnNameUpload[1].ToString();
                        if (dataSource.Where(x => x.BlockingCategory == blockingCategory && x.BlockingType == blockingType).ToList().Count == 0)
                        {
                            dataSource.Add(new UploadExcelContentDataStudentBlockingValidationQueryResult
                            {
                                BlockingCategory = blockingCategory,
                                BlockingType = blockingType
                            });
                        }

                    }
                }

            }
            var columnBlockingCategory = dataSource.GetRange(0, dataSource.Count)
                  .Select(x => new
                  {
                      ColumnName = $"{x.BlockingCategory}"
                  })
                  .ToList();

            var columnDummyBlockingType = dataSource.GetRange(0, dataSource.Count)
                  .Select(x => new
                  {
                      ColumnName = $"{x.BlockingType}"
                  })
                  .ToList();


            _columns1.AddRange(columnBlockingCategory.Select(c => c.ColumnName));

            _columns2.Add("Student");
            _columns2.Add("Student ID");
            _columns2.AddRange(columnDummyBlockingType.Select(c => c.ColumnName));

            var columnResult = new UploadExcelContentDataStudentBlockingValidationResult
            {
                Columns1 = _columns1,
                Columns2 = _columns2
            };

            return Request.CreateApiResult2(columnResult as object);
        }

    }
}

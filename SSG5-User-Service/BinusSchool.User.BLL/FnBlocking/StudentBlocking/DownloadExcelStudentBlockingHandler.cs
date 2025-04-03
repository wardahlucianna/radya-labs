using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.User.FnBlocking.StudentBlocking
{
    public class DownloadExcelStudentBlockingHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public DownloadExcelStudentBlockingHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }
        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<DownloadExcelStudentBlockingRequest>();

            var dataBlocking1 = await _dbContext.Entity<MsBlockingCategoryType>()
                .Include(x => x.BlockingCategory).ThenInclude(x => x.UserBlockings)
                .Include(x => x.BlockingCategory).ThenInclude(x => x.StudentBlockings)
                .Include(x => x.BlockingType)
                .Where(x => x.BlockingType.Category != "FEATURE")
                .OrderBy(x => x.BlockingCategory.Name).ThenBy(x => x.BlockingType.Order)
                .ToListAsync(CancellationToken);
            var dataBlocking2 = await _dbContext.Entity<MsBlockingCategoryType>()
                .Include(x => x.BlockingCategory).ThenInclude(x => x.UserBlockings)
                .Include(x => x.BlockingCategory).ThenInclude(x => x.StudentBlockings)
                .Include(x => x.BlockingType)
                .Where(x => x.BlockingType.Category == "FEATURE")
                .OrderBy(x => x.BlockingCategory.Name).ThenBy(x => x.BlockingType.Name)
                .ToListAsync(CancellationToken);

            var dataAccessBlocking = await _dbContext.Entity<MsUserBlocking>()
                .Include(x => x.BlockingCategory)
                .Where(x => x.IdUser == param.IdUser && x.BlockingCategory.IdSchool == param.IdSchool)
                .Select(x=> x.IdBlockingCategory)
                .ToListAsync(CancellationToken);

            dataBlocking1 = dataBlocking1.Where(x => dataAccessBlocking.Contains(x.IdBlockingCategory)).ToList();
            dataBlocking2 = dataBlocking2.Where(x => dataAccessBlocking.Contains(x.IdBlockingCategory)).ToList();

            var dataBlocking = dataBlocking1.Concat(dataBlocking2).OrderBy(x=>x.BlockingCategory.Name).Select(y=> new GetDownloadExcelColumnNameQueryResult
            {
                BlockingCategory = y.BlockingCategory.Name,
                BlockingType = y.BlockingType.Name,
            }).ToList();

            #region Unused Code
            //var dataBlocking = await _dbContext.Entity<MsBlockingCategoryType>()
            //    .Include(x => x.Category)
            //    .Include(x => x.Type)
            //    .OrderBy(x => x.Category.Name).ThenBy(x => x.Type.Order)
            //    .Select(x => new GetDownloadExcelColumnNameQueryResult
            //    {
            //        BlockingCategory = x.Category.Name,
            //        BlockingType = x.Type.Name
            //    })
            //    .ToListAsync();
            //var dataSource = await (from mbc in _dbContext.Entity<MsBlockingCategory>()
            //                    join mbct in _dbContext.Entity<MsBlockingCategoryType>() on mbc.Id equals mbct.IdCategory
            //                    join ct in _dbContext.Entity<MsBlockingType>() on mbct.IdType equals ct.Id
            //                    where mbc.IdSchool == param.IdSchool && ct.Order <= 2
            //                    orderby mbc.Name, ct.Order
            //                    select new GetDownloadExcelColumnNameQueryResult
            //                    {
            //                        BlockingCategory = mbc.Name,
            //                        BlockingType = ct.Name
            //                    }).ToListAsync();

            //var dataSource2 = await (from mbc in _dbContext.Entity<MsBlockingCategory>()
            //                    join mbct in _dbContext.Entity<MsBlockingCategoryType>() on mbc.Id equals mbct.IdCategory
            //                    join ct in _dbContext.Entity<MsBlockingType>() on mbct.IdType equals ct.Id
            //                    where mbc.IdSchool == param.IdSchool && ct.Order > 2
            //                    orderby mbc.Name, ct.Name
            //                    select new GetDownloadExcelColumnNameQueryResult
            //                    {
            //                        BlockingCategory = mbc.Name,
            //                        BlockingType = ct.Name
            //                    }).ToListAsync();

            //dataSource.AddRange(dataSource2);
            #endregion

            var title = "Student Blocking Template";

            var generateExcelByte = GenerateExcel(title, dataBlocking);

            return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                //FileDownloadName = $"{title}_{_machineDateTime.ServerTime.ToString("ddMMyyyy")}.xlsx"
                FileDownloadName = $"{title}.xlsx"
            };
        }

        public byte[] GenerateExcel(string sheetTitle, List<GetDownloadExcelColumnNameQueryResult> dataSource)
        {
         
            var columnDummyitem = dataSource.GetRange(0, dataSource.Count)
                  .Select(x => new
                  {
                      ColumnName = $"{x.BlockingCategory} | {x.BlockingType}"
                  })
                  .ToList();

            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(sheetTitle);

                //Create style
                ICellStyle style = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsItalic = true;
                style.SetFont(font);

                //header 
                var fontBold = workbook.CreateFont();
                fontBold.IsBold = true;
                var boldStyle = workbook.CreateCellStyle();
                boldStyle.SetFont(fontBold);

                //header information
                IRow rowHeader = excelSheet.CreateRow(0);

                var cellParticipant = rowHeader.CreateCell(0);
                //Column1
                cellParticipant = rowHeader.CreateCell(0);
                cellParticipant.SetCellValue("Student");
                excelSheet.AutoSizeColumn(0);

                //Column2
                cellParticipant = rowHeader.CreateCell(1);
                cellParticipant.SetCellValue("Student ID");
                excelSheet.AutoSizeColumn(1);

                //Dynamic Columns
                //
                var columnCounter = 1;
                foreach (var column in columnDummyitem)
                {
                    columnCounter += 1;
                    cellParticipant = rowHeader.CreateCell(columnCounter);
                    cellParticipant.SetCellValue(column.ColumnName.ToUpper());
                    excelSheet.AutoSizeColumn(columnCounter);
                }

                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }
    }
}

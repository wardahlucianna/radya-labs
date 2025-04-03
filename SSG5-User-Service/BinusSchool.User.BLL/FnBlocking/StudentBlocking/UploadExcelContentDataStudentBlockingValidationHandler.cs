using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.Blocking;
using BinusSchool.Data.Model.User.FnBlocking.BlockingCategory;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.School;
using BinusSchool.Persistence.UserDb.Entities.Student;
using BinusSchool.User.FnBlocking.StudentBlocking.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.XSSF.UserModel;

namespace BinusSchool.User.FnBlocking.StudentBlocking
{
    public class UploadExcelContentDataStudentBlockingValidationHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public UploadExcelContentDataStudentBlockingValidationHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = Request.ValidateParams<UploadExcelContentDataStudentBlockingValidationRequest>(nameof(UploadExcelContentDataStudentBlockingValidationRequest.IdSchool));

            if (string.IsNullOrEmpty(param.IdSchool))
            {
                throw new BadRequestException("Id School cannot be null");
            }

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

            if (maxCell <= 2)
                throw new BadRequestException("No data is imported");


            var dataSourceColumnName = new List<UploadExcelColumnNameStudentBlockingValidationQueryResult>();
            var _columns1 = new List<string>();

            for (var column = 0; column <= maxCell; column++)
            {
                var rowVal = sheet.GetRow(0);
                if (rowVal.GetCell(column) is null)
                    continue;

                var columnNameUpload = rowVal.GetCell(column).ToString();
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
                        var blockingCategory = splitColumnNameUpload[0].ToString().ToString().TrimStart().TrimEnd();
                        var blockingType = splitColumnNameUpload[1].ToString().ToString().TrimStart().TrimEnd();
                        if (dataSourceColumnName.Where(x => x.BlockingCategory == blockingCategory && x.BlockingType == blockingType).ToList().Count == 0)
                        {
                            dataSourceColumnName.Add(new UploadExcelColumnNameStudentBlockingValidationQueryResult
                            {
                                BlockingCategory = blockingCategory,
                                BlockingType = blockingType
                            });
                        }

                    }
                }

            }

            var dataBlockingCategory = await _dbContext.Entity<MsBlockingCategory>()
                                        .Where(x => x.IdSchool == param.IdSchool)
                                        .Select(x => new
                                        {
                                            Id = x.Id,
                                            Name = x.Name
                                        })
                                        .ToListAsync(CancellationToken);

            var dataBlockingType = await _dbContext.Entity<MsBlockingType>()
                            .Where(x => x.IdSchool == param.IdSchool)
                                        .Select(x => new
                                        {
                                            Id = x.Id,
                                            Name = x.Name
                                        })
                            .ToListAsync(CancellationToken);

            var dataBlockingCategoryType = await _dbContext.Entity<MsBlockingCategoryType>()
                            .Include(x => x.BlockingCategory)
                            .Where(x => x.BlockingCategory.IdSchool == param.IdSchool)
                            .Select(x => new
                            {
                                IdCategory = x.IdBlockingCategory,
                                IdCategoryType = x.IdBlockingType
                            }).ToListAsync(CancellationToken);


            var dataUser = await (from st in _dbContext.Entity<MsStudent>()
                                      //join usr in _dbContext.Entity<MsUser>() on st.Id equals usr.Id
                                  select new
                                  {
                                      Id = st.Id,
                                      Name = string.IsNullOrEmpty(st.LastName) ? $"{st.FirstName} {st.MiddleName}" : $"{st.FirstName} {st.LastName}"
                                  }).ToListAsync();

            var dataAccessBlock = await _dbContext.Entity<MsUserBlocking>()
                .Include(x => x.BlockingCategory)
                .Where(x => x.IdUser == AuthInfo.UserId && x.BlockingCategory.IdSchool == param.IdSchool)
                .ToListAsync(CancellationToken);

            var dataSource = new List<UploadExcelContentDataStudentBlockingValidationQueryResult>();

            for (var row = 1; row <= sheet.LastRowNum; row++)
            {
                var startColumn = 2;
                var rowVal = sheet.GetRow(row);
                for (var column = startColumn; column <= maxCell; column++)
                {
                    if (rowVal == null)
                        break;

                    var rowValHeader = sheet.GetRow(0);

                    if (rowValHeader.GetCell(column) == null)
                        continue;

                    var columnNameUpload = rowValHeader.GetCell(column).ToString();

                    if (string.IsNullOrEmpty(columnNameUpload))
                        continue;

                    if (string.IsNullOrEmpty(rowVal.GetCell(0).ToString()) || string.IsNullOrEmpty(rowVal.GetCell(1).ToString()))
                        continue;

                    var splitColumnNameUpload = columnNameUpload.Split(" | ");

                    if (splitColumnNameUpload.Length == 2)
                    {
                        var blockingCategory = splitColumnNameUpload[0];
                        var blockingType = splitColumnNameUpload[1];
                        var nameStudent = rowVal.GetCell(0).ToString();
                        var idStudent = rowVal.GetCell(1).ToString();
                        var IdBlockingCategory = dataBlockingCategory.Where(x => x.Name.ToLower() == blockingCategory.ToLower()).Select(y => y.Id).FirstOrDefault();
                        var IdBlockingType = dataBlockingType.Where(x => x.Name.ToLower() == blockingType.ToLower()).Select(y => y.Id).FirstOrDefault();
                        var isRealtionBLockingCategoryType = dataBlockingCategoryType.Any(x => x.IdCategory == IdBlockingCategory && x.IdCategoryType == IdBlockingType);
                        var haveAccessBlock = dataAccessBlock.Any(x => x.BlockingCategory.Id == IdBlockingCategory);

                        var isIdStudent = dataUser.Where(x => x.Id == idStudent).FirstOrDefault() == null ? false : true;
                        var student = dataUser.Where(x => x.Id == idStudent).Select(x => x.Name).FirstOrDefault();
                        var isNameStudent = student == nameStudent;

                        var isBlocked = (rowVal.GetCell(column) == null ? false : (rowVal.GetCell(column).ToString().ToLower() == "x") ? true : false);
                        if (!isBlocked) haveAccessBlock = true;
                        #region unusecode
                        //if (dataSource.Where(x => x.IdStudent == idStudent && x.BlockingCategory == blockingCategory && x.BlockingType == blockingType).ToList().Count == 0)
                        //{
                        //    dataSource.Add(new UploadExcelContentDataStudentBlockingValidationQueryResult
                        //    {
                        //        StudentName = nameStudent,
                        //        IdStudent = idStudent,
                        //        BlockingCategory = blockingCategory,
                        //        BlockingType = blockingType,
                        //        IdBlockingCategory = IdBlockingCategory,
                        //        IdBlockingType = IdBlockingType,
                        //        IsBlocking = isBlocked,
                        //        IsStudentID = isIdStudent,
                        //        IsNameStudent = isNameStudent,
                        //        IsRealtionBLockingCategoryType = (isRealtionBLockingCategoryType == true && !string.IsNullOrEmpty(IdBlockingCategory) && !string.IsNullOrEmpty(IdBlockingType)) ? true : false ,
                        //        IsSuccess = (IdBlockingCategory != null & IdBlockingType != null && isIdStudent == true && isNameStudent == true && isRealtionBLockingCategoryType == true) ? true : false
                        //    });
                        //}
                        #endregion

                        dataSource.Add(new UploadExcelContentDataStudentBlockingValidationQueryResult
                        {
                            StudentName = nameStudent,
                            IdStudent = idStudent,
                            BlockingCategory = blockingCategory,
                            BlockingType = blockingType,
                            IdBlockingCategory = IdBlockingCategory,
                            IdBlockingType = IdBlockingType,
                            IsBlocking = isBlocked,
                            IsStudentID = isIdStudent,
                            IsNameStudent = isNameStudent,
                            IsRealtionBLockingCategoryType = (haveAccessBlock && isRealtionBLockingCategoryType && !string.IsNullOrEmpty(IdBlockingCategory) && !string.IsNullOrEmpty(IdBlockingType)) ? true : false,
                            IsSuccess = (IdBlockingCategory != null & IdBlockingType != null && isIdStudent && isNameStudent && isRealtionBLockingCategoryType && haveAccessBlock) ? true : false
                        });
                    }
                }
            }
            List<IDictionary<string, object>> respone = new List<IDictionary<string, object>>();

            foreach (var student in dataSource.Select(x => new { StudentId = x.IdStudent, StudentName = x.StudentName }).Distinct().ToList())
            {
                IDictionary<string, object> dataRespone = new Dictionary<string, object>();
                dataRespone.Add("StudentName", $"{student.StudentName}|{dataSource.Where(x => x.IdStudent == student.StudentId).Select(y => y.IsNameStudent).FirstOrDefault().ToString()}");
                dataRespone.Add("StudentId", $"{student.StudentId}|{dataSource.Where(x => x.IdStudent == student.StudentId).Select(y => y.IsStudentID).FirstOrDefault().ToString()}");
                dataRespone.Add("StatusImport", dataSource.Any(x => x.IsSuccess == false && x.IdStudent == student.StudentId) ? false : true);

                foreach (var item in dataSource.Where(x => x.IdStudent == student.StudentId))
                {
                    dataRespone.Add($"{item.BlockingCategory} | {item.BlockingType}",
                        $"{item.IsBlocking}|{item.IdBlockingCategory}|{item.IdBlockingType}|{false.ToString()}|{item.IsRealtionBLockingCategoryType}");
                }
                respone.Add(dataRespone);
            }

            #region UnUsed Code
            //var GetDataItem = new DataTable();
            //var columnItem = dataSourceColumnName.GetRange(0, dataSourceColumnName.Count)
            //      .Select(x => new
            //      {
            //          ColumnName = $"{x.BlockingCategory} | {x.BlockingType}"
            //      })
            //      .ToList();
            //GetDataItem.Columns.Add(new DataColumn("StudentName", typeof(string)));
            //GetDataItem.Columns.Add(new DataColumn("StudentId", typeof(string)));
            //GetDataItem.Columns.Add(new DataColumn("StatusImport", typeof(string)));
            //GetDataItem.Columns.AddRange(columnItem.Select(c => new DataColumn(c.ColumnName, typeof(string)))
            //.ToArray());

            //foreach (var Student in dataSource.Select(x => new { StudentId = x.IdStudent, StudentName = x.StudentName }).Distinct().ToList())
            //{

            //    var row = GetDataItem.NewRow();
            //    foreach (var c in columnItem)
            //    {
            //        var columnName = c.ColumnName;
            //        var splitColumnNameUpload = columnName.Split(" | ");
            //        var blockingCategory = splitColumnNameUpload[0];
            //        var blockingType = splitColumnNameUpload[1];
            //        var IdBlockingCategory = dataBlockingCategory.Where(x => x.Name.ToLower() == blockingCategory.ToLower()).Select(y => y.Id).FirstOrDefault();
            //        var IdBlockingType = dataBlockingType.Where(x => x.Name.ToLower() == blockingType.ToLower()).Select(y => y.Id).FirstOrDefault();


            //        if (dataSource.Any(x => x.IdStudent == Student.StudentId && x.IdBlockingCategory == IdBlockingCategory && x.IdBlockingType == IdBlockingType))
            //        {
            //            var data = dataSource.Where(x => x.IdStudent == Student.StudentId && x.IdBlockingCategory == IdBlockingCategory && x.IdBlockingType == IdBlockingType).FirstOrDefault();
            //            row["StudentName"] = $"{Student.StudentName}|{data.IsNameStudent.ToString()}";
            //            row["StudentId"] = $"{Student.StudentId}|{data.IsStudentID.ToString()}";
            //            row[columnName.ToString()] = $"{data.IsBlocking.ToString()}|{data.IdBlockingCategory}|{data.IdBlockingType}|{false.ToString()}|{data.IsRealtionBLockingCategoryType.ToString()}";                    
            //        }                   
            //    }
            //    row["StatusImport"] = dataSource.Any(x => x.IsSuccess == false && x.IdStudent == Student.StudentId) ? false : true;
            //    GetDataItem.Rows.Add(row);
            //}
            #endregion


            return Request.CreateApiResult2(respone as object);
        }
    }
}

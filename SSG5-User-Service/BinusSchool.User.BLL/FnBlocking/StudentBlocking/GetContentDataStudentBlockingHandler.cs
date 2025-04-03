using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using BinusSchool.Persistence.UserDb.Entities.School;
using BinusSchool.Persistence.UserDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.Formula.PTG;
using NPOI.Util;
//using static FluentValidation.Validators.PredicateValidator<T, TProperty>;

namespace BinusSchool.User.FnBlocking.StudentBlocking
{
    public class GetContentDataStudentBlockingHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetContentDataStudentBlockingHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetContentDataStudentBlockingRequest>();

            var semester = Int32.Parse(param.Semester);
            var predicate = PredicateBuilder.Create<MsHomeroomStudent>(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear
                                                                && x.Homeroom.Semester == semester);

            var columns = new[] { "StudentId", "StudentName", "HomeRoom" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "StudentId" },
                { columns[1], "StudentName" },
                { columns[2], "HomeRoom" },
            };

            var queryHomeroomStudent = _dbContext.Entity<MsHomeroomStudent>()
                        .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                        .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.MsClassroom)
                        .Include(e => e.Student)
                        .Where(predicate);

            if (!string.IsNullOrEmpty(param.IdLevel))
                queryHomeroomStudent = queryHomeroomStudent.Where(e => e.Homeroom.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                queryHomeroomStudent = queryHomeroomStudent.Where(e => e.Homeroom.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdHoomRoom))
                queryHomeroomStudent = queryHomeroomStudent.Where(e => e.Homeroom.Id == param.IdHoomRoom);

            var listStudent = await queryHomeroomStudent
                    .Select(e => new GetStudentListResult
                    {
                        StudentName = $" {NameUtil.GenerateFullName(e.Student.FirstName, e.Student.MiddleName, e.Student.LastName)} - {e.IdStudent}",
                        Homeroom = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.MsClassroom.Code,
                        IdStudent = e.IdStudent
                    }).ToListAsync(CancellationToken);

            var query = listStudent.Distinct();

            if (!string.IsNullOrEmpty(param.IdStudent))
            {
                query = query.Where(x => x.IdStudent == param.IdStudent);
            }

            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => x.IdStudent.Contains(param.Search) || x.StudentName.Contains(param.Search));
            }

            ////ordering
            query = query.OrderBy(x => x.IdStudent);

            switch (param.OrderBy)
            {
                case "StudentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName)
                        : query.OrderBy(x => x.StudentName);
                    break;
                case "StudentId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IdStudent)
                        : query.OrderBy(x => x.IdStudent);
                    break;
                case "HomeRoom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom)
                        : query.OrderBy(x => x.Homeroom);
                    break;
            };

            var dataBlocking1 = await _dbContext.Entity<MsBlockingCategoryType>()
                .Include(x => x.BlockingCategory).ThenInclude(x => x.UserBlockings)
                .Include(x => x.BlockingCategory).ThenInclude(x => x.StudentBlockings)
                .Include(x => x.BlockingType)
                .Where(x => x.BlockingType.Category != "FEATURE" && x.BlockingCategory.IdSchool == param.IdSchool)
                .OrderBy(x => x.BlockingCategory.Name).ThenBy(x => x.BlockingType.Order)
                .ToListAsync();                             
            var dataBlocking2 = await _dbContext.Entity<MsBlockingCategoryType>()
                .Include(x => x.BlockingCategory).ThenInclude(x => x.UserBlockings)
                .Include(x => x.BlockingCategory).ThenInclude(x => x.StudentBlockings)
                .Include(x => x.BlockingType)
                .Where(x => x.BlockingType.Category == "FEATURE" && x.BlockingCategory.IdSchool == param.IdSchool)
                .OrderBy(x => x.BlockingCategory.Name).ThenBy(x => x.BlockingType.Name)
                .ToListAsync();
            var dataBlocking = dataBlocking1.Concat(dataBlocking2).OrderBy(x => x.BlockingCategory.Name);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();

                items = result.ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            List<IDictionary<string, object>> respone = new List<IDictionary<string, object>>();

            foreach (var student in query)
            {
                IDictionary<string, object> dataRespone = new Dictionary<string, object>();

                dataRespone.Add("studentName", student.StudentName);
                dataRespone.Add("studentId", student.IdStudent);
                dataRespone.Add("class/HomeRoom", student.Homeroom);

                foreach (var item in dataBlocking)
                {
                    dataRespone.Add($"{item.BlockingCategory.Name} | {item.BlockingType.Name}",
                        $"{item.BlockingCategory.StudentBlockings.Any(x => x.IdStudent == student.IdStudent && x.IdBlockingCategory == item.IdBlockingCategory && x.IdBlockingType == item.IdBlockingType && x.IsBlocked)}|{item.IdBlockingCategory}|{item.IdBlockingType}|{item.BlockingCategory.UserBlockings.Any(x => x.IdUser == param.IdUser)}");
                }

                respone.Add(dataRespone);
            }

            return Request.CreateApiResult2(respone as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));


            #region Unused Code
                        //var query = (from st in _dbContext.Entity<MsStudent>()
            //             join hrs in _dbContext.Entity<MsHomeroomStudent>() on st.Id equals hrs.IdStudent
            //             join hr in _dbContext.Entity<MsHomeroom>() on hrs.IdHomeroom equals hr.Id
            //             join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on hr.IdGradePathwayClassRoom equals gpc.Id
            //             join gp in _dbContext.Entity<MsGradePathway>() on gpc.IdGradePathway equals gp.Id
            //             join g in _dbContext.Entity<MsGrade>() on gp.IdGrade equals g.Id
            //             join lv in _dbContext.Entity<MsLevel>() on g.IdLevel equals lv.Id
            //             join cr in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals cr.Id
            //             join sb_ in _dbContext.Entity<MsStudentBlocking>() on st.Id equals sb_.IdStudent into _sb
            //             from sb in _sb.DefaultIfEmpty()
            //             join bc_ in _dbContext.Entity<MsBlockingCategory>() on sb.IdBlockingCategory equals bc_.Id into _bc
            //             from bc in _bc.DefaultIfEmpty()
            //             join bt_ in _dbContext.Entity<MsBlockingCategoryType>() on bc.Id equals bt_.IdCategory into _bt
            //             from bt in _bt.DefaultIfEmpty()
            //             where lv.IdAcademicYear == param.IdAcademicYear
            //             select new
            //             {
            //                 IdStudent = st.Id,
            //                 StudentName = string.IsNullOrEmpty(st.LastName) ? $"{st.FirstName}{st.MiddleName}" : $"{st.FirstName}{st.LastName}",
            //                 IdLevel = lv.Id,
            //                 IdGrade = g.Id,
            //                 Semester = hr.Semester.ToString(),
            //                 idHomeRoom = hr.Id,
            //                 homeroom = g.Code + cr.Code,
            //                 IdBlockingCategory = bc.Id,
            //                 BlockingCategory = bc.Name,
            //                 IdBlockingType = bt.IdType,
            //                 BlockingType = bt.Type.Name,
            //                 IsBlock = (!string.IsNullOrEmpty(bc.Id) && !string.IsNullOrEmpty(bt.IdType)) ? sb.IsBlocked : false,
            //             }).AsQueryable();


            //if (!string.IsNullOrEmpty(param.IdLevel))
            //{
            //    query = query.Where(x => x.IdLevel == param.IdLevel);
            //}

            //if (!string.IsNullOrEmpty(param.IdGrade))
            //{
            //    query = query.Where(x => x.IdGrade == param.IdGrade);
            //}

            //if (!string.IsNullOrEmpty(param.Semester))
            //{
            //    query = query.Where(x => x.Semester == param.Semester);
            //}

            //if (!string.IsNullOrEmpty(param.IdHoomRoom))
            //{
            //    query = query.Where(x => x.idHomeRoom == param.IdHoomRoom);
            //}

            //if (!string.IsNullOrEmpty(param.Search))
            //{
            //    query = query.Where(x => EF.Functions.Like(x.IdStudent, param.SearchPattern()) ||
            //    EF.Functions.Like(x.StudentName, param.SearchPattern()));
            //}

            //////ordering
            //query = query.OrderBy(x => x.IdStudent);

            //switch (param.OrderBy)
            //{
            //    case "StudentName":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.StudentName)
            //            : query.OrderBy(x => x.StudentName);
            //        break;
            //    case "StudentId":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.IdStudent)
            //            : query.OrderBy(x => x.IdStudent);
            //        break;
            //    case "HomeRoom":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.homeroom)
            //            : query.OrderBy(x => x.homeroom);
            //        break;
            //};

            //var dataSource = new List<GetContentDataColumnsNameQueryResult>();
            //if (param.Return == CollectionType.Lov)
            //{
            //    var result = await query
            //        .ToListAsync(CancellationToken);

            //    var dataStudentBlocking = await _dbContext.Entity<MsStudentBlocking>()
            //                                .Include(x => x.BlockingCategory)
            //                                .Include(x => x.BlockingType)
            //                                .Where(x => result.Select(y => y.IdStudent).ToList().Contains(x.IdStudent))
            //                                .ToListAsync();


            //    dataSource = result
            //            .Select(x => new GetContentDataColumnsNameQueryResult
            //            {
            //                IdStudent = x.IdStudent,
            //                StudentName = x.StudentName,
            //                Homeroom = x.homeroom,
            //                IdBlockingCategory = x.IdBlockingCategory,
            //                BlockingCategory = x.BlockingCategory,
            //                IdBlockingType = x.IdBlockingType,
            //                BlockingType = x.BlockingType,
            //                IsBlocking = x.IsBlock,
            //            }).ToList();
            //}
            //else
            //{
            //    var result = await query
            //        .SetPagination(param)
            //        .ToListAsync(CancellationToken);

            //    dataSource = result
            //            .Select(x => new GetContentDataColumnsNameQueryResult
            //            {
            //                IdStudent = x.IdStudent,
            //                StudentName = x.StudentName,
            //                Homeroom = x.homeroom,
            //                IdBlockingCategory = x.IdBlockingCategory,
            //                BlockingCategory = x.BlockingCategory,
            //                IdBlockingType = x.IdBlockingType,
            //                BlockingType = x.BlockingType,
            //                IsBlocking = x.IsBlock,
            //            }).ToList();
            //}
            //var count = param.CanCountWithoutFetchDb(dataSource.Count)
            //    ? dataSource.Count
            //    : query.ToList().Count;

            //var dataAssignUser = await (from usb in _dbContext.Entity<MsUserBlocking>()
            //                            join usr in _dbContext.Entity<MsUser>() on usb.IdUser equals usr.Id
            //                            join mbc in _dbContext.Entity<MsBlockingCategory>() on usb.IdBlockingCategory equals mbc.Id
            //                            join mbt in _dbContext.Entity<MsBlockingCategoryType>() on mbc.Id equals mbt.IdCategory
            //                            where usb.IdUser == param.IdUser && mbc.IdSchool == param.IdSchool
            //                            select new
            //                            {
            //                                IdStudent = usb.IdUser,
            //                                IdBlockingCategory = usb.IdBlockingCategory,
            //                                idBlockingType = mbt.IdType
            //                            }).ToListAsync(CancellationToken);

            //foreach (var data in dataSource)
            //{
            //    data.CanEdit = dataAssignUser.Any(x => x.idBlockingType == data.IdBlockingCategory && x.IdBlockingCategory == data.IdBlockingCategory);
            //}

            //var dataBlockingCategory = await _dbContext.Entity<MsBlockingCategory>()
            //    .Where(x => x.IdSchool == param.IdSchool)
            //    .Select(x => new
            //    {
            //        Id = x.Id,
            //        Name = x.Name
            //    })
            //    .ToListAsync(CancellationToken);

            //var dataBlockingType = await _dbContext.Entity<MsBlockingType>()
            //                .Where(x => x.IdSchool == param.IdSchool)
            //                            .Select(x => new
            //                            {
            //                                Id = x.Id,
            //                                Name = x.Name
            //                            })
            //                .ToListAsync(CancellationToken);

            //var dataColumns = await (from mbc in _dbContext.Entity<MsBlockingCategory>()
            //                         join mbct in _dbContext.Entity<MsBlockingCategoryType>() on mbc.Id equals mbct.IdCategory
            //                         join ct in _dbContext.Entity<MsBlockingType>() on mbct.IdType equals ct.Id
            //                         where mbc.IdSchool == param.IdSchool
            //                         select new
            //                         {
            //                             BlockingCategory = mbc.Name,
            //                             BlockingType = ct.Name,
            //                             Order = ct.Order,
            //                         }).ToListAsync();

            //var dataSourceColumns = dataColumns
            //.Where(e => e.Order == 2 || e.Order == 1)
            //.OrderBy(e => e.Order)
            //.ToList()
            //.Union(
            //    dataColumns
            //    .Where(e => e.Order != 2 || e.Order != 1)
            //    .OrderBy(e => e.BlockingType)
            //    .ToList()
            //).OrderBy(e => e.BlockingCategory).ToList();


            //var columnNameitem = dataSourceColumns.GetRange(0, dataSourceColumns.Count)
            //      .Select(x => new
            //      {
            //          ColumnName = $"{x.BlockingCategory} | {x.BlockingType}"
            //      })
            //      .ToList();

            //var GetDataItem = new DataTable();

            //GetDataItem.Columns.Add(new DataColumn("StudentName", typeof(string)));
            //GetDataItem.Columns.Add(new DataColumn("StudentId", typeof(string)));
            //GetDataItem.Columns.Add(new DataColumn("Class/HomeRoom", typeof(string)));
            //GetDataItem.Columns.AddRange(columnNameitem.Select(c => new DataColumn(c.ColumnName, typeof(string)))
            //.ToArray());

            //foreach (var Student in dataSource.Select(x => new { StudentId = x.IdStudent, StudentName = x.StudentName, Homeroom = x.Homeroom }).Distinct().ToList())
            //{

            //    var row = GetDataItem.NewRow();

            //    foreach (var c in columnNameitem)
            //    {
            //        var columnName = c.ColumnName;
            //        var splitColumnNameUpload = columnName.Split("|");
            //        var blockingCategory = splitColumnNameUpload[0].ToString().ToString().TrimStart().TrimEnd();
            //        var blockingType = splitColumnNameUpload[1].ToString().ToString().TrimStart().TrimEnd();
            //        var IdBlockingCategory = dataBlockingCategory.Where(x => x.Name.ToLower() == blockingCategory.ToLower()).Select(y => y.Id).FirstOrDefault();
            //        var IdBlockingType = dataBlockingType.Where(x => x.Name.ToLower() == blockingType.ToLower()).Select(y => y.Id).FirstOrDefault();

            //        row["StudentName"] = Student.StudentName;
            //        row["StudentId"] = Student.StudentId;
            //        row["Class/HomeRoom"] = Student.Homeroom;
            //        if (dataSource.Any(x => x.IdStudent == Student.StudentId && x.IdBlockingCategory == IdBlockingCategory && x.IdBlockingType == IdBlockingType))
            //        {
            //            var data = dataSource.Where(x => x.IdStudent == Student.StudentId && x.IdBlockingCategory == IdBlockingCategory && x.IdBlockingType == IdBlockingType).FirstOrDefault();
            //            row[columnName.ToString()] = $"{data.IsBlocking.ToString()}|{data.IdBlockingCategory}|{data.IdBlockingType}|{data.CanEdit.ToString()}";
            //        }
            //        else
            //        {
            //            var canEdit = dataAssignUser.Any(x => x.IdBlockingCategory == IdBlockingCategory && x.idBlockingType == IdBlockingType);
            //            row[columnName.ToString()] = $"{false.ToString()}|{IdBlockingCategory}|{IdBlockingType}|{canEdit.ToString()}";
            //        }
            //    }
            //    GetDataItem.Rows.Add(row);
            //}
            //return Request.CreateApiResult2(GetDataItem as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));

            //DataTableStudentBlockingResultModel dataTableStudentBlockingResultModel = new DataTableStudentBlockingResultModel
            //{
            //    Data = JsonConvert.SerializeObject(GetDataItem),
            //    Draw = 1,
            //    RecordsFiltered = 10,
            //    RecordsTotal = 100                
            //};
            //return Request.CreateApiResult2(GetDataItem as object);
            #endregion



        }
    }
}

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
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using BinusSchool.Persistence.UserDb.Entities.School;
using BinusSchool.Persistence.UserDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.User.FnBlocking.StudentBlocking
{
    public class GetDataStudentBlockingHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetDataStudentBlockingHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDataStudentBlockingRequest>();

            var columns = new[] { "IdStudent", "StudentName", "HomeRoom" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "IdStudent" },
                { columns[1], "StudentName" },
                { columns[2], "HomeRoom" },
            };

            //Get All Student From Filter
            #region Student
            var dataHomeroom = await (from hrs in _dbContext.Entity<MsHomeroomStudent>()
                                join st in _dbContext.Entity<MsStudent>() on hrs.IdStudent equals st.Id
                                join hr in _dbContext.Entity<MsHomeroom>() on hrs.IdHomeroom equals hr.Id
                                join g in _dbContext.Entity<MsGrade>() on hr.IdGrade equals g.Id
                                join Level in _dbContext.Entity<MsLevel>() on g.IdLevel equals Level.Id
                                join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on hr.IdGradePathwayClassRoom equals gpc.Id
                                join cr in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals cr.Id
                                where hrs.Semester.ToString() == param.Semester
                                      select new {
                                                IdStudent = hrs.IdStudent,
                                                Semester = hrs.Semester,
                                                IdHomeRoom = hrs.IdHomeroom,
                                                Homeroom = g.Code + cr.Code,
                                                Student = string.IsNullOrEmpty(st.LastName) ? $"{st.FirstName} {st.MiddleName}" : $"{st.FirstName} {st.LastName}",
                                                IdLevel = Level.Id,
                                                IdGrade = g.Id
                                        }).ToListAsync();

            var query = (from hrs in dataHomeroom
                         select new 
                         {
                             IdStudent = hrs.IdStudent,
                             StudentName = hrs.Student,
                             IdLevel = hrs.IdLevel,
                             IdGrade = hrs.IdGrade,
                             Semester = hrs.Semester.ToString(),
                             IdHomeRoom = hrs.IdHomeRoom,
                             Homeroom = hrs.Homeroom,
                         }).Distinct();


            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdLevel == param.IdLevel);            

            if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(x => x.IdGrade == param.IdGrade);            

            if (!string.IsNullOrEmpty(param.Semester))
                query = query.Where(x => x.Semester.ToString() == param.Semester);            

            if (!string.IsNullOrEmpty(param.IdHoomRoom))
                query = query.Where(x => x.IdHomeRoom == param.IdHoomRoom);

            if (!string.IsNullOrEmpty(param.IdStudent))
                query = query.Where(x => x.IdStudent == param.IdStudent);

            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.IdStudent.Contains(param.Search) || x.StudentName.Contains(param.Search));
            
            ////ordering
            query = query.OrderBy(x => x.IdStudent);

            switch (param.OrderBy)
            {
                case "StudentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName)
                        : query.OrderBy(x => x.StudentName);
                    break;
                case "IdStudent":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IdStudent)
                        : query.OrderBy(x => x.IdStudent);
                    break;
                case "HomeRoom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom)
                        : query.OrderBy(x => x.Homeroom);
                    break;
                default:
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName)
                        : query.OrderBy(x => x.StudentName);
                    break;
            };

            var dataStudent = await _dbContext.Entity<MsStudentBlocking>()
                .Include(x => x.BlockingCategory).ThenInclude(x => x.BlockingCategoryTypes)
                .Where(x => x.BlockingCategory.Id == param.IdBlockingCategory && x.IsBlocked)
                .ToListAsync();

            if (!string.IsNullOrEmpty(param.IdBlockingType))
                dataStudent = dataStudent.Where(x => x.IdBlockingType == param.IdBlockingType).ToList();
            
            var idStudentBlocking = dataStudent.Select(x => x.IdStudent).ToList();
            
            query = query.Where(x => idStudentBlocking.Any(t => t == x.IdStudent));

            var count = 0;
            count = query.Count();
            if (param.Return != CollectionType.Lov)
            {

                query = query.SetPagination(param);
            }

            var items = query
                    .Select(x => new GetDataStudentBlockingResult
                    {
                        IdStudent = x.IdStudent,
                        StudentName = x.StudentName,
                        Homeroom = x.Homeroom
                    })
                    .ToList();
            #endregion

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}

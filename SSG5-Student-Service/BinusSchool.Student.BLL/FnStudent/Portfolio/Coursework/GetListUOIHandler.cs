using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.Portfolio.Coursework.Validator;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.User;

namespace BinusSchool.Student.FnStudent.Portfolio.Coursework
{
    public class GetListUOIHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListUOIHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListUOIRequest>();
            
            var query = _dbContext.Entity<MsUOI>()
                .OrderByDescending(x => x.DateIn);

            List<MsUOI> result = new List<MsUOI>();

            result = await query.Select(x => new MsUOI
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync(CancellationToken);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = result.Select(x => new CodeWithIdVm
                {
                    Id = x.Id,
                    Code = x.Id,
                    Description = x.Name
                }).ToList();
            }
            else
            {
                items = result
                .SetPagination(param)
                .Select(x => new CodeWithIdVm
                {
                    Id = x.Id,
                    Code = x.Id,
                    Description = x.Name
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.Student.FnStudent.Achievement
{
    public class GetFocusAreaHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetFocusAreaHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var item = await _dbContext.Entity<MsFocusArea>()
                        .Select(e => new ItemValueVm
                        {
                            Id = e.Id,
                            Description = e.Description
                        }).ToListAsync(CancellationToken);


            return Request.CreateApiResult2(item as object);
        }
    }
}

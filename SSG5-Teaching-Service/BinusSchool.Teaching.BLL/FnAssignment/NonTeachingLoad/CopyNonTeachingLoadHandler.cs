using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Teaching.FnAssignment.NonTeachingLoad.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnAssignment.NonTeachingLoad
{
    public class CopyNonTeachingLoadHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public CopyNonTeachingLoadHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CopyNonTeachingLoadRequest, CopyNonTeachingLoadValidator>();

            var nonTeachingLoads = await _dbContext.Entity<MsNonTeachingLoad>()
                .Where(x => body.Ids.Contains(x.Id))
                .Select(x => new
                {
                    idTeacherPosition = x.IdTeacherPosition,
                    category = x.Category,
                    load = x.Load,
                    parameter = x.Parameter
                }).ToListAsync(CancellationToken);

            if (nonTeachingLoads == null)
                throw new NotFoundException("Non Teaching Load data not found");

            List<MsNonTeachingLoad> newTeachingLoad = new List<MsNonTeachingLoad>();

            foreach (var nonTeachingLoad in nonTeachingLoads)
            {
                newTeachingLoad.Add(new MsNonTeachingLoad
                {
                    Id = Guid.NewGuid().ToString().ToUpper(),
                    IdTeacherPosition = nonTeachingLoad.idTeacherPosition,
                    IdAcademicYear = body.IdAcadyearTarget,
                    Category = nonTeachingLoad.category,
                    Load = nonTeachingLoad.load,
                    Parameter = nonTeachingLoad.parameter,
                });
            }

            await _dbContext.Entity<MsNonTeachingLoad>().AddRangeAsync(newTeachingLoad, CancellationToken);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

    }
}

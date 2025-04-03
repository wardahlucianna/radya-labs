using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.ExemplaryCharacter.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter
{
    public class DeleteExemplaryCategorySettingsHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public DeleteExemplaryCategorySettingsHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteExemplaryCategorySettingsRequest, DeleteExemplaryCategorySettingsValidator>();

            var deleteExemplaryCategoryList = await _dbContext.Entity<LtExemplaryCategory>()
                                                .Where(x => param.IdExemplaryCategoryList.Any(y => y == x.Id))
                                                .ToListAsync(CancellationToken);

            // soft delete
            deleteExemplaryCategoryList.ForEach(x => x.IsActive = false);

            _dbContext.Entity<LtExemplaryCategory>().UpdateRange(deleteExemplaryCategoryList);
            await _dbContext.SaveChangesAsync();

            return Request.CreateApiResult2();
        }
    }
}

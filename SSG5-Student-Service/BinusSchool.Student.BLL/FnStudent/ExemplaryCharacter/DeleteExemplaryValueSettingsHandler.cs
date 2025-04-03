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
    public class DeleteExemplaryValueSettingsHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public DeleteExemplaryValueSettingsHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteExemplaryValueSettingsRequest, DeleteExemplaryValueSettingsValidator>();

            var deleteExemplaryValueList = await _dbContext.Entity<LtExemplaryValue>()
                                           .Where(x => param.IdExemplaryValueList.Any(y => y == x.Id))
                                           .ToListAsync(CancellationToken);

            // Soft Delete
            deleteExemplaryValueList.ForEach(x => x.IsActive = false);

            _dbContext.Entity<LtExemplaryValue>().UpdateRange(deleteExemplaryValueList);
            await _dbContext.SaveChangesAsync();

            return Request.CreateApiResult2();
        }
    }
}

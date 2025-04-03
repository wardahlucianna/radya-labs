using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
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
    public class UpdateExemplaryCategorySettingsHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public UpdateExemplaryCategorySettingsHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateExemplaryCategorySettingsRequest, UpdateExemplaryCategorySettingsValidator>();

            var ltExemplaryCategory = await _dbContext.Entity<LtExemplaryCategory>()
                                        .Where(a => a.Id == param.IdExemplaryCategory)
                                        .FirstOrDefaultAsync(CancellationToken);

            var hasDuplicateCategory = _dbContext.Entity<LtExemplaryCategory>()
                                        .Where(x => x.Id != param.IdExemplaryCategory &&
                                                    (x.ShortDesc.Trim().ToUpper() == param.ShortDesc.Trim().ToUpper() ||
                                                    x.LongDesc.Trim().ToUpper() == param.LongDesc.Trim().ToUpper()) &&
                                                    x.IdSchool == ltExemplaryCategory.IdSchool)
                                        .Any();

            if (hasDuplicateCategory)
                throw new BadRequestException("An exemplary category with the same description already exists");

            ltExemplaryCategory.ShortDesc = param.ShortDesc.Trim();
            ltExemplaryCategory.LongDesc = param.LongDesc.Trim();
            ltExemplaryCategory.CurrentStatus = param.CurrentStatus;

            _dbContext.Entity<LtExemplaryCategory>().Update(ltExemplaryCategory);
            await _dbContext.SaveChangesAsync();

            return Request.CreateApiResult2();
        }
    }
}

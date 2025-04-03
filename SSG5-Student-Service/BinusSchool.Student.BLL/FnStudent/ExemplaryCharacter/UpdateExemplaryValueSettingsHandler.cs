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

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter
{
    public class UpdateExemplaryValueSettingsHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public UpdateExemplaryValueSettingsHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateExemplaryValueSettingsRequest, UpdateExemplaryValueSettingsValidator>();

            var ltExemplaryValue = await _dbContext.Entity<LtExemplaryValue>()
                                   .FindAsync(param.IdExemplaryValue);

            var hasDuplicateValue = _dbContext.Entity<LtExemplaryValue>()
                                    .Where(x => x.Id != param.IdExemplaryValue &&
                                               (x.ShortDesc.Trim().ToUpper() == param.ShortDesc.Trim().ToUpper() ||
                                                x.LongDesc.Trim().ToUpper() == param.LongDesc.Trim().ToUpper()) &&
                                                x.IdSchool == ltExemplaryValue.IdSchool)
                                    .Any();

            if (hasDuplicateValue)
                throw new BadRequestException("An exemplary value with the same description already exists");

            ltExemplaryValue.ShortDesc = param.ShortDesc.Trim();
            ltExemplaryValue.LongDesc = param.LongDesc.Trim();
            ltExemplaryValue.CurrentStatus = param.CurrentStatus;

            _dbContext.Entity<LtExemplaryValue>().Update(ltExemplaryValue);
            await _dbContext.SaveChangesAsync();

            return Request.CreateApiResult2();
        }
    }
}

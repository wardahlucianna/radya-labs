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
    public class CreateExemplaryValueSettingsHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public CreateExemplaryValueSettingsHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<CreateExemplaryValueSettingsRequest, CreateExemplaryValueSettingsValidator>();

            var hasDuplicateValue = _dbContext.Entity<LtExemplaryValue>()
                                    .Where(x => (x.ShortDesc.Trim().ToUpper() == param.ShortDesc.Trim().ToUpper() ||
                                                 x.LongDesc.Trim().ToUpper() == param.LongDesc.Trim().ToUpper()) &&
                                                 x.IdSchool == param.IdSchool
                                                 )
                                    .Any();

            var highestOrderNumber = _dbContext.Entity<LtExemplaryValue>().Any() ? _dbContext.Entity<LtExemplaryValue>().Max(a => a.OrderNumber) : 0;

            var newOrderNumber = highestOrderNumber + 1;

            if (hasDuplicateValue)
                throw new BadRequestException("An exemplary value with the same decription already exists");

            var newLtExemplaryValue = new LtExemplaryValue
            {
                Id = Guid.NewGuid().ToString(),
                LongDesc = param.LongDesc.Trim(),
                ShortDesc = param.ShortDesc.Trim(),
                IdSchool = param.IdSchool,
                OrderNumber = newOrderNumber,
                CurrentStatus = param.CurrentStatus
            };
                                    
            _dbContext.Entity<LtExemplaryValue>().Add(newLtExemplaryValue);
            await _dbContext.SaveChangesAsync();

            return Request.CreateApiResult2();
        }
    }
}

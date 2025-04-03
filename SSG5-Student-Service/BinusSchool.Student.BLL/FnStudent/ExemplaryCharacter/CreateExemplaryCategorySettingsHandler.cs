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
    public class CreateExemplaryCategorySettingsHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public CreateExemplaryCategorySettingsHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<CreateExemplaryCategorySettingsRequest, CreateExemplaryCategorySettingsValidator>();

            var hasDuplicateCategory = _dbContext.Entity<LtExemplaryCategory>()
                                        .Where(x => (x.ShortDesc.Trim().ToUpper() == param.ShortDesc.Trim().ToUpper() ||
                                                    x.LongDesc.Trim().ToUpper() == param.LongDesc.Trim().ToUpper()) &&
                                                    x.IdSchool == param.IdSchool
                                                    )
                                        .Any();

            var highestOrderNumber = _dbContext.Entity<LtExemplaryCategory>().Any() ? _dbContext.Entity<LtExemplaryCategory>().Max(x => x.OrderNumber) : 0;

            var newOrderNumber = highestOrderNumber + 1;

            if (hasDuplicateCategory)
                throw new BadRequestException("An exemplary category with the same description already exists");

            var newLtExemplaryCategory = new LtExemplaryCategory
            {
                Id = Guid.NewGuid().ToString(),
                LongDesc = param.LongDesc.Trim(),
                ShortDesc = param.ShortDesc.Trim(),
                IdSchool = param.IdSchool,
                CurrentStatus = param.CurrentStatus,
                OrderNumber = newOrderNumber
            };

            _dbContext.Entity<LtExemplaryCategory>().Add(newLtExemplaryCategory);
            await _dbContext.SaveChangesAsync();

            return Request.CreateApiResult2();
        }
    }
}

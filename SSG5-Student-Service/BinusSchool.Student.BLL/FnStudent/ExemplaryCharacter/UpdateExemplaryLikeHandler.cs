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
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.ExemplaryCharacter.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter
{
    public class UpdateExemplaryLikeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public UpdateExemplaryLikeHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var body = await Request.ValidateBody<UpdateExemplaryLikeRequest, UpdateExemplaryLikeValidator>();


            var UserExists = await _dbContext.Entity<MsUser>().FindAsync(body.IdUser);
            if (UserExists is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["User"], "Id", body.IdUser));


            var ExemplaryExists = await _dbContext.Entity<TrExemplary>().Include(x => x.ExemplaryLikes).Where(a => a.Id == body.idExemplary).FirstOrDefaultAsync();
            if (ExemplaryExists is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Exemplary"], "Id", body.idExemplary));


            if (ExemplaryExists.ExemplaryLikes.Where(a => a.UserIn == body.IdUser).Count() > 0)
            {
                List<TrExemplaryLikes> ExemplaryLikeExists = ExemplaryExists.ExemplaryLikes.Where(a => a.UserIn == body.IdUser).ToList();
                _dbContext.Entity<TrExemplaryLikes>().RemoveRange(ExemplaryLikeExists);
            }
            else
            {
                TrExemplaryLikes AddExemplaryLikes = new TrExemplaryLikes();
                AddExemplaryLikes.Id = Guid.NewGuid().ToString();
                AddExemplaryLikes.UserIn = body.IdUser;
                AddExemplaryLikes.IdExemplary = body.idExemplary;
                _dbContext.Entity<TrExemplaryLikes>().Add(AddExemplaryLikes);
            }


            await _dbContext.SaveChangesAsync(CancellationToken);
            UpdateExemplaryLikeResult ReturnResult = new UpdateExemplaryLikeResult();
            ReturnResult.IdExemplary = body.idExemplary;
            ReturnResult.CountExemplaryLikes = (_dbContext.Entity<TrExemplary>().Include(x => x.ExemplaryLikes).Where(a => a.Id == body.idExemplary).First().ExemplaryLikes.Count());

            return Request.CreateApiResult2(ReturnResult as object);
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class UpdateMeritDemeritByIdHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public UpdateMeritDemeritByIdHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateMeritDemeritByIdRequest, UpdateMeritDemeritByIdValidator>();

            if (param.IsMerit)
            {
                var query = await _dbContext.Entity<TrEntryMeritStudent>()
                    .Include(x => x.MeritDemeritMapping)
                    .Where(x => x.Id == param.Id).FirstOrDefaultAsync(CancellationToken);

                if (query is null)
                    throw new NotFoundException("Entry Merit not found");

                if (query.Type != Common.Model.Enums.EntryMeritStudentType.Merit)
                    throw new BadRequestException("Cannot edit achievement type");

                if (query.MeritUserCreate != param.IdUser)
                    throw new BadRequestException("Cannot edit merit with this user");

                query.DateMerit = param.Date;
                query.Note = param.Note;
                if (query.IdMeritDemeritMapping != param.IdMeritDemeritMapping)
                {
                    var queryMeritMapping = await _dbContext.Entity<MsMeritDemeritMapping>()
                        .Where(x => x.Id == param.IdMeritDemeritMapping)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (queryMeritMapping is null)
                        throw new NotFoundException("Merit Mapping not found");

                    query.IdMeritDemeritMapping = param.IdMeritDemeritMapping;
                    query.Point = queryMeritMapping.Point.Value;

                    var queryStudentPoint = await _dbContext.Entity<TrStudentPoint>()
                        .Where(x => x.IdHomeroomStudent == query.IdHomeroomStudent)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (queryStudentPoint is null)
                        throw new NotFoundException("Student point not found");

                    queryStudentPoint.MeritPoint = queryMeritMapping.Point.Value;

                    _dbContext.Entity<TrStudentPoint>().Update(queryStudentPoint);
                }

                _dbContext.Entity<TrEntryMeritStudent>().Update(query);
            }
            else
            {
                var query = await _dbContext.Entity<TrEntryDemeritStudent>()
                    .Include(x => x.MeritDemeritMapping)
                    .Where(x => x.Id == param.Id).FirstOrDefaultAsync(CancellationToken);

                if (query is null)
                    throw new NotFoundException("Entry Merit not found");

                query.DateDemerit = param.Date;
                query.Note = param.Note;
                if (query.IdMeritDemeritMapping != param.IdMeritDemeritMapping)
                {
                    var queryMeritMapping = await _dbContext.Entity<MsMeritDemeritMapping>()
                        .Where(x => x.Id == param.IdMeritDemeritMapping)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (queryMeritMapping is null)
                        throw new NotFoundException("Merit Mapping not found");

                    query.IdMeritDemeritMapping = param.IdMeritDemeritMapping;
                    query.Point = queryMeritMapping.Point.Value;

                    var queryStudentPoint = await _dbContext.Entity<TrStudentPoint>()
                        .Where(x => x.IdHomeroomStudent == query.IdHomeroomStudent)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (queryStudentPoint is null)
                        throw new NotFoundException("Student point not found");

                    queryStudentPoint.DemeritPoint = queryMeritMapping.Point.Value;

                    _dbContext.Entity<TrStudentPoint>().Update(queryStudentPoint);
                }

                _dbContext.Entity<TrEntryDemeritStudent>().Update(query);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}

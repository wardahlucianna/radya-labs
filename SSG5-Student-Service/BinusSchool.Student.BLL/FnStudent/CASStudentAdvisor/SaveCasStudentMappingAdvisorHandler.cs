using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.CASStudentAdvisor.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.CASStudentAdvisor
{
    public class SaveCasStudentMappingAdvisorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveCasStudentMappingAdvisorHandler(
                IStudentDbContext dbContext
            )
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveCasStudentMappingAdvisorRequest, SaveCasStudentMappingAdvisorValidator>();

            var paramUpdate = param.Data.Where(x => x.IdCasAdvisorStudent != null && x.IdCasAdvisor != null).ToList();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var data = await _dbContext.Entity<TrCasAdvisorStudent>()
                            .Where(x => x.HomeroomStudent.Homeroom.IdGrade == param.IdGrade)
                            .ToListAsync(CancellationToken);

                //insert when IdCasAdvisorStudent is null, and IdCasAdvisor not null
                var insert = param.Data.Where(x => x.IdCasAdvisorStudent == null && x.IdCasAdvisor != null)
                        .Select(x => new TrCasAdvisorStudent
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroomStudent = x.IdHomeroomStudent,
                            IdCasAdvisor = x.IdCasAdvisor
                        })
                        .ToList();

                //update when IdCasAdvisorStudent and IdCasAdvisor not null
                var update = param.Data
                            .Join(await _dbContext.Entity<TrCasAdvisorStudent>()
                            .Where(x => paramUpdate.Select(y => y.IdCasAdvisorStudent).ToList().Any(y => y == x.Id))
                            .ToListAsync(CancellationToken),
                            c => c.IdCasAdvisorStudent,
                            s => s.Id,
                            (c, s) => new { data = c, db = s })
                            .Where(x => x.data.IdCasAdvisor != x.db.IdCasAdvisor)
                            .Select(x => new TrCasAdvisorStudent
                            {
                                Id = x.db.Id,
                                IdHomeroomStudent = x.db.IdHomeroomStudent,
                                IdCasAdvisor = x.data.IdCasAdvisor
                            })
                            .ToList();

                var nonUpdate = param.Data
                            .Join(await _dbContext.Entity<TrCasAdvisorStudent>()
                            .Where(x => paramUpdate.Select(y => y.IdCasAdvisorStudent).ToList().Any(y => y == x.Id))
                            .ToListAsync(CancellationToken),
                            c => c.IdCasAdvisorStudent,
                            s => s.Id,
                            (c, s) => new { data = c, db = s })
                            .Where(x => x.data.IdCasAdvisor == x.db.IdCasAdvisor)
                            .Select(x => new TrCasAdvisorStudent
                            {
                                Id = x.db.Id,
                                IdHomeroomStudent = x.db.IdHomeroomStudent,
                                IdCasAdvisor = x.data.IdCasAdvisor
                            })
                            .ToList();

                //delete where IdCasAdvisor is null and IdCasAdvisorStudent not null
                var delete = param.Data
                            .Where(x => x.IdCasAdvisor == null && x.IdCasAdvisorStudent != null)
                            .Select(x => new TrCasAdvisorStudent
                            {
                                Id = x.IdCasAdvisorStudent,
                                IdHomeroomStudent = x.IdHomeroomStudent,
                                IdCasAdvisor = x.IdCasAdvisor
                            })
                            .ToList();


                var dataToDelete = data.Where(x => delete.Select(y => y.Id).ToList().Any(y => y == x.Id)).ToList();
                var dataToUpdate = data.Where(x => update.Select(y => y.Id).ToList().Any(y => y == x.Id)).ToList();

                if (dataToDelete.Count() > 0)
                {
                    foreach (var del in dataToDelete)
                    {
                        del.IsActive = false;
                        _dbContext.Entity<TrCasAdvisorStudent>().Update(del);
                    }
                }

                if(update.Count() > 0)
                {
                    foreach (var datas in dataToUpdate)
                    {
                        var updated = update.Where(x => x.Id == datas.Id).FirstOrDefault();
                        datas.IdCasAdvisor = updated.IdCasAdvisor;

                        _dbContext.Entity<TrCasAdvisorStudent>().Update(datas);
                    }
                }

                if(insert.Count() > 0)
                    _dbContext.Entity<TrCasAdvisorStudent>().AddRange(insert);

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }

    }
}

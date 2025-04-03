using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MoveStudentPathway;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MoveStudentPahway.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MoveStudentPahway
{
    public class MoveStudentPathwayHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly IStudentDbContext _dbContext;

        public MoveStudentPathwayHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddMoveStudentPathwayRequest, AddMoveStudentPathwayValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var existStudents = await _dbContext.Entity<MsStudent>()
                .Include(x => x.StudentGrades).ThenInclude(x => x.StudentGradePathways)
                .Where(x => body.IdStudents.Contains(x.Id))
                .ToListAsync(CancellationToken);

            foreach (var student in existStudents)
            {
                var currentGrade = student.StudentGrades.FirstOrDefault();
                if (currentGrade != null)
                {
                    // set inactive current pathway
                    var currentPathway = currentGrade.StudentGradePathways.FirstOrDefault();
                    if (currentPathway != null)
                    {
                        currentPathway.IsActive = false;
                        _dbContext.Entity<MsStudentGradePathway>().Update(currentPathway);
                    }

                    var newPathway = new MsStudentGradePathway
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdStudentGrade = currentGrade.Id,
                        IdPathway = body.IdPathway
                    };
                    _dbContext.Entity<MsStudentGradePathway>().Add(newPathway);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(null);
        }
    }
}
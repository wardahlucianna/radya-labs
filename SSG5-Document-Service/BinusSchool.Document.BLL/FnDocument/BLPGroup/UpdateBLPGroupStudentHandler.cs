using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.BLPGroup;
using BinusSchool.Document.FnDocument.BLPGroup.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.FnDocument.BLPGroup
{
    public class UpdateBLPGroupStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private IDbContextTransaction _transaction;

        public UpdateBLPGroupStudentHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime
            )
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var paramList = await Request.ValidateBody<List<UpdateBLPGroupStudentRequest>, UpdateBLPGroupStudentValidator>();

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                var idAcademicYearParamList = new List<string>();
                var semesterParamList = new List<int>();
                var idBLPGroupStudentParamList = new List<string>();
                var idStudentParamList = new List<string>();
                var idHomeroomParamList = new List<string>();

                foreach (var param in paramList)
                {
                    idAcademicYearParamList.Add(param.IdAcademicYear);
                    semesterParamList.Add(param.Semester);
                    idBLPGroupStudentParamList.Add(param.IdBLPGroupStudent);
                    idStudentParamList.Add(param.IdStudent);
                    idHomeroomParamList.Add(param.IdHomeroom);
                }

                var getBLPGroupStudents = _dbContext.Entity<TrBLPGroupStudent>()
                                            .Include(x => x.HomeroomStudent)
                                            .Where(x => idAcademicYearParamList.Any(y => y == x.IdAcademicYear) &&
                                                        semesterParamList.Any(y => y == x.Semester) &&
                                                        idBLPGroupStudentParamList.Any(y => y == x.Id) &&
                                                        idStudentParamList.Any(y => y == x.IdStudent)
                                                        )
                                            .ToList();

                var getHomeroomStudents = _dbContext.Entity<MsHomeroomStudent>()
                                            .Where(x => idHomeroomParamList.Any(y => y == x.IdHomeroom) &&
                                                        idStudentParamList.Any(y => y == x.IdStudent) &&
                                                        semesterParamList.Any(y => y == x.Semester))
                                            .ToList();

                foreach (var param in paramList)
                {
                    // New TrBLPGroupStudent
                    if (string.IsNullOrEmpty(param.IdBLPGroupStudent))
                    {
                        var homeroomStudent = getHomeroomStudents
                                                .Where(x => x.IdHomeroom == param.IdHomeroom &&
                                                            x.IdStudent == param.IdStudent &&
                                                            x.Semester == param.Semester)
                                                .FirstOrDefault();

                        if (homeroomStudent == null)
                            throw new BadRequestException($"Error! No Homeroom Student found for student with IdStudent: {param.IdStudent}");

                        // check if BLP Group student is already exists
                        var getBLPGroup = getBLPGroupStudents
                                            .Where(x => x.IdAcademicYear == param.IdAcademicYear &&
                                                        x.Semester == param.Semester &&
                                                        x.HomeroomStudent.IdHomeroom == param.IdHomeroom &&
                                                        x.IdStudent == param.IdStudent)
                                            .FirstOrDefault();

                        if (getBLPGroup != null)
                            throw new BadRequestException($"Error! BLP Group is already exists for student with IdStudent: {param.IdStudent}");

                        var newTrBLPGroupStudent = new TrBLPGroupStudent()
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = param.IdAcademicYear,
                            Semester = param.Semester,
                            IdStudent = param.IdStudent,
                            IdBLPStatus = string.IsNullOrEmpty(param.IdBLPStatus) ? null : param.IdBLPStatus,
                            IdBLPGroup = string.IsNullOrEmpty(param.IdBLPGroup) ? null : param.IdBLPGroup,
                            IdHomeroomStudent = homeroomStudent.Id,
                            HardCopySubmissionDate = param.HardcopySubmissionDate
                        };

                        _dbContext.Entity<TrBLPGroupStudent>().Add(newTrBLPGroupStudent);
                    }

                    // Update TrBLPGroupStudent
                    else
                    {
                        var getBLPGroup = getBLPGroupStudents
                                            .Where(x => x.Id == param.IdBLPGroupStudent)
                                            .FirstOrDefault();

                        if (getBLPGroup == null)
                            throw new BadRequestException($"Error! No BLP Group found for student with IdStudent: {param.IdStudent}");

                        getBLPGroup.IdBLPStatus = string.IsNullOrEmpty(param.IdBLPStatus) ? getBLPGroup.IdBLPStatus : param.IdBLPStatus;
                        getBLPGroup.IdBLPGroup = string.IsNullOrEmpty(param.IdBLPGroup) ? getBLPGroup.IdBLPGroup : param.IdBLPGroup;
                        getBLPGroup.HardCopySubmissionDate = param.HardcopySubmissionDate;

                        _dbContext.Entity<TrBLPGroupStudent>().Update(getBLPGroup);
                    }
                }

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
                _transaction.Dispose();
            }
        }
    }
}

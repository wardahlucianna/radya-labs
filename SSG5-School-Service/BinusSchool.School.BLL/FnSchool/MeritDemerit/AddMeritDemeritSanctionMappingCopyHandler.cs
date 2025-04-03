using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.MeritDemerit.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.MeritDemerit
{
    public class AddMeritDemeritSanctionMappingCopyHandler : FunctionsHttpSingleHandler
    {

        private readonly ISchoolDbContext _dbContext;

        public AddMeritDemeritSanctionMappingCopyHandler(ISchoolDbContext MeritDemetitDbContext)
        {
            _dbContext = MeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddMeritDemeritSanctionMappingCopyRequest, AddMeritDemeritSanctionMappingCopyValidator>();
            List<DemeritSanctionMapping> ExsisSunctionCopy = new List<DemeritSanctionMapping>();

            var SunctionMappingCopyTo = await _dbContext.Entity<MsSanctionMapping>()
                .Include(e=>e.AcademicYear)
              .Where(e => e.IdAcademicYear == body.IdAcademicYearCopyTo)
             .ToListAsync(CancellationToken);

            var SunctionMapping = await _dbContext.Entity<MsSanctionMapping>()
                .Include(e => e.AcademicYear)
                .Include(e => e.SanctionMappingAttentionBies).ThenInclude(e => e.TeacherPosition)
                .Include(e => e.SanctionMappingAttentionBies).ThenInclude(e => e.Role)
              .Where(e => body.IdSunctionMapping.Contains(e.Id))
             .ToListAsync(CancellationToken);

            var CountSuccses = 0;
            foreach (var ItemBodySunctionMapping in SunctionMapping)
            {
                var exsisMappingCopyTo = SunctionMappingCopyTo.Any(e => e.SanctionName == ItemBodySunctionMapping.SanctionName);
                var SunctionMappingById = SunctionMapping.SingleOrDefault(e => e.Id == ItemBodySunctionMapping.Id);

                if (!exsisMappingCopyTo)
                {
                    var newSanctionMapping = new MsSanctionMapping
                    {
                        Id = Guid.NewGuid().ToString(),
                        SanctionName = SunctionMappingById.SanctionName,
                        IdAcademicYear = body.IdAcademicYearCopyTo,
                        Min = SunctionMappingById.Min,
                        Max = SunctionMappingById.Max,
                    };
                    _dbContext.Entity<MsSanctionMapping>().Add(newSanctionMapping);

                    foreach (var itemSuctionAttantionBy in SunctionMappingById.SanctionMappingAttentionBies)
                    {
                        var newSanctionMappingAttantionBy = new MsSanctionMappingAttentionBy
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdSanctionMapping = newSanctionMapping.Id,
                            IdRole = itemSuctionAttantionBy.IdRole,
                            IdTeacherPosition = itemSuctionAttantionBy.IdTeacherPosition == null ? null : itemSuctionAttantionBy.IdTeacherPosition,
                        };
                        _dbContext.Entity<MsSanctionMappingAttentionBy>().Add(newSanctionMappingAttantionBy);

                    }

                    CountSuccses++;
                }
                else
                {
                    ExsisSunctionCopy.Add(new DemeritSanctionMapping
                    {
                        AcademicYear = SunctionMappingById.AcademicYear.Description,
                        SunctionName = SunctionMappingById.SanctionName,
                        DemeritMin = SunctionMappingById.Min.ToString(),
                        DemeritMax = SunctionMappingById.Max.ToString(),
                        AttentionBy = GetAttentionBy(SunctionMappingById.SanctionMappingAttentionBies.Select(e => e.TeacherPosition == null ? e.Role.Description : e.TeacherPosition.Description).ToList()),
                    });
                }

               
            }

            AddMeritDemeritSanctionMappingCopyResult Items = new AddMeritDemeritSanctionMappingCopyResult
            {
                CountSucces = CountSuccses.ToString(),
                DemeritSanctionMappingFailed = ExsisSunctionCopy,
            };

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2(Items as object);
        }

        public string GetAttentionBy(List<string> SanctionMappingAttentionBies)
        {
            var AttentionBy = "";

            foreach (var item in SanctionMappingAttentionBies)
            {
                if (item != null)
                    AttentionBy += SanctionMappingAttentionBies.IndexOf(item) + 1 == SanctionMappingAttentionBies.Count() ? item : item + ", ";
            }

            return AttentionBy;
        }
    }
}

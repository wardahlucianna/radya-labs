using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class GetTreatmentAndMedicItemByConditionHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;

        public GetTreatmentAndMedicItemByConditionHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.GetBody<GetTreatmentAndMedicItemByConditionRequest>();

            if (string.IsNullOrEmpty(request.IdSchool))
                throw new BadRequestException($"IdSchool cannot be empty");

            if (request.IdCondition == null || !request.IdCondition.Any())
                throw new BadRequestException($"IdCondition cannot be empty");

            var response = await GetTreatmentAndMedicItemByCondition(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<GetTreatmentAndMedicItemByConditionResponse> GetTreatmentAndMedicItemByCondition(GetTreatmentAndMedicItemByConditionRequest request)
        {
            var response = new GetTreatmentAndMedicItemByConditionResponse();

            var getTreatment = _context.Entity<MsMedicalCondition>()
                .Include(a => a.MappingTreatmentConditions)
                    .ThenInclude(b => b.MedicalTreatment)
                .Where(a => a.IdSchool == request.IdSchool
                    && request.IdCondition.Any(b => b == a.Id))
                .ToList();

            var getMedicItem = _context.Entity<MsMedicalCondition>()
                .Include(a => a.MappingMedicalItemConditions)
                    .ThenInclude(b => b.MedicalItem.MedicalItemType)
                .Include(a => a.MappingMedicalItemConditions)
                    .ThenInclude(b => b.MedicalItem.DosageType)
                .Where(a => a.IdSchool == request.IdSchool
                    && request.IdCondition.Any(b => b == a.Id))
                .ToList();

            var insertTreatment = getMedicItem
                .SelectMany(a => a.MappingTreatmentConditions.Select(b => new GetTreatmentAndMedicItemByConditionResponse_Treatment
                {
                    Id = b.MedicalTreatment.Id,
                    Description = b.MedicalTreatment.MedicalTreatmentName,
                }))
                .ToList();

            var insertMedicItem = getMedicItem
                .SelectMany(a => a.MappingMedicalItemConditions.Select(b => new GetTreatmentAndMedicItemByConditionResponse_MedicItem
                {
                    Id = b.MedicalItem.Id,
                    Description = b.MedicalItem.MedicalItemName,
                    MedicalItemType = b.MedicalItem.MedicalItemType.MedicalItemTypeName,
                    DosageType = b.MedicalItem.DosageType.DosageTypeName,
                }))
                .ToList();

            var groupedTreatment = insertTreatment
                .GroupBy(x => new { x.Id, x.Description })
                .Select(g => new GetTreatmentAndMedicItemByConditionResponse_Treatment
                {
                    Id = g.Key.Id,
                    Description = g.Key.Description,
                })
                .ToList();

            var groupedMedicItem = insertMedicItem
                .GroupBy(x => new { x.Id, x.Description, x.MedicalItemType, x.DosageType })
                .Select(g => new GetTreatmentAndMedicItemByConditionResponse_MedicItem
                {
                    Id = g.Key.Id,
                    Description = g.Key.Description,
                    MedicalItemType = g.Key.MedicalItemType,
                    DosageType = g.Key.DosageType,
                })
                .ToList();


            response.Treatments = groupedTreatment;
            response.MedicItems = groupedMedicItem;

            return response;
        }
    }
}

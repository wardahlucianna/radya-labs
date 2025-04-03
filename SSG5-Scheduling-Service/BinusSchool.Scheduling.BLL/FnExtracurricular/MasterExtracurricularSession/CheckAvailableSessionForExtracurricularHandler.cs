using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Domain.Extensions;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricularSession;
using BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricularSession.Validator;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricularSession
{
    public class CheckAvailableSessionForExtracurricularHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetMasterExtracurricularRequest.IdAcademicYear) };

        private readonly ISchedulingDbContext _dbContext;

        public CheckAvailableSessionForExtracurricularHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CheckAvailableSessionForExtracurricularRequest, CheckAvailableSessionForExtracurricularValidator>();

            DateTime startDate = body.SessionStartDate;
            DateTime endDate = body.SessionEndDate;

            int days = (endDate - startDate).Days;
            int totalSession = 0;

            foreach (var item in body.Day)
            {

                string myDay = item.Description;
                var myDayDayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), myDay); //convert string to DayOfWeek type

                var allDates = new List<DateTime>();
                for (int i = 0; i <= days; i++)
                {
                    var currDate = startDate.AddDays(i);
                    if (currDate.DayOfWeek == myDayDayOfWeek)
                    {
                        allDates.Add(currDate);
                    }
                }

                totalSession += allDates.Count();
            }

            var retVal = new CheckAvailableSessionForExtracurricularResult {
                TotalSession = totalSession
            };

            return Request.CreateApiResult2(retVal as object);
        }
    }
}

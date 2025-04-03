using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.EmailRecepient;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnMovingStudent.EmailRecepient
{
    public class GetEmailBccAndTosHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetEmailBccAndTosHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEmailBccAndTosRequest>();
            var predicate = PredicateBuilder.Create<MsEmailRecepient>(x => x.Type == param.Type && x.Role.IdSchool == param.IdSchool);

            var getEmailRecepient = await _dbContext.Entity<MsEmailRecepient>().Include(x => x.Role)
                                        .Where(e =>e.Type == param.Type && e.Role.IdSchool == param.IdSchool)
                                        .ToListAsync(CancellationToken);

            var listIdTecaherPosition = getEmailRecepient.Where(e => e.IdTeacherPosition != null).Select(e => e.IdTeacherPosition).ToList();

            var getNonTeachingLoad = await _dbContext.Entity<TrNonTeachingLoad>()
                                        .Include(e => e.MsNonTeachingLoad)
                                        .Where(e => listIdTecaherPosition.Contains(e.MsNonTeachingLoad.IdTeacherPosition))
                                        .ToListAsync(CancellationToken);

            #region Tos
            var getEmailRecepientTos = getEmailRecepient.Where(e => !e.IsCC).ToList();
            var listIdTecaherPositionTos = getEmailRecepientTos.Where(e => e.IdTeacherPosition != null).Select(e => e.IdTeacherPosition).ToList();
            var getNonTeachingLoadTos = getNonTeachingLoad.Where(e => listIdTecaherPositionTos.Contains(e.MsNonTeachingLoad.IdTeacherPosition)).ToList();
            var idUserTos = GetIdUser(getEmailRecepientTos, getNonTeachingLoadTos);
            #endregion

            #region BCC
            var getEmailRecepientBcc = getEmailRecepient.Where(e => e.IsCC).ToList();
            var listIdTecaherPositionBcc = getEmailRecepientBcc.Where(e => e.IdTeacherPosition != null).Select(e => e.IdTeacherPosition).ToList();
            var getNonTeachingLoadBcc = getNonTeachingLoad.Where(e => listIdTecaherPositionBcc.Contains(e.MsNonTeachingLoad.IdTeacherPosition)).ToList();
            var idUserBcc = GetIdUser(getEmailRecepientBcc, getNonTeachingLoadBcc);
            #endregion

            var result = new GetEmailBccAndTosResult
            {
                Tos = idUserTos,
                Bcc = idUserBcc
            };

            return Request.CreateApiResult2(result as object);
        }

        public static List<string> GetIdUser(List<MsEmailRecepient> emailRecepient, List<TrNonTeachingLoad> nonTeachingLoads)
        {
            List<string> listIdUser = new List<string>();

            foreach (var item in emailRecepient)
            {
                if (item.IdTeacherPosition != null)
                {
                    listIdUser.AddRange(nonTeachingLoads.Where(e => e.MsNonTeachingLoad.IdTeacherPosition == item.IdTeacherPosition).Select(e => e.IdUser).ToList());
                }

                if (item.IdBinusian != null)
                {
                    listIdUser.Add(item.IdBinusian);
                }
            }

            return listIdUser;
        }

       
    }
}

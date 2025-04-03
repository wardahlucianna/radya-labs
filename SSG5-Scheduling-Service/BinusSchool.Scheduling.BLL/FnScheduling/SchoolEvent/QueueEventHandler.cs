using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class QueueEventHandler : FunctionsHttpSingleHandler
    {
        private readonly IMachineDateTime _datetime;
        private readonly IServiceProvider _serviceProvider;
        public QueueEventHandler(IMachineDateTime dateTime, IServiceProvider serviceProvider)
        {
            _datetime = dateTime;
            _serviceProvider = serviceProvider;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<QueueEventRequest>();

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var queue = JsonConvert.SerializeObject(param);
                collector.Add(queue);
            }

            return Request.CreateApiResult2();
        }
    }
}

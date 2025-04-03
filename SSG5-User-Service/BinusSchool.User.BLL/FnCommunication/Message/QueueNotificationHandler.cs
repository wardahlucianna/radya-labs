using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.UserDb.Abstractions;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Transactions;
using NPOI.SS.UserModel;
using Microsoft.Azure.Amqp.Framing;
using BinusSchool.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.User.FnCommunication.Message
{
    public class QueueNotificationHandler : FunctionsHttpSingleHandler
    {
        private readonly IMachineDateTime _datetime;
        private readonly IServiceProvider _serviceProvider;
        public QueueNotificationHandler(IMachineDateTime dateTime, IServiceProvider serviceProvider)
        {
            _datetime = dateTime;
            _serviceProvider = serviceProvider;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<QueueMessagesRequest>();

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var queue = JsonConvert.SerializeObject(param);
                collector.Add(queue);
            }

            return Request.CreateApiResult2();
        }
    }
}

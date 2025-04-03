using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BinusSchool.Common.Functions.Abstractions
{
    public interface IFunctionsNotificationHandler
    {
        Task Execute(string idSchool, IEnumerable<string> idUserRecipients, IDictionary<string, object> keyValues, CancellationToken cancellationToken = default);
    }
}

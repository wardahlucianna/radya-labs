using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BinusSchool.Common.Functions.Handler
{
    public static class FunctionSyncUpdater<T> where T : DbContext, IAppDbContext
    {
        public static async Task UpdateDatabaseAsync(JToken syncRefTable,
            T dbContext,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var id = syncRefTable[nameof(SyncRefTable.Id)]?.ToObject<IDictionary<string, string>>() ??
                     new Dictionary<string, string>();
            var action = syncRefTable.Value<int>(nameof(SyncRefTable.Action));
            var jValue = syncRefTable[nameof(SyncRefTable.Value)];

            // define & assign field identity from single or composite PK of entity
            foreach (var pk in id)
                jValue[pk.Key] = pk.Value;

            var entityName = syncRefTable.Value<string>(nameof(SyncRefTable.Table));

            logger.LogInformation("Given entity type: {FullName}", entityName);

            var entityType =
                typeof(T).Assembly.ExportedTypes.FirstOrDefault(x =>
                    x.Name == entityName && typeof(IEntity).IsAssignableFrom(x));
            if (entityType is null)
                return;

            var entityInstance = JsonConvert.DeserializeObject(jValue.ToString(), entityType);
            if (entityInstance is null)
                return;

            var ids = id.Select(x => (object)x.Value).ToArray();
            var pkId = ids.First().ToString();

            logger.LogInformation("Using entity type: {FullName} of {Id}", entityType.FullName, pkId);

            var existingEntry = await dbContext.FindAsync(entityType,
                ids,
                cancellationToken);

            var auditAction = (AuditAction)action;
            if (auditAction == AuditAction.Insert || auditAction == AuditAction.Update)
            {
                if (existingEntry is null)
                {
                    logger.LogInformation("Entity is being added");
                    await dbContext.AddAsync(entityInstance, cancellationToken);
                }
                else
                {
                    logger.LogInformation("Entity is being updated");
                    var destProps = existingEntry.GetType()
                        .GetProperties()
                        .Where(x => x.CanWrite)
                        .Select(x => new { x.Name, x.PropertyType })
                        .ToList();
                    foreach (var jToken in jValue)
                    {
                        var prop = (JProperty)jToken;
                        var propDestination = destProps.Find(x => x.Name == prop.Name);

                        if (propDestination is null)
                            continue;

                        dbContext.Entry(existingEntry).Property(prop.Name).CurrentValue =
                            prop.Value.ToObject(propDestination.PropertyType);
                    }

                    dbContext.Update(existingEntry);
                }
            }

            if (auditAction == AuditAction.Remove && existingEntry is { })
                dbContext.Remove(existingEntry);
        }
    }
}

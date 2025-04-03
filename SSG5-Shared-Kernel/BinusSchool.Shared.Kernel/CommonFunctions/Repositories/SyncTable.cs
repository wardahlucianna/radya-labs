using System;
using Microsoft.Azure.Cosmos;
using Nexus.Base.CosmosDBRepository;

namespace BinusSchool.Common.Functions.Repositories
{
    public class SyncTable : ModelBase
    {
        /// <summary>
        /// Source event hub, like scheduling-attendance.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Domain like, Attendance, User, Scheduling.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Storage name
        /// </summary>
        public string Storage { get; set; }

        /// <summary>
        /// Blob container
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        /// Blob name
        /// </summary>
        public string Filename { get; set; }

        public bool IsFixed { get; set; } = false;

        public DateTime? FixedDt { get; set; }

        public string FixedMessage { get; set; }

        public bool IsFixedBecauseOfExceedRetry { get; set; }

        public int ErrorCount { get; set; } = 0;

        public string Message { get; set; }

        public string InnerMessage { get; set; }

        public string StackTrace { get; set; }

        public bool IsFileDeleted { get; set; }
    }

    public class SyncTableHistory : ModelBase
    {
        public string SyncTableId { get; set; }
        public string Message { get; set; }
        public string InnerMessage { get; set; }
        public string StackTrace { get; set; }
    }

    public static class CollectionOfRepositories
    {
        public class SyncTableRepository : DocumentDBRepository<SyncTable>
        {
            public SyncTableRepository(CosmosClient client) : base(
                databaseId: "Sync",
                cosmosDBClient: client,
                createDatabaseIfNotExist: true)
            {
            }
        }

        public class SyncTableHistoryRepository : DocumentDBRepository<SyncTableHistory>
        {
            public SyncTableHistoryRepository(CosmosClient client) : base(
                databaseId: "Sync",
                cosmosDBClient: client,
                createDatabaseIfNotExist: true)
            {
            }
        }
    }
}

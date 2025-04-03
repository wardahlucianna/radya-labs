using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Helper
{
    public class BookingVenueAndEquipmentBlobHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;

        private const string _containerNameInit = "booking-venue-and-equipment";

        public BookingVenueAndEquipmentBlobHelper(IConfiguration configuration, IMachineDateTime dateTime)
        {
            _configuration = configuration;
            _dateTime = dateTime;
        }

        private string BuildContainerName(string IdBinusian)
        {
            return _containerNameInit + "/" + IdBinusian;
        }

        private static string GetBookingVenueAndEquipmentLink(string fileName, string containerLink)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            string url = containerLink.Replace("?", "/" + fileName + "?");

            return url;
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            try
            {
                var account = CloudStorageAccount.Parse(_configuration.GetConnectionString("Scheduling:AccountStorage"));
                return account;
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Failed to Upload File\n{ex.Message}");
            }
        }

        private string GetContainerSasUri(string idBinusian, int expiryHour, string storePolicyName = null)
        {
            string sasContainerToken;

            CloudBlobContainer container;

            CloudStorageAccount account = GetCloudStorageAccount();
            CloudBlobClient client = account.CreateCloudBlobClient();
            container = client.GetContainerReference(_containerNameInit);

            if (storePolicyName == null)
            {
                SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(expiryHour),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);
            }
            else
            {
                sasContainerToken = container.GetSharedAccessSignature(null, storePolicyName);
            }

            return container.Uri + sasContainerToken;
        }

        private void CreateContainerIfNotExists(string idBinusian)
        {
            string containerName = BuildContainerName(idBinusian);

            var account = GetCloudStorageAccount();
            var client = account.CreateCloudBlobClient();
            var containers = new[] { containerName };

            foreach (var item in containers)
            {
                var container = client.GetContainerReference(item);
                container.CreateIfNotExistsAsync();
            }
        }

        public async Task<IEnumerable<string>> MoveFilesFromSourceContainerAsync(string url, string idBinusian, string fileType)
        {
            // Get source container reference
            CloudStorageAccount sourceAccount = GetCloudStorageAccount();
            CloudBlobClient sourceClient = sourceAccount.CreateCloudBlobClient();
            CloudBlobContainer sourceContainer = sourceClient.GetContainerReference(_containerNameInit);

            // Get destination container reference
            CloudStorageAccount destinationAccount = GetCloudStorageAccount();
            CloudBlobClient destinationClient = destinationAccount.CreateCloudBlobClient();
            CloudBlobContainer destinationContainer = destinationClient.GetContainerReference(_containerNameInit);

            // Get temp file upload
            CloudBlockBlob sourceBlob = sourceContainer.GetBlockBlobReference(url);

            // Check if source exist
            if (!await sourceBlob.ExistsAsync())
            {
                throw new ArgumentException($"The specified blob URL '{sourceBlob.Uri.AbsoluteUri}' does not exist.", nameof(sourceBlob));
            }

            // Set destination blob name and new file format
            string destinationBlobName = idBinusian + "." + fileType;

            // Check if file have same name
            CloudBlockBlob existBlob = destinationContainer.GetBlockBlobReference("layout/" + idBinusian);
            if (await existBlob.ExistsAsync())
                await existBlob.DeleteAsync();

            // Copy file to destination folder
            CloudBlobDirectory destinationDirectory = destinationContainer.GetDirectoryReference("layout");
            CloudBlockBlob destinationBlob = destinationDirectory.GetBlockBlobReference(destinationBlobName);
            await destinationBlob.StartCopyAsync(sourceBlob);

            // Wait for copy operation
            while (true)
            {
                await destinationBlob.FetchAttributesAsync();
                if (destinationBlob.CopyState.Status != CopyStatus.Pending)
                    break;
                await Task.Delay(1000);
            }

            if (destinationBlob.CopyState.Status != CopyStatus.Success)
                throw new Exception($"Failed to copy the file from source blob URL '{sourceBlob.Uri.AbsoluteUri}' to the destination container.");

            // Delete file from source container
            await sourceBlob.DeleteAsync();

            // Add new URL moved file
            var newUrl = new List<string> { destinationBlob.Uri.AbsoluteUri };

            return newUrl;
        }

        public async Task RemoveFileIfExist(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            var acount = GetCloudStorageAccount();
            var client = acount.CreateCloudBlobClient();
            var container = client.GetContainerReference(_containerNameInit);
            var fileFolder = container.GetDirectoryReference("layout");
            var fileBlob = fileFolder.GetBlockBlobReference(fileName);

            await fileBlob.DeleteIfExistsAsync();
        }
    }
}

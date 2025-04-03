namespace BinusSchool.Common.Model
{
    public class MessageInBlob
    {
        public MessageInBlob(string storageName, string container, string name, string executor = "SYSTEM")
        {
            StorageName = storageName;
            BlobContainer = container;
            BlobName = name;
            Executor = executor;
        }
        
        public string StorageName { get; set; }
        public string BlobContainer { get; set; }
        public string BlobName { get; set; }
        public string Executor { get; set; }
        public bool StoredInBlob => !string.IsNullOrEmpty(BlobName);
    }
}
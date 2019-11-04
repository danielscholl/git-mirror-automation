namespace GitMirrorAutomation.Logic.Storage
{
    public class StorageFactory : IStorageFactory
    {
        public IStorageProvider FromConnectionString(string connectionString, string containerName)
        {
            return new AzureBlobStorageProvider(connectionString, containerName);
        }
    }
}

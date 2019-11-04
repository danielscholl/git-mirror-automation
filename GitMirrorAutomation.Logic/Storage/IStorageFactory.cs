namespace GitMirrorAutomation.Logic.Storage
{
    public interface IStorageFactory
    {
        /// <summary>
        /// Given a connection string returns a provider that can access the specific container using the connection string.
        /// Note that no permissions are checked while creating the provider.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        IStorageProvider FromConnectionString(string connectionString, string containerName);
    }
}

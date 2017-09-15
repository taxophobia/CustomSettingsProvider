namespace BWC.Utility.CustomSettingsProvider.DefaultProviders
{
    using System.IO.Abstractions;
    using BWC.Utility.CustomSettingsProvider.Interfaces;

    public class FileSystemProvider : IFileSystemProvider
    {
        private IFileSystem fileSystem;

        public IFileSystem FileSystem
        {
            get
            {
                if (this.fileSystem == null)
                {
                    this.fileSystem = new FileSystem();
                }

                return this.fileSystem;
            }
        }
    }
}

namespace BWC.Utility.CustomSettingsProvider.Interfaces
{
    using System.IO.Abstractions;

    public interface IFileSystemProvider
    {
        IFileSystem FileSystem { get; }
    }
}

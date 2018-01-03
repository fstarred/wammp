
namespace Wammp.Services
{
    public interface IDialogFileService
    {
        string SaveFile();
        string[] OpenMultipleFiles();
        string OpenFile();

    }
}

using System;
using System.IO;
using System.Linq;
using TinyIoC;

namespace Wammp.Services
{
    public class PlaylistService
    {
        public PlaylistService()
        {

        }
        
        public void Save()
        {
            var container = TinyIoCContainer.Current;

            IDialogFileService service = container.Resolve<IDialogFileService>(Naming.ContainerNSR.DLG_SAVE_PLAYLIST);

            string filename = service.SaveFile();

            if (!String.IsNullOrEmpty(filename))
            {
                string extension = Path.GetExtension(filename);

                if (File.Exists(filename))
                    File.Delete(filename);

                if (extension.Equals(".pls", StringComparison.InvariantCultureIgnoreCase))
                    Utils.AudioUtility.SavePLSFile(filename, TracklistProvider.Instance.Tracks.ToArray());
                else if (extension.Equals(".m3u", StringComparison.InvariantCultureIgnoreCase))
                    Utils.AudioUtility.SaveM3UFile(filename, TracklistProvider.Instance.Tracks.ToArray());
            }
        }
    }
}

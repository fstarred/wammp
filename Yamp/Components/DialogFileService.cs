using Microsoft.Win32;
using Yemp.Services;

namespace Yemp.Components
{
    class DialogFileService : IDialogFileService
    {
        public string Title { get; set; }
        public string Filter { get; set; }

        public string[] OpenMultipleFiles()
        {
            string[] filenames = null;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = this.Filter;
            dialog.Title = this.Title;
            if (dialog.ShowDialog() == true)
            {
                filenames = dialog.FileNames;
            }

            return filenames ?? new string[0];
        }

        public string OpenFile()
        {
            string[] filenames = null;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = this.Filter;
            dialog.Title = this.Title;
            if (dialog.ShowDialog() == true)
            {
                filenames = dialog.FileNames;
            }

            string output = null;

            if (filenames != null)
                output = filenames[0];

            return output;
        }

        public string SaveFile()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = this.Filter;
            dialog.Title = this.Title;
            dialog.ShowDialog();

            return dialog.FileName;
        }
    }
}

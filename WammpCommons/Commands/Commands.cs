using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using WammpCommons.View;
using WammpCommons.ViewModel;

namespace WammpCommons.Commands
{
    public class Commands
    {
        //public static ICommand OpenFetchStatusWinCommand { get { return new MvvmFoundation.Wpf.RelayCommand<Window>(OpenFetchStatusWin); } }
        public static ICommand OpenResourceCommand { get { return new MvvmFoundation.Wpf.RelayCommand<string>(OpenResource, CanOpenResource); } }
        public static ICommand SelectFileInExplorerCommand { get { return new MvvmFoundation.Wpf.RelayCommand<string>(SelectFileInExplorer, CanSelectFileInExplorer); } }
        public static ICommand OpenUrlCommand { get { return new MvvmFoundation.Wpf.RelayCommand<Uri>(OpenUrl); } }
        public static ICommand CloseWindowCommand { get { return new MvvmFoundation.Wpf.RelayCommand<Window>(CloseWindow); } }
        public static ICommand CopyTextToClipboardCommand { get { return new MvvmFoundation.Wpf.RelayCommand<object>(CopyTextToClipboard); } }
        public static ICommand ShowMessageCommand { get { return new MvvmFoundation.Wpf.RelayCommand<string>(ShowMessage); } }

        static bool CanSelectFileInExplorer(string file)
        {
            return File.Exists(file);
        }

        static void SelectFileInExplorer(string file)
        {
            Process.Start(new ProcessStartInfo("explorer.exe", "/select, " + file));
        }

        static bool CanOpenResource(string resource)
        {
            return String.IsNullOrEmpty(resource) == false;
        }

        static void OpenResource(string resource)
        {
            System.Diagnostics.Process.Start((new ProcessStartInfo(resource)));
        }

        static void OpenUrl(Uri uri)
        {
            System.Diagnostics.Process.Start((new ProcessStartInfo(uri.AbsoluteUri)));
        }

        static void CloseWindow(Window w)
        {
            if (w != null)
            {
                if (w.Owner != null)
                {
                    w.Owner.IsEnabled = true;
                }
                w.Close();
            }
        }

        static void CopyTextToClipboard(object input)
        {
            Clipboard.SetText(input.ToString());
        }

        static void ShowMessage(string message)
        {
            MessageVM vm = new MessageVM();
            vm.IsConfirm = false;
            vm.Message = message;
            vm.Title = "Message";

            ViewMessage view = new ViewMessage
            {
                DataContext = vm
            };

            view.ShowDialog();
        }

        //static void OpenFetchStatusWin(Window owner)
        //{
        //    View.ViewFetchStatus view = new View.ViewFetchStatus();
        //    view.Owner = owner;
        //    view.Owner.IsEnabled = false;
        //    view.Show();
        //}
        
    }
}

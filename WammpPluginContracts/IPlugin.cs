using System;
using System.Diagnostics;
using System.Net;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;
using WammpCommons.Services;

namespace WammpPluginContracts
{
    public enum VIEW_TYPE { MAIN, SETTINGS, NONE }

    public enum PLAYER_ACTION { PLAY, STOP, PAUSE, RESTART, RESET }

    public class SignalArgs : EventArgs
    {
        public SignalArgs(PLAYER_ACTION action)
        {
            this.Action = action;
        }

        public PLAYER_ACTION Action { get; private set; }
    }

    public interface IPlugin
    {
        //IAppUpdateCheckService UpdateCheckService { get; set; }

        event EventHandler<SignalArgs> SendActionSignal;

        IWebProxy Proxy { get; set; }

        string Message { get; }

        //VIEW_TYPE ViewType { get; }

        bool Init(string libpath, int mixer);

        void RetrieveInfo(string filename, BASS_CHANNELINFO info, TAG_INFO tagInfo);

        void ChannelDispose();
        
        string Name { get; }

        string AuthorName { get; }

        int PrepareStream(int source);

        byte[] Icon();

        object View { get; }

        object SettingsView { get; }

        object ToolBar { get; }

        void TracklistUpdated(string[] filenames);

        void CurrentTrackIndexChanged(int index);

        void Load();

        void Save();

        bool CanLoadSave();

        Version GetLatestVersion();

        Version GetCurrentVersion();

        bool IsVersionUptoDate();
    }
}

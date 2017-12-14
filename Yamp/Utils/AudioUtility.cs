using Nini.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Un4seen.Bass;
using Yemp.ViewModel;

namespace Yemp.Utils
{
    static class AudioUtility
    {
        public static BASS_DEVICEINFO[] GetDevices()
        {
            return Bass.BASS_GetDeviceInfos();
        }

        public static void SetDevice(int device)
        {            
            Bass.BASS_SetDevice(device);            
        }

        public static bool IsPlaylist(string filename)
        {
            string ext = Path.GetExtension(filename);

            return (ext.Equals(".pls", StringComparison.InvariantCultureIgnoreCase)
                || ext.Equals(".m3u", StringComparison.InvariantCultureIgnoreCase)
                || ext.Equals(".impls", StringComparison.InvariantCultureIgnoreCase)
                );
        }

        public static IList<string> GetTracksFromPlaylistFile(string filename, Dictionary<int, string> pluginsLoaded)
        {
            string ext = Path.GetExtension(filename);

            IList<string> tracks = null;

            if (ext.Equals(".pls", StringComparison.InvariantCultureIgnoreCase))
                tracks = GetTracksFromPLSFile(filename, pluginsLoaded);
            else if (ext.Equals(".m3u", StringComparison.InvariantCultureIgnoreCase))
                tracks = GetTracksFromM3UFile(filename, pluginsLoaded);
            else if (ext.Equals(".impls", StringComparison.InvariantCultureIgnoreCase))
                tracks = GetTracksFromIMPLSFile(filename, pluginsLoaded);

            return tracks;
        }


        static IList<string> GetTracksFromPLSFile(string filename, Dictionary<int, string> pluginsLoaded)
        {
            const string section = "playlist";

            IList<string> tracks = new List<string>();

            try
            {
                IConfigSource source = new IniConfigSource(filename);
                ((IniConfigSource)source).CaseSensitive = false;

                int totaltracks = source.Configs[section].GetInt("NumberOfEntries");

                for (int i = 1; i <= totaltracks; i++)
                {
                    string location = source.Configs[section].GetString(String.Format("File{0}", i), filename);

                    if (location.StartsWith("http") || Un4seen.Bass.Utils.BASSAddOnIsFileSupported(pluginsLoaded, location))
                    { 
                        tracks.Add(location);
                    }

                    //Track track = Track.GetTrack(location, false);

                    //tracks.Add(Track.GetTrack(location, false));

                }

                //this.TrackList.Tracks.AddRange(tracks);

            }
            catch (Exception)
            {

            }

            return tracks;
        }

        static IList<string> GetTracksFromIMPLSFile(string filename, Dictionary<int, string> pluginsLoaded)
        {
            const string section = "ImpulseMediaPlaylist";

            IList<string> tracks = new List<string>();

            try
            {
                IConfigSource source = new IniConfigSource(filename);
                ((IniConfigSource)source).CaseSensitive = false;

                //int totaltracks = int.Parse(IniFileHandler.IniReadValue(section, "totalentries", filename));

                int totaltracks = source.Configs[section].GetInt("totalentries");

                for (int i = 0; i < totaltracks; i++)
                {
                    //string location = IniFileHandler.IniReadValue(section, string.Format("file{0}", i), filename);
                    
                    string location = source.Configs[section].GetString(string.Format("file{0}", i));

                    if (location.StartsWith("http") || Un4seen.Bass.Utils.BASSAddOnIsFileSupported(pluginsLoaded, location))
                    {
                        //Track track = Track.GetTrack(location, false);

                        //string tempoIniValue = IniFileHandler.IniReadValue(section, string.Format("tempo{0}", i), filename);
                        //string pitchIniValue = IniFileHandler.IniReadValue(section, string.Format("pitch{0}", i), filename);

                        float tempoIniValue = source.Configs[section].GetFloat(string.Format("tempo{0}", i));
                        float pitchIniValue = source.Configs[section].GetFloat(string.Format("pitch{0}", i));

                        //float tempo = 0;
                        //float pitch = 0;

                        //track.Tempo = tempo;
                        //track.Pitch = pitch;

                        //tracks.Add(track);           

                        tracks.Add(location);
                    }
                }

                //this.TrackList.Tracks.AddRange(tracks);

            }
            catch (Exception)
            {

            }

            return tracks;
        }

        static List<string> GetTracksFromM3UFile(string filename, Dictionary<int, string> pluginsLoaded)
        {
            List<string> tracks = new List<string>();

            try
            {

                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(filename))
                {
                    string line;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Substring(0, 1).Equals("#") == false)
                        {
                            //tracks.Add(Track.GetTrack(line, false));                            
                            if (line.StartsWith("http") || Un4seen.Bass.Utils.BASSAddOnIsFileSupported(pluginsLoaded, line))
                            {
                                tracks.Add(line);
                            }
                        }
                    }
                }
                
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return tracks;
        }

        public static void SavePLSFile(string filename, TrackVM[] tracks)
        {
            const string section = "playlist";

            int index = 0;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true))
            {
                file.WriteLine("[" + section + "]");

                foreach (TrackVM track in tracks)
                {
                    DateTime dt = DateTime.ParseExact(track.Length, "mm:ss", CultureInfo.InvariantCulture);

                    int seconds = (60 * dt.Minute) + dt.Second;

                    index++;
                    file.WriteLine(String.Format("File{0}={1}", index, track.Location));
                    file.WriteLine(String.Format("Title{0}={1}", index, track.Title));
                    file.WriteLine(String.Format("Length{0}={1}", index, seconds));
                }

                file.WriteLine("NumberOfEntries=" + tracks.Count().ToString());
                file.WriteLine("Version=" + "2");
            }                        
        }

        public static void SaveM3UFile(string filename, TrackVM[] tracks)
        {
            const string header = "#EXTM3U";
            const string extraInfo = "#EXTINF";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true))
            {
                file.WriteLine(header);

                foreach (TrackVM track in tracks)
                {
                    DateTime dt = DateTime.ParseExact(track.Length, "mm:ss", CultureInfo.InvariantCulture);

                    int seconds = (60 * dt.Minute) + dt.Second;

                    string artist = track.Artist;
                    string title = track.Title;
                    string location = track.Location;

                    string format = "{0}:{1},{2} - {3}";

                    file.WriteLine(String.Format(format, extraInfo, seconds.ToString(), artist, title));

                    file.WriteLine(location);
                }
            }
        }
    }
}

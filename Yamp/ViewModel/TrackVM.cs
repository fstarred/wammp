using YempCommons.ViewModel;

namespace Yemp.ViewModel
{
    public class TrackVM : ObservableObject
    {
        public TrackVM()
        {

        }

        public TrackVM(string location)
        {
            this.Location = location;
        }

        private string location;

        public string Location
        {
            get { return location; }
            set {
                location = value;
                RaisePropertyChanged(() => Location);
            }
        }

        private string title;

        public string Title
        {
            get { return title; }
            set {
                title = value;
                RaisePropertyChanged(() => Title);
            }
        }


        private string artist;

        public string Artist
        {
            get { return artist; }
            set {
                artist = value;
                RaisePropertyChanged(() => Artist);
            }
        }

        private string album;

        public string Album
        {
            get { return album; }
            set
            {
                album = value;
                RaisePropertyChanged(() => Album);
            }
        }

        private string length;

        public string Length
        {
            get { return length; }
            set
            {
                length = value;
                RaisePropertyChanged(() => Length);
            }
        }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set {
                isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }        
    }
}

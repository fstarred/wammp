using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yemp.Services
{
    class ResourceProvider : IResourceProvider
    {
        public Uri Apphomepage
        {
            get
            {
                return (Uri)App.Current.Resources["Apphomepage"];
            }
            set => throw new NotImplementedException();
        }

        public Uri VersionCheckUri {
            get
            {
                return (Uri)App.Current.Resources["VersionCheckUri"];
            }
            set => throw new NotImplementedException();
        }

        public string FacebookAppId
        {
            get
            {
                return App.Current.Resources["FacebookAppId"].ToString();
            }
            set => throw new NotImplementedException();
        }

        public string FacebookSecretKey
        {
            get
            {
                return App.Current.Resources["FacebookSecretKey"].ToString();
            }
            set => throw new NotImplementedException();
        }
        
    }
}

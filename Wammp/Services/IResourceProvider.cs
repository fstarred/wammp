using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wammp.Services
{
    interface IResourceProvider
    {
        Uri Apphomepage { get; set; }
        Uri VersionCheckUri { get; set; }
        string FacebookAppId { get; set; }
        string FacebookSecretKey { get; set; }
    }
}

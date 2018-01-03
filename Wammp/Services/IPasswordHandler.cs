using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wammp.Services
{
    interface IPasswordHandler
    {
        event EventHandler PasswordResetEvent;
    }
}

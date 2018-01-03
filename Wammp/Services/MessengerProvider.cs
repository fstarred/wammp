using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wammp.Services
{
    class MessengerProvider
    {
        private MessengerProvider()
        {

        }

        static Messenger instance;

        public static Messenger Instance
        {
            get
            {
                return instance ?? (instance = new Messenger());
            }
        }
    }
}

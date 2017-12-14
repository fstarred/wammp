﻿using System.Collections.Generic;
using System.Linq;
using Yemp.Components;
using Yemp.Model;
using YempPluginContracts;

namespace Yemp.Services
{
    public class PluginsHandler
    {
        private PluginsHandler()
        {

        }

        PluginLoader<IPlugin> pluginLoader;

        static PluginsHandler instance;

        public void Load(string path)
        {
            pluginLoader = new PluginLoader<IPlugin>(path);
        }

        public void Dispose()
        {
            pluginLoader.Dispose();
        }

        public IEnumerable<IPlugin> Plugins
        {
            get
            {
                return pluginLoader.Plugins;
            }
        }

        public static PluginsHandler Instance
        {
            get
            {
                return instance ?? (instance = new PluginsHandler());
            }
        }
    }
}

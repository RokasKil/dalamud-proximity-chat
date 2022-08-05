using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace SamplePlugin
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public List<XivChatType> Channels { get; set; } = new List<XivChatType>();
        public double Distance { get; set; } = 20;

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}

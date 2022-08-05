using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proximity_chat
{
    public class PluginServices
    {
        [PluginService] public static ChatGui ChatGui { get; set; } = null!;
        [PluginService] public static ObjectTable ObjectTable { get; set; } = null!;
        [PluginService] public static ClientState ClientState { get; set; } = null!;

        public static void Initialize(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<PluginServices>();
        }
    }
}

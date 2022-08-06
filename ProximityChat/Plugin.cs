using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Proximity_chat;
using Proximity_chat.Chat;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace SamplePlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Proximity chat";

        private const string commandName = "/proximitychat";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            PluginServices.Initialize(pluginInterface);

            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            this.PluginUi = new PluginUI(this.Configuration);

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens Proximity Chat configuration"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            PluginServices.ChatGui.ChatMessage += OnChatMessage;
        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            this.CommandManager.RemoveHandler(commandName);

            PluginServices.ChatGui.ChatMessage -= OnChatMessage;
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            this.PluginUi.SettingsVisible = true;
        }

        private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (Configuration.Channels.Contains(type))
            {
                double distance = double.NaN;
                if (ChatUtils.IsSelfMessage(sender))
                {
                    PluginLog.Debug($"(SELF)[{type}] {sender}: {message}");
                }
                else
                {
                    StringMatch? match = ChatUtils.GetStringMatch(sender);
                    if (match?.GameObject is PlayerCharacter playerCharacter && PluginServices.ClientState.LocalPlayer != null)
                    {
                        distance = Vector3.Distance(playerCharacter.Position, PluginServices.ClientState.LocalPlayer.Position);
                        if (distance > Configuration.Distance)
                        {
                            isHandled = true;
                            PluginLog.Debug("Message hidden");
                        }
                    }
                    else
                    {
                        isHandled = true;
                    }
                    PluginLog.Debug($"[{(isHandled ? "HIDDEN" : "SHOWN")}][{distance} away][{type}] {sender}: {message}");
                }
            }
        }
        private void DrawUI()
        {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }
    }
}

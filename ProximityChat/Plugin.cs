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

        private const string commandName = "/pmycommand";

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

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            this.PluginUi = new PluginUI(this.Configuration, goatImage);

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            PluginServices.ChatGui.ChatMessage += Chat_ChatMessage;
        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            this.CommandManager.RemoveHandler(commandName);

            PluginServices.ChatGui.ChatMessage -= Chat_ChatMessage;
        }

        private void OnCommand(string command, string args)
        {
            PluginLog.Log("Command ran");
            // in response to the slash command, just display our main ui
            this.PluginUi.Visible = true;
        }

        private class StringMatch
        {
            /// <summary>
            /// The string that the match was found in.
            /// </summary>
            public SeString SeString { get; init; }

            /// <summary>
            /// The matching text payload.
            /// </summary>
            public TextPayload? TextPayload { get; init; }

            /// <summary>
            /// The matching game object if one exists
            /// </summary>
            public GameObject? GameObject { get; init; }

            /// <summary>
            /// A matching player payload if one exists.
            /// </summary>
            public PlayerPayload? PlayerPayload { get; init; }

            public Payload? PreferredPayload
            {
                get
                {
                    if (TextPayload != null)
                    {
                        return TextPayload;
                    }

                    return PlayerPayload;
                }
            }

            public StringMatch(SeString seString)
            {
                SeString = seString;
            }

            /// <summary>
            /// Gets the matches text.
            /// </summary>
            /// <returns>The match text.</returns>
            public string GetMatchText()
            {
                if (GameObject != null)
                {
                    return GameObject.Name.TextValue;
                }

                if (TextPayload != null)
                {
                    return TextPayload.Text;
                }

                if (PlayerPayload != null)
                {
                    return PlayerPayload.PlayerName;
                }

                return SeString.TextValue;
            }
        }


        private StringMatch? GetStringMatch(SeString seString)
        {

            for (int payloadIndex = 0; payloadIndex < seString.Payloads.Count; ++payloadIndex)
            {
                var payload = seString.Payloads[payloadIndex];
                if (payload is PlayerPayload playerPayload)
                {
                    var gameObject = PluginServices.ObjectTable.FirstOrDefault(gameObject => gameObject.Name.TextValue == playerPayload.PlayerName);

                    TextPayload? textPayload = null;

                    // The next payload MUST be a text payload
                    if (payloadIndex + 1 < seString.Payloads.Count)
                    {
                        textPayload = seString.Payloads[payloadIndex + 1] as TextPayload;

                        // Don't handle the text payload twice
                        payloadIndex++;
                    }

                    var stringMatch = new StringMatch(seString)
                    {
                        GameObject = gameObject,
                        PlayerPayload = playerPayload,
                        TextPayload = textPayload
                    };
                    return stringMatch;
                }
            }
            return null;
        }

        public bool IsSelfMessage(SeString seString)
        {
            if (PluginServices.ClientState.LocalPlayer != null)
            {
                foreach (var payload in seString.Payloads.ToArray())
                {
                    if (payload is not TextPayload textPayload)
                    {
                        continue;
                    }
                    var playerName = PluginServices.ClientState.LocalPlayer.Name.TextValue;
                    if (textPayload.Text == playerName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void Chat_ChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (Configuration.Channels.Contains(type))
            {
                double distance = double.NaN;
                if (IsSelfMessage(sender))
                {
                    PluginLog.Log($"(SELF)[{type}] {sender}: {message}");
                }
                else
                {
                    StringMatch? match = GetStringMatch(sender);
                    if (match?.GameObject is PlayerCharacter playerCharacter && PluginServices.ClientState.LocalPlayer != null)
                    {
                        distance = Vector3.Distance(playerCharacter.Position, PluginServices.ClientState.LocalPlayer.Position);
                        if (distance > Configuration.Distance)
                        {
                            isHandled = true;
                            PluginLog.Log("Message hidden");
                        }
                    }
                    else
                    {
                        isHandled = true;
                    }
                    PluginLog.Log($"[{(isHandled ? "HIDDEN" : "SHOWN")}][{distance} away][{type}] {sender}: {message}");
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

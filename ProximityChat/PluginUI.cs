using Dalamud.Game.Text;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SamplePlugin
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private readonly Configuration configuration;
        private static readonly List<XivChatType> possibleChannels = new List<XivChatType>() { XivChatType.Ls1, XivChatType.Ls2, XivChatType.Ls3, XivChatType.Ls4,
                    XivChatType.Ls5, XivChatType.Ls6, XivChatType.Ls7, XivChatType.Ls8, XivChatType.CrossLinkShell1,
                    XivChatType.CrossLinkShell2, XivChatType.CrossLinkShell3, XivChatType.CrossLinkShell4,
                    XivChatType.CrossLinkShell5, XivChatType.CrossLinkShell6, XivChatType.CrossLinkShell7,
                    XivChatType.CrossLinkShell8, XivChatType.FreeCompany, XivChatType.Party, XivChatType.CrossParty,
                    XivChatType.Alliance };

    private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawSettingsWindow();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(300, 350), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Proximity Chat configuration", ref this.settingsVisible,
                  ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                var distance = configuration.Distance;

                //partially stolen from https://github.com/Haplo064/ChatBubbles
                if (ImGui.InputDouble("Yalm distance", ref distance))
                {
                    PluginLog.Debug($"Distance set to {distance}");
                    configuration.Distance = distance;
                    this.configuration.Save();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Anything over this distance in Yalms will not be shown\nDefault say chat range is 20");
                }
                ImGui.Columns(2);
                foreach (var e in possibleChannels)
                {
                    var enabled = configuration.Channels.Contains(e);
                    if (ImGui.Checkbox($"{e}", ref enabled))
                    {
                        if (enabled)
                        {
                            PluginLog.Debug($"Channel {e} enabled");
                            configuration.Channels.Add(e);
                        }
                        else
                        {
                            PluginLog.Debug($"Channel {e} disabled");
                            configuration.Channels.Remove(e);
                        }
                        this.configuration.Save();
                    }
                    ImGui.NextColumn();
                }

                ImGui.Columns(1);

            }
            ImGui.End();
        }
    }
}

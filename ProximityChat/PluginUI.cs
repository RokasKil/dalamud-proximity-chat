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
        private Configuration configuration;

        private ImGuiScene.TextureWrap goatImage;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        // passing in the image here just for simplicity
        public PluginUI(Configuration configuration, ImGuiScene.TextureWrap goatImage)
        {
            this.configuration = configuration;
            this.goatImage = goatImage;
        }

        public void Dispose()
        {
            this.goatImage.Dispose();
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
            DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("My Amazing Window", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {

                if (ImGui.Button("Show Settings"))
                {
                    SettingsVisible = true;
                }

                ImGui.Spacing();

                ImGui.Text("Have a goat:");
                ImGui.Indent(55);
                ImGui.Image(this.goatImage.ImGuiHandle, new Vector2(this.goatImage.Width, this.goatImage.Height));
                ImGui.Unindent(55);
            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(300, 400), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Proximity chat config", ref this.settingsVisible,
                  ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                var distance = configuration.Distance;
                if (ImGui.InputDouble("Yalm distance", ref distance))
                {
                    PluginLog.Debug($"Distance set to {distance}");
                    configuration.Distance = distance; //might need some conversion here idk?
                    this.configuration.Save();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Anything over this distance in Yalms will not be shown");
                }
                ImGui.Columns(2);

                foreach (var e in new List<XivChatType>() { XivChatType.Ls1, XivChatType.Ls2, XivChatType.Ls3, XivChatType.Ls4,
                    XivChatType.Ls5, XivChatType.Ls6, XivChatType.Ls7, XivChatType.Ls8, XivChatType.CrossLinkShell1,
                    XivChatType.CrossLinkShell2, XivChatType.CrossLinkShell3, XivChatType.CrossLinkShell4,
                    XivChatType.CrossLinkShell5, XivChatType.CrossLinkShell6, XivChatType.CrossLinkShell7,
                    XivChatType.CrossLinkShell8, XivChatType.FreeCompany, XivChatType.Party, XivChatType.CrossParty,
                    XivChatType.Alliance })
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

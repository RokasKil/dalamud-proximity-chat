using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Proximity_chat.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proximity_chat
{
    public class ChatUtils
    {
        public static StringMatch? GetStringMatch(SeString senderSeString)
        {

            for (int payloadIndex = 0; payloadIndex < senderSeString.Payloads.Count; ++payloadIndex)
            {
                var payload = senderSeString.Payloads[payloadIndex];
                if (payload is PlayerPayload playerPayload)
                {
                    var gameObject = PluginServices.ObjectTable.FirstOrDefault(gameObject => gameObject.Name.TextValue == playerPayload.PlayerName);

                    TextPayload? textPayload = null;

                    // The next payload MUST be a text payload
                    if (payloadIndex + 1 < senderSeString.Payloads.Count)
                    {
                        textPayload = senderSeString.Payloads[payloadIndex + 1] as TextPayload;

                        // Don't handle the text payload twice
                        payloadIndex++;
                    }

                    var stringMatch = new StringMatch(senderSeString)
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

        public static bool IsSelfMessage(SeString senderSeString)
        {
            if (PluginServices.ClientState.LocalPlayer != null)
            {
                foreach (var payload in senderSeString.Payloads.ToArray())
                {
                    if (payload is not TextPayload textPayload)
                    {
                        continue;
                    }
                    if (textPayload.Text != null && textPayload.Text.Contains(PluginServices.ClientState.LocalPlayer.Name.TextValue))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

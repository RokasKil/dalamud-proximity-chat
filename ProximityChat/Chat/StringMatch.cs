using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proximity_chat.Chat
{
    //Stolen from https://github.com/Pilzinsel64/PlayerTags
    public class StringMatch
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


}

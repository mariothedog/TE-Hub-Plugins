using System.Globalization;
using TShockAPI;

namespace TEHub
{
    class Util
    {
        /// <summary>
        /// Returns the TSPlayer that best fits the playerName parameter.
        /// Returns null if a TSPlayer cannot be decided.
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public static TSPlayer GetPlayer(string playerName)
        {
            TSPlayer closestPlayer = null;
            int mostSimilarCharacters = 0;

            foreach (TSPlayer tSPlayer in TShock.Players)
            {
                if (tSPlayer == null)
                {
                    continue;
                }

                int similarCharacters = CountSimilarCharacters(tSPlayer.Name.ToLower(), playerName.ToLower());

                if (similarCharacters > mostSimilarCharacters)
                {
                    mostSimilarCharacters = similarCharacters;

                    closestPlayer = tSPlayer;
                }
            }

            return closestPlayer;
        }

        public static int CountSimilarCharacters(string str1, string str2)
        {
            int similarCharacters = 0;

            for (int i = 0; i < str1.Length; i++)
            {
                if (i > str2.Length - 1)
                {
                    return similarCharacters;
                }

                if (str1[i] == str2[i])
                {
                    similarCharacters++;
                }
            }

            return similarCharacters;
        }
    }
}

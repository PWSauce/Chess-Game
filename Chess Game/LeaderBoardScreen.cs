﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Chess_Game
{
    /// <summary>
    /// LeaderBoardScreen inheritar ifrån Screen klassen och ritar själva leaderboardrutan.
    /// </summary>
    class LeaderBoardScreen : Screen
    {
        Leaderboard leaderboard = Leaderboard.Load();

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(Font, "Best 20 Matches:", new Vector2(Game1.ScreenMiddle.X - 300, Game1.ScreenMiddle.Y - 220), Color.Black);
            int leaderboardCount = (leaderboard.MatchResults.Count < 20) ? leaderboard.MatchResults.Count : 20;
            // Ritar leaderboarden och räknar ut allt som behöver visas.
            for (int i = 0; i < leaderboardCount; i++)
            {
                string result = "";
                // Rundorna ökar när båda spelarna har gjort ett drag, börjar på runda 1.
                result += "Rounds: " + (int)Math.Ceiling(((double)leaderboard.MatchResults[i].Turns + 1) / 2) + "  ";
                switch (leaderboard.MatchResults[i].Winner)
                {
                    case Winner.White:
                        result += "White won";
                        break;
                    case Winner.Black:
                        result += "Black won";
                        break;
                    case Winner.Draw:
                        result += "Draw";
                        break;
                }

                spriteBatch.DrawString(Font, $"{i + 1}: {result}", new Vector2(Game1.ScreenMiddle.X - 300, Game1.ScreenMiddle.Y - 200 + 20 * i), Color.Black);
            }
        }
    }
}

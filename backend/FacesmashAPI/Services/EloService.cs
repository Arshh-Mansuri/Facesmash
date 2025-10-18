using System;

namespace FacesmashAPI.Services
{
    /// <summary>
    /// Service for calculating ELO ratings used in the Facesmash voting system.
    /// ELO is a method for calculating relative skill levels between players.
    /// </summary>
    public static class EloService
    {
        /// <summary>
        /// K-factor determines how much a rating can change after a single match.
        /// Higher K-factor means more volatile ratings. Standard value is 24.
        /// </summary>
        private const int KFactor = 24;

        /// <summary>
        /// Calculates the expected score for a player based on their rating and their opponent's rating.
        /// </summary>
        /// <param name="playerRating">The rating of the player whose expected score we're calculating.</param>
        /// <param name="opponentRating">The rating of the opponent.</param>
        /// <returns>A value between 0 and 1 representing the expected score (probability of winning).</returns>
        public static double CalculateExpectedScore(int playerRating, int opponentRating)
        {
            // ELO formula: Expected Score = 1 / (1 + 10^((OpponentRating - PlayerRating) / 400))
            return 1.0 / (1.0 + Math.Pow(10, (opponentRating - playerRating) / 400.0));
        }

        /// <summary>
        /// Updates a player's rating based on their expected score and actual result.
        /// </summary>
        /// <param name="currentRating">The player's current rating.</param>
        /// <param name="expectedScore">The expected score (probability of winning).</param>
        /// <param name="actualScore">The actual result (1 for win, 0 for loss, 0.5 for draw).</param>
        /// <returns>The player's new rating after the match.</returns>
        public static int UpdateRating(int currentRating, double expectedScore, double actualScore)
        {
            // ELO formula: New Rating = Old Rating + K * (Actual Score - Expected Score)
            return (int)Math.Round(currentRating + KFactor * (actualScore - expectedScore));
        }

        /// <summary>
        /// Calculates the rating change for both players after a match result.
        /// </summary>
        /// <param name="winnerRating">The rating of the winning player.</param>
        /// <param name="loserRating">The rating of the losing player.</param>
        /// <returns>A tuple containing (winnerNewRating, loserNewRating).</returns>
        public static (int winnerNewRating, int loserNewRating) CalculateMatchResult(int winnerRating, int loserRating)
        {
            double winnerExpected = CalculateExpectedScore(winnerRating, loserRating);
            double loserExpected = CalculateExpectedScore(loserRating, winnerRating);

            int winnerNewRating = UpdateRating(winnerRating, winnerExpected, 1.0); // Winner gets 1 point
            int loserNewRating = UpdateRating(loserRating, loserExpected, 0.0);   // Loser gets 0 points

            return (winnerNewRating, loserNewRating);
        }
    }
}

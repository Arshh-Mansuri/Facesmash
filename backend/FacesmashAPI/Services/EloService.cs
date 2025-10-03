using System;

namespace FacesmashAPI.Services
{
    public class EloService
    {
        private const int K = 24; // can tweak later

        public static double GetExpectation(int ratingA, int ratingB)
        {
            return 1.0 / (1.0 + Math.Pow(10, (ratingB - ratingA) / 400.0));
        }

        public static int UpdateRating(int currentRating, double expected, double actual)
        {
            return (int)Math.Round(currentRating + K * (actual - expected));
        }
    }
}

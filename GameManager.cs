using System;

namespace CatchTheSquare
{
    public class GameManager
    {
        private int _score;
        private int _timeElapsed;

        public int Score => _score;
        public int TimeElapsed => _timeElapsed;

        public void AddScore(int points)
        {
            if (points < 0) throw new ArgumentOutOfRangeException(nameof(points));
            _score += points;
        }

        public void Tick() => _timeElapsed++;

        public void ResetGame()
        {
            _score = 0;
            _timeElapsed = 0;
        }


        public bool CheckCollision(double playerX, double playerY,
                                   double targetX, double targetY,
                                   double size)
        {
            return playerX < targetX + size &&
                   playerX + size > targetX &&
                   playerY < targetY + size &&
                   playerY + size > targetY;
        }
    }
}
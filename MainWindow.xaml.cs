using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CatchTheSquare
{
    public partial class MainWindow : Window
    {
        private const double SquareSize = 40;
        private const double Step = 15;

        private readonly GameManager _game = new GameManager();
        private readonly DispatcherTimer _timer;

        private bool _isGameRunning;
        private bool _isPaused;
        private bool _hasStarted;   

        private readonly Random _rnd = new Random();

        private readonly System.Collections.Generic.HashSet<Key> _pressedKeys = new();

        public MainWindow()
        {
            InitializeComponent();
            Focusable = true;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _timer.Tick += Timer_Tick;

            Loaded += (_, _) => Keyboard.Focus(this);
        }

        // Таймер 
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!_isGameRunning || _isPaused) return;

            // Движение игрока
            double px = Canvas.GetLeft(PlayerRect);
            double py = Canvas.GetTop(PlayerRect);
            if (double.IsNaN(px)) px = 0;
            if (double.IsNaN(py)) py = 0;

            if (_pressedKeys.Contains(Key.Left) || _pressedKeys.Contains(Key.A))
                px = Math.Max(0, px - Step);
            if (_pressedKeys.Contains(Key.Right) || _pressedKeys.Contains(Key.D))
                px = Math.Min(GameCanvas.ActualWidth - SquareSize, px + Step);
            if (_pressedKeys.Contains(Key.Up) || _pressedKeys.Contains(Key.W))
                py = Math.Max(0, py - Step);
            if (_pressedKeys.Contains(Key.Down) || _pressedKeys.Contains(Key.S))
                py = Math.Min(GameCanvas.ActualHeight - SquareSize, py + Step);

            Canvas.SetLeft(PlayerRect, px);
            Canvas.SetTop(PlayerRect, py);

            // Проверка столкновения
            double tx = Canvas.GetLeft(TargetRect);
            double ty = Canvas.GetTop(TargetRect);

            if (_game.CheckCollision(px, py, tx, ty, SquareSize))
            {
                _game.AddScore(1);
                MoveTargetToRandomPosition();
                UpdateStats();
            }


            _tickCounter++;
            if (_tickCounter % 33 == 0)
            {
                _game.Tick();
                UpdateStats();
            }
        }

        private int _tickCounter;

        // Клавиатура
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Left or Key.Right or Key.Up or Key.Down
                      or Key.W or Key.A or Key.S or Key.D)
            {
                _pressedKeys.Add(e.Key);
                e.Handled = true;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (_pressedKeys.Remove(e.Key))
                e.Handled = true;
        }

        // Кнопки 
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _game.ResetGame();
                _hasStarted = true;
                _isPaused = false;
                _isGameRunning = true;
                _tickCounter = 0;

                PlayerRect.Visibility = Visibility.Visible;
                TargetRect.Visibility = Visibility.Visible;

                Canvas.SetLeft(PlayerRect, GameCanvas.ActualWidth / 2);
                Canvas.SetTop(PlayerRect, GameCanvas.ActualHeight / 2);
                MoveTargetToRandomPosition();

                UpdateStats();
                _timer.Start();

                MessageBox.Show($"Игра началась. Текущий счёт: {_game.Score}",
                                "Старт", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (!_isGameRunning)
            {
                MessageBox.Show("Игра ещё не запущена.",
                                "Пауза", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isPaused = true;
            MessageBox.Show($"Игра приостановлена. Текущий счёт: {_game.Score}",
                            "Пауза", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnResume_Click(object sender, RoutedEventArgs e)
        {

            if (!_hasStarted || !_isGameRunning)
            {
                MessageBox.Show("Нельзя продолжить: игра не была начата.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_isPaused)
            {
                MessageBox.Show("Игра уже идёт.",
                                "Продолжение", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _isPaused = false;
            MessageBox.Show($"Игра продолжена. Текущий счёт: {_game.Score}",
                            "Продолжение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _isGameRunning = false;
            _isPaused = false;
            _hasStarted = false;
            _tickCounter = 0;
            _game.ResetGame();
            _pressedKeys.Clear();

            GameCanvas.Children.Remove(PlayerRect);
            GameCanvas.Children.Remove(TargetRect);
            PlayerRect.Visibility = Visibility.Collapsed;
            TargetRect.Visibility = Visibility.Collapsed;

            UpdateStats();

            MessageBox.Show($"Игра сброшена. Текущий счёт: {_game.Score}",
                            "Сброс", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnRules_Click(object sender, RoutedEventArgs e)
        {
            var rules = new RulesWindow { Owner = this };
            rules.ShowDialog();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void MoveTargetToRandomPosition()
        {
            double maxX = Math.Max(0, GameCanvas.ActualWidth - SquareSize);
            double maxY = Math.Max(0, GameCanvas.ActualHeight - SquareSize);

            Canvas.SetLeft(TargetRect, _rnd.NextDouble() * maxX);
            Canvas.SetTop(TargetRect, _rnd.NextDouble() * maxY);
        }

        private void UpdateStats()
        {
            ScoreText.Text = $"Счёт: {_game.Score}";
            TimeText.Text = $"Время: {_game.TimeElapsed}";
        }
    }
}
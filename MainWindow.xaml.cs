using System;
using System.Windows;
using System.Windows.Threading;
using SudokuProject.Models;

namespace SudokuProject
{
    /// <summary>
    /// Main application window holding the menu, timer, and the board control.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        private SudokuEngine engine;
        private DispatcherTimer timer;
        private int secondsElapsed;
        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();
            engine = new SudokuEngine();
            BoardControl.Initialize(engine);

            // Initialize game timer
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += Timer_Tick;

            StartNewGame(40); // Default to easy (40 clues) on startup
        }
        #endregion

        #region Core Methods
        /// <summary>
        /// Prepares and starts a new game with a specified number of initial clues.
        /// </summary>
        private void StartNewGame(int clues)
        {
            engine.GeneratePuzzle(clues);
            BoardControl.RefreshUI();
            secondsElapsed = 0;
            timer.Start();
        }
        #endregion

        #region Timer Events
        private void Timer_Tick(object? sender, EventArgs e)
        {
            secondsElapsed++;
            TimeSpan time = TimeSpan.FromSeconds(secondsElapsed);
            TimerText.Text = $"Time: {time.ToString(@"mm\:ss")}";
        }
        #endregion

        #region Menu Event Handlers
        private void MenuNewEasy_Click(object sender, RoutedEventArgs e) => StartNewGame(40);

        private void MenuNewHard_Click(object sender, RoutedEventArgs e) => StartNewGame(25);

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                engine.SaveToXml("sudoku_save.xml");
                MessageBox.Show("Game successfully saved to sudoku_save.xml", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex)
            {
                MessageBox.Show($"Failed to save game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                engine.LoadFromXml("sudoku_save.xml");
                BoardControl.RefreshUI();
                MessageBox.Show("Game Loaded Successfully!", "Load", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch
            {
                MessageBox.Show("No valid save file found or file is corrupted.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MenuUndo_Click(object sender, RoutedEventArgs e)
        {
            if (engine.UndoStack.Count > 0)
            {
                var move = engine.UndoStack.Pop();
                engine.Grid[move.Row, move.Col] = move.OldValue;
                engine.RedoStack.Push(move);
                BoardControl.RefreshUI();
            }
        }

        private void MenuRedo_Click(object sender, RoutedEventArgs e)
        {
            if (engine.RedoStack.Count > 0)
            {
                var move = engine.RedoStack.Pop();
                engine.Grid[move.Row, move.Col] = move.NewValue;
                engine.UndoStack.Push(move);
                BoardControl.RefreshUI();
            }
        }

        private void MenuSolve_Click(object sender, RoutedEventArgs e)
        {
            Array.Copy(engine.SolutionGrid, engine.Grid, engine.Grid.Length);
            BoardControl.RefreshUI();
            BoardControl.LockBoard();
            timer.Stop();
        }

        private void MenuCheck_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(engine.IsSolved() ? "Puzzle is solved correctly!" : "Puzzle is incomplete or incorrect.", "Status");
        }

        private void MenuCheckMistakes_Click(object sender, RoutedEventArgs e)
        {
            int mistakes = 0;

            for (int r = 0; r < engine.BoardSize; r++)
            {
                for (int c = 0; c < engine.BoardSize; c++)
                {
                    if (engine.Grid[r, c] != 0 && engine.Grid[r, c] != engine.SolutionGrid[r, c])
                    {
                        mistakes++;
                    }
                }
            }

            if (mistakes == 0)
            {
                MessageBox.Show("Looking good! You are on the right track with no mistakes.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
            } else
            {
                MessageBox.Show($"Careful! You have {mistakes} incorrect number(s) on the board.", "Hint", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion
    }
}
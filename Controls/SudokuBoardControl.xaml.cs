using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SudokuProject.Models;

namespace SudokuProject.Controls
{
    /// <summary>
    /// UserControl responsible for visually rendering the Sudoku grid and handling cell input.
    /// </summary>
    public partial class SudokuBoardControl : UserControl
    {
        #region Fields
        private TextBox[,] textBoxes = null!;
        private SudokuEngine engine = null!;
        #endregion

        #region Constructors
        public SudokuBoardControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Sets up the grid UI dynamically based on the engine's board size.
        /// </summary>
        public void Initialize(SudokuEngine engine)
        {
            this.engine = engine;
            textBoxes = new TextBox[this.engine.BoardSize, this.engine.BoardSize];

            GridContainer.Children.Clear();
            GridContainer.Rows = this.engine.BoardSize;
            GridContainer.Columns = this.engine.BoardSize;

            for (int r = 0; r < this.engine.BoardSize; r++)
            {
                for (int c = 0; c < this.engine.BoardSize; c++)
                {
                    TextBox tb = new TextBox {
                        FontSize = this.engine.BoardSize > 9 ? 16 : 24,
                        TextAlignment = TextAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        MaxLength = this.engine.BoardSize > 9 ? 2 : 1,
                        Tag = new int[] { r, c }, 
                        BorderBrush = Brushes.Gray,

                        // Thicker borders to visually separate the subgrids
                        BorderThickness = new Thickness(
                            c % this.engine.SubgridSize == 0 ? 2 : 0.5,
                            r % this.engine.SubgridSize == 0 ? 2 : 0.5,
                            c == this.engine.BoardSize - 1 ? 2 : 0.5,
                            r == this.engine.BoardSize - 1 ? 2 : 0.5)
                    };

                    tb.GotFocus += (s, ev) => tb.SelectAll();
                    tb.PreviewTextInput += Tb_PreviewTextInput;
                    tb.TextChanged += Tb_TextChanged;

                    textBoxes[r, c] = tb;
                    GridContainer.Children.Add(tb);
                }
            }
            RefreshUI();
        }
        #endregion

        #region UI Updates
        /// <summary>
        /// Prevents further input on all cells.
        /// </summary>
        public void LockBoard()
        {
            for (int r = 0; r < engine.BoardSize; r++)
            {
                for (int c = 0; c < engine.BoardSize; c++)
                {
                    textBoxes[r, c].IsReadOnly = true;
                }
            }
        }

        /// <summary>
        /// Synchronizes the visual TextBoxes with the underlying engine's data grid.
        /// </summary>
        public void RefreshUI()
        {
            for (int r = 0; r < engine.BoardSize; r++)
            {
                for (int c = 0; c < engine.BoardSize; c++)
                {
                    // Temporarily detach the event to avoid triggering false moves
                    textBoxes[r, c].TextChanged -= Tb_TextChanged;

                    int val = engine.Grid[r, c];
                    textBoxes[r, c].Text = val == 0 ? string.Empty : val.ToString();
                    textBoxes[r, c].IsReadOnly = engine.IsFixed[r, c];
                    textBoxes[r, c].Foreground = engine.IsFixed[r, c] ? Brushes.Black : Brushes.Blue;
                    textBoxes[r, c].Background = engine.IsFixed[r, c] ? Brushes.LightGray : Brushes.White;

                    // Reattach the event
                    textBoxes[r, c].TextChanged += Tb_TextChanged;
                }
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Intercepts typing to ensure only valid Sudoku moves are entered.
        /// </summary>
        private void Tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int[] pos = (int[])tb.Tag;

            if (!int.TryParse(e.Text, out int num) || num == 0)
            {
                e.Handled = true;
                return;
            }

            // Temporarily clear the cell to check validity
            int original = engine.Grid[pos[0], pos[1]];
            engine.Grid[pos[0], pos[1]] = 0;

            if (!engine.IsValidMove(pos[0], pos[1], num))
            {
                e.Handled = true; // Reject input if invalid
            }

            // Restore original value, TextChanged will handle the actual update
            engine.Grid[pos[0], pos[1]] = original;
        }

        /// <summary>
        /// Handles the logic when a cell's text successfully changes, recording history for undo/redo.
        /// </summary>
        private void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int[] pos = (int[])tb.Tag;
            int r = pos[0], c = pos[1];

            int oldVal = engine.Grid[r, c];
            int newVal = string.IsNullOrEmpty(tb.Text) ? 0 : int.Parse(tb.Text);

            if (oldVal != newVal)
            {
                engine.UndoStack.Push(new Move { Row = r, Col = c, OldValue = oldVal, NewValue = newVal });
                engine.RedoStack.Clear(); // Clear redo history when a new manual move is made
                engine.Grid[r, c] = newVal;
            }

            if (engine.IsSolved())
            {
                LockBoard();
                MessageBox.Show("Congratulations! You solved the puzzle!", "Winner", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion
    }
}           
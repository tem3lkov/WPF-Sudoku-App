using System;
using System.Collections.Generic;
using System.IO;
using StudentXMLSerialization;

namespace SudokuProject.Models
{
    /// <summary>
    /// Core logic engine for generating, validating, and solving the Sudoku puzzle.
    /// </summary>
    public class SudokuEngine
    {
        #region Properties
        public int SubgridSize { get; private set; }
        public int BoardSize { get; private set; }
        public int[,] Grid { get; private set; }
        public int[,] SolutionGrid { get; private set; }
        public bool[,] IsFixed { get; private set; }
        #endregion

        #region Stack Related Properties
        public Stack<Move> UndoStack { get; } = new Stack<Move>();
        public Stack<Move> RedoStack { get; } = new Stack<Move>();
        #endregion

        #region Fields
        private static Random rand = new Random();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the Sudoku engine.
        /// </summary>
        /// <param name="subgridSize">The size of the subgrid (default is 3 for a 9x9 board).</param>
        public SudokuEngine(int subgridSize = 3)
        {
            SubgridSize = subgridSize;
            BoardSize = subgridSize * subgridSize;

            Grid = new int[BoardSize, BoardSize];
            SolutionGrid = new int[BoardSize, BoardSize];
            IsFixed = new bool[BoardSize, BoardSize];
        }
        #endregion

        #region Board Operations
        /// <summary>
        /// Resets the board and clears undo/redo history.
        /// </summary>
        public void ClearBoard()
        {
            Grid = new int[BoardSize, BoardSize];
            IsFixed = new bool[BoardSize, BoardSize];
            UndoStack.Clear();
            RedoStack.Clear();
        }

        /// <summary>
        /// Checks if placing a number at the specified row and column is valid according to Sudoku rules.
        /// </summary>
        public bool IsValidMove(int row, int col, int num)
        {
            // Check row and column
            for (int i = 0; i < BoardSize; i++)
            {
                if (Grid[row, i] == num) return false;
                if (Grid[i, col] == num) return false;
            }

            // Check the subgrid (e.g., 3x3 block)
            int startRow = row - (row % SubgridSize);
            int startCol = col - (col % SubgridSize);
            for (int i = 0; i < SubgridSize; i++)
            {
                for (int j = 0; j < SubgridSize; j++)
                {
                    if (Grid[startRow + i, startCol + j] == num) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if there are any empty cells left on the board.
        /// </summary>
        public bool IsSolved()
        {
            for (int r = 0; r < BoardSize; r++)
            {
                for (int c = 0; c < BoardSize; c++)
                {
                    if (Grid[r, c] == 0) return false;
                }
            }
            return true;
        }
        #endregion

        #region Generation & Solving
        /// <summary>
        /// Solves the board using a randomized backtracking algorithm.
        /// </summary>
        public bool SolveRandom()
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (Grid[row, col] == 0)
                    {
                        List<int> numbers = new List<int>();
                        for (int i = 1; i <= BoardSize; i++) numbers.Add(i);
                        Shuffle(numbers);

                        foreach (int num in numbers)
                        {
                            if (IsValidMove(row, col, num))
                            {
                                Grid[row, col] = num;
                                if (SolveRandom()) return true;
                                Grid[row, col] = 0; // Backtrack
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Helper method to shuffle a list of items randomly.
        /// </summary>
        private void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Counts how many valid solutions exist for the current board state.
        /// Used to ensure the puzzle has exactly one unique solution.
        /// </summary>
        private void CountSolutions(ref int count)
        {
            if (count > 1) return;

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (Grid[row, col] == 0)
                    {
                        for (int num = 1; num <= BoardSize; num++)
                        {
                            if (IsValidMove(row, col, num))
                            {
                                Grid[row, col] = num;
                                CountSolutions(ref count);
                                Grid[row, col] = 0;
                            }
                        }
                        return;
                    }
                }
            }
            count++;
        }

        /// <summary>
        /// Generates a new playable Sudoku puzzle by creating a full board and removing cells.
        /// </summary>
        /// <param name="clues">The number of starting numbers to leave on the board.</param>
        public void GeneratePuzzle(int clues)
        {
            ClearBoard();
            SolveRandom();
            Array.Copy(Grid, SolutionGrid, Grid.Length);

            int cellsToRemove = (BoardSize * BoardSize) - clues;
            List<int> positions = new List<int>();
            for (int i = 0; i < (BoardSize * BoardSize); i++) positions.Add(i);
            Shuffle(positions);

            foreach (int pos in positions)
            {
                if (cellsToRemove <= 0) break;

                int r = pos / BoardSize;
                int c = pos % BoardSize;

                int backup = Grid[r, c];
                Grid[r, c] = 0;

                int solutions = 0;
                CountSolutions(ref solutions);

                if (solutions == 1)
                {
                    cellsToRemove--;
                } else
                {
                    Grid[r, c] = backup; // Put it back if it breaks uniqueness
                }
            }

            // Mark remaining cells as fixed (clues)
            for (int r = 0; r < BoardSize; r++)
            {
                for (int c = 0; c < BoardSize; c++)
                {
                    if (Grid[r, c] != 0) IsFixed[r, c] = true;
                }
            }
        }
        #endregion

        #region Serialization (Save/Load)
        /// <summary>
        /// Saves the current game state to an XML file using the provided XMLutils.
        /// </summary>
        public void SaveToXml(string filePath)
        {
            // Map our 2D arrays to a serializable DTO object
            var saveState = new SudokuSaveData { SubgridSize = this.SubgridSize };

            for (int r = 0; r < BoardSize; r++)
            {
                for (int c = 0; c < BoardSize; c++)
                {
                    saveState.Cells.Add(new CellData {
                        Row = r,
                        Col = c,
                        Value = Grid[r, c],
                        IsFixed = IsFixed[r, c]
                    });
                }
            }

            string xmlContent = XMLutils.XmlSerializeToString(saveState);
            File.WriteAllText(filePath, xmlContent);
        }

        /// <summary>
        /// Loads a saved game state from an XML file.
        /// </summary>
        public void LoadFromXml(string filePath)
        {
            string xmlContent = File.ReadAllText(filePath);

            var loadedState = XMLutils.XmlDeserializeFromString<SudokuSaveData>(xmlContent);

            // Re-initialize board sizes
            SubgridSize = loadedState.SubgridSize;
            BoardSize = SubgridSize * SubgridSize;
            ClearBoard();

            // Restore the grid and fixed statuses
            foreach (var cell in loadedState.Cells)
            {
                Grid[cell.Row, cell.Col] = cell.Value;
                IsFixed[cell.Row, cell.Col] = cell.IsFixed;
            }

            // Generate the solution grid based on the fixed clues
            int[,] backup = new int[BoardSize, BoardSize];
            Array.Copy(Grid, backup, Grid.Length);

            for (int r = 0; r < BoardSize; r++)
            {
                for (int c = 0; c < BoardSize; c++)
                {
                    if (!IsFixed[r, c]) Grid[r, c] = 0;
                }
            }

            SolveRandom();
            Array.Copy(Grid, SolutionGrid, Grid.Length);

            // Restore player's progress
            Array.Copy(backup, Grid, Grid.Length);
        }
        #endregion
    }
}
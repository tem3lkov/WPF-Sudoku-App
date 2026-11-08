namespace SudokuProject.Models
{
    /// <summary>
    /// Represents a single move made by the player, used for Undo/Redo operations.
    /// </summary>
    public class Move
    {
        #region Properties
        public int Row { get; set; }
        public int Col { get; set; }
        public int OldValue { get; set; }
        public int NewValue { get; set; }
        #endregion
    }
}
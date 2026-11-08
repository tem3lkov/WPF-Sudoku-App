using System.Collections.Generic;

namespace SudokuProject.Models
{
    #region Serialization Models
    /// <summary>
    /// Represents the state of a single cell for serialization purposes.
    /// </summary>
    public class CellData
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Value { get; set; }
        public bool IsFixed { get; set; }
    }

    /// <summary>
    /// Represents the entire board state to be saved or loaded via XML.
    /// </summary>
    public class SudokuSaveData
    {
        public int SubgridSize { get; set; }
        public List<CellData> Cells { get; set; } = new List<CellData>();
    }
    #endregion
}
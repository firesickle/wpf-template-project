using System.Collections.Generic;

namespace DataToolkit.Models
{
    /// <summary>
    /// Represents a column mapping between input and output columns
    /// </summary>
    public class DataMap
    {
        /// <summary>
        /// Name of the input column
        /// </summary>
        public string InputColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Position of the input column in the data source
        /// </summary>
        public int InputColumnPosition { get; set; }

        /// <summary>
        /// Sample data from the input column for preview
        /// </summary>
        public List<string> SampleData { get; set; } = new List<string>();

        /// <summary>
        /// Name of the output column
        /// </summary>
        public string OutputColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Position of the output column in the target
        /// </summary>
        public int OutputColumnPosition { get; set; }

        /// <summary>
        /// Indicates whether the column is mapped to a target column
        /// </summary>
        public bool IsMapped { get; set; }

        /// <summary>
        /// Indicates whether the column has special processing rules
        /// </summary>
        public bool IsSpecialCase { get; set; }

        /// <summary>
        /// Indicates whether the column value is hard-coded rather than from source
        /// </summary>
        public bool IsHardCoded { get; set; }
    }
}
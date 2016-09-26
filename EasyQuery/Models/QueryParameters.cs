namespace EasyQuery.Models
{
    public class QueryParameters
    {
        /// <summary>
        ///     Property to be sorted by.
        /// </summary>
        public string[] Sort { get; set; } = new string[0];

        /// <summary>
        ///     Sort Ascending bool, defaults to true.
        /// </summary>
        public bool[] SortAscending { get; set; } = new bool[0];

        /// <summary>
        ///     SearchMatrix matrix
        /// </summary>
        public string[][] SearchMatrix { get; set; }
    }
}
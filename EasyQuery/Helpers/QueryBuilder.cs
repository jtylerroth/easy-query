using System.Collections.Specialized;
using System.Web;
using EasyQuery.Models;

namespace EasyQuery.Helpers
{
    public static class QueryBuilder
    {
        public static QueryParameters ParseQuery()
        {
            return BuildQuery(HttpContext.Current.Request.QueryString);
        }


        private static QueryParameters BuildQuery(NameValueCollection query)
        {
            if (!query.HasKeys())
                return null;
            var sort = query["sort"]?.Trim();

            var queryParams = new QueryParameters();

            var validIndex = 0;

            queryParams.SearchMatrix = new string[query.AllKeys.Length][];

            foreach (var key in query.AllKeys)
            {
                if (key == "sort")
                    continue;

                var terms = query[key].Split(',');

                queryParams.SearchMatrix[validIndex] = new string[terms.Length + 1];

                queryParams.SearchMatrix[validIndex][0] = key;
                for (var i = 0; i < terms.Length; i++)
                    queryParams.SearchMatrix[validIndex][i + 1] = terms[i].Trim();

                validIndex++;
            }

            if (string.IsNullOrWhiteSpace(sort)) return queryParams;

            var orderArray = sort.Split(',');
            queryParams.Sort = new string[orderArray.Length];
            queryParams.SortAscending = new bool[orderArray.Length];

            for (var i = 0; i < orderArray.Length; i++)
            {
                var orderArg = orderArray[i];
                queryParams.SortAscending[i] = orderArg.ToCharArray()[0] != '-';
                queryParams.Sort[i] = orderArg.TrimStart('-');
            }

            return queryParams;
        }
    }
}
namespace DeaneBarker.Optimizely.Webhooks.Helpers
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Select all items where the value is not null
        /// </summary>
        /// <typeparam name="T">The value type held by the collection</typeparam>
        /// <param name="collection">The collection to filter</param>
        /// <returns>The collection, with the not null filter applied</returns>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> collection) => collection.Where(item => item is not null);

        /// <summary>
        /// Execute a return value less method for every item in the collection
        /// </summary>
        /// <typeparam name="T">The value type held by the collection</typeparam>
        /// <param name="collection">The collection to filter</param>
        /// <param name="action">The method to execute for every item in the collection</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection) action(item);
        }
    }
}

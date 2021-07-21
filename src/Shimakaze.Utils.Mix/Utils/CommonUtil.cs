using System;
using System.Collections.Generic;

namespace Shimakaze.Utils.Mix.Utils
{
    internal static class CommonUtil
    {
        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (var item in @this)
                action(item);
        }
    }
}
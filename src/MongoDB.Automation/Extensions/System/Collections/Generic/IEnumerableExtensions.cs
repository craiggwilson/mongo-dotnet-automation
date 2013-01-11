using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
    internal static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (var item in @this)
            {
                action(item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> @this, Action<int, T> action)
        {
            int index = 0;
            foreach (var item in @this)
            {
                action(index++, item);
            }
        }
    }
}
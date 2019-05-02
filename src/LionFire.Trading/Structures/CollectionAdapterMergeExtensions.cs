using System;
using System.Collections.Generic;
using System.Linq;

namespace LionFire.Structures
{
    public static class CollectionAdapterMergeExtensions
    {
        public static void SetTo<TItem>(this List<TItem> target, IEnumerable<TItem> source)
        {
            var toRemove = new List<TItem>(target);

            foreach (var sourceItem in source)
            {
                if (!toRemove.Remove(sourceItem))
                {
                    target.Add(sourceItem);
                }
            }

            foreach (var removal in toRemove)
            {
                target.Remove(removal);
            }
        }

        public static void SetTo<TSource, TTarget>(this ICollection<TTarget> target, IEnumerable<TSource> source, Func<TSource, TTarget> factory, Func<TTarget, TSource, bool> equals)
            where TTarget : class
        {
            var toRemove = new List<TTarget>(target);

            foreach (var sourceItem in source)
            {
                var targetMatch = target.Where(t => equals(t, sourceItem)).FirstOrDefault();

                if (targetMatch != null)
                {
                    toRemove.Remove(targetMatch);
                }
                else
                {
                    target.Add(factory(sourceItem));
                }
            }

            foreach (var removal in toRemove)
            {
                target.Remove(removal);
            }
        }
        public static void AddMissingFrom<TSource, TTarget>(this ICollection<TTarget> target, IEnumerable<TSource> source, Func<TSource, TTarget> factory, Func<TTarget, TSource, bool> equals)
        {
            foreach (var sourceItem in source)
            {
                var targetMatch = target.Where(t => equals(t, sourceItem)).FirstOrDefault();

                if (targetMatch == null)
                {
                    target.Add(factory(sourceItem));
                }
            }
        }
    }
}

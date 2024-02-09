using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Maui.Graphics;

namespace TimeClock.Helpers
{
    public static class PointExtensions
    {
        public static string ToJsonString(this IEnumerable<PointF> points)
        {
            var pointList = points.ToList();

            return pointList switch
            {
                { Count: 0 } => "[]",
                { Count: 1 } => JsonSerializer.Serialize(new[] { new { lx = pointList[0].X, ly = pointList[0].Y, mx = pointList[0].X, my = pointList[0].Y } }),
                _ => JsonSerializer.Serialize(pointList[..^1].Batch(3).Select(batch => new { lx = batch[0].X, ly = batch[0].Y, mx = batch[1].X, my = batch[1].Y }))
            };
        }

        private static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            using var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return YieldBatchElements(enumerator, size - 1);
            }
        }

        private static IEnumerable<T> YieldBatchElements<T>(IEnumerator<T> source, int size)
        {
            yield return source.Current;
            for (var i = 0; i < size && source.MoveNext(); i++)
            {
                yield return source.Current;
            }
        }
    }
}
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace TimeClock.Helpers
{
    public static class PointExtensions
    {
        public static string ToJsonString(this IEnumerable<PointF> points)
        {
            var jsonPoints = new JArray();
            var pointList = points.ToList();

            // Handle case with no points.
            if (pointList.Count == 0)
                return jsonPoints.ToString();

            // Handle case with only one point.
            if (pointList.Count == 1)
                jsonPoints.Add(JObject.FromObject(new
                {
                    lx = pointList[0].X,
                    ly = pointList[0].Y,
                    mx = pointList[0].X,
                    my = pointList[0].Y,
                }));

            // Create an object with a start and end point for each successive set of points.
            for (var i = 0; i < pointList.Count - 2; i = i + 3)
            {
                jsonPoints.Add(JObject.FromObject(new
                {
                    lx = pointList[i].X,
                    ly = pointList[i].Y,
                    mx = pointList[i + 1].X,
                    my = pointList[i + 1].Y,
                }));
            }

            // Serialize and return string.
            var jsonPointsString = jsonPoints.ToString();
            return jsonPointsString;
        }
    }
}
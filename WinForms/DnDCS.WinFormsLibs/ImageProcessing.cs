﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DnDCS.Libs.SimpleObjects;
using ClipperLib;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace DnDCS.WinFormsLibs
{
    public static class ImageProcessing
    {
        public static unsafe void ApplyFog(Bitmap fog, params FogUpdate[] fogUpdates)
        {
            if (fog == null || fogUpdates == null || !fogUpdates.Any())
                return;

            // TODO: Look into a better way to handle multiple fog updates at the same time?
            foreach (var fogUpdate in fogUpdates)
            {
                var points = fogUpdate.Points;
                var isAddingFog = !fogUpdate.IsClearing;

                var polygon = new List<IntPoint>(points.Select(x => new IntPoint(x.X, x.Y)));
                var polygons = new List<List<IntPoint>>() { polygon };
                polygons = Clipper.OffsetPolygons(polygons, 48, JoinType.jtRound);

                var offsetPoints = polygons[0].Select(x => new SimplePoint((int)x.X, (int)x.Y)).ToArray();

                var boundingBoxBuffered = GetBoundingBox(fog, offsetPoints, 4);
                var boundingBox = GetBoundingBox(fog, points, 0);

                var bmd = fog.LockBits(boundingBoxBuffered, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var pixelSize = 4;
                Parallel.For(0, bmd.Height, (y) =>
                {
                    var row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                    for (var x = 0; x < bmd.Width; x++)
                    {
                        var offsetX = x + boundingBoxBuffered.X;
                        var offsetY = y + boundingBoxBuffered.Y;

                        if (isAddingFog && row[x * pixelSize + 3] == 255)
                        {
                            continue;
                        }

                        if (IsPointInPolygon(points, offsetX, offsetY))
                        {
                            row[x * pixelSize + 3] = (byte)(isAddingFog ? 255 : 0);
                        }
                        else if (IsPointInPolygon(offsetPoints, offsetX, offsetY))
                        {
                            var testPoint = new SimplePoint(offsetX, offsetY);
                            var dist = LineToPointDistance2D(points[0], points[1], testPoint);
                            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
                            {
                                var newDist = LineToPointDistance2D(points[j], points[i], testPoint);
                                if (newDist < dist)
                                    dist = newDist;
                            }

                            var alpha = (255 - 5.5 * dist);
                            alpha = Math.Max(Math.Floor(alpha), 0);
                            if (isAddingFog)
                                alpha = Math.Min(alpha + row[x * pixelSize + 3], 255);
                            else
                                alpha = Math.Max(row[x * pixelSize + 3] - alpha, 0);
                            row[x * pixelSize + 3] = (byte)(alpha);
                        }
                    }
                });

                fog.UnlockBits(bmd);
            }
        }

        private static bool IsPointInPolygon(SimplePoint[] polygon, float testx, float testy)
        {
            int nvert = polygon.Length;
            var vertx = polygon.Select(x => (float)(x.X)).ToArray();
            var verty = polygon.Select(x => (float)(x.Y)).ToArray();

            int i, j = 0;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((verty[i] > testy) != (verty[j] > testy)) &&
                 (testx < (vertx[j] - vertx[i]) * (testy - verty[i]) / (verty[j] - verty[i]) + vertx[i]))
                    c = !c;
            }
            return c;
        }

        private static Rectangle GetBoundingBox(Image fog, SimplePoint[] points, int buffer = 8)
        {
            if (points.Length == 0)
            {
                return new Rectangle(0, 0, 0, 0);
            }

            var left = points[0].X;
            var right = points[0].X;
            var top = points[0].Y;
            var bottom = points[0].Y;
            foreach (var point in points)
            {
                if (point.X < left)
                    left = point.X;
                if (point.X > right)
                    right = point.X;
                if (point.Y < top)
                    top = point.Y;
                if (point.Y > bottom)
                    bottom = point.Y;
            }

            var rect = new Rectangle(left, top, right - left, bottom - top);
            rect.X = Math.Max(0, rect.X - buffer);
            rect.Y = Math.Max(0, rect.Y - buffer);
            rect.Width = Math.Min(fog.Width - rect.X, rect.Width + buffer);
            rect.Height = Math.Min(fog.Height - rect.Y, rect.Height + buffer);
            return rect;
        }

        #region Maths

        //Compute the dot product AB . AC
        private static double DotProduct(double[] pointA, double[] pointB, double[] pointC)
        {
            double[] AB = new double[2];
            double[] BC = new double[2];
            AB[0] = pointB[0] - pointA[0];
            AB[1] = pointB[1] - pointA[1];
            BC[0] = pointC[0] - pointB[0];
            BC[1] = pointC[1] - pointB[1];
            double dot = AB[0] * BC[0] + AB[1] * BC[1];

            return dot;
        }

        //Compute the cross product AB x AC
        private static double CrossProduct(double[] pointA, double[] pointB, double[] pointC)
        {
            double[] AB = new double[2];
            double[] AC = new double[2];
            AB[0] = pointB[0] - pointA[0];
            AB[1] = pointB[1] - pointA[1];
            AC[0] = pointC[0] - pointA[0];
            AC[1] = pointC[1] - pointA[1];
            double cross = AB[0] * AC[1] - AB[1] * AC[0];

            return cross;
        }

        //Compute the distance from A to B
        private static double Distance(double[] pointA, double[] pointB)
        {
            double d1 = pointA[0] - pointB[0];
            double d2 = pointA[1] - pointB[1];

            return Math.Sqrt(d1 * d1 + d2 * d2);
        }

        //Compute the distance from AB to C
        //if isSegment is true, AB is a segment, not a line.
        private static double LineToPointDistance2D(SimplePoint linePoint1, SimplePoint linePoint2, SimplePoint pointTest, bool isSegment = true)
        {
            if (linePoint1.X == linePoint1.X && linePoint1.Y == linePoint2.Y)
                return 255;

            var pointA = new double[] { linePoint1.X, linePoint1.Y };
            var pointB = new double[] { linePoint2.X, linePoint2.Y };
            var pointC = new double[] { pointTest.X, pointTest.Y };
            double dist = CrossProduct(pointA, pointB, pointC) / Distance(pointA, pointB);
            if (isSegment)
            {
                double dot1 = DotProduct(pointA, pointB, pointC);
                if (dot1 > 0)
                    return Distance(pointB, pointC);

                double dot2 = DotProduct(pointB, pointA, pointC);
                if (dot2 > 0)
                    return Distance(pointA, pointC);
            }

            return Math.Abs(dist);
        }

        #endregion Maths

    }
}

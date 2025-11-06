using CulvertEditor.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CulvertEditor.Services
{
    public class PlanViewService
    {
        public void Draw(Canvas canvas, CulvertParameters parameters)
        {
            if (canvas == null) return;
            canvas.Children.Clear();

            double centerX = canvas.Width / 2;
            double centerY = canvas.Height / 2;

            var points = CalculatePoints(parameters, centerX, centerY);
            DrawGeometry(canvas, points);
            DrawDecks(canvas, points);

            if (parameters.ShowPoints)
                DrawingHelpers.AddPointMarkers(points, canvas);

            if (parameters.ShowDimensions)
                AddDimensions(canvas, points, parameters);

            AddLabels(canvas, points, centerX, parameters);
        }

        private Dictionary<string, Point> CalculatePoints(CulvertParameters p, double centerX, double centerY)
        {
            var points = new Dictionary<string, Point>();

            points["P1"] = new Point(centerX, centerY);
            points["P2"] = new Point(centerX, centerY - p.W / 2 * DrawingHelpers.SCALE);
            points["P3"] = new Point(centerX, centerY + p.W / 2 * DrawingHelpers.SCALE);

            points["P4_top_left"] = new Point(points["P2"].X - p.L3 / 2 * DrawingHelpers.SCALE, points["P2"].Y);
            points["P4_bot_left"] = new Point(points["P3"].X - p.L3 / 2 * DrawingHelpers.SCALE, points["P3"].Y);
            points["P5_bot_left"] = new Point(points["P4_bot_left"].X, points["P4_bot_left"].Y + p.W3 / 2 * DrawingHelpers.SCALE);
            points["P5_top_left"] = new Point(points["P4_top_left"].X, points["P4_top_left"].Y - p.W3 / 2 * DrawingHelpers.SCALE);
            points["P6_bot_left"] = new Point(points["P5_bot_left"].X - p.W2 * DrawingHelpers.SCALE, points["P5_bot_left"].Y);
            points["P6_top_left"] = new Point(points["P5_top_left"].X - p.W2 * DrawingHelpers.SCALE, points["P5_top_left"].Y);

            points["P8_top_left"] = new Point(points["P2"].X - p.L1 / 2 * DrawingHelpers.SCALE, points["P2"].Y);
            points["P8_top_right"] = new Point(points["P2"].X + p.L1 / 2 * DrawingHelpers.SCALE, points["P2"].Y);
            points["P9_top_left"] = new Point(points["P8_top_left"].X, points["P8_top_left"].Y - p.W1 * DrawingHelpers.SCALE);
            points["P9_top_right"] = new Point(points["P8_top_right"].X, points["P8_top_right"].Y - p.W1 * DrawingHelpers.SCALE);

            points["P8_bot_left"] = new Point(points["P3"].X - p.L1 / 2 * DrawingHelpers.SCALE, points["P3"].Y);
            points["P8_bot_right"] = new Point(points["P3"].X + p.L1 / 2 * DrawingHelpers.SCALE, points["P3"].Y);
            points["P9_bot_left"] = new Point(points["P8_bot_left"].X, points["P8_bot_left"].Y + p.W1 * DrawingHelpers.SCALE);
            points["P9_bot_right"] = new Point(points["P8_bot_right"].X, points["P8_bot_right"].Y + p.W1 * DrawingHelpers.SCALE);

            // Calculate P7 with angle
            double alphaRad = p.Alpha * Math.PI / 180.0;
            points["P7_top_left"] = new Point(
                points["P6_top_left"].X - p.L2 * Math.Cos(alphaRad) * DrawingHelpers.SCALE,
                points["P6_top_left"].Y - p.L2 * Math.Sin(alphaRad) * DrawingHelpers.SCALE
            );
            points["P7_bot_left"] = new Point(
                points["P6_bot_left"].X - p.L2 * Math.Cos(alphaRad) * DrawingHelpers.SCALE,
                points["P6_bot_left"].Y + p.L2 * Math.Sin(alphaRad) * DrawingHelpers.SCALE
            );

            // Mirror points
            points["P4_top_right"] = DrawingHelpers.MirrorPointX(points["P4_top_left"], centerX);
            points["P4_bot_right"] = DrawingHelpers.MirrorPointX(points["P4_bot_left"], centerX);
            points["P5_bot_right"] = DrawingHelpers.MirrorPointX(points["P5_bot_left"], centerX);
            points["P5_top_right"] = DrawingHelpers.MirrorPointX(points["P5_top_left"], centerX);
            points["P6_bot_right"] = DrawingHelpers.MirrorPointX(points["P6_bot_left"], centerX);
            points["P6_top_right"] = DrawingHelpers.MirrorPointX(points["P6_top_left"], centerX);
            points["P7_top_right"] = DrawingHelpers.MirrorPointX(points["P7_top_left"], centerX);
            points["P7_bot_right"] = DrawingHelpers.MirrorPointX(points["P7_bot_left"], centerX);

            return points;
        }

        private void DrawGeometry(Canvas canvas, Dictionary<string, Point> points)
        {
            // Draw main lines
            DrawingHelpers.DrawLine(canvas, points["P7_top_left"], points["P7_bot_left"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P7_top_right"], points["P7_bot_right"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P4_top_left"], points["P5_top_left"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P4_bot_left"], points["P5_bot_left"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P4_top_right"], points["P5_top_right"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P4_bot_right"], points["P5_bot_right"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P5_top_left"], points["P6_top_left"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P5_bot_left"], points["P6_bot_left"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P5_top_right"], points["P6_top_right"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P5_bot_right"], points["P6_bot_right"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P6_top_left"], points["P6_bot_left"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P6_top_right"], points["P6_bot_right"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P6_top_left"], points["P7_top_left"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P6_bot_left"], points["P7_bot_left"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P6_top_right"], points["P7_top_right"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P6_bot_right"], points["P7_bot_right"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P4_top_left"], points["P4_top_right"], DrawingHelpers.COLOR_MAIN_LINE, 1);
            DrawingHelpers.DrawLine(canvas, points["P4_bot_left"], points["P4_bot_right"], DrawingHelpers.COLOR_MAIN_LINE, 1);
        }

        private void DrawDecks(Canvas canvas, Dictionary<string, Point> points)
        {
            DrawingHelpers.DrawDashedRectangle(canvas,
                points["P8_top_left"], points["P9_top_left"],
                points["P8_top_right"], points["P9_top_right"],
                DrawingHelpers.COLOR_DECK);

            DrawingHelpers.DrawDashedRectangle(canvas,
                points["P8_bot_left"], points["P9_bot_left"],
                points["P8_bot_right"], points["P9_bot_right"],
                DrawingHelpers.COLOR_DECK);

            DrawingHelpers.DrawDashedLine(canvas,
                points["P4_top_left"], points["P4_bot_left"],
                new SolidColorBrush(DrawingHelpers.COLOR_DECK),
                new DoubleCollection { 8, 4 });

            DrawingHelpers.DrawDashedLine(canvas,
                points["P4_top_right"], points["P4_bot_right"],
                new SolidColorBrush(DrawingHelpers.COLOR_DECK),
                new DoubleCollection { 8, 4 });
        }

        private void AddLabels(Canvas canvas, Dictionary<string, Point> points, double centerX, CulvertParameters p)
        {
            DrawingHelpers.AddLabel("BẢN QUÁ ĐỘ", centerX,
                points["P2"].Y - p.W1 / 2 * DrawingHelpers.SCALE,
                new SolidColorBrush(DrawingHelpers.COLOR_TEXT), 14, canvas);

            DrawingHelpers.AddLabel("BẢN QUÁ ĐỘ", centerX,
                points["P3"].Y + p.W1 / 2 * DrawingHelpers.SCALE,
                new SolidColorBrush(DrawingHelpers.COLOR_TEXT), 14, canvas);
        }

        private void AddDimensions(Canvas canvas, Dictionary<string, Point> points, CulvertParameters p)
        {
            var dimBrush = new SolidColorBrush(DrawingHelpers.COLOR_DIMENSION);
            double offset = 40;

            DrawingHelpers.AddHorizontalDimension(points["P9_top_left"].X, points["P9_top_left"].Y - offset,
                p.L1, "L1=" + p.L1, dimBrush, canvas);
            DrawingHelpers.AddHorizontalDimension(points["P7_top_left"].X, points["P7_top_left"].Y - offset,
                p.L2, "L2=" + p.L2, dimBrush, canvas);
            DrawingHelpers.AddHorizontalDimension(points["P5_top_left"].X, points["P4_top_left"].Y - offset - 100,
                p.L3, "L3=" + p.L3, dimBrush, canvas);
            DrawingHelpers.AddVerticalDimension(points["P2"].X - offset, points["P2"].Y,
                p.W, "W=" + p.W, dimBrush, canvas);
        }
    }
}
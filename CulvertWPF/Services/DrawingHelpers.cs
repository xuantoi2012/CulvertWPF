using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CulvertEditor.Services
{
    public static class DrawingHelpers
    {
        public const double SCALE = 0.015;

        // Colors
        public static readonly Color COLOR_MAIN_LINE = Color.FromRgb(0, 153, 51);
        public static readonly Color COLOR_DECK = Color.FromRgb(0, 204, 255);
        public static readonly Color COLOR_DIMENSION = Color.FromRgb(220, 20, 60);
        public static readonly Color COLOR_POINT = Color.FromRgb(0, 120, 212);
        public static readonly Color COLOR_TEXT = Color.FromRgb(0, 255, 128);
        public static readonly Color COLOR_EXCAVATION = Color.FromRgb(150, 150, 150);

        public static void DrawLine(Canvas canvas, Point a, Point b, Color color, double thickness)
        {
            canvas.Children.Add(new Line
            {
                X1 = a.X,
                Y1 = a.Y,
                X2 = b.X,
                Y2 = b.Y,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = thickness
            });
        }

        public static void DrawDashedLine(Canvas canvas, Point a, Point b, Brush brush, DoubleCollection dashArray)
        {
            canvas.Children.Add(new Line
            {
                X1 = a.X,
                Y1 = a.Y,
                X2 = b.X,
                Y2 = b.Y,
                Stroke = brush,
                StrokeThickness = 2,
                StrokeDashArray = dashArray
            });
        }

        public static void DrawDashedRectangle(Canvas canvas, Point p1, Point p2, Point p3, Point p4, Color color)
        {
            var brush = new SolidColorBrush(color);
            var dashArray = new DoubleCollection { 8, 4 };
            DrawDashedLine(canvas, p1, p3, brush, dashArray);
            DrawDashedLine(canvas, p1, p2, brush, dashArray);
            DrawDashedLine(canvas, p2, p4, brush, dashArray);
            DrawDashedLine(canvas, p4, p3, brush, dashArray);
        }

        public static void AddLabel(string text, double x, double y, Brush color, int fontSize, Canvas canvas)
        {
            TextBlock label = new TextBlock
            {
                Text = text,
                Foreground = color,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Color.FromArgb(200, 50, 50, 50)),
                Padding = new Thickness(3, 1, 3, 1)
            };
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(label, x - label.DesiredSize.Width / 2);
            Canvas.SetTop(label, y - label.DesiredSize.Height / 2);
            canvas.Children.Add(label);
        }

        public static void AddHorizontalDimension(double x, double y, double length, string label, Brush color, Canvas canvas)
        {
            double endX = x + length * SCALE;
            DrawLine(canvas, new Point(x, y), new Point(endX, y), COLOR_DIMENSION, 1.5);
            AddLabel(label, x + length * SCALE / 2, y - 12, color, 10, canvas);
        }

        public static void AddVerticalDimension(double x, double y, double length, string label, Brush color, Canvas canvas)
        {
            double endY = y + length * SCALE;
            DrawLine(canvas, new Point(x, y), new Point(x, endY), COLOR_DIMENSION, 1.5);
            AddLabel(label, x - 20, y + length * SCALE / 2, color, 10, canvas);
        }

        public static void AddPointMarkers(Dictionary<string, Point> points, Canvas canvas)
        {
            var brush = new SolidColorBrush(COLOR_POINT);
            foreach (var kvp in points)
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = brush,
                    Stroke = new SolidColorBrush(COLOR_MAIN_LINE),
                    StrokeThickness = 2
                };
                Canvas.SetLeft(ellipse, kvp.Value.X - 4);
                Canvas.SetTop(ellipse, kvp.Value.Y - 4);
                canvas.Children.Add(ellipse);

                TextBlock label = new TextBlock
                {
                    Text = kvp.Key.Replace("_", " "),
                    Foreground = brush,
                    FontSize = 9,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(label, kvp.Value.X + 6);
                Canvas.SetTop(label, kvp.Value.Y - 12);
                canvas.Children.Add(label);
            }
        }

        public static Point MirrorPointX(Point p, double axisX)
        {
            return new Point(2 * axisX - p.X, p.Y);
        }
    }
}
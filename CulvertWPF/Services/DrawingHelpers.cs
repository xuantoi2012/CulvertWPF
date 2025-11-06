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

        // Frozen brushes (giữ nguyên)
        public static readonly Brush BRUSH_MAIN_LINE;
        public static readonly Brush BRUSH_DECK;
        public static readonly Brush BRUSH_DIMENSION;
        public static readonly Brush BRUSH_POINT;
        public static readonly Brush BRUSH_TEXT;
        public static readonly Brush BRUSH_EXCAVATION;
        public static readonly Brush BRUSH_LABEL_BG;

        public static readonly Color COLOR_MAIN_LINE = Color.FromRgb(0, 153, 51);
        public static readonly Color COLOR_DECK = Color.FromRgb(0, 204, 255);
        public static readonly Color COLOR_DIMENSION = Color.FromRgb(220, 20, 60);
        public static readonly Color COLOR_POINT = Color.FromRgb(0, 120, 212);
        public static readonly Color COLOR_TEXT = Color.FromRgb(0, 255, 128);
        public static readonly Color COLOR_EXCAVATION = Color.FromRgb(150, 150, 150);

        static DrawingHelpers()
        {
            BRUSH_MAIN_LINE = CreateFrozenBrush(COLOR_MAIN_LINE);
            BRUSH_DECK = CreateFrozenBrush(COLOR_DECK);
            BRUSH_DIMENSION = CreateFrozenBrush(COLOR_DIMENSION);
            BRUSH_POINT = CreateFrozenBrush(COLOR_POINT);
            BRUSH_TEXT = CreateFrozenBrush(COLOR_TEXT);
            BRUSH_EXCAVATION = CreateFrozenBrush(COLOR_EXCAVATION);
            BRUSH_LABEL_BG = CreateFrozenBrush(Color.FromArgb(200, 50, 50, 50));
        }

        private static Brush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        // Drawing methods (giữ nguyên)
        public static void DrawLine(Canvas canvas, Point a, Point b, Color color, double thickness)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();

            var line = new Line
            {
                X1 = a.X,
                Y1 = a.Y,
                X2 = b.X,
                Y2 = b.Y,
                Stroke = brush,
                StrokeThickness = thickness,
                SnapsToDevicePixels = true,
                UseLayoutRounding = true
            };

            RenderOptions.SetEdgeMode(line, EdgeMode.Aliased);
            canvas.Children.Add(line);
        }

        public static void DrawLine(Canvas canvas, Point a, Point b, Brush brush, double thickness)
        {
            var line = new Line
            {
                X1 = a.X,
                Y1 = a.Y,
                X2 = b.X,
                Y2 = b.Y,
                Stroke = brush,
                StrokeThickness = thickness,
                SnapsToDevicePixels = true,
                UseLayoutRounding = true
            };

            RenderOptions.SetEdgeMode(line, EdgeMode.Aliased);
            canvas.Children.Add(line);
        }

        public static void DrawDashedLine(Canvas canvas, Point a, Point b, Brush brush, DoubleCollection dashArray)
        {
            var line = new Line
            {
                X1 = a.X,
                Y1 = a.Y,
                X2 = b.X,
                Y2 = b.Y,
                Stroke = brush,
                StrokeThickness = 2,
                StrokeDashArray = dashArray,
                SnapsToDevicePixels = true,
                UseLayoutRounding = true
            };

            RenderOptions.SetEdgeMode(line, EdgeMode.Aliased);
            canvas.Children.Add(line);
        }

        public static void DrawDashedRectangle(Canvas canvas, Point p1, Point p2, Point p3, Point p4, Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();

            var dashArray = new DoubleCollection { 8, 4 };
            dashArray.Freeze();

            DrawDashedLine(canvas, p1, p3, brush, dashArray);
            DrawDashedLine(canvas, p1, p2, brush, dashArray);
            DrawDashedLine(canvas, p2, p4, brush, dashArray);
            DrawDashedLine(canvas, p4, p3, brush, dashArray);
        }

        // ✅ ULTRA SHARP TEXT - SCALE INDEPENDENT
        public static void AddLabel(string text, double x, double y, Brush color, int fontSize, Canvas canvas)
        {
            TextBlock label = new TextBlock
            {
                Text = text,
                Foreground = color,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                Background = BRUSH_LABEL_BG,
                Padding = new Thickness(3, 1, 3, 1),

                // ✅ SHARP RENDERING
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            // ✅ BEST TEXT QUALITY SETTINGS
            TextOptions.SetTextRenderingMode(label, TextRenderingMode.ClearType);
            TextOptions.SetTextFormattingMode(label, TextFormattingMode.Ideal);
            TextOptions.SetTextHintingMode(label, TextHintingMode.Fixed);

            RenderOptions.SetClearTypeHint(label, ClearTypeHint.Enabled);
            RenderOptions.SetBitmapScalingMode(label, BitmapScalingMode.HighQuality);
            RenderOptions.SetEdgeMode(label, EdgeMode.Aliased);

            // ✅ KEY FIX: Counter-scale để text không bị zoom
            var group = canvas.RenderTransform as TransformGroup;
            if (group != null && group.Children.Count > 0)
            {
                var scaleTransform = group.Children[0] as ScaleTransform;
                if (scaleTransform != null)
                {
                    double currentZoom = scaleTransform.ScaleX;

                    // Counter-scale text to keep original size
                    var counterScale = new ScaleTransform(1.0 / currentZoom, 1.0 / currentZoom);
                    label.LayoutTransform = counterScale;
                }
            }

            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(label, x - label.DesiredSize.Width / 2);
            Canvas.SetTop(label, y - label.DesiredSize.Height / 2);
            canvas.Children.Add(label);
        }

        public static void AddHorizontalDimension(double x, double y, double length, string label, Brush color, Canvas canvas)
        {
            double endX = x + length * SCALE;
            DrawLine(canvas, new Point(x, y), new Point(endX, y), BRUSH_DIMENSION, 1.5);
            AddLabel(label, x + length * SCALE / 2, y - 12, color, 10, canvas);
        }

        public static void AddVerticalDimension(double x, double y, double length, string label, Brush color, Canvas canvas)
        {
            double endY = y + length * SCALE;
            DrawLine(canvas, new Point(x, y), new Point(x, endY), BRUSH_DIMENSION, 1.5);
            AddLabel(label, x - 20, y + length * SCALE / 2, color, 10, canvas);
        }

        public static void AddPointMarkers(Dictionary<string, Point> points, Canvas canvas)
        {
            foreach (var kvp in points)
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = BRUSH_POINT,
                    Stroke = BRUSH_MAIN_LINE,
                    StrokeThickness = 2,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = true
                };

                RenderOptions.SetEdgeMode(ellipse, EdgeMode.Aliased);
                RenderOptions.SetBitmapScalingMode(ellipse, BitmapScalingMode.HighQuality);

                Canvas.SetLeft(ellipse, kvp.Value.X - 4);
                Canvas.SetTop(ellipse, kvp.Value.Y - 4);
                canvas.Children.Add(ellipse);

                TextBlock label = new TextBlock
                {
                    Text = kvp.Key.Replace("_", " "),
                    Foreground = BRUSH_POINT,
                    FontSize = 9,
                    FontWeight = FontWeights.Bold,
                    UseLayoutRounding = true,
                    SnapsToDevicePixels = true
                };

                // ✅ Counter-scale for point labels too
                var group = canvas.RenderTransform as TransformGroup;
                if (group != null && group.Children.Count > 0)
                {
                    var scaleTransform = group.Children[0] as ScaleTransform;
                    if (scaleTransform != null)
                    {
                        double currentZoom = scaleTransform.ScaleX;
                        var counterScale = new ScaleTransform(1.0 / currentZoom, 1.0 / currentZoom);
                        label.LayoutTransform = counterScale;
                    }
                }

                TextOptions.SetTextRenderingMode(label, TextRenderingMode.ClearType);
                TextOptions.SetTextFormattingMode(label, TextFormattingMode.Ideal);
                TextOptions.SetTextHintingMode(label, TextHintingMode.Fixed);
                RenderOptions.SetClearTypeHint(label, ClearTypeHint.Enabled);

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
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CulvertEditor
{
    public partial class MainWindow : Window
    {
        private const double SCALE = 0.015; // Scale nhỏ hơn để fit tất cả
        private double currentZoom = 1.0;
        private Point? lastMousePosition;
        private bool isPanning = false;

        // Colors
        private static readonly Color COLOR_MAIN_LINE = Color.FromRgb(0, 153, 51);      // Xanh lá
        private static readonly Color COLOR_DECK = Color.FromRgb(0, 204, 255);          // Xanh cyan
        private static readonly Color COLOR_DIMENSION = Color.FromRgb(220, 20, 60);
        private static readonly Color COLOR_POINT = Color.FromRgb(0, 120, 212);       // Xanh dương
        private static readonly Color COLOR_TEXT = Color.FromRgb(0, 255, 128);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => DrawPlan()),
                System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void OnDimensionChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded) DrawPlan();
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            txtL1.Text = "24500";
            txtW1.Text = "4000";
            txtL2.Text = "5000";
            txtW2.Text = "1800";
            txtW3.Text = "2000";
            txtW.Text = "4000";
            txtL3.Text = "30000";
            txtAlpha.Text = "20";
            chkShowDimensions.IsChecked = true;
            chkShowPoints.IsChecked = true;
        }

        // Zoom/Pan handlers (giữ nguyên như cũ)
        private void ZoomGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePos = e.GetPosition(planCanvas);
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            double oldZoom = currentZoom;
            currentZoom *= zoomFactor;
            currentZoom = Math.Max(0.1, Math.Min(currentZoom, 5.0));
            scaleTransform.ScaleX = currentZoom;
            scaleTransform.ScaleY = currentZoom;
            double zoomChange = currentZoom / oldZoom;
            double newOffsetX = scrollViewer.HorizontalOffset * zoomChange + mousePos.X * (zoomChange - 1);
            double newOffsetY = scrollViewer.VerticalOffset * zoomChange + mousePos.Y * (zoomChange - 1);
            scrollViewer.ScrollToHorizontalOffset(newOffsetX);
            scrollViewer.ScrollToVerticalOffset(newOffsetY);
            txtZoomLevel.Text = $"{(int)(currentZoom * 100)}%";
            e.Handled = true;
        }

        private void ZoomGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPanning = true;
            lastMousePosition = e.GetPosition(scrollViewer);
            zoomGrid.CaptureMouse();
            zoomGrid.Cursor = Cursors.Hand;
        }

        private void ZoomGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPanning = false;
            lastMousePosition = null;
            zoomGrid.ReleaseMouseCapture();
            zoomGrid.Cursor = Cursors.Arrow;
        }

        private void ZoomGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning && lastMousePosition.HasValue)
            {
                Point currentPosition = e.GetPosition(scrollViewer);
                double deltaX = currentPosition.X - lastMousePosition.Value.X;
                double deltaY = currentPosition.Y - lastMousePosition.Value.Y;
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - deltaX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - deltaY);
                lastMousePosition = currentPosition;
            }
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            Point centerPos = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            ZoomToPoint(1.2, centerPos);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            Point centerPos = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            ZoomToPoint(0.8, centerPos);
        }

        private void ZoomToPoint(double factor, Point point)
        {
            double oldZoom = currentZoom;
            currentZoom *= factor;
            currentZoom = Math.Max(0.1, Math.Min(currentZoom, 5.0));
            scaleTransform.ScaleX = currentZoom;
            scaleTransform.ScaleY = currentZoom;
            double zoomChange = currentZoom / oldZoom;
            double newOffsetX = (scrollViewer.HorizontalOffset + point.X) * zoomChange - point.X;
            double newOffsetY = (scrollViewer.VerticalOffset + point.Y) * zoomChange - point.Y;
            scrollViewer.ScrollToHorizontalOffset(newOffsetX);
            scrollViewer.ScrollToVerticalOffset(newOffsetY);
            txtZoomLevel.Text = $"{(int)(currentZoom * 100)}%";
        }

        private void ZoomReset_Click(object sender, RoutedEventArgs e)
        {
            currentZoom = 1.0;
            scaleTransform.ScaleX = 1.0;
            scaleTransform.ScaleY = 1.0;
            scrollViewer.ScrollToHorizontalOffset(0);
            scrollViewer.ScrollToVerticalOffset(0);
            txtZoomLevel.Text = "100%";
        }

        private bool TryGetValue(TextBox textBox, out double value)
        {
            return double.TryParse(textBox.Text, out value) && value > 0;
        }

        // ========== MAIN DRAWING FUNCTION ==========
        private void DrawPlan()
        {
            if (planCanvas == null) return;
            planCanvas.Children.Clear();

            // Parse inputs
            if (!TryGetValue(txtL1, out double L1)) L1 = 24500;
            if (!TryGetValue(txtW1, out double W1)) W1 = 4000;
            if (!TryGetValue(txtL2, out double L2)) L2 = 5000;
            if (!TryGetValue(txtW2, out double W2)) W2 = 1800;
            if (!TryGetValue(txtW3, out double W3)) W3 = 2000;
            if (!TryGetValue(txtW, out double W)) W = 4000;
            if (!TryGetValue(txtL3, out double L3)) L3 = 30000;
            if (!TryGetValue(txtAlpha, out double Alpha)) Alpha = 20;

            double centerX = planCanvas.Width / 2;
            double centerY = planCanvas.Height / 2;

            var points = new Dictionary<string, Point>();

            // ===== TÍNH TỌA ĐỘ CÁC ĐIỂM THEO BẢN VẼ =====

            // P1: Tâm gốc
            points["P1"] = new Point(centerX, centerY);

            // P2, P3: W/2 từ P1 (trái/phải)
            points["P2"] = new Point(centerX, centerY - W / 2 * SCALE);
            points["P3"] = new Point(centerX, centerY + W / 2 * SCALE);

            // P4_top, P4_bot: L3/2 từ P1 (trái/phải)
            points["P4_top_left"] = new Point(points["P2"].X - L3/2 * SCALE, points["P2"].Y);
            points["P4_bot_left"] = new Point(points["P3"].X - L3/2 * SCALE, points["P3"].Y);

            // P5: P4 + W3 (xuống dưới)
            points["P5_bot_left"] = new Point(points["P4_bot_left"].X, points["P4_bot_left"].Y + W3 / 2 * SCALE);
            points["P5_top_left"] = new Point(points["P4_top_left"].X, points["P4_top_left"].Y - W3 / 2 * SCALE);

            // P6_extend: P6 - W3
            points["P6_bot_left"] = new Point(points["P5_bot_left"].X - W2 * SCALE, points["P5_bot_left"].Y);
            points["P6_top_left"] = new Point(points["P5_top_left"].X - W2 * SCALE, points["P5_top_left"].Y);

            // P8, P9: Bản quá độ trên
            points["P8_top_left"] = new Point(points["P2"].X - L1 / 2 * SCALE, points["P2"].Y);
            points["P8_top_right"] = new Point(points["P2"].X + L1 / 2 * SCALE, points["P2"].Y);

            // P8_inner, P9_inner: viền trong bản quá độ trên
            points["P9_top_left"] = new Point(points["P8_top_left"].X, points["P8_top_left"].Y - W1 * SCALE);
            points["P9_top_right"] = new Point(points["P8_top_right"].X, points["P8_top_right"].Y - W1 * SCALE);

            // P8_bot, P9_bot: Bản quá độ dưới
            points["P8_bot_left"] = new Point(points["P3"].X - L1 / 2 * SCALE, points["P3"].Y);
            points["P8_bot_right"] = new Point(points["P3"].X + L1 / 2 * SCALE, points["P3"].Y);

            // P8_bot_inner, P9_bot_inner
            points["P9_bot_left"] = new Point(points["P8_bot_left"].X, points["P8_bot_left"].Y + W1 * SCALE);
            points["P9_bot_right"] = new Point(points["P8_bot_right"].X, points["P8_bot_right"].Y + W1 * SCALE);

            // ===== TÍNH ĐIỂM TƯỜNG CÁNH (P7) - Góc xiên Alpha =====
            double alphaRad = Alpha * Math.PI / 180.0;

            // P7 trái trên
            double P7_top_left_X = points["P6_top_left"].X - L2 * Math.Cos(alphaRad) * SCALE;
            double P7_top_left_Y = points["P6_top_left"].Y - L2 * Math.Sin(alphaRad) * SCALE;
            points["P7_top_left"] = new Point(P7_top_left_X, P7_top_left_Y);

            // P7 trái dưới
            double P7_left_bot_X = points["P6_bot_left"].X - L2 * Math.Cos(alphaRad) * SCALE;
            double P7_left_bot_Y = points["P6_bot_left"].Y + L2 * Math.Sin(alphaRad) * SCALE;
            points["P7_bot_left"] = new Point(P7_left_bot_X, P7_left_bot_Y);

            //MIRROR CÁC ĐIỂM SANG BÊN PHẢI
            // P4 phải trên (mirror)
            points["P4_top_right"] = MirrorPointX(points["P4_top_left"], centerX);
            // P4 phải dưới (mirror)
            points["P4_bot_right"] = MirrorPointX(points["P4_bot_left"], centerX);
            // P5 phải dưới (mirror)
            points["P5_bot_right"] = MirrorPointX(points["P5_bot_left"], centerX);
            // P5 phải trên (mirror)
            points["P5_top_right"] = MirrorPointX(points["P5_top_left"], centerX);
            // P6 phải dưới (mirror)
            points["P6_bot_right"] = MirrorPointX(points["P6_bot_left"], centerX);
            // P6 phải trên (mirror)
            points["P6_top_right"] = MirrorPointX(points["P6_top_left"], centerX);
            // P7 phải trên (mirror)
            points["P7_top_right"] = MirrorPointX(points["P7_top_left"], centerX);
            // P7 phải dưới (mirror)
            points["P7_bot_right"] = MirrorPointX(points["P7_bot_left"], centerX);

            // ===== VẼ ĐƯỜNG VIỀN CHÍNH =====

            // Viền ngoài cùng
            DrawLine(planCanvas, points["P7_top_left"], points["P7_bot_left"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P7_top_right"], points["P7_bot_right"], COLOR_MAIN_LINE, 1);

            DrawLine(planCanvas, points["P4_top_left"], points["P5_top_left"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P4_bot_left"], points["P5_bot_left"], COLOR_MAIN_LINE, 1);

            DrawLine(planCanvas, points["P4_top_right"], points["P5_top_right"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P4_bot_right"], points["P5_bot_right"], COLOR_MAIN_LINE, 1);

            DrawLine(planCanvas, points["P5_top_left"], points["P6_top_left"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P5_bot_left"], points["P6_bot_left"], COLOR_MAIN_LINE, 1);

            DrawLine(planCanvas, points["P5_top_right"], points["P6_top_right"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P5_bot_right"], points["P6_bot_right"], COLOR_MAIN_LINE, 1);

            DrawLine(planCanvas, points["P6_top_left"], points["P6_bot_left"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P6_top_right"], points["P6_bot_right"], COLOR_MAIN_LINE, 1);
            
            // Tường cánh trái
            DrawLine(planCanvas, points["P6_top_left"], points["P7_top_left"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P6_bot_left"], points["P7_bot_left"], COLOR_MAIN_LINE, 1);
            
            // Tường cánh phải
            DrawLine(planCanvas, points["P6_top_right"], points["P7_top_right"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P6_bot_right"], points["P7_bot_right"], COLOR_MAIN_LINE, 1);

            // Cống hộp
            DrawLine(planCanvas, points["P4_top_left"], points["P4_top_right"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P4_bot_left"], points["P4_bot_right"], COLOR_MAIN_LINE, 1);

            // Bản quá độ
            DrawDashedRectangle(planCanvas, points["P8_top_left"], points["P9_top_left"], points["P8_top_right"], points["P9_top_right"], COLOR_DECK);
            DrawDashedRectangle(planCanvas, points["P8_bot_left"], points["P9_bot_left"], points["P8_bot_right"], points["P9_bot_right"], COLOR_DECK);

            DrawDashedLine(planCanvas, points["P4_top_left"], points["P4_bot_left"], new SolidColorBrush(COLOR_DECK), new DoubleCollection { 8, 4 });

            DrawDashedLine(planCanvas, points["P4_top_right"], points["P4_bot_right"], new SolidColorBrush(COLOR_DECK), new DoubleCollection { 8, 4 });


            // Text
            AddLabel("BẢN QUÁ ĐỘ", centerX, points["P2"].Y - W1 / 2 * SCALE, new SolidColorBrush(COLOR_TEXT), 14);
            AddLabel("BẢN QUÁ ĐỘ", centerX, points["P3"].Y + W1 / 2 * SCALE, new SolidColorBrush(COLOR_TEXT), 14);

            // Điểm
            if (chkShowPoints?.IsChecked == true)
            {
                AddPointMarkers(points);
            }

            // Dimensions
            if (chkShowDimensions?.IsChecked == true)
            {
                AddAllDimensions(points, L1, W1, L2, W2, W3, W, L3, Alpha);
            }
        }

        private Point MirrorPointX(Point p, double axisX)
        {
            return new Point(2 * axisX - p.X, p.Y);
        }

        private void DrawLine(Canvas canvas, Point a, Point b, Color color, double thickness)
        {
            Line line = new Line
            {
                X1 = a.X,
                Y1 = a.Y,
                X2 = b.X,
                Y2 = b.Y,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = thickness
            };
            canvas.Children.Add(line);
        }

        private void DrawDashedRectangle(Canvas canvas, Point p8_top_left, Point p9_top_left, Point p8_top_right, Point p9_top_right, Color color)
        {
            var dashArray = new DoubleCollection { 8, 4 };
            var brush = new SolidColorBrush(color);

            DrawDashedLine(canvas, p8_top_left, p8_top_right, brush, dashArray);
            DrawDashedLine(canvas, p8_top_left, p9_top_left, brush, dashArray);
            DrawDashedLine(canvas, p9_top_left, p9_top_right, brush, dashArray);
            DrawDashedLine(canvas, p9_top_right, p8_top_right, brush, dashArray);
        }

        private void DrawDashedLine(Canvas canvas, Point a, Point b, Brush brush, DoubleCollection dashArray)
        {
            Line line = new Line
            {
                X1 = a.X,
                Y1 = a.Y,
                X2 = b.X,
                Y2 = b.Y,
                Stroke = brush,
                StrokeThickness = 2,
                StrokeDashArray = dashArray
            };
            canvas.Children.Add(line);
        }

        private void AddPointMarkers(Dictionary<string, Point> points)
        {
            var brush = new SolidColorBrush(COLOR_POINT);
            foreach (var kvp in points)
            {
                Ellipse point = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = brush,
                    Stroke = new SolidColorBrush(COLOR_MAIN_LINE),
                    StrokeThickness = 2
                };
                Canvas.SetLeft(point, kvp.Value.X - 4);
                Canvas.SetTop(point, kvp.Value.Y - 4);
                planCanvas.Children.Add(point);

                TextBlock label = new TextBlock
                {
                    Text = kvp.Key.Replace("_", " "),
                    Foreground = brush,
                    FontSize = 9,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(label, kvp.Value.X + 6);
                Canvas.SetTop(label, kvp.Value.Y - 12);
                planCanvas.Children.Add(label);
            }
        }

        private void AddAllDimensions(Dictionary<string, Point> points, double L1, double W1, double L2, double W2, double W3, double W, double L3, double Alpha)
        {
            var dimBrush = new SolidColorBrush(COLOR_DIMENSION);
            double offset = 40;

            // L1, L2, L3, W, W1, W2, W3, Alpha dimensions
            AddHorizontalDimension(points["P9_top_left"].X, points["P9_top_left"].Y - offset, L1, "L1=" + L1, dimBrush);
            AddHorizontalDimension(points["P7_top_left"].X, points["P7_top_left"].Y - offset, L2, "L2=" + L2, dimBrush);
            AddHorizontalDimension(points["P5_top_left"].X, points["P4_top_left"].Y - offset - 100, L3, "L3=" + L3, dimBrush);
            AddVerticalDimension(points["P2"].X - offset, points["P2"].Y, W, "W=" + W, dimBrush);
        }

        private void AddHorizontalDimension(double x, double y, double length, string label, Brush color)
        {
            double endX = x + length * SCALE;
            DrawLine(planCanvas, new Point(x, y), new Point(endX, y), Color.FromRgb(220, 20, 60), 1.5);
            AddLabel(label, x + length * SCALE / 2, y - 12, color, 10);
        }

        private void AddVerticalDimension(double x, double y, double length, string label, Brush color)
        {
            double endY = y + length * SCALE;
            DrawLine(planCanvas, new Point(x, y), new Point(x, endY), Color.FromRgb(220, 20, 60), 1.5);
            AddLabel(label, x - 20, y + length * SCALE / 2, color, 10);
        }

        private void AddLabel(string text, double x, double y, Brush color, int fontSize)
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
            planCanvas.Children.Add(label);
        }
    }
}
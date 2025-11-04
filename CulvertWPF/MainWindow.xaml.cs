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
        private const double SCALE = 0.02; // Điều chỉnh scale cho phù hợp
        private double currentZoom = 1.0;
        private Point? lastMousePosition;
        private bool isPanning = false;

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
            txtL_Total.Text = "35000";
            txtH_Total.Text = "10700";
            txtL_Deck.Text = "24500";
            txtH_Deck.Text = "4000";
            txtOffset_Top.Text = "400";
            txtOffset_Bottom.Text = "400";
            txtL_LeftOutlet.Text = "1800";
            txtH_LeftTop.Text = "4200";
            txtH_LeftBottom.Text = "2000";
            txtW_LeftOuter.Text = "6000";
            txtL_RightOutlet.Text = "1800";
            txtH_RightTop.Text = "4200";
            txtH_RightBottom.Text = "2000";
            txtW_RightOuter.Text = "6000";
            chkShowDimensions.IsChecked = true;
            chkShowPoints.IsChecked = true;
        }

        // ========== ZOOM/PAN HANDLERS ==========
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

        // ========== DRAWING FUNCTIONS ==========
        private bool TryGetValue(TextBox textBox, out double value)
        {
            return double.TryParse(textBox.Text, out value) && value > 0;
        }

        private void DrawPlan()
        {
            if (planCanvas == null) return;
            planCanvas.Children.Clear();

            // Parse dimensions
            if (!TryGetValue(txtL_Total, out double L_Total)) L_Total = 35000;
            if (!TryGetValue(txtH_Total, out double H_Total)) H_Total = 10700;
            if (!TryGetValue(txtL_Deck, out double L_Deck)) L_Deck = 24500;
            if (!TryGetValue(txtH_Deck, out double H_Deck)) H_Deck = 4000;
            if (!TryGetValue(txtOffset_Top, out double Offset_Top)) Offset_Top = 400;
            if (!TryGetValue(txtOffset_Bottom, out double Offset_Bottom)) Offset_Bottom = 400;
            if (!TryGetValue(txtL_LeftOutlet, out double L_LeftOutlet)) L_LeftOutlet = 1800;
            if (!TryGetValue(txtH_LeftTop, out double H_LeftTop)) H_LeftTop = 4200;
            if (!TryGetValue(txtH_LeftBottom, out double H_LeftBottom)) H_LeftBottom = 2000;
            if (!TryGetValue(txtW_LeftOuter, out double W_LeftOuter)) W_LeftOuter = 6000;
            if (!TryGetValue(txtL_RightOutlet, out double L_RightOutlet)) L_RightOutlet = 1800;
            if (!TryGetValue(txtH_RightTop, out double H_RightTop)) H_RightTop = 4200;
            if (!TryGetValue(txtH_RightBottom, out double H_RightBottom)) H_RightBottom = 2000;
            if (!TryGetValue(txtW_RightOuter, out double W_RightOuter)) W_RightOuter = 6000;

            // Center point P1 - tâm canvas
            double centerX = planCanvas.Width / 2;
            double centerY = planCanvas.Height / 2;

            var points = new Dictionary<string, Point>();

            // P1 - tâm (trung điểm chiều dọc tổng)
            points["P1"] = new Point(centerX, centerY);

            // === Deck trên (BẢN QUÁ ĐỘ trên) ===
            // P2 - tâm deck trên
            points["P2"] = new Point(centerX, centerY - Offset_Top * SCALE);

            // P0, P10 - 2 đầu deck trên (góc ngoài)
            points["P0"] = new Point(centerX - L_Deck / 2 * SCALE, centerY - Offset_Top * SCALE);
            points["P10"] = new Point(centerX + L_Deck / 2 * SCALE, centerY - Offset_Top * SCALE);

            // P8, P9 - 2 đầu deck trên (góc trong - cách 2000 từ góc ngoài)
            double deckInnerOffset = 2000;
            points["P8"] = new Point(centerX - L_Deck / 2 * SCALE + deckInnerOffset * SCALE, centerY - Offset_Top * SCALE - deckInnerOffset * SCALE);
            points["P9"] = new Point(centerX + L_Deck / 2 * SCALE - deckInnerOffset * SCALE, centerY - Offset_Top * SCALE - deckInnerOffset * SCALE);

            // === Deck dưới (BẢN QUÁ ĐỘ dưới) ===
            // P3 - tâm deck dưới
            points["P3"] = new Point(centerX, centerY + Offset_Bottom * SCALE);

            // P9_bot, P10_bot - 2 đầu deck dưới (góc ngoài)
            points["P9_bot"] = new Point(centerX - L_Deck / 2 * SCALE, centerY + Offset_Bottom * SCALE);
            points["P10_bot"] = new Point(centerX + L_Deck / 2 * SCALE, centerY + Offset_Bottom * SCALE);

            // P7, P8_bot - 2 đầu deck dưới (góc trong)
            points["P7"] = new Point(centerX - L_Deck / 2 * SCALE + deckInnerOffset * SCALE, centerY + Offset_Bottom * SCALE + deckInnerOffset * SCALE);
            points["P8_bot"] = new Point(centerX + L_Deck / 2 * SCALE - deckInnerOffset * SCALE, centerY + Offset_Bottom * SCALE + deckInnerOffset * SCALE);

            // === Cửa xả trái ===
            // P11 (góc ngoài trái trên)
            points["P11"] = new Point(centerX - L_Total / 2 * SCALE, centerY - H_Total / 2 * SCALE);

            // P'11 (góc ngoài trái dưới)
            points["P11_bot"] = new Point(centerX - L_Total / 2 * SCALE, centerY + H_Total / 2 * SCALE);

            // P'1 (điểm nối cửa trái với deck trên - inner)
            points["P1_left_top"] = new Point(centerX - L_Deck / 2 * SCALE - L_LeftOutlet * SCALE, centerY - Offset_Top * SCALE - H_LeftTop * SCALE);

            // P5 (điểm góc cửa trái trên)
            points["P5"] = new Point(centerX - L_Deck / 2 * SCALE, centerY - Offset_Top * SCALE - H_LeftTop * SCALE);

            // P4 (điểm góc deck trong trái trên)
            points["P4"] = new Point(centerX - L_Deck / 2 * SCALE + deckInnerOffset * SCALE, centerY - Offset_Top * SCALE - deckInnerOffset * SCALE);

            // P6 (điểm góc deck trong trái dưới)
            points["P6"] = new Point(centerX - L_Deck / 2 * SCALE + deckInnerOffset * SCALE, centerY + Offset_Bottom * SCALE + deckInnerOffset * SCALE);

            // P7_left (điểm nối cửa trái với deck dưới)
            points["P7_left"] = new Point(centerX - L_Deck / 2 * SCALE, centerY + Offset_Bottom * SCALE + H_LeftBottom * SCALE);

            // P1_left_bot (điểm góc cửa trái dưới inner)
            points["P1_left_bot"] = new Point(centerX - L_Deck / 2 * SCALE - L_LeftOutlet * SCALE, centerY + Offset_Bottom * SCALE + H_LeftBottom * SCALE);

            // === Cửa xả phải (đối xứng) ===
            points["P11_right"] = new Point(centerX + L_Total / 2 * SCALE, centerY - H_Total / 2 * SCALE);
            points["P11_right_bot"] = new Point(centerX + L_Total / 2 * SCALE, centerY + H_Total / 2 * SCALE);
            points["P1_right_top"] = new Point(centerX + L_Deck / 2 * SCALE + L_RightOutlet * SCALE, centerY - Offset_Top * SCALE - H_RightTop * SCALE);
            points["P5_right"] = new Point(centerX + L_Deck / 2 * SCALE, centerY - Offset_Top * SCALE - H_RightTop * SCALE);
            points["P4_right"] = new Point(centerX + L_Deck / 2 * SCALE - deckInnerOffset * SCALE, centerY - Offset_Top * SCALE - deckInnerOffset * SCALE);
            points["P6_right"] = new Point(centerX + L_Deck / 2 * SCALE - deckInnerOffset * SCALE, centerY + Offset_Bottom * SCALE + deckInnerOffset * SCALE);
            points["P7_right"] = new Point(centerX + L_Deck / 2 * SCALE, centerY + Offset_Bottom * SCALE + H_RightBottom * SCALE);
            points["P1_right_bot"] = new Point(centerX + L_Deck / 2 * SCALE + L_RightOutlet * SCALE, centerY + Offset_Bottom * SCALE + H_RightBottom * SCALE);

            // === VẼ ĐƯỜNG VIỀN NGOÀI (solid green) ===
            DrawLine(planCanvas, points["P11"], points["P11_right"], Colors.DarkBlue, 2);
            DrawLine(planCanvas, points["P11_right"], points["P11_right_bot"], Colors.DarkBlue, 2);
            DrawLine(planCanvas, points["P11_right_bot"], points["P11_bot"], Colors.DarkBlue, 2);
            DrawLine(planCanvas, points["P11_bot"], points["P11"], Colors.DarkBlue, 2);

            // Cửa xả trái
            DrawLine(planCanvas, points["P11"], points["P1_left_top"], Colors.DarkBlue, 2);
            DrawLine(planCanvas, points["P1_left_top"], points["P5"], Colors.DarkBlue, 2);
            DrawLine(planCanvas, points["P11_bot"], points["P1_left_bot"], Colors.DarkBlue, 2);
            DrawLine(planCanvas, points["P1_left_bot"], points["P7_left"], Colors.DarkBlue, 2);

            // Cửa xả phải
            DrawLine(planCanvas, points["P11_right"], points["P1_right_top"], Colors.DarkBlue, 2);
            DrawLine(planCanvas, points["P1_right_top"], points["P5_right"], Colors.DarkBlue, 2);
            DrawLine(planCanvas, points["P11_right_bot"], points["P1_right_bot"], Colors.DarkBlue, 2);
            DrawLine(planCanvas, points["P1_right_bot"], points["P7_right"], Colors.DarkBlue, 2);

            // Nối deck
            DrawLine(planCanvas, points["P5"], points["P5_right"], Colors.DarkBlue, 2);
            DrawLine(planCanvas, points["P7_left"], points["P7_right"], Colors.DarkBlue, 2);

            // === VẼ DECK (dashed cyan) ===
            DrawDashedRectangle(planCanvas, points["P0"], points["P10"], points["P9_bot"], points["P10_bot"], Colors.DarkBlue);

            // Thêm text "BẢN QUÁ ĐỘ"
            AddLabel("BẢN QUÁ ĐỘ", centerX, centerY - Offset_Top * SCALE / 2, Brushes.DarkBlue, 16);
            AddLabel("BẢN QUÁ ĐỘ", centerX, centerY + Offset_Bottom * SCALE * 1.5, Brushes.DarkBlue, 16);

            // === VẼ ĐIỂM ===
            if (chkShowPoints?.IsChecked == true)
            {
                AddPointMarkers(points);
            }

            // === VẼ KÍCH THƯỚC ===
            if (chkShowDimensions?.IsChecked == true)
            {
                AddAllDimensions(points, L_Total, H_Total, L_Deck, H_Deck, L_LeftOutlet, L_RightOutlet, W_LeftOuter, H_LeftTop, H_LeftBottom);
            }
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

        private void DrawDashedRectangle(Canvas canvas, Point p0, Point p10, Point p9_bot, Point p10_bot, Color color)
        {
            var dashArray = new DoubleCollection { 5, 3 };

            Line l1 = new Line { X1 = p0.X, Y1 = p0.Y, X2 = p10.X, Y2 = p10.Y, Stroke = new SolidColorBrush(color), StrokeThickness = 1, StrokeDashArray = dashArray };
            Line l2 = new Line { X1 = p10.X, Y1 = p10.Y, X2 = p10_bot.X, Y2 = p10_bot.Y, Stroke = new SolidColorBrush(color), StrokeThickness = 1, StrokeDashArray = dashArray };
            Line l3 = new Line { X1 = p10_bot.X, Y1 = p10_bot.Y, X2 = p9_bot.X, Y2 = p9_bot.Y, Stroke = new SolidColorBrush(color), StrokeThickness = 1, StrokeDashArray = dashArray };
            Line l4 = new Line { X1 = p9_bot.X, Y1 = p9_bot.Y, X2 = p0.X, Y2 = p0.Y, Stroke = new SolidColorBrush(color), StrokeThickness = 1, StrokeDashArray = dashArray };

            canvas.Children.Add(l1);
            canvas.Children.Add(l2);
            canvas.Children.Add(l3);
            canvas.Children.Add(l4);
        }

        private void AddPointMarkers(Dictionary<string, Point> points)
        {
            foreach (var kvp in points)
            {
                Ellipse point = new Ellipse { Width = 8, Height = 8, Fill = Brushes.DarkBlue, Stroke = Brushes.DarkBlue, StrokeThickness = 2 };
                Canvas.SetLeft(point, kvp.Value.X - 4);
                Canvas.SetTop(point, kvp.Value.Y - 4);
                planCanvas.Children.Add(point);

                TextBlock label = new TextBlock { Text = kvp.Key, Foreground = Brushes.DarkBlue, FontSize = 10, FontWeight = FontWeights.Bold };
                Canvas.SetLeft(label, kvp.Value.X + 8);
                Canvas.SetTop(label, kvp.Value.Y - 12);
                planCanvas.Children.Add(label);
            }
        }

        private void AddAllDimensions(Dictionary<string, Point> points, double L_Total, double H_Total, double L_Deck, double H_Deck, double L_LeftOutlet, double L_RightOutlet, double W_LeftOuter, double H_LeftTop, double H_LeftBottom)
        {
            // Thêm các dimension theo hình vẽ của bạn
            double offset = 30;

            // Chiều dài tổng
            AddHorizontalDimension(points["P11"].X, points["P11"].Y - offset, L_Total, L_Total.ToString(), Brushes.DarkBlue);

            // Chiều dài deck
            AddHorizontalDimension(points["P0"].X, points["P0"].Y - offset, L_Deck, L_Deck.ToString(), Brushes.DarkBlue);

            // Chiều cao tổng
            AddVerticalDimension(points["P11"].X - offset, points["P11"].Y, H_Total, H_Total.ToString(), Brushes.DarkBlue);
        }

        private void AddHorizontalDimension(double x, double y, double length, string label, Brush color)
        {
            double endX = x + length * SCALE;
            Line line = new Line { X1 = x, Y1 = y, X2 = endX, Y2 = y, Stroke = color, StrokeThickness = 1 };
            planCanvas.Children.Add(line);

            Line tick1 = new Line { X1 = x, Y1 = y - 5, X2 = x, Y2 = y + 5, Stroke = color, StrokeThickness = 1 };
            Line tick2 = new Line { X1 = endX, Y1 = y - 5, X2 = endX, Y2 = y + 5, Stroke = color, StrokeThickness = 1 };
            planCanvas.Children.Add(tick1);
            planCanvas.Children.Add(tick2);

            AddLabel(label, x + length * SCALE / 2, y - 12, color, 10);
        }

        private void AddVerticalDimension(double x, double y, double length, string label, Brush color)
        {
            double endY = y + length * SCALE;
            Line line = new Line { X1 = x, Y1 = y, X2 = x, Y2 = endY, Stroke = color, StrokeThickness = 1 };
            planCanvas.Children.Add(line);

            Line tick1 = new Line { X1 = x - 5, Y1 = y, X2 = x + 5, Y2 = y, Stroke = color, StrokeThickness = 1 };
            Line tick2 = new Line { X1 = x - 5, Y1 = endY, X2 = x + 5, Y2 = endY, Stroke = color, StrokeThickness = 1 };
            planCanvas.Children.Add(tick1);
            planCanvas.Children.Add(tick2);

            AddLabel(label, x + 12, y + length * SCALE / 2, color, 10);
        }

        private void AddLabel(string text, double x, double y, Brush color, int fontSize)
        {
            TextBlock label = new TextBlock { Text = text, Foreground = color, FontSize = fontSize, FontWeight = FontWeights.Bold };
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(label, x - label.DesiredSize.Width / 2);
            Canvas.SetTop(label, y - label.DesiredSize.Height / 2);
            planCanvas.Children.Add(label);
        }
    }
}
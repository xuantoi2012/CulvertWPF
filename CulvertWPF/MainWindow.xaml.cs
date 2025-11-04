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
        private const double SCALE = 0.025; // 1mm = 0.025 pixels
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
            txtL_Total.Text = "36000";
            txtH_Total.Text = "10700";
            txtL_Deck.Text = "24500";
            txtH_Deck.Text = "4000";
            txtOffset_Top.Text = "200";
            txtOffset_Bottom.Text = "200";
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
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            currentZoom *= zoomFactor;
            currentZoom = Math.Max(0.1, Math.Min(currentZoom, 5.0));

            scaleTransform.ScaleX = currentZoom;
            scaleTransform.ScaleY = currentZoom;
            txtZoomLevel.Text = $"{(int)(currentZoom * 100)}%";
        }

        private void ZoomGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                isPanning = true;
                lastMousePosition = e.GetPosition(scrollViewer);
                zoomGrid.CaptureMouse();
                zoomGrid.Cursor = Cursors.Hand;
            }
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
            currentZoom *= 1.2;
            currentZoom = Math.Min(currentZoom, 5.0);
            scaleTransform.ScaleX = currentZoom;
            scaleTransform.ScaleY = currentZoom;
            txtZoomLevel.Text = $"{(int)(currentZoom * 100)}%";
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            currentZoom *= 0.8;
            currentZoom = Math.Max(currentZoom, 0.1);
            scaleTransform.ScaleX = currentZoom;
            scaleTransform.ScaleY = currentZoom;
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
            if (!TryGetValue(txtL_Total, out double L_Total)) return;
            if (!TryGetValue(txtH_Total, out double H_Total)) return;
            if (!TryGetValue(txtL_Deck, out double L_Deck)) return;
            if (!TryGetValue(txtH_Deck, out double H_Deck)) return;
            if (!TryGetValue(txtOffset_Top, out double Offset_Top)) return;
            if (!TryGetValue(txtOffset_Bottom, out double Offset_Bottom)) return;
            if (!TryGetValue(txtL_LeftOutlet, out double L_LeftOutlet)) return;
            if (!TryGetValue(txtH_LeftTop, out double H_LeftTop)) return;
            if (!TryGetValue(txtH_LeftBottom, out double H_LeftBottom)) return;
            if (!TryGetValue(txtW_LeftOuter, out double W_LeftOuter)) return;
            if (!TryGetValue(txtL_RightOutlet, out double L_RightOutlet)) return;
            if (!TryGetValue(txtH_RightTop, out double H_RightTop)) return;
            if (!TryGetValue(txtH_RightBottom, out double H_RightBottom)) return;
            if (!TryGetValue(txtW_RightOuter, out double W_RightOuter)) return;

            // Center point P1
            double centerX = planCanvas.Width / 2;
            double centerY = planCanvas.Height / 2;

            var points = new Dictionary<string, Point>();
            points["P1"] = new Point(centerX, centerY);

            // Calculate other points from P1
            // P2 - right along deck center
            points["P2"] = new Point(centerX + (L_Deck / 2) * SCALE, centerY);

            // P3 - far right
            points["P3"] = new Point(centerX + (L_Total / 2) * SCALE, centerY);

            // P0 - left of deck
            points["P0"] = new Point(centerX - (L_Deck / 2) * SCALE, centerY - Offset_Top * SCALE);

            // P10 - right of deck top
            points["P10"] = new Point(centerX + (L_Deck / 2) * SCALE, centerY - Offset_Top * SCALE);

            // P11 - far left
            points["P11"] = new Point(centerX - (L_Total / 2) * SCALE, centerY - H_LeftTop * SCALE);

            // P4 - left outlet inner top
            points["P4"] = new Point(centerX - (L_Deck / 2) * SCALE - L_LeftOutlet * SCALE,
                                     centerY - H_LeftTop * SCALE);

            // P5 - left outlet outer top
            points["P5"] = new Point(centerX - (L_Deck / 2) * SCALE - L_LeftOutlet * SCALE,
                                     centerY - H_LeftTop * SCALE + (H_LeftTop - H_LeftBottom) * SCALE);

            // P6 - bottom left outlet
            points["P6"] = new Point(centerX - (L_Deck / 2) * SCALE - L_LeftOutlet * SCALE,
                                     centerY + H_LeftBottom * SCALE);

            // P7 - bottom left deck
            points["P7"] = new Point(centerX - (L_Deck / 2) * SCALE,
                                     centerY + Offset_Bottom * SCALE);

            // P8 - top right deck
            points["P8"] = new Point(centerX + (L_Deck / 2) * SCALE,
                                     centerY - Offset_Top * SCALE);

            // P9 - right outlet top
            points["P9"] = new Point(centerX + (L_Deck / 2) * SCALE + L_RightOutlet * SCALE,
                                     centerY - H_RightTop * SCALE);

            // Draw main outline
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure { StartPoint = points["P1"] };

            figure.Segments.Add(new LineSegment(points["P2"], true));
            figure.Segments.Add(new LineSegment(points["P3"], true));
            figure.Segments.Add(new LineSegment(points["P10"], true));
            figure.Segments.Add(new LineSegment(points["P8"], true));
            figure.Segments.Add(new LineSegment(points["P9"], true));
            figure.Segments.Add(new LineSegment(points["P10"], true));
            figure.Segments.Add(new LineSegment(points["P0"], true));
            figure.Segments.Add(new LineSegment(points["P4"], true));
            figure.Segments.Add(new LineSegment(points["P5"], true));
            figure.Segments.Add(new LineSegment(points["P6"], true));
            figure.Segments.Add(new LineSegment(points["P7"], true));
            figure.Segments.Add(new LineSegment(points["P1"], true));

            figure.IsClosed = true;
            geometry.Figures.Add(figure);

            Path mainPath = new Path
            {
                Data = geometry,
                Stroke = Brushes.LimeGreen,
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };
            planCanvas.Children.Add(mainPath);

            // Draw deck
            DrawDeckOutline(centerX - (L_Deck / 2) * SCALE, centerY - Offset_Top * SCALE, L_Deck, H_Deck);

            // Add points
            if (chkShowPoints?.IsChecked == true)
            {
                AddPointMarkers(points);
            }

            // Add dimensions
            if (chkShowDimensions?.IsChecked == true)
            {
                AddDimensions(centerX, centerY, L_Total, L_Deck, H_Deck, L_LeftOutlet);
            }
        }

        private void DrawDeckOutline(double x, double y, double length, double height)
        {
            Rectangle deck = new Rectangle
            {
                Width = length * SCALE,
                Height = height * SCALE,
                Stroke = Brushes.Cyan,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 5, 3 }
            };
            Canvas.SetLeft(deck, x);
            Canvas.SetTop(deck, y);
            planCanvas.Children.Add(deck);

            TextBlock label = new TextBlock
            {
                Text = "BẢN QUÁ ĐỘ",
                Foreground = Brushes.Cyan,
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(label, x + length * SCALE / 2 - 40);
            Canvas.SetTop(label, y + height * SCALE / 2 - 10);
            planCanvas.Children.Add(label);
        }

        private void AddPointMarkers(Dictionary<string, Point> points)
        {
            foreach (var kvp in points)
            {
                Ellipse point = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.White,
                    Stroke = Brushes.LimeGreen,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(point, kvp.Value.X - 4);
                Canvas.SetTop(point, kvp.Value.Y - 4);
                planCanvas.Children.Add(point);

                TextBlock label = new TextBlock
                {
                    Text = kvp.Key,
                    Foreground = Brushes.Yellow,
                    FontSize = 10,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(label, kvp.Value.X + 8);
                Canvas.SetTop(label, kvp.Value.Y - 12);
                planCanvas.Children.Add(label);
            }
        }

        private void AddDimensions(double centerX, double centerY, double L_Total, double L_Deck, double H_Deck, double L_LeftOutlet)
        {
            // Total length dimension
            AddHorizontalDimension(centerX - L_Total / 2 * SCALE, centerY + 50, L_Total,
                L_Total.ToString(), Brushes.White);

            // Deck length dimension
            AddHorizontalDimension(centerX - L_Deck / 2 * SCALE, centerY - 50, L_Deck,
                L_Deck.ToString(), Brushes.Cyan);
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

            TextBlock lbl = new TextBlock
            {
                Text = label,
                Foreground = color,
                FontSize = 10,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(lbl, x + length * SCALE / 2 - 20);
            Canvas.SetTop(lbl, y - 20);
            planCanvas.Children.Add(lbl);
        }
    }
}
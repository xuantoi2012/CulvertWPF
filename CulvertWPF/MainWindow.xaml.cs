using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CulvertEditor
{
    public partial class MainWindow : Window
    {
        private const double SCALE = 0.05; // 1mm = 0.05 pixels

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
            if (IsLoaded)
            {
                DrawPlan();
            }
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            txtTotalLength.Text = "10600";
            txtTotalWidth.Text = "6000";
            txtMiddleLength.Text = "5720";
            txtTopOutletLength.Text = "2000";
            txtTopOutletInnerWidth.Text = "300";
            txtTopOutletOuterWidth.Text = "1500";
            txtTopOutletOffset.Text = "300";
            txtBottomOutletLength.Text = "2000";
            txtBottomOutletInnerWidth.Text = "300";
            txtBottomOutletOuterWidth.Text = "1500";
            txtBottomOutletOffset.Text = "300";
            txtOutletAngle.Text = "20";
            chkShowDimensions.IsChecked = true;
            chkShowHatching.IsChecked = true;
        }

        private bool TryGetValue(TextBox textBox, out double value)
        {
            return double.TryParse(textBox.Text, out value) && value > 0;
        }

        private void DrawPlan()
        {
            if (planCanvas == null) return;
            planCanvas.Children.Clear();

            // Parse all dimensions
            if (!TryGetValue(txtTotalLength, out double totalLength)) return;
            if (!TryGetValue(txtTotalWidth, out double totalWidth)) return;
            if (!TryGetValue(txtMiddleLength, out double middleLength)) return;
            if (!TryGetValue(txtTopOutletLength, out double topOutletLength)) return;
            if (!TryGetValue(txtTopOutletInnerWidth, out double topOutletInnerWidth)) return;
            if (!TryGetValue(txtTopOutletOuterWidth, out double topOutletOuterWidth)) return;
            if (!TryGetValue(txtTopOutletOffset, out double topOutletOffset)) return;
            if (!TryGetValue(txtBottomOutletLength, out double bottomOutletLength)) return;
            if (!TryGetValue(txtBottomOutletInnerWidth, out double bottomOutletInnerWidth)) return;
            if (!TryGetValue(txtBottomOutletOuterWidth, out double bottomOutletOuterWidth)) return;
            if (!TryGetValue(txtBottomOutletOffset, out double bottomOutletOffset)) return;
            if (!TryGetValue(txtOutletAngle, out double outletAngle)) return;

            double startX = 150;
            double startY = 100;

            // Create main path for the entire culvert box
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure();

            // Start at top-left corner
            figure.StartPoint = new Point(startX, startY);

            // Top edge
            figure.Segments.Add(new LineSegment(
                new Point(startX + totalWidth * SCALE, startY), true));

            // Right edge going down to top outlet
            double topOutletStart = startY + topOutletOffset * SCALE;
            figure.Segments.Add(new LineSegment(
                new Point(startX + totalWidth * SCALE, topOutletStart), true));

            // Top outlet - trapezoid shape going outward
            DrawOutletSegments(figure, startX + totalWidth * SCALE, topOutletStart,
                topOutletLength, topOutletInnerWidth, topOutletOuterWidth, outletAngle, true);

            // Continue down right edge to middle section
            double middleStart = topOutletStart + topOutletLength * SCALE;
            double middleEnd = middleStart + middleLength * SCALE;
            figure.Segments.Add(new LineSegment(
                new Point(startX + totalWidth * SCALE, middleEnd), true));

            // Bottom outlet - trapezoid shape going outward
            DrawOutletSegments(figure, startX + totalWidth * SCALE, middleEnd,
                bottomOutletLength, bottomOutletInnerWidth, bottomOutletOuterWidth, outletAngle, true);

            // Continue to bottom-right corner
            double bottomY = startY + totalLength * SCALE;
            figure.Segments.Add(new LineSegment(
                new Point(startX + totalWidth * SCALE, bottomY), true));

            // Bottom edge
            figure.Segments.Add(new LineSegment(
                new Point(startX, bottomY), true));

            // Left edge going up to bottom outlet (from left side)
            double bottomOutletStartLeft = bottomY - bottomOutletOffset * SCALE - bottomOutletLength * SCALE;
            figure.Segments.Add(new LineSegment(
                new Point(startX, bottomOutletStartLeft + bottomOutletLength * SCALE), true));

            // Bottom outlet from left side
            DrawOutletSegments(figure, startX, bottomOutletStartLeft + bottomOutletLength * SCALE,
                bottomOutletLength, bottomOutletInnerWidth, bottomOutletOuterWidth, outletAngle, false);

            // Continue up left edge to middle section
            double topOutletEndLeft = startY + topOutletOffset * SCALE + topOutletLength * SCALE;
            figure.Segments.Add(new LineSegment(
                new Point(startX, topOutletEndLeft), true));

            // Top outlet from left side
            DrawOutletSegments(figure, startX, topOutletEndLeft,
                topOutletLength, topOutletInnerWidth, topOutletOuterWidth, outletAngle, false);

            // Complete the path back to start
            figure.Segments.Add(new LineSegment(
                new Point(startX, startY), true));

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

            // Add hatching if enabled
            if (chkShowHatching.IsChecked == true)
            {
                AddHatching(startX, startY, totalWidth, totalLength, outletAngle);
            }

            // Add dimensions if enabled
            if (chkShowDimensions.IsChecked == true)
            {
                AddDimensions(startX, startY, totalLength, totalWidth, middleLength,
                    topOutletLength, topOutletOffset, bottomOutletLength, bottomOutletOffset);
            }
        }

        private void DrawOutletSegments(PathFigure figure, double startX, double startY,
            double outletLength, double innerWidth, double outerWidth, double angle, bool isRightSide)
        {
            double angleRad = angle * Math.PI / 180;
            double widthDiff = (outerWidth - innerWidth) / 2;

            if (isRightSide)
            {
                // Going outward to the right
                double outwardX = startX + outletLength * SCALE;

                // Top-right corner of outlet
                figure.Segments.Add(new LineSegment(
                    new Point(outwardX, startY - widthDiff * SCALE), true));

                // Bottom-right corner of outlet
                figure.Segments.Add(new LineSegment(
                    new Point(outwardX, startY + outletLength * SCALE + widthDiff * SCALE), true));

                // Back to the main edge
                figure.Segments.Add(new LineSegment(
                    new Point(startX, startY + outletLength * SCALE), true));
            }
            else
            {
                // Going outward to the left
                double outwardX = startX - outletLength * SCALE;

                // Going backward (upward)
                figure.Segments.Add(new LineSegment(
                    new Point(outwardX, startY + widthDiff * SCALE), true));

                figure.Segments.Add(new LineSegment(
                    new Point(outwardX, startY - outletLength * SCALE - widthDiff * SCALE), true));

                figure.Segments.Add(new LineSegment(
                    new Point(startX, startY - outletLength * SCALE), true));
            }
        }

        private void AddHatching(double startX, double startY, double width, double length, double angle)
        {
            double spacing = 20;
            double angleRad = angle * Math.PI / 180;

            // Add diagonal hatching lines
            for (double offset = -length; offset
< width + length; offset += spacing)
            {
                double x1 = startX + offset;
                double y1 = startY;
                double x2 = x1 + length * SCALE * Math.Tan(angleRad);
                double y2 = startY + length * SCALE;

                Line line = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = Brushes.LimeGreen,
                    StrokeThickness = 0.5,
                    Opacity = 0.5
                };
                planCanvas.Children.Add(line);
            }
        }

        private void AddDimensions(double startX, double startY, double totalLength, double totalWidth,
            double middleLength, double topOutletLength, double topOutletOffset,
            double bottomOutletLength, double bottomOutletOffset)
        {
            double offset = 30;

            // Total length (left side)
            AddVerticalDimension(startX - offset, startY, totalLength,
                totalLength.ToString(), Brushes.White);

            // Total width (top)
            AddHorizontalDimension(startX, startY - offset, totalWidth,
                totalWidth.ToString(), Brushes.White);

            // Middle length (right side)
            double middleStartY = startY + topOutletOffset * SCALE + topOutletLength * SCALE;
            AddVerticalDimension(startX + totalWidth * SCALE + offset, middleStartY,
                middleLength, middleLength.ToString(), Brushes.Cyan);

            // Top outlet length
            double topOutletY = startY + topOutletOffset * SCALE;
            AddVerticalDimension(startX + totalWidth * SCALE + offset * 2, topOutletY,
                topOutletLength, topOutletLength.ToString(), Brushes.Yellow);

            // Bottom outlet offset
            AddVerticalDimension(startX + totalWidth * SCALE + offset,
                startY + totalLength * SCALE - bottomOutletOffset * SCALE - bottomOutletLength * SCALE,
                bottomOutletOffset, bottomOutletOffset.ToString(), Brushes.Orange);
        }

        private void AddHorizontalDimension(double x, double y, double length, string label, Brush color)
        {
            double endX = x + length * SCALE;

            Line line = new Line
            {
                X1 = x,
                Y1 = y,
                X2 = endX,
                Y2 = y,
                Stroke = color,
                StrokeThickness = 1
            };
            planCanvas.Children.Add(line);

            Line tick1 = new Line { X1 = x, Y1 = y - 5, X2 = x, Y2 = y + 5, Stroke = color, StrokeThickness = 1 };
            Line tick2 = new Line { X1 = endX, Y1 = y - 5, X2 = endX, Y2 = y + 5, Stroke = color, StrokeThickness = 1 };
            planCanvas.Children.Add(tick1);
            planCanvas.Children.Add(tick2);

            AddLabel(label, x + length * SCALE / 2, y - 15, color, 10);
        }

        private void AddVerticalDimension(double x, double y, double length, string label, Brush color)
        {
            double endY = y + length * SCALE;

            Line line = new Line
            {
                X1 = x,
                Y1 = y,
                X2 = x,
                Y2 = endY,
                Stroke = color,
                StrokeThickness = 1
            };
            planCanvas.Children.Add(line);

            Line tick1 = new Line { X1 = x - 5, Y1 = y, X2 = x + 5, Y2 = y, Stroke = color, StrokeThickness = 1 };
            Line tick2 = new Line { X1 = x - 5, Y1 = endY, X2 = x + 5, Y2 = endY, Stroke = color, StrokeThickness = 1 };
            planCanvas.Children.Add(tick1);
            planCanvas.Children.Add(tick2);

            AddLabel(label, x + 12, y + length * SCALE / 2, color, 10);
        }

        private void AddLabel(string text, double x, double y, Brush color, int fontSize)
        {
            TextBlock label = new TextBlock
            {
                Text = text,
                Foreground = color,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold
            };
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(label, x - label.DesiredSize.Width / 2);
            Canvas.SetTop(label, y - label.DesiredSize.Height / 2);
            planCanvas.Children.Add(label);
        }
    }
}
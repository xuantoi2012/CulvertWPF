using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BridgeEditor
{
    public partial class MainWindow : Window
    {
        private const double SCALE = 0.04; // 1mm = 0.04 pixels

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => DrawBridge()),
                System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void OnDimensionChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                DrawBridge();
            }
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            txtDeckWidth.Text = "14000";
            txtDeckThickness.Text = "200";
            txtBeamHeight.Text = "2000";
            txtTopFlangeWidth.Text = "600";
            txtTopFlangeThickness.Text = "200";
            txtWebWidth.Text = "300";
            txtBottomFlangeWidth.Text = "800";
            txtBottomFlangeThickness.Text = "200";
            txtBeamSpacing.Text = "12000";
            txtBeamOffset.Text = "400";
            txtBeamAngle.Text = "20";
            chkShowHatching.IsChecked = true;
        }

        private bool TryGetValue(TextBox textBox, out double value)
        {
            return double.TryParse(textBox.Text, out value) && value > 0;
        }

        private void DrawBridge()
        {
            if (previewCanvas == null) return;
            previewCanvas.Children.Clear();

            // Parse dimensions
            if (!TryGetValue(txtDeckWidth, out double deckWidth)) return;
            if (!TryGetValue(txtDeckThickness, out double deckThickness)) return;
            if (!TryGetValue(txtBeamHeight, out double beamHeight)) return;
            if (!TryGetValue(txtTopFlangeWidth, out double topFlangeWidth)) return;
            if (!TryGetValue(txtTopFlangeThickness, out double topFlangeThickness)) return;
            if (!TryGetValue(txtWebWidth, out double webWidth)) return;
            if (!TryGetValue(txtBottomFlangeWidth, out double bottomFlangeWidth)) return;
            if (!TryGetValue(txtBottomFlangeThickness, out double bottomFlangeThickness)) return;
            if (!TryGetValue(txtBeamSpacing, out double beamSpacing)) return;
            if (!TryGetValue(txtBeamOffset, out double beamOffset)) return;
            if (!TryGetValue(txtBeamAngle, out double beamAngle)) return;

            double totalWidth = Math.Max(deckWidth, beamSpacing + bottomFlangeWidth);
            double totalHeight = beamHeight + beamOffset + deckThickness;

            double centerX = 500;
            double centerY = 100;

            // Draw deck slab
            double deckLeft = centerX - deckWidth * SCALE / 2;
            double deckTop = centerY;
            Rectangle deck = new Rectangle
            {
                Width = deckWidth * SCALE,
                Height = deckThickness * SCALE,
                Fill = Brushes.White,
                Stroke = Brushes.Blue,
                StrokeThickness = 2
            };
            Canvas.SetLeft(deck, deckLeft);
            Canvas.SetTop(deck, deckTop);
            previewCanvas.Children.Add(deck);

            // Add deck label
            AddLabel("BẢN QUÁ ĐỘ", centerX, centerY + deckThickness * SCALE / 2, Brushes.Blue, 14);

            // Draw left beam
            double leftBeamCenterX = centerX - beamSpacing * SCALE / 2;
            DrawBeam(leftBeamCenterX, centerY + deckThickness * SCALE + beamOffset * SCALE,
                beamHeight, topFlangeWidth, topFlangeThickness, webWidth,
                bottomFlangeWidth, bottomFlangeThickness, beamAngle, true);

            // Draw right beam
            double rightBeamCenterX = centerX + beamSpacing * SCALE / 2;
            DrawBeam(rightBeamCenterX, centerY + deckThickness * SCALE + beamOffset * SCALE,
                beamHeight, topFlangeWidth, topFlangeThickness, webWidth,
                bottomFlangeWidth, bottomFlangeThickness, beamAngle, false);

            // Add dimensions
            AddDimensions(centerX, centerY, deckWidth, deckThickness, beamHeight,
                beamSpacing, beamOffset, topFlangeWidth, bottomFlangeWidth);

            // Add labels
            AddLabel("THƯỢNG LƯU", 100, 30, Brushes.Blue, 12);
            AddLabel("HẠ LƯU", 900, 30, Brushes.Blue, 12);

            // Draw arrows
            DrawArrow(100, 35, 150, 35, Brushes.Blue);
            DrawArrow(900, 35, 850, 35, Brushes.Blue);
        }

        private void DrawBeam(double centerX, double topY, double height, double topWidth,
            double topThickness, double webWidth, double bottomWidth, double bottomThickness,
            double angle, bool isLeft)
        {
            double angleRad = angle * Math.PI / 180;
            double webHeight = height - topThickness - bottomThickness;

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure();

            // Start at top-left of top flange
            double topLeft = centerX - topWidth * SCALE / 2;
            figure.StartPoint = new Point(topLeft, topY);

            // Top flange
            figure.Segments.Add(new LineSegment(new Point(topLeft + topWidth * SCALE, topY), true));
            figure.Segments.Add(new LineSegment(new Point(topLeft + topWidth * SCALE, topY + topThickness * SCALE), true));

            // Right side of web (angled)
            double webTop = topY + topThickness * SCALE;
            double webRight = centerX + webWidth * SCALE / 2;
            double angleOffset = webHeight * SCALE * Math.Tan(angleRad) * (isLeft ? 1 : -1);

            figure.Segments.Add(new LineSegment(new Point(webRight, webTop), true));
            figure.Segments.Add(new LineSegment(new Point(webRight + angleOffset, webTop + webHeight * SCALE), true));

            // Bottom flange
            double bottomTop = webTop + webHeight * SCALE;
            double bottomLeft = centerX - bottomWidth * SCALE / 2;

            figure.Segments.Add(new LineSegment(new Point(bottomLeft + bottomWidth * SCALE, bottomTop), true));
            figure.Segments.Add(new LineSegment(new Point(bottomLeft + bottomWidth * SCALE, bottomTop + bottomThickness * SCALE), true));
            figure.Segments.Add(new LineSegment(new Point(bottomLeft, bottomTop + bottomThickness * SCALE), true));
            figure.Segments.Add(new LineSegment(new Point(bottomLeft, bottomTop), true));

            // Left side of web (angled)
            double webLeft = centerX - webWidth * SCALE / 2;
            figure.Segments.Add(new LineSegment(new Point(webLeft + angleOffset, webTop + webHeight * SCALE), true));
            figure.Segments.Add(new LineSegment(new Point(webLeft, webTop), true));
            figure.Segments.Add(new LineSegment(new Point(topLeft, webTop), true));

            figure.IsClosed = true;
            geometry.Figures.Add(figure);

            Path beam = new Path
            {
                Data = geometry,
                Fill = Brushes.White,
                Stroke = Brushes.Blue,
                StrokeThickness = 2
            };
            previewCanvas.Children.Add(beam);

            // Add hatching
            if (chkShowHatching.IsChecked == true)
            {
                AddHatching(geometry, Brushes.Blue);
            }
        }

        private void AddHatching(PathGeometry geometry, Brush color)
        {
            Rect bounds = geometry.Bounds;
            double spacing = 10;

            for (double x = bounds.Left; x
< bounds.Right; x += spacing)
            {
                Line line = new Line
                {
                    X1 = x,
                    Y1 = bounds.Top,
                    X2 = x + bounds.Height,
                    Y2 = bounds.Bottom,
                    Stroke = color,
                    StrokeThickness = 0.5,
                    Opacity = 0.3
                };

                // Clip to geometry
                line.Clip = new GeometryGroup { Children = { geometry } };
                previewCanvas.Children.Add(line);
            }
        }

        private void AddDimensions(double centerX, double centerY, double deckWidth,
            double deckThickness, double beamHeight, double beamSpacing, double beamOffset,
            double topWidth, double bottomWidth)
        {
            double offset = 30;

            // Deck width
            AddHorizontalDimension(centerX - deckWidth * SCALE / 2, centerY + deckThickness * SCALE + offset,
                deckWidth, deckWidth.ToString(), Brushes.Black);

            // Beam height
            double beamLeft = centerX - beamSpacing * SCALE / 2 - topWidth * SCALE / 2;
            double beamTop = centerY + deckThickness * SCALE + beamOffset * SCALE;
            AddVerticalDimension(beamLeft - offset, beamTop, beamHeight, beamHeight.ToString(), Brushes.Red);

            // Beam spacing
            double beamY = centerY + deckThickness * SCALE + beamOffset * SCALE + beamHeight * SCALE + offset;
            AddHorizontalDimension(centerX - beamSpacing * SCALE / 2, beamY,
                beamSpacing, beamSpacing.ToString(), Brushes.Black);

            // Offset from deck
            AddVerticalDimension(centerX + deckWidth * SCALE / 2 + offset,
                centerY + deckThickness * SCALE, beamOffset, beamOffset.ToString(), Brushes.Orange);
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
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 5, 3 }
            };
            previewCanvas.Children.Add(line);

            Line tick1 = new Line { X1 = x, Y1 = y - 5, X2 = x, Y2 = y + 5, Stroke = color, StrokeThickness = 1 };
            Line tick2 = new Line { X1 = endX, Y1 = y - 5, X2 = endX, Y2 = y + 5, Stroke = color, StrokeThickness = 1 };
            previewCanvas.Children.Add(tick1);
            previewCanvas.Children.Add(tick2);

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
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 5, 3 }
            };
            previewCanvas.Children.Add(line);

            Line tick1 = new Line { X1 = x - 5, Y1 = y, X2 = x + 5, Y2 = y, Stroke = color, StrokeThickness = 1 };
            Line tick2 = new Line { X1 = x - 5, Y1 = endY, X2 = x + 5, Y2 = endY, Stroke = color, StrokeThickness = 1 };
            previewCanvas.Children.Add(tick1);
            previewCanvas.Children.Add(tick2);

            AddLabel(label, x + 10, y + length * SCALE / 2, color, 10);
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
            previewCanvas.Children.Add(label);
        }

        private void DrawArrow(double x1, double y1, double x2, double y2, Brush color)
        {
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = color,
                StrokeThickness = 2
            };
            previewCanvas.Children.Add(line);

            double angle = Math.Atan2(y2 - y1, x2 - x1);
            double arrowLength = 10;

            Line arrow1 = new Line
            {
                X1 = x2,
                Y1 = y2,
                X2 = x2 - arrowLength * Math.Cos(angle - Math.PI / 6),
                Y2 = y2 - arrowLength * Math.Sin(angle - Math.PI / 6),
                Stroke = color,
                StrokeThickness = 2
            };

            Line arrow2 = new Line
            {
                X1 = x2,
                Y1 = y2,
                X2 = x2 - arrowLength * Math.Cos(angle + Math.PI / 6),
                Y2 = y2 - arrowLength * Math.Sin(angle + Math.PI / 6),
                Stroke = color,
                StrokeThickness = 2
            };

            previewCanvas.Children.Add(arrow1);
            previewCanvas.Children.Add(arrow2);
        }
    }
}
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CulvertEditor.Services
{
    public class ZoomPanService
    {
        private double zoom = 1.0;
        private Point origin;
        private Point start;
        private bool isPanning = false;
        private bool isRotating = false;

        private Grid container;
        private Canvas canvas;
        private TextBlock zoomLevelText;

        public double MinZoom { get; set; } = 0.1;
        public double MaxZoom { get; set; } = 20.0;
        public double ZoomSensitivity { get; set; } = 0.001;

        public void Initialize(Grid containerGrid, Canvas targetCanvas, TextBlock zoomText)
        {
            container = containerGrid;
            canvas = targetCanvas;
            zoomLevelText = zoomText;

            var group = new TransformGroup();
            var scaleTransform = new ScaleTransform();
            var translateTransform = new TranslateTransform();
            group.Children.Add(scaleTransform);
            group.Children.Add(translateTransform);

            canvas.RenderTransform = group;
            canvas.RenderTransformOrigin = new Point(0, 0);

            RenderOptions.SetBitmapScalingMode(canvas, BitmapScalingMode.HighQuality);
            RenderOptions.SetCachingHint(canvas, CachingHint.Cache);
            RenderOptions.SetEdgeMode(canvas, EdgeMode.Aliased);

            // ✅ MOUSE EVENTS - CAD STYLE
            container.MouseWheel += Container_MouseWheel;
            container.MouseDown += Container_MouseDown;  // ✅ Unified MouseDown
            container.MouseUp += Container_MouseUp;      // ✅ Unified MouseUp
            container.MouseMove += Container_MouseMove;
            container.MouseLeave += Container_MouseLeave;

            UpdateZoomText();
        }

        private void Container_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var group = canvas.RenderTransform as TransformGroup;
            var scaleTransform = group.Children[0] as ScaleTransform;
            var translateTransform = group.Children[1] as TranslateTransform;

            Point mousePos = e.GetPosition(canvas);

            double delta = e.Delta * ZoomSensitivity;
            double newZoom = zoom * (1 + delta);
            newZoom = Math.Max(MinZoom, Math.Min(newZoom, MaxZoom));

            if (Math.Abs(newZoom - zoom) < 0.0001)
            {
                e.Handled = true;
                return;
            }

            double factor = newZoom / zoom;

            translateTransform.X = mousePos.X - factor * (mousePos.X - translateTransform.X);
            translateTransform.Y = mousePos.Y - factor * (mousePos.Y - translateTransform.Y);

            zoom = newZoom;
            scaleTransform.ScaleX = zoom;
            scaleTransform.ScaleY = zoom;

            UpdateZoomText();
            e.Handled = true;
        }

        // ✅ UNIFIED MOUSE DOWN - CAD STYLE
        private void Container_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var group = canvas.RenderTransform as TransformGroup;
            var translateTransform = group.Children[1] as TranslateTransform;

            // ✅ MIDDLE CLICK = PAN (như CAD)
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                // Check if Shift is pressed
                bool isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

                if (isShiftPressed)
                {
                    // ✅ Shift + Middle = Rotate (for 2D views, we can ignore or implement rotation)
                    // For 2D canvas, rotation doesn't make much sense, so we skip it
                    isRotating = false;
                }
                else
                {
                    // ✅ Middle Click only = PAN
                    origin = new Point(translateTransform.X, translateTransform.Y);
                    start = e.GetPosition(container);
                    isPanning = true;

                    container.CaptureMouse();
                    container.Cursor = Cursors.SizeAll; // ✅ Pan cursor (4-way arrow)
                    e.Handled = true;
                }
            }
            // ✅ LEFT CLICK = Optional (can be used for selection)
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Left click can be used for selection or other purposes
                // For now, we don't use it for pan
            }
        }

        // ✅ UNIFIED MOUSE UP
        private void Container_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released)
            {
                isPanning = false;
                isRotating = false;
                container.ReleaseMouseCapture();
                container.Cursor = Cursors.Arrow;
                e.Handled = true;
            }
        }

        private void Container_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isPanning) return;

            var group = canvas.RenderTransform as TransformGroup;
            var translateTransform = group.Children[1] as TranslateTransform;

            Point current = e.GetPosition(container);
            Vector delta = current - start;

            translateTransform.X = origin.X + delta.X;
            translateTransform.Y = origin.Y + delta.Y;

            e.Handled = true;
        }

        private void Container_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isPanning || isRotating)
            {
                isPanning = false;
                isRotating = false;
                container.ReleaseMouseCapture();
                container.Cursor = Cursors.Arrow;
            }
        }

        public void ZoomIn()
        {
            var group = canvas.RenderTransform as TransformGroup;
            var scaleTransform = group.Children[0] as ScaleTransform;
            var translateTransform = group.Children[1] as TranslateTransform;

            double newZoom = zoom * 1.2;
            newZoom = Math.Max(MinZoom, Math.Min(newZoom, MaxZoom));

            double factor = newZoom / zoom;

            double containerCenterX = container.ActualWidth / 2;
            double containerCenterY = container.ActualHeight / 2;

            translateTransform.X = containerCenterX - factor * (containerCenterX - translateTransform.X);
            translateTransform.Y = containerCenterY - factor * (containerCenterY - translateTransform.Y);

            zoom = newZoom;
            scaleTransform.ScaleX = zoom;
            scaleTransform.ScaleY = zoom;

            UpdateZoomText();
        }

        public void ZoomOut()
        {
            var group = canvas.RenderTransform as TransformGroup;
            var scaleTransform = group.Children[0] as ScaleTransform;
            var translateTransform = group.Children[1] as TranslateTransform;

            double newZoom = zoom / 1.2;
            newZoom = Math.Max(MinZoom, Math.Min(newZoom, MaxZoom));

            double factor = newZoom / zoom;

            double containerCenterX = container.ActualWidth / 2;
            double containerCenterY = container.ActualHeight / 2;

            translateTransform.X = containerCenterX - factor * (containerCenterX - translateTransform.X);
            translateTransform.Y = containerCenterY - factor * (containerCenterY - translateTransform.Y);

            zoom = newZoom;
            scaleTransform.ScaleX = zoom;
            scaleTransform.ScaleY = zoom;

            UpdateZoomText();
        }

        public void Reset()
        {
            var group = canvas.RenderTransform as TransformGroup;
            var scaleTransform = group.Children[0] as ScaleTransform;
            var translateTransform = group.Children[1] as TranslateTransform;

            zoom = 1.0;
            scaleTransform.ScaleX = 1.0;
            scaleTransform.ScaleY = 1.0;
            translateTransform.X = 0;
            translateTransform.Y = 0;

            UpdateZoomText();
        }

        public void ZoomToFit()
        {
            if (container == null || canvas == null) return;

            var group = canvas.RenderTransform as TransformGroup;
            if (group == null || group.Children.Count < 2) return;

            var scaleTransform = group.Children[0] as ScaleTransform;
            var translateTransform = group.Children[1] as TranslateTransform;

            container.UpdateLayout();
            canvas.UpdateLayout();

            double containerWidth = container.ActualWidth;
            double containerHeight = container.ActualHeight;

            if (containerWidth <= 0 || containerHeight <= 0)
                return;

            Rect objectsBounds = CalculateObjectsBoundary();

            if (objectsBounds.IsEmpty || objectsBounds.Width <= 0 || objectsBounds.Height <= 0)
            {
                objectsBounds = new Rect(0, 0, canvas.Width, canvas.Height);
            }

            double paddingX = Math.Max(objectsBounds.Width * 0.1, 50);
            double paddingY = Math.Max(objectsBounds.Height * 0.1, 50);

            Rect paddedBounds = new Rect(
                objectsBounds.X - paddingX,
                objectsBounds.Y - paddingY,
                objectsBounds.Width + paddingX * 2,
                objectsBounds.Height + paddingY * 2
            );

            double scaleX = containerWidth / paddedBounds.Width;
            double scaleY = containerHeight / paddedBounds.Height;
            double newZoom = Math.Min(scaleX, scaleY);

            newZoom = Math.Max(MinZoom, Math.Min(newZoom, MaxZoom));

            zoom = newZoom;
            scaleTransform.ScaleX = zoom;
            scaleTransform.ScaleY = zoom;

            double objectsCenterX = paddedBounds.X + paddedBounds.Width / 2;
            double objectsCenterY = paddedBounds.Y + paddedBounds.Height / 2;

            double containerCenterX = containerWidth / 2;
            double containerCenterY = containerHeight / 2;

            translateTransform.X = containerCenterX - objectsCenterX * zoom;
            translateTransform.Y = containerCenterY - objectsCenterY * zoom;

            UpdateZoomText();
        }

        private Rect CalculateObjectsBoundary()
        {
            if (canvas == null || canvas.Children.Count == 0)
                return Rect.Empty;

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            bool foundAny = false;

            foreach (UIElement child in canvas.Children)
            {
                if (child is TextBlock)
                    continue;

                if (child is System.Windows.Shapes.Line line)
                {
                    minX = Math.Min(minX, Math.Min(line.X1, line.X2));
                    minY = Math.Min(minY, Math.Min(line.Y1, line.Y2));
                    maxX = Math.Max(maxX, Math.Max(line.X1, line.X2));
                    maxY = Math.Max(maxY, Math.Max(line.Y1, line.Y2));
                    foundAny = true;
                }
                else if (child is System.Windows.Shapes.Shape shape)
                {
                    double left = Canvas.GetLeft(shape);
                    double top = Canvas.GetTop(shape);

                    if (double.IsNaN(left)) left = 0;
                    if (double.IsNaN(top)) top = 0;

                    if (shape.ActualWidth > 0 && shape.ActualHeight > 0)
                    {
                        minX = Math.Min(minX, left);
                        minY = Math.Min(minY, top);
                        maxX = Math.Max(maxX, left + shape.ActualWidth);
                        maxY = Math.Max(maxY, top + shape.ActualHeight);
                        foundAny = true;
                    }
                }
            }

            if (!foundAny || minX == double.MaxValue)
                return Rect.Empty;

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        private void UpdateZoomText()
        {
            if (zoomLevelText != null)
            {
                zoomLevelText.Dispatcher.BeginInvoke(new Action(() =>
                {
                    zoomLevelText.Text = $"{(int)(zoom * 100)}%";
                }), System.Windows.Threading.DispatcherPriority.Render);
            }
        }

        public double GetCurrentZoom() => zoom;

        public void Cleanup()
        {
            if (container != null)
            {
                container.MouseWheel -= Container_MouseWheel;
                container.MouseDown -= Container_MouseDown;
                container.MouseUp -= Container_MouseUp;
                container.MouseMove -= Container_MouseMove;
                container.MouseLeave -= Container_MouseLeave;
            }
        }
    }
}
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

            container.MouseWheel += Container_MouseWheel;
            container.MouseLeftButtonDown += Container_MouseLeftButtonDown;
            container.MouseLeftButtonUp += Container_MouseLeftButtonUp;
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

        private void Container_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var group = canvas.RenderTransform as TransformGroup;
            var translateTransform = group.Children[1] as TranslateTransform;

            origin = new Point(translateTransform.X, translateTransform.Y);
            start = e.GetPosition(container);
            isPanning = true;

            container.CaptureMouse();
            container.Cursor = Cursors.Hand;
            e.Handled = true;
        }

        private void Container_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPanning = false;
            container.ReleaseMouseCapture();
            container.Cursor = Cursors.Arrow;
            e.Handled = true;
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
            if (isPanning)
            {
                isPanning = false;
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
            var scaleTransform = group.Children[0] as ScaleTransform;
            var translateTransform = group.Children[1] as TranslateTransform;

            double containerWidth = container.ActualWidth;
            double containerHeight = container.ActualHeight;

            if (containerWidth == 0 || containerHeight == 0)
                return;

            // ✅ FIX: Drawing luôn được vẽ centered tại canvas.Width/2, canvas.Height/2
            // Nên ta chỉ cần center canvas itself, không cần GetContentBounds()

            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;

            // Calculate zoom to fit canvas with padding
            double scaleX = containerWidth / canvasWidth;
            double scaleY = containerHeight / canvasHeight;
            double newZoom = Math.Min(scaleX, scaleY) * 0.9; // 90% padding

            newZoom = Math.Max(MinZoom, Math.Min(newZoom, MaxZoom));

            zoom = newZoom;
            scaleTransform.ScaleX = zoom;
            scaleTransform.ScaleY = zoom;

            // ✅ Center canvas origin (0,0) in container
            // Since drawing is at canvas center, this will center the drawing too
            double scaledCanvasWidth = canvasWidth * zoom;
            double scaledCanvasHeight = canvasHeight * zoom;

            translateTransform.X = (containerWidth - scaledCanvasWidth) / 2;
            translateTransform.Y = (containerHeight - scaledCanvasHeight) / 2;

            UpdateZoomText();
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
                container.MouseLeftButtonDown -= Container_MouseLeftButtonDown;
                container.MouseLeftButtonUp -= Container_MouseLeftButtonUp;
                container.MouseMove -= Container_MouseMove;
                container.MouseLeave -= Container_MouseLeave;
            }
        }
    }
}
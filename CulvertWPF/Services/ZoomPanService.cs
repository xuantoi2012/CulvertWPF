using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CulvertEditor.Services
{
    public class ZoomPanService
    {
        // Zoom/Pan state
        private double currentZoom = 1.0;
        private Point? lastMousePosition;
        private bool isPanning = false;

        // UI references
        private Grid zoomGrid;
        private ScrollViewer scrollViewer;
        private ScaleTransform scaleTransform;
        private TextBlock zoomLevelText;
        private Canvas canvas;

        // Configuration
        public double MinZoom { get; set; } = 0.1;
        public double MaxZoom { get; set; } = 5.0;
        public double ZoomSpeed { get; set; } = 1.1;

        public void Initialize(Grid grid, ScrollViewer scroll, ScaleTransform scale,
            TextBlock zoomText, Canvas targetCanvas)
        {
            zoomGrid = grid;
            scrollViewer = scroll;
            scaleTransform = scale;
            zoomLevelText = zoomText;
            canvas = targetCanvas;

            // Attach events
            zoomGrid.MouseWheel += OnMouseWheel;
            zoomGrid.MouseLeftButtonDown += OnMouseLeftButtonDown;
            zoomGrid.MouseLeftButtonUp += OnMouseLeftButtonUp;
            zoomGrid.MouseMove += OnMouseMove;
        }

        // ========== MOUSE WHEEL (ZOOM) ==========
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePos = e.GetPosition(canvas);
            double zoomFactor = e.Delta > 0 ? ZoomSpeed : 1.0 / ZoomSpeed;

            double oldZoom = currentZoom;
            currentZoom *= zoomFactor;
            currentZoom = Math.Max(MinZoom, Math.Min(currentZoom, MaxZoom));

            scaleTransform.ScaleX = currentZoom;
            scaleTransform.ScaleY = currentZoom;

            double zoomChange = currentZoom / oldZoom;
            double newOffsetX = scrollViewer.HorizontalOffset * zoomChange + mousePos.X * (zoomChange - 1);
            double newOffsetY = scrollViewer.VerticalOffset * zoomChange + mousePos.Y * (zoomChange - 1);

            scrollViewer.ScrollToHorizontalOffset(newOffsetX);
            scrollViewer.ScrollToVerticalOffset(newOffsetY);

            UpdateZoomText();
            e.Handled = true;
        }

        // ========== MOUSE LEFT BUTTON DOWN (START PAN) ==========
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPanning = true;
            lastMousePosition = e.GetPosition(scrollViewer);
            zoomGrid.CaptureMouse();
            zoomGrid.Cursor = Cursors.Hand;
        }

        // ========== MOUSE LEFT BUTTON UP (STOP PAN) ==========
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPanning = false;
            lastMousePosition = null;
            zoomGrid.ReleaseMouseCapture();
            zoomGrid.Cursor = Cursors.Arrow;
        }

        // ========== MOUSE MOVE (PAN) ==========
        private void OnMouseMove(object sender, MouseEventArgs e)
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

        // ========== ZOOM IN ==========
        public void ZoomIn()
        {
            Point centerPos = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            ZoomToPoint(1.2, centerPos);
        }

        // ========== ZOOM OUT ==========
        public void ZoomOut()
        {
            Point centerPos = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            ZoomToPoint(0.8, centerPos);
        }

        // ========== ZOOM TO POINT ==========
        private void ZoomToPoint(double factor, Point point)
        {
            double oldZoom = currentZoom;
            currentZoom *= factor;
            currentZoom = Math.Max(MinZoom, Math.Min(currentZoom, MaxZoom));

            scaleTransform.ScaleX = currentZoom;
            scaleTransform.ScaleY = currentZoom;

            double zoomChange = currentZoom / oldZoom;
            double newOffsetX = (scrollViewer.HorizontalOffset + point.X) * zoomChange - point.X;
            double newOffsetY = (scrollViewer.VerticalOffset + point.Y) * zoomChange - point.Y;

            scrollViewer.ScrollToHorizontalOffset(newOffsetX);
            scrollViewer.ScrollToVerticalOffset(newOffsetY);

            UpdateZoomText();
        }

        // ========== RESET ZOOM ==========
        public void Reset()
        {
            currentZoom = 1.0;
            scaleTransform.ScaleX = 1.0;
            scaleTransform.ScaleY = 1.0;
            scrollViewer.ScrollToHorizontalOffset(0);
            scrollViewer.ScrollToVerticalOffset(0);
            UpdateZoomText();
        }

        // ========== UPDATE ZOOM TEXT ==========
        private void UpdateZoomText()
        {
            if (zoomLevelText != null)
                zoomLevelText.Text = $"{(int)(currentZoom * 100)}%";
        }

        // ========== CLEANUP ==========
        public void Cleanup()
        {
            if (zoomGrid != null)
            {
                zoomGrid.MouseWheel -= OnMouseWheel;
                zoomGrid.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                zoomGrid.MouseLeftButtonUp -= OnMouseLeftButtonUp;
                zoomGrid.MouseMove -= OnMouseMove;
            }
        }
    }
}
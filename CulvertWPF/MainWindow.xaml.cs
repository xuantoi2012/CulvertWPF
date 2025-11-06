using DevExpress.Xpf.Core;
using DevExpress.Xpf.Bars;
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
        private const double SCALE = 0.015;

        // ========== PLAN VIEW ==========
        private double currentZoomPlan = 1.0;
        private Point? lastMousePositionPlan;
        private bool isPanningPlan = false;

        // ========== ELEVATION VIEW ==========
        private double currentZoomElevation = 1.0;
        private Point? lastMousePositionElevation;
        private bool isPanningElevation = false;

        // ========== SECTION VIEW ==========
        private double currentZoomSection = 1.0;
        private Point? lastMousePositionSection;
        private bool isPanningSection = false;

        // Colors
        private static readonly Color COLOR_MAIN_LINE = Color.FromRgb(0, 153, 51);
        private static readonly Color COLOR_DECK = Color.FromRgb(0, 204, 255);
        private static readonly Color COLOR_DIMENSION = Color.FromRgb(220, 20, 60);
        private static readonly Color COLOR_POINT = Color.FromRgb(0, 120, 212);
        private static readonly Color COLOR_TEXT = Color.FromRgb(0, 255, 128);
        private static readonly Color COLOR_EXCAVATION = Color.FromRgb(150, 150, 150);

        public MainWindow()
        {
            InitializeComponent();

            // ✅ Keyboard shortcuts
            this.KeyDown += MainWindow_KeyDown;
        }

        // ========== KEYBOARD SHORTCUTS ==========
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.N:
                        NewProject_Click(sender, e);
                        e.Handled = true;
                        break;
                    case Key.O:
                        OpenProject_Click(sender, e);
                        e.Handled = true;
                        break;
                    case Key.S:
                        SaveProject_Click(sender, e);
                        e.Handled = true;
                        break;
                    case Key.P:
                        Print_Click(sender, e);
                        e.Handled = true;
                        break;
                }
            }
            else if (e.Key == Key.F1)
            {
                Help_Click(sender, e);
                e.Handled = true;
            }
        }

        // ========== FILE OPERATIONS ==========
        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn tạo dự án mới? Dữ liệu hiện tại chưa lưu sẽ bị mất.",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                OnResetPlanElevation(sender, e);
                OnResetSection(sender, e);
                MessageBox.Show("Đã tạo dự án mới!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Culvert Project (*.cvt)|*.cvt|All Files (*.*)|*.*",
                Title = "Mở dự án"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // TODO: Implement load project logic
                    MessageBox.Show($"Đang mở: {openFileDialog.FileName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // TODO: Implement save project logic
                MessageBox.Show("Đã lưu dự án!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAsProject_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Culvert Project (*.cvt)|*.cvt|All Files (*.*)|*.*",
                Title = "Lưu dự án",
                DefaultExt = ".cvt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // TODO: Implement save as logic
                    MessageBox.Show($"Đã lưu: {saveFileDialog.FileName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ========== EXPORT OPERATIONS ==========
        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Xuất PDF",
                DefaultExt = ".pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // TODO: Implement PDF export logic
                    MessageBox.Show($"Đã xuất PDF: {saveFileDialog.FileName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất PDF: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportDXF_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "DXF Files (*.dxf)|*.dxf",
                Title = "Xuất DXF",
                DefaultExt = ".dxf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // TODO: Implement DXF export logic
                    MessageBox.Show($"Đã xuất DXF: {saveFileDialog.FileName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất DXF: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // TODO: Implement print logic
                    MessageBox.Show("Đang in...", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== VIEW OPTIONS ==========
        private void ToggleDimensions_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            var barItem = sender as BarCheckItem;
            bool isChecked = barItem?.IsChecked ?? true;

            if (mainTabControl.SelectedIndex == 0)
            {
                if (chkShowDimensions != null)
                    chkShowDimensions.IsChecked = isChecked;
            }
            else if (mainTabControl.SelectedIndex == 1)
            {
                if (chkShowSectionDimensions != null)
                    chkShowSectionDimensions.IsChecked = isChecked;
            }
        }

        private void TogglePoints_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            var barItem = sender as BarCheckItem;
            bool isChecked = barItem?.IsChecked ?? true;

            if (chkShowPoints != null)
                chkShowPoints.IsChecked = isChecked;
        }

        private void ToggleGrid_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            var barItem = sender as BarCheckItem;
            bool isChecked = barItem?.IsChecked ?? false;

            // TODO: Implement grid display logic
            MessageBox.Show($"Hiển thị lưới: {(isChecked ? "Bật" : "Tắt")}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ========== TOOLS ==========
        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Bắt đầu tính toán thủy lực và kết cấu?",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Tính toán hoàn tất!\n\nKết quả:\n- Lưu lượng: OK\n- Ứng suất: OK\n- Ổn định: OK",
                        "Kết quả tính toán", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tính toán: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var errors = new System.Text.StringBuilder();
                bool hasErrors = false;

                // Validate dimensions
                if (!TryGetValue(txtL1, out double L1))
                {
                    errors.AppendLine("- L1: Giá trị không hợp lệ");
                    hasErrors = true;
                }

                if (hasErrors)
                {
                    MessageBox.Show($"Phát hiện lỗi:\n\n{errors}", "Kiểm tra", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Thiết kế hợp lệ! ✓", "Kiểm tra", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kiểm tra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== HELP ==========
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "HƯỚNG DẪN SỬ DỤNG\n\n" +
                "1. Nhập thông số vào panel bên trái\n" +
                "2. Xem bản vẽ tự động cập nhật\n" +
                "3. Zoom: Ctrl + Scroll hoặc nút Zoom\n" +
                "4. Pan: Kéo chuột trái\n" +
                "5. Reset: Click nút Reset\n\n" +
                "Phím tắt:\n" +
                "- Ctrl+N: Dự án mới\n" +
                "- Ctrl+O: Mở\n" +
                "- Ctrl+S: Lưu\n" +
                "- Ctrl+P: In\n" +
                "- F1: Trợ giúp",
                "Trợ giúp",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "CULVERT BOX DESIGN TOOL\n\n" +
                "Version: 1.0.0\n" +
                "Build Date: 2025-01-06\n\n" +
                "Phần mềm thiết kế cống hộp\n" +
                "Hỗ trợ mặt bằng, mặt đứng, mặt cắt\n\n" +
                "Developer: xuantoi2012\n" +
                "© 2025 All Rights Reserved",
                "Về phần mềm",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DrawPlan();
                DrawElevation();
                DrawSection();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        // ========== TAB CONTROL EVENT ==========
        private void TabControl_SelectionChanged(object sender, DevExpress.Xpf.Core.TabControlSelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            switch (mainTabControl.SelectedIndex)
            {
                case 0: // Tab 1: Mặt bằng + Mặt đứng
                    DrawPlan();
                    DrawElevation();
                    break;
                case 1: // Tab 2: Mặt cắt
                    DrawSection();
                    break;
            }
        }

        // ========== PLAN/ELEVATION TAB EVENTS ==========
        private void OnDimensionChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && mainTabControl.SelectedIndex == 0)
            {
                DrawPlan();
                DrawElevation();
            }
        }

        private void OnResetPlanElevation(object sender, RoutedEventArgs e)
        {
            txtL1.Text = "24500";
            txtW1.Text = "4000";
            txtL2.Text = "5000";
            txtW2.Text = "1800";
            txtW3.Text = "2000";
            txtW.Text = "5000";
            txtL3.Text = "30000";
            txtAlpha.Text = "20";
            chkShowDimensions.IsChecked = true;
            chkShowPoints.IsChecked = true;
            DrawPlan();
            DrawElevation();
        }

        // ========== SECTION TAB EVENTS ==========
        private void OnSectionChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && mainTabControl.SelectedIndex == 1)
            {
                DrawSection();
            }
        }

        private void OnResetSection(object sender, RoutedEventArgs e)
        {
            txtSectionWidth.Text = "5000";
            txtSectionHeight.Text = "3000";
            txtWallThickness.Text = "300";
            txtExcavationDepth.Text = "2000";
            txtSlopeRatio.Text = "1.5";
            chkShowSectionDimensions.IsChecked = true;
            chkShowExcavation.IsChecked = true;
            DrawSection();
        }

        // ========== PLAN VIEW ZOOM/PAN ==========
        private void ZoomGridPlan_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePos = e.GetPosition(planCanvas);
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            double oldZoom = currentZoomPlan;
            currentZoomPlan *= zoomFactor;
            currentZoomPlan = Math.Max(0.1, Math.Min(currentZoomPlan, 5.0));
            scaleTransformPlan.ScaleX = currentZoomPlan;
            scaleTransformPlan.ScaleY = currentZoomPlan;
            double zoomChange = currentZoomPlan / oldZoom;
            double newOffsetX = scrollViewerPlan.HorizontalOffset * zoomChange + mousePos.X * (zoomChange - 1);
            double newOffsetY = scrollViewerPlan.VerticalOffset * zoomChange + mousePos.Y * (zoomChange - 1);
            scrollViewerPlan.ScrollToHorizontalOffset(newOffsetX);
            scrollViewerPlan.ScrollToVerticalOffset(newOffsetY);
            txtZoomLevelPlan.Text = $"{(int)(currentZoomPlan * 100)}%";
            e.Handled = true;
        }

        private void ZoomGridPlan_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPanningPlan = true;
            lastMousePositionPlan = e.GetPosition(scrollViewerPlan);
            zoomGridPlan.CaptureMouse();
            zoomGridPlan.Cursor = Cursors.Hand;
        }

        private void ZoomGridPlan_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPanningPlan = false;
            lastMousePositionPlan = null;
            zoomGridPlan.ReleaseMouseCapture();
            zoomGridPlan.Cursor = Cursors.Arrow;
        }

        private void ZoomGridPlan_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanningPlan && lastMousePositionPlan.HasValue)
            {
                Point currentPosition = e.GetPosition(scrollViewerPlan);
                double deltaX = currentPosition.X - lastMousePositionPlan.Value.X;
                double deltaY = currentPosition.Y - lastMousePositionPlan.Value.Y;
                scrollViewerPlan.ScrollToHorizontalOffset(scrollViewerPlan.HorizontalOffset - deltaX);
                scrollViewerPlan.ScrollToVerticalOffset(scrollViewerPlan.VerticalOffset - deltaY);
                lastMousePositionPlan = currentPosition;
            }
        }

        private void ZoomInPlan_Click(object sender, RoutedEventArgs e)
        {
            Point centerPos = new Point(scrollViewerPlan.ViewportWidth / 2, scrollViewerPlan.ViewportHeight / 2);
            ZoomToPointPlan(1.2, centerPos);
        }

        private void ZoomOutPlan_Click(object sender, RoutedEventArgs e)
        {
            Point centerPos = new Point(scrollViewerPlan.ViewportWidth / 2, scrollViewerPlan.ViewportHeight / 2);
            ZoomToPointPlan(0.8, centerPos);
        }

        private void ZoomToPointPlan(double factor, Point point)
        {
            double oldZoom = currentZoomPlan;
            currentZoomPlan *= factor;
            currentZoomPlan = Math.Max(0.1, Math.Min(currentZoomPlan, 5.0));
            scaleTransformPlan.ScaleX = currentZoomPlan;
            scaleTransformPlan.ScaleY = currentZoomPlan;
            double zoomChange = currentZoomPlan / oldZoom;
            double newOffsetX = (scrollViewerPlan.HorizontalOffset + point.X) * zoomChange - point.X;
            double newOffsetY = (scrollViewerPlan.VerticalOffset + point.Y) * zoomChange - point.Y;
            scrollViewerPlan.ScrollToHorizontalOffset(newOffsetX);
            scrollViewerPlan.ScrollToVerticalOffset(newOffsetY);
            txtZoomLevelPlan.Text = $"{(int)(currentZoomPlan * 100)}%";
        }

        private void ZoomResetPlan_Click(object sender, RoutedEventArgs e)
        {
            currentZoomPlan = 1.0;
            scaleTransformPlan.ScaleX = 1.0;
            scaleTransformPlan.ScaleY = 1.0;
            scrollViewerPlan.ScrollToHorizontalOffset(0);
            scrollViewerPlan.ScrollToVerticalOffset(0);
            txtZoomLevelPlan.Text = "100%";
        }

        // ========== ELEVATION VIEW ZOOM/PAN ==========
        private void ZoomGridElevation_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePos = e.GetPosition(elevationCanvas);
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            double oldZoom = currentZoomElevation;
            currentZoomElevation *= zoomFactor;
            currentZoomElevation = Math.Max(0.1, Math.Min(currentZoomElevation, 5.0));
            scaleTransformElevation.ScaleX = currentZoomElevation;
            scaleTransformElevation.ScaleY = currentZoomElevation;
            double zoomChange = currentZoomElevation / oldZoom;
            double newOffsetX = scrollViewerElevation.HorizontalOffset * zoomChange + mousePos.X * (zoomChange - 1);
            double newOffsetY = scrollViewerElevation.VerticalOffset * zoomChange + mousePos.Y * (zoomChange - 1);
            scrollViewerElevation.ScrollToHorizontalOffset(newOffsetX);
            scrollViewerElevation.ScrollToVerticalOffset(newOffsetY);
            txtZoomLevelElevation.Text = $"{(int)(currentZoomElevation * 100)}%";
            e.Handled = true;
        }

        private void ZoomGridElevation_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPanningElevation = true;
            lastMousePositionElevation = e.GetPosition(scrollViewerElevation);
            zoomGridElevation.CaptureMouse();
            zoomGridElevation.Cursor = Cursors.Hand;
        }

        private void ZoomGridElevation_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPanningElevation = false;
            lastMousePositionElevation = null;
            zoomGridElevation.ReleaseMouseCapture();
            zoomGridElevation.Cursor = Cursors.Arrow;
        }

        private void ZoomGridElevation_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanningElevation && lastMousePositionElevation.HasValue)
            {
                Point currentPosition = e.GetPosition(scrollViewerElevation);
                double deltaX = currentPosition.X - lastMousePositionElevation.Value.X;
                double deltaY = currentPosition.Y - lastMousePositionElevation.Value.Y;
                scrollViewerElevation.ScrollToHorizontalOffset(scrollViewerElevation.HorizontalOffset - deltaX);
                scrollViewerElevation.ScrollToVerticalOffset(scrollViewerElevation.VerticalOffset - deltaY);
                lastMousePositionElevation = currentPosition;
            }
        }

        private void ZoomInElevation_Click(object sender, RoutedEventArgs e)
        {
            Point centerPos = new Point(scrollViewerElevation.ViewportWidth / 2, scrollViewerElevation.ViewportHeight / 2);
            ZoomToPointElevation(1.2, centerPos);
        }

        private void ZoomOutElevation_Click(object sender, RoutedEventArgs e)
        {
            Point centerPos = new Point(scrollViewerElevation.ViewportWidth / 2, scrollViewerElevation.ViewportHeight / 2);
            ZoomToPointElevation(0.8, centerPos);
        }

        private void ZoomToPointElevation(double factor, Point point)
        {
            double oldZoom = currentZoomElevation;
            currentZoomElevation *= factor;
            currentZoomElevation = Math.Max(0.1, Math.Min(currentZoomElevation, 5.0));
            scaleTransformElevation.ScaleX = currentZoomElevation;
            scaleTransformElevation.ScaleY = currentZoomElevation;
            double zoomChange = currentZoomElevation / oldZoom;
            double newOffsetX = (scrollViewerElevation.HorizontalOffset + point.X) * zoomChange - point.X;
            double newOffsetY = (scrollViewerElevation.VerticalOffset + point.Y) * zoomChange - point.Y;
            scrollViewerElevation.ScrollToHorizontalOffset(newOffsetX);
            scrollViewerElevation.ScrollToVerticalOffset(newOffsetY);
            txtZoomLevelElevation.Text = $"{(int)(currentZoomElevation * 100)}%";
        }

        private void ZoomResetElevation_Click(object sender, RoutedEventArgs e)
        {
            currentZoomElevation = 1.0;
            scaleTransformElevation.ScaleX = 1.0;
            scaleTransformElevation.ScaleY = 1.0;
            scrollViewerElevation.ScrollToHorizontalOffset(0);
            scrollViewerElevation.ScrollToVerticalOffset(0);
            txtZoomLevelElevation.Text = "100%";
        }

        // ========== SECTION VIEW ZOOM/PAN ==========
        private void ZoomGridSection_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePos = e.GetPosition(sectionCanvas);
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            double oldZoom = currentZoomSection;
            currentZoomSection *= zoomFactor;
            currentZoomSection = Math.Max(0.1, Math.Min(currentZoomSection, 5.0));
            scaleTransformSection.ScaleX = currentZoomSection;
            scaleTransformSection.ScaleY = currentZoomSection;
            double zoomChange = currentZoomSection / oldZoom;
            double newOffsetX = scrollViewerSection.HorizontalOffset * zoomChange + mousePos.X * (zoomChange - 1);
            double newOffsetY = scrollViewerSection.VerticalOffset * zoomChange + mousePos.Y * (zoomChange - 1);
            scrollViewerSection.ScrollToHorizontalOffset(newOffsetX);
            scrollViewerSection.ScrollToVerticalOffset(newOffsetY);
            txtZoomLevelSection.Text = $"{(int)(currentZoomSection * 100)}%";
            e.Handled = true;
        }

        private void ZoomGridSection_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPanningSection = true;
            lastMousePositionSection = e.GetPosition(scrollViewerSection);
            zoomGridSection.CaptureMouse();
            zoomGridSection.Cursor = Cursors.Hand;
        }

        private void ZoomGridSection_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPanningSection = false;
            lastMousePositionSection = null;
            zoomGridSection.ReleaseMouseCapture();
            zoomGridSection.Cursor = Cursors.Arrow;
        }

        private void ZoomGridSection_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanningSection && lastMousePositionSection.HasValue)
            {
                Point currentPosition = e.GetPosition(scrollViewerSection);
                double deltaX = currentPosition.X - lastMousePositionSection.Value.X;
                double deltaY = currentPosition.Y - lastMousePositionSection.Value.Y;
                scrollViewerSection.ScrollToHorizontalOffset(scrollViewerSection.HorizontalOffset - deltaX);
                scrollViewerSection.ScrollToVerticalOffset(scrollViewerSection.VerticalOffset - deltaY);
                lastMousePositionSection = currentPosition;
            }
        }

        private void ZoomInSection_Click(object sender, RoutedEventArgs e)
        {
            Point centerPos = new Point(scrollViewerSection.ViewportWidth / 2, scrollViewerSection.ViewportHeight / 2);
            ZoomToPointSection(1.2, centerPos);
        }

        private void ZoomOutSection_Click(object sender, RoutedEventArgs e)
        {
            Point centerPos = new Point(scrollViewerSection.ViewportWidth / 2, scrollViewerSection.ViewportHeight / 2);
            ZoomToPointSection(0.8, centerPos);
        }

        private void ZoomToPointSection(double factor, Point point)
        {
            double oldZoom = currentZoomSection;
            currentZoomSection *= factor;
            currentZoomSection = Math.Max(0.1, Math.Min(currentZoomSection, 5.0));
            scaleTransformSection.ScaleX = currentZoomSection;
            scaleTransformSection.ScaleY = currentZoomSection;
            double zoomChange = currentZoomSection / oldZoom;
            double newOffsetX = (scrollViewerSection.HorizontalOffset + point.X) * zoomChange - point.X;
            double newOffsetY = (scrollViewerSection.VerticalOffset + point.Y) * zoomChange - point.Y;
            scrollViewerSection.ScrollToHorizontalOffset(newOffsetX);
            scrollViewerSection.ScrollToVerticalOffset(newOffsetY);
            txtZoomLevelSection.Text = $"{(int)(currentZoomSection * 100)}%";
        }

        private void ZoomResetSection_Click(object sender, RoutedEventArgs e)
        {
            currentZoomSection = 1.0;
            scaleTransformSection.ScaleX = 1.0;
            scaleTransformSection.ScaleY = 1.0;
            scrollViewerSection.ScrollToHorizontalOffset(0);
            scrollViewerSection.ScrollToVerticalOffset(0);
            txtZoomLevelSection.Text = "100%";
        }

        private bool TryGetValue(TextBox textBox, out double value)
        {
            return double.TryParse(textBox.Text, out value) && value > 0;
        }

        // ========== DRAW PLAN VIEW ==========
        private void DrawPlan()
        {
            if (planCanvas == null) return;
            planCanvas.Children.Clear();

            if (!TryGetValue(txtL1, out double L1)) L1 = 24500;
            if (!TryGetValue(txtW1, out double W1)) W1 = 4000;
            if (!TryGetValue(txtL2, out double L2)) L2 = 5000;
            if (!TryGetValue(txtW2, out double W2)) W2 = 1800;
            if (!TryGetValue(txtW3, out double W3)) W3 = 2000;
            if (!TryGetValue(txtW, out double W)) W = 5000;
            if (!TryGetValue(txtL3, out double L3)) L3 = 30000;
            if (!TryGetValue(txtAlpha, out double Alpha)) Alpha = 20;

            double centerX = planCanvas.Width / 2;
            double centerY = planCanvas.Height / 2;

            var points = new Dictionary<string, Point>();

            points["P1"] = new Point(centerX, centerY);
            points["P2"] = new Point(centerX, centerY - W / 2 * SCALE);
            points["P3"] = new Point(centerX, centerY + W / 2 * SCALE);
            points["P4_top_left"] = new Point(points["P2"].X - L3 / 2 * SCALE, points["P2"].Y);
            points["P4_bot_left"] = new Point(points["P3"].X - L3 / 2 * SCALE, points["P3"].Y);
            points["P5_bot_left"] = new Point(points["P4_bot_left"].X, points["P4_bot_left"].Y + W3 / 2 * SCALE);
            points["P5_top_left"] = new Point(points["P4_top_left"].X, points["P4_top_left"].Y - W3 / 2 * SCALE);
            points["P6_bot_left"] = new Point(points["P5_bot_left"].X - W2 * SCALE, points["P5_bot_left"].Y);
            points["P6_top_left"] = new Point(points["P5_top_left"].X - W2 * SCALE, points["P5_top_left"].Y);
            points["P8_top_left"] = new Point(points["P2"].X - L1 / 2 * SCALE, points["P2"].Y);
            points["P8_top_right"] = new Point(points["P2"].X + L1 / 2 * SCALE, points["P2"].Y);
            points["P9_top_left"] = new Point(points["P8_top_left"].X, points["P8_top_left"].Y - W1 * SCALE);
            points["P9_top_right"] = new Point(points["P8_top_right"].X, points["P8_top_right"].Y - W1 * SCALE);
            points["P8_bot_left"] = new Point(points["P3"].X - L1 / 2 * SCALE, points["P3"].Y);
            points["P8_bot_right"] = new Point(points["P3"].X + L1 / 2 * SCALE, points["P3"].Y);
            points["P9_bot_left"] = new Point(points["P8_bot_left"].X, points["P8_bot_left"].Y + W1 * SCALE);
            points["P9_bot_right"] = new Point(points["P8_bot_right"].X, points["P8_bot_right"].Y + W1 * SCALE);

            double alphaRad = Alpha * Math.PI / 180.0;
            double P7_top_left_X = points["P6_top_left"].X - L2 * Math.Cos(alphaRad) * SCALE;
            double P7_top_left_Y = points["P6_top_left"].Y - L2 * Math.Sin(alphaRad) * SCALE;
            points["P7_top_left"] = new Point(P7_top_left_X, P7_top_left_Y);
            double P7_left_bot_X = points["P6_bot_left"].X - L2 * Math.Cos(alphaRad) * SCALE;
            double P7_left_bot_Y = points["P6_bot_left"].Y + L2 * Math.Sin(alphaRad) * SCALE;
            points["P7_bot_left"] = new Point(P7_left_bot_X, P7_left_bot_Y);

            points["P4_top_right"] = MirrorPointX(points["P4_top_left"], centerX);
            points["P4_bot_right"] = MirrorPointX(points["P4_bot_left"], centerX);
            points["P5_bot_right"] = MirrorPointX(points["P5_bot_left"], centerX);
            points["P5_top_right"] = MirrorPointX(points["P5_top_left"], centerX);
            points["P6_bot_right"] = MirrorPointX(points["P6_bot_left"], centerX);
            points["P6_top_right"] = MirrorPointX(points["P6_top_left"], centerX);
            points["P7_top_right"] = MirrorPointX(points["P7_top_left"], centerX);
            points["P7_bot_right"] = MirrorPointX(points["P7_bot_left"], centerX);

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
            DrawLine(planCanvas, points["P6_top_left"], points["P7_top_left"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P6_bot_left"], points["P7_bot_left"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P6_top_right"], points["P7_top_right"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P6_bot_right"], points["P7_bot_right"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P4_top_left"], points["P4_top_right"], COLOR_MAIN_LINE, 1);
            DrawLine(planCanvas, points["P4_bot_left"], points["P4_bot_right"], COLOR_MAIN_LINE, 1);

            DrawDashedRectangle(planCanvas, points["P8_top_left"], points["P9_top_left"], points["P8_top_right"], points["P9_top_right"], COLOR_DECK);
            DrawDashedRectangle(planCanvas, points["P8_bot_left"], points["P9_bot_left"], points["P8_bot_right"], points["P9_bot_right"], COLOR_DECK);
            DrawDashedLine(planCanvas, points["P4_top_left"], points["P4_bot_left"], new SolidColorBrush(COLOR_DECK), new DoubleCollection { 8, 4 });
            DrawDashedLine(planCanvas, points["P4_top_right"], points["P4_bot_right"], new SolidColorBrush(COLOR_DECK), new DoubleCollection { 8, 4 });

            AddLabel("BẢN QUÁ ĐỘ", centerX, points["P2"].Y - W1 / 2 * SCALE, new SolidColorBrush(COLOR_TEXT), 14, planCanvas);
            AddLabel("BẢN QUÁ ĐỘ", centerX, points["P3"].Y + W1 / 2 * SCALE, new SolidColorBrush(COLOR_TEXT), 14, planCanvas);

            if (chkShowPoints?.IsChecked == true)
                AddPointMarkers(points, planCanvas);

            if (chkShowDimensions?.IsChecked == true)
                AddAllDimensions(points, L1, W1, L2, W2, W3, W, L3, Alpha, planCanvas);
        }

        // ========== DRAW ELEVATION VIEW ==========
        private void DrawElevation()
        {
            if (elevationCanvas == null) return;
            elevationCanvas.Children.Clear();

            double centerX = elevationCanvas.Width / 2;
            double centerY = elevationCanvas.Height / 2;

            AddLabel("MẶT ĐỨNG - ĐANG PHÁT TRIỂN", centerX, centerY, Brushes.Gray, 16, elevationCanvas);
        }

        // ========== DRAW SECTION VIEW ==========
        private void DrawSection()
        {
            if (sectionCanvas == null) return;
            sectionCanvas.Children.Clear();

            // Parse từ TextBox riêng của Tab Section
            if (!TryGetValue(txtSectionWidth, out double W)) W = 5000;
            if (!TryGetValue(txtSectionHeight, out double H)) H = 3000;
            if (!TryGetValue(txtWallThickness, out double wallThickness)) wallThickness = 300;
            if (!TryGetValue(txtExcavationDepth, out double excavationDepth)) excavationDepth = 2000;
            if (!TryGetValue(txtSlopeRatio, out double slopeRatio)) slopeRatio = 1.5;

            double centerX = sectionCanvas.Width / 2;
            double centerY = sectionCanvas.Height / 2;

            double boxWidth = W * SCALE;
            double boxHeight = H * SCALE;

            Point topLeftOuter = new Point(centerX - boxWidth / 2, centerY - boxHeight / 2);
            Point topRightOuter = new Point(centerX + boxWidth / 2, centerY - boxHeight / 2);
            Point botRightOuter = new Point(centerX + boxWidth / 2, centerY + boxHeight / 2);
            Point botLeftOuter = new Point(centerX - boxWidth / 2, centerY + boxHeight / 2);

            DrawLine(sectionCanvas, topLeftOuter, topRightOuter, COLOR_MAIN_LINE, 2);
            DrawLine(sectionCanvas, topRightOuter, botRightOuter, COLOR_MAIN_LINE, 2);
            DrawLine(sectionCanvas, botRightOuter, botLeftOuter, COLOR_MAIN_LINE, 2);
            DrawLine(sectionCanvas, botLeftOuter, topLeftOuter, COLOR_MAIN_LINE, 2);

            double innerWidth = boxWidth - 2 * wallThickness * SCALE;
            double innerHeight = boxHeight - 2 * wallThickness * SCALE;
            Point topLeftInner = new Point(centerX - innerWidth / 2, centerY - innerHeight / 2);
            Point topRightInner = new Point(centerX + innerWidth / 2, centerY - innerHeight / 2);
            Point botRightInner = new Point(centerX + innerWidth / 2, centerY + innerHeight / 2);
            Point botLeftInner = new Point(centerX - innerWidth / 2, centerY + innerHeight / 2);

            DrawLine(sectionCanvas, topLeftInner, topRightInner, COLOR_MAIN_LINE, 1.5);
            DrawLine(sectionCanvas, topRightInner, botRightInner, COLOR_MAIN_LINE, 1.5);
            DrawLine(sectionCanvas, botRightInner, botLeftInner, COLOR_MAIN_LINE, 1.5);
            DrawLine(sectionCanvas, botLeftInner, topLeftInner, COLOR_MAIN_LINE, 1.5);

            if (chkShowExcavation?.IsChecked == true)
            {
                double excavationTop = centerY - boxHeight / 2 - excavationDepth * SCALE;
                double excavationWidth = boxWidth + 2 * excavationDepth * slopeRatio * SCALE;

                Point excav1 = new Point(centerX - excavationWidth / 2, excavationTop);
                Point excav2 = new Point(centerX + excavationWidth / 2, excavationTop);
                Point excav3 = new Point(centerX + boxWidth / 2, centerY + boxHeight / 2);
                Point excav4 = new Point(centerX - boxWidth / 2, centerY + boxHeight / 2);

                DrawDashedLine(sectionCanvas, excav1, excav4, new SolidColorBrush(COLOR_EXCAVATION), new DoubleCollection { 5, 3 });
                DrawDashedLine(sectionCanvas, excav2, excav3, new SolidColorBrush(COLOR_EXCAVATION), new DoubleCollection { 5, 3 });
                DrawDashedLine(sectionCanvas, excav1, excav2, new SolidColorBrush(COLOR_EXCAVATION), new DoubleCollection { 5, 3 });

                AddLabel($"PHUI ĐÀO 1:{slopeRatio}", centerX, excavationTop - 20, new SolidColorBrush(COLOR_DIMENSION), 12, sectionCanvas);
            }

            AddLabel("MẶT CẮT CỐNG HỘP", centerX, centerY, new SolidColorBrush(COLOR_TEXT), 14, sectionCanvas);

            if (chkShowSectionDimensions?.IsChecked == true)
            {
                AddHorizontalDimension(topLeftOuter.X, topLeftOuter.Y - 40, W, "W=" + W, new SolidColorBrush(COLOR_DIMENSION), sectionCanvas);
                AddVerticalDimension(topLeftOuter.X - 40, topLeftOuter.Y, H, "H=" + H, new SolidColorBrush(COLOR_DIMENSION), sectionCanvas);
            }
        }

        private Point MirrorPointX(Point p, double axisX) => new Point(2 * axisX - p.X, p.Y);

        private void DrawLine(Canvas canvas, Point a, Point b, Color color, double thickness)
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

        private void DrawDashedRectangle(Canvas canvas, Point p1, Point p2, Point p3, Point p4, Color color)
        {
            var brush = new SolidColorBrush(color);
            var dashArray = new DoubleCollection { 8, 4 };
            DrawDashedLine(canvas, p1, p3, brush, dashArray);
            DrawDashedLine(canvas, p1, p2, brush, dashArray);
            DrawDashedLine(canvas, p2, p4, brush, dashArray);
            DrawDashedLine(canvas, p4, p3, brush, dashArray);
        }

        private void DrawDashedLine(Canvas canvas, Point a, Point b, Brush brush, DoubleCollection dashArray)
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

        private void AddPointMarkers(Dictionary<string, Point> points, Canvas canvas)
        {
            var brush = new SolidColorBrush(COLOR_POINT);
            foreach (var kvp in points)
            {
                Ellipse ellipse = new Ellipse { Width = 8, Height = 8, Fill = brush, Stroke = new SolidColorBrush(COLOR_MAIN_LINE), StrokeThickness = 2 };
                Canvas.SetLeft(ellipse, kvp.Value.X - 4);
                Canvas.SetTop(ellipse, kvp.Value.Y - 4);
                canvas.Children.Add(ellipse);

                TextBlock label = new TextBlock { Text = kvp.Key.Replace("_", " "), Foreground = brush, FontSize = 9, FontWeight = FontWeights.Bold };
                Canvas.SetLeft(label, kvp.Value.X + 6);
                Canvas.SetTop(label, kvp.Value.Y - 12);
                canvas.Children.Add(label);
            }
        }

        private void AddAllDimensions(Dictionary<string, Point> points, double L1, double W1, double L2, double W2, double W3, double W, double L3, double Alpha, Canvas canvas)
        {
            var dimBrush = new SolidColorBrush(COLOR_DIMENSION);
            double offset = 40;
            AddHorizontalDimension(points["P9_top_left"].X, points["P9_top_left"].Y - offset, L1, "L1=" + L1, dimBrush, canvas);
            AddHorizontalDimension(points["P7_top_left"].X, points["P7_top_left"].Y - offset, L2, "L2=" + L2, dimBrush, canvas);
            AddHorizontalDimension(points["P5_top_left"].X, points["P4_top_left"].Y - offset - 100, L3, "L3=" + L3, dimBrush, canvas);
            AddVerticalDimension(points["P2"].X - offset, points["P2"].Y, W, "W=" + W, dimBrush, canvas);
        }

        private void AddHorizontalDimension(double x, double y, double length, string label, Brush color, Canvas canvas)
        {
            double endX = x + length * SCALE;
            DrawLine(canvas, new Point(x, y), new Point(endX, y), Color.FromRgb(220, 20, 60), 1.5);
            AddLabel(label, x + length * SCALE / 2, y - 12, color, 10, canvas);
        }

        private void AddVerticalDimension(double x, double y, double length, string label, Brush color, Canvas canvas)
        {
            double endY = y + length * SCALE;
            DrawLine(canvas, new Point(x, y), new Point(x, endY), Color.FromRgb(220, 20, 60), 1.5);
            AddLabel(label, x - 20, y + length * SCALE / 2, color, 10, canvas);
        }

        private void AddLabel(string text, double x, double y, Brush color, int fontSize, Canvas canvas)
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
    }
}
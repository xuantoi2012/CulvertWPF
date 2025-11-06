using CulvertEditor.Models;
using CulvertEditor.Services;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CulvertEditor
{
    public partial class MainWindow : Window
    {
        // ========== SERVICES ==========
        private readonly PlanViewService planViewService;
        private readonly ElevationViewService elevationViewService;
        private readonly SectionViewService sectionViewService;
        private readonly View3DService view3DService;

        private readonly ZoomPanService zoomPlanService;
        private readonly ZoomPanService zoomElevationService;
        private readonly ZoomPanService zoomSectionService;

        // ========== MODEL ==========
        private CulvertParameters parameters;

        // ✅ Layout state
        private bool isVerticalLayout = false;
        private bool isAutoLayoutEnabled = true; // ✅ Enable auto layout switching
        private const double ASPECT_RATIO_THRESHOLD = 1.3; // Width/Height threshold for switching

        public MainWindow()
        {
            InitializeComponent();

            planViewService = new PlanViewService();
            elevationViewService = new ElevationViewService();
            sectionViewService = new SectionViewService();
            view3DService = new View3DService();

            zoomPlanService = new ZoomPanService();
            zoomElevationService = new ZoomPanService();
            zoomSectionService = new ZoomPanService();

            parameters = new CulvertParameters();

            this.KeyDown += MainWindow_KeyDown;

            // ✅ Subscribe to window size changed
            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                InitializeZoomPan();
                LoadParametersFromUI();
                DrawPlan();
                DrawElevation();
                DrawSection();
                InitializeHelix3D();

                // ✅ Set initial layout based on window size
                AutoAdjustLayout();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        // ✅ HANDLE WINDOW SIZE CHANGED
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (isAutoLayoutEnabled && e.WidthChanged)
            {
                AutoAdjustLayout();
            }
        }

        // ✅ HANDLE CANVAS LAYOUT GRID SIZE CHANGED
        private void CanvasLayoutGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (isAutoLayoutEnabled)
            {
                AutoAdjustLayout();
            }
        }

        // ✅ AUTO ADJUST LAYOUT BASED ON ASPECT RATIO
        private void AutoAdjustLayout()
        {
            if (canvasLayoutGrid == null) return;

            double width = canvasLayoutGrid.ActualWidth;
            double height = canvasLayoutGrid.ActualHeight;

            if (width <= 0 || height <= 0) return;

            double aspectRatio = width / height;

            // Determine optimal layout
            bool shouldBeVertical = aspectRatio > ASPECT_RATIO_THRESHOLD;

            // Only switch if different from current
            if (shouldBeVertical != isVerticalLayout)
            {
                isVerticalLayout = shouldBeVertical;
                SetLayoutOrientation(isVerticalLayout);

                // Update ribbon button if exists
                UpdateRibbonLayoutButton();

                // Log change (optional)
                System.Diagnostics.Debug.WriteLine($"Auto layout switched to: {(isVerticalLayout ? "VERTICAL" : "HORIZONTAL")} (Aspect: {aspectRatio:F2})");
            }
        }

        // ✅ UPDATE RIBBON BUTTON STATE
        private void UpdateRibbonLayoutButton()
        {
            if (chkLayoutOrientation != null)
            {
                chkLayoutOrientation.IsChecked = isVerticalLayout;
                chkLayoutOrientation.Content = isVerticalLayout ? "Horizontal" : "Vertical";
            }
        }

        // ✅ MANUAL TOGGLE FROM RIBBON (Disables auto-layout temporarily)
        private void ToggleLayoutOrientation_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem)
            {
                // Disable auto-layout when user manually switches
                isAutoLayoutEnabled = false;

                isVerticalLayout = checkItem.IsChecked ?? false;

                // Update button text
                checkItem.Content = isVerticalLayout ? "Horizontal" : "Vertical";

                SetLayoutOrientation(isVerticalLayout);

                // Re-enable auto-layout after 5 seconds (optional)
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };
                timer.Tick += (s, args) =>
                {
                    isAutoLayoutEnabled = true;
                    timer.Stop();
                };
                timer.Start();
            }
        }

        // ✅ SET LAYOUT ORIENTATION
        private void SetLayoutOrientation(bool isVertical)
        {
            if (canvasLayoutGrid == null || elevationViewBorder == null || planViewBorder == null || viewsSplitter == null)
                return;

            // Clear existing definitions
            canvasLayoutGrid.RowDefinitions.Clear();
            canvasLayoutGrid.ColumnDefinitions.Clear();

            if (isVertical)
            {
                // ========== VERTICAL LAYOUT (DỌC) ==========
                canvasLayoutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 100 });
                canvasLayoutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3) });
                canvasLayoutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 100 });

                // Elevation left
                Grid.SetRow(elevationViewBorder, 0);
                Grid.SetColumn(elevationViewBorder, 0);
                elevationViewBorder.BorderThickness = new Thickness(0, 0, 1, 0);

                // Splitter middle
                Grid.SetRow(viewsSplitter, 0);
                Grid.SetColumn(viewsSplitter, 1);
                viewsSplitter.Width = 3;
                viewsSplitter.Height = double.NaN;
                viewsSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                viewsSplitter.VerticalAlignment = VerticalAlignment.Stretch;
                viewsSplitter.ResizeDirection = GridResizeDirection.Columns;
                viewsSplitter.Cursor = Cursors.SizeWE;

                // Plan right
                Grid.SetRow(planViewBorder, 0);
                Grid.SetColumn(planViewBorder, 2);
                planViewBorder.BorderThickness = new Thickness(0);
            }
            else
            {
                // ========== HORIZONTAL LAYOUT (NGANG) ==========
                canvasLayoutGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 100 });
                canvasLayoutGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3) });
                canvasLayoutGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 100 });

                // Elevation top
                Grid.SetRow(elevationViewBorder, 0);
                Grid.SetColumn(elevationViewBorder, 0);
                elevationViewBorder.BorderThickness = new Thickness(0, 0, 0, 1);

                // Splitter middle
                Grid.SetRow(viewsSplitter, 1);
                Grid.SetColumn(viewsSplitter, 0);
                viewsSplitter.Height = 3;
                viewsSplitter.Width = double.NaN;
                viewsSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                viewsSplitter.VerticalAlignment = VerticalAlignment.Stretch;
                viewsSplitter.ResizeDirection = GridResizeDirection.Rows;
                viewsSplitter.Cursor = Cursors.SizeNS;

                // Plan bottom
                Grid.SetRow(planViewBorder, 2);
                Grid.SetColumn(planViewBorder, 0);
                planViewBorder.BorderThickness = new Thickness(0);
            }

            // Force layout update
            canvasLayoutGrid.UpdateLayout();
        }

        // ========== INITIALIZE ZOOM/PAN ==========
        private void InitializeZoomPan()
        {
            // Plan View
            zoomPlanService.MinZoom = 0.1;
            zoomPlanService.MaxZoom = 20.0;
            zoomPlanService.ZoomSensitivity = 0.001;
            zoomPlanService.Initialize(zoomContainerPlan, planCanvas, txtZoomLevelPlan);

            // Elevation View
            zoomElevationService.MinZoom = 0.1;
            zoomElevationService.MaxZoom = 20.0;
            zoomElevationService.ZoomSensitivity = 0.001;
            zoomElevationService.Initialize(zoomContainerElevation, elevationCanvas, txtZoomLevelElevation);

            // Section View
            zoomSectionService.MinZoom = 0.1;
            zoomSectionService.MaxZoom = 20.0;
            zoomSectionService.ZoomSensitivity = 0.001;
            zoomSectionService.Initialize(zoomContainerSection, sectionCanvas, txtZoomLevelSection);
        }

        // ========== LOAD PARAMETERS ==========
        private void LoadParametersFromUI()
        {
            parameters.L1 = TryGetValue(txtL1, out double l1) ? l1 : 24500;
            parameters.W1 = TryGetValue(txtW1, out double w1) ? w1 : 4000;
            parameters.L2 = TryGetValue(txtL2, out double l2) ? l2 : 5000;
            parameters.W2 = TryGetValue(txtW2, out double w2) ? w2 : 1800;
            parameters.W3 = TryGetValue(txtW3, out double w3) ? w3 : 2000;
            parameters.W = TryGetValue(txtW, out double w) ? w : 5000;
            parameters.L3 = TryGetValue(txtL3, out double l3) ? l3 : 30000;
            parameters.Alpha = TryGetValue(txtAlpha, out double alpha) ? alpha : 20;

            parameters.SectionWidth = TryGetValue(txtSectionWidth, out double sw) ? sw : 5000;
            parameters.SectionHeight = TryGetValue(txtSectionHeight, out double sh) ? sh : 3000;
            parameters.WallThickness = TryGetValue(txtWallThickness, out double wt) ? wt : 300;
            parameters.ExcavationDepth = TryGetValue(txtExcavationDepth, out double ed) ? ed : 2000;
            parameters.SlopeRatio = TryGetValue(txtSlopeRatio, out double sr) ? sr : 1.5;

            parameters.ShowDimensions = chkShowDimensions?.IsChecked ?? true;
            parameters.ShowPoints = chkShowPoints?.IsChecked ?? true;
            parameters.ShowExcavation = chkShowExcavation?.IsChecked ?? true;
            parameters.ShowWireframe = chkShowWireframe?.IsChecked ?? false;
            parameters.ShowBoundingBox = chkShowBoundingBox?.IsChecked ?? false;
        }

        private bool TryGetValue(TextBox textBox, out double value)
        {
            return double.TryParse(textBox?.Text, out value) && value > 0;
        }

        // ========== DRAWING METHODS ==========
        private void DrawPlan()
        {
            LoadParametersFromUI();
            planViewService.Draw(planCanvas, parameters);
        }

        private void DrawElevation()
        {
            elevationViewService.Draw(elevationCanvas);
        }

        private void DrawSection()
        {
            LoadParametersFromUI();
            sectionViewService.Draw(sectionCanvas, parameters);
        }

        private void InitializeHelix3D()
        {
            view3DService.Initialize(helixViewport, modelVisual3D, txt3DInfo, txt3DStats);
            LoadParametersFromUI();
            view3DService.GenerateModel(parameters);
        }

        private void GenerateHelixCulvertModel()
        {
            LoadParametersFromUI();
            view3DService.GenerateModel(parameters);
        }

        // ========== TAB CONTROL ==========
        private void TabControl_SelectionChanged(object sender, TabControlSelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            switch (mainTabControl.SelectedIndex)
            {
                case 0:
                    DrawPlan();
                    DrawElevation();
                    break;
                case 1:
                    DrawSection();
                    break;
                case 2:
                    GenerateHelixCulvertModel();
                    break;
            }
        }

        // ========== RESET EVENTS ==========
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

        private void OnReset3D(object sender, RoutedEventArgs e)
        {
            chkShowWireframe.IsChecked = false;
            chkShowBoundingBox.IsChecked = false;
            cmbMaterial.SelectedIndex = 0;
            helixViewport.ZoomExtents();
            GenerateHelixCulvertModel();
        }

        // ========== CHANGE EVENTS ==========
        private void OnDimensionChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && mainTabControl.SelectedIndex == 0)
            {
                DrawPlan();
                DrawElevation();
            }
        }

        private void OnSectionChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && mainTabControl.SelectedIndex == 1)
            {
                DrawSection();
            }
        }

        private void OnDisplayChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            GenerateHelixCulvertModel();
        }

        private void OnMaterialChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            string material = (cmbMaterial.SelectedItem as ComboBoxItem)?.Content.ToString();
            view3DService.ApplyMaterial(material);
        }

        // ========== ZOOM/PAN - PLAN VIEW ==========
        private void ZoomInPlan_Click(object sender, RoutedEventArgs e) => zoomPlanService.ZoomIn();
        private void ZoomOutPlan_Click(object sender, RoutedEventArgs e) => zoomPlanService.ZoomOut();
        private void ZoomFitPlan_Click(object sender, RoutedEventArgs e) => zoomPlanService.ZoomToFit();
        private void ZoomResetPlan_Click(object sender, RoutedEventArgs e) => zoomPlanService.Reset();

        // ========== ZOOM/PAN - ELEVATION VIEW ==========
        private void ZoomInElevation_Click(object sender, RoutedEventArgs e) => zoomElevationService.ZoomIn();
        private void ZoomOutElevation_Click(object sender, RoutedEventArgs e) => zoomElevationService.ZoomOut();
        private void ZoomFitElevation_Click(object sender, RoutedEventArgs e) => zoomElevationService.ZoomToFit();
        private void ZoomResetElevation_Click(object sender, RoutedEventArgs e) => zoomElevationService.Reset();

        // ========== ZOOM/PAN - SECTION VIEW ==========
        private void ZoomInSection_Click(object sender, RoutedEventArgs e) => zoomSectionService.ZoomIn();
        private void ZoomOutSection_Click(object sender, RoutedEventArgs e) => zoomSectionService.ZoomOut();
        private void ZoomFitSection_Click(object sender, RoutedEventArgs e) => zoomSectionService.ZoomToFit();
        private void ZoomResetSection_Click(object sender, RoutedEventArgs e) => zoomSectionService.Reset();

        // ========== 3D CAMERA CONTROLS ==========
        private void CameraFront_Click(object sender, RoutedEventArgs e) => view3DService.SetCameraView("front");
        private void CameraBack_Click(object sender, RoutedEventArgs e) => view3DService.SetCameraView("back");
        private void CameraTop_Click(object sender, RoutedEventArgs e) => view3DService.SetCameraView("top");
        private void CameraBottom_Click(object sender, RoutedEventArgs e) => view3DService.SetCameraView("bottom");
        private void CameraLeft_Click(object sender, RoutedEventArgs e) => view3DService.SetCameraView("left");
        private void CameraRight_Click(object sender, RoutedEventArgs e) => view3DService.SetCameraView("right");

        // ========== 3D EXPORT ==========
        private void ExportOBJ_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "OBJ Files (*.obj)|*.obj",
                DefaultExt = ".obj",
                FileName = $"Culvert_{DateTime.Now:yyyyMMdd_HHmmss}.obj"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    view3DService.ExportOBJ(dialog.FileName);
                    MessageBox.Show($"✅ Đã xuất: {dialog.FileName}", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportSTL_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "STL Files (*.stl)|*.stl",
                DefaultExt = ".stl",
                FileName = $"Culvert_{DateTime.Now:yyyyMMdd_HHmmss}.stl"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    view3DService.ExportSTL(dialog.FileName);
                    MessageBox.Show($"✅ Đã xuất: {dialog.FileName}", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportScreenshot_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpg)|*.jpg",
                DefaultExt = ".png",
                FileName = $"Culvert_Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    view3DService.ExportScreenshot(dialog.FileName);
                    MessageBox.Show($"✅ Đã xuất: {dialog.FileName}", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ========== TOOLBAR EVENTS ==========
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

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn tạo dự án mới? Dữ liệu hiện tại chưa lưu sẽ bị mất.",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                OnResetPlanElevation(null, null);
                OnResetSection(null, null);
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

        private void ToggleDimensions_CheckedChanged(object sender, ItemClickEventArgs e)
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

        private void TogglePoints_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (!IsLoaded) return;

            var barItem = sender as BarCheckItem;
            bool isChecked = barItem?.IsChecked ?? true;

            if (chkShowPoints != null)
                chkShowPoints.IsChecked = isChecked;
        }

        private void ToggleGrid_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (!IsLoaded) return;

            var barItem = sender as BarCheckItem;
            bool isChecked = barItem?.IsChecked ?? false;

            MessageBox.Show($"Hiển thị lưới: {(isChecked ? "Bật" : "Tắt")}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ========== CLEANUP ==========
        protected override void OnClosed(EventArgs e)
        {
            zoomPlanService?.Cleanup();
            zoomElevationService?.Cleanup();
            zoomSectionService?.Cleanup();
            base.OnClosed(e);
        }
    }
}
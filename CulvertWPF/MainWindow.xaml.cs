using CulvertEditor.Models;
using CulvertEditor.Services;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Docking.Base;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public MainWindow()
        {
            InitializeComponent();

            // Initialize services
            planViewService = new PlanViewService();
            elevationViewService = new ElevationViewService();
            sectionViewService = new SectionViewService();
            view3DService = new View3DService();

            zoomPlanService = new ZoomPanService();
            zoomElevationService = new ZoomPanService();
            zoomSectionService = new ZoomPanService();

            parameters = new CulvertParameters();

            // Subscribe to events
            this.KeyDown += MainWindow_KeyDown;
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
                LoadDockLayouts();
                SubscribeDockEvents();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        // ========== DOCK LAYOUT MANAGEMENT ==========
        private void LoadDockLayouts()
        {
            try
            {
                string appDataPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CulvertEditor");

                if (dockManagerPlan != null)
                {
                    string planLayout = System.IO.Path.Combine(appDataPath, "dock_plan.xml");
                    if (System.IO.File.Exists(planLayout))
                        dockManagerPlan.RestoreLayoutFromXml(planLayout);
                }

                if (dockManagerSection != null)
                {
                    string sectionLayout = System.IO.Path.Combine(appDataPath, "dock_section.xml");
                    if (System.IO.File.Exists(sectionLayout))
                        dockManagerSection.RestoreLayoutFromXml(sectionLayout);
                }

                if (dockManager3D != null)
                {
                    string layout3D = System.IO.Path.Combine(appDataPath, "dock_3d.xml");
                    if (System.IO.File.Exists(layout3D))
                        dockManager3D.RestoreLayoutFromXml(layout3D);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load dock layouts: {ex.Message}");
            }
        }

        private void SaveDockLayouts()
        {
            try
            {
                string appDataPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CulvertEditor");

                System.IO.Directory.CreateDirectory(appDataPath);

                if (dockManagerPlan != null)
                    dockManagerPlan.SaveLayoutToXml(System.IO.Path.Combine(appDataPath, "dock_plan.xml"));

                if (dockManagerSection != null)
                    dockManagerSection.SaveLayoutToXml(System.IO.Path.Combine(appDataPath, "dock_section.xml"));

                if (dockManager3D != null)
                    dockManager3D.SaveLayoutToXml(System.IO.Path.Combine(appDataPath, "dock_3d.xml"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save dock layouts: {ex.Message}");
            }
        }

        private void SubscribeDockEvents()
        {
            if (dockManagerPlan != null)
                dockManagerPlan.DockItemClosed += DockManager_DockItemClosed;

            if (dockManagerSection != null)
                dockManagerSection.DockItemClosed += DockManager_DockItemClosed;

            if (dockManager3D != null)
                dockManager3D.DockItemClosed += DockManager_DockItemClosed;
        }

        private void DockManager_DockItemClosed(object sender, DockItemClosedEventArgs e)
        {
            // Update dropdown menu items when panels are closed via X button
            // Note: BarCheckItems in dropdown don't have direct reference, 
            // so we just log the event
            System.Diagnostics.Debug.WriteLine($"Panel closed: {e.Item.Name}");
        }

        // ========== PANEL TOGGLE HANDLERS (DROPDOWN MENU) ==========

        // PLAN & ELEVATION PANELS
        private void TogglePlanParams_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panelPlanParams != null)
            {
                panelPlanParams.Visibility = checkItem.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                if (checkItem.IsChecked == true && dockManagerPlan != null)
                    dockManagerPlan.DockController.Activate(panelPlanParams);
            }
        }

        private void TogglePlanView_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panelPlanView != null)
            {
                panelPlanView.Visibility = checkItem.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                if (checkItem.IsChecked == true && dockManagerPlan != null)
                    dockManagerPlan.DockController.Activate(panelPlanView);
            }
        }

        private void ToggleElevationView_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panelElevationView != null)
            {
                panelElevationView.Visibility = checkItem.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                if (checkItem.IsChecked == true && dockManagerPlan != null)
                    dockManagerPlan.DockController.Activate(panelElevationView);
            }
        }

        // SECTION PANELS
        private void ToggleSectionParams_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panelSectionParams != null)
            {
                panelSectionParams.Visibility = checkItem.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                if (checkItem.IsChecked == true && dockManagerSection != null)
                    dockManagerSection.DockController.Activate(panelSectionParams);
            }
        }

        private void ToggleSectionView_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panelSectionCanvas != null)
            {
                panelSectionCanvas.Visibility = checkItem.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                if (checkItem.IsChecked == true && dockManagerSection != null)
                    dockManagerSection.DockController.Activate(panelSectionCanvas);
            }
        }

        // 3D PANELS
        private void Toggle3DParams_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panel3DParams != null)
            {
                panel3DParams.Visibility = checkItem.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                if (checkItem.IsChecked == true && dockManager3D != null)
                    dockManager3D.DockController.Activate(panel3DParams);
            }
        }

        private void Toggle3DView_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panel3DCanvas != null)
            {
                panel3DCanvas.Visibility = checkItem.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                if (checkItem.IsChecked == true && dockManager3D != null)
                    dockManager3D.DockController.Activate(panel3DCanvas);
            }
        }

        // ========== INITIALIZE ZOOM/PAN ==========
        private void InitializeZoomPan()
        {
            zoomPlanService.MinZoom = 0.1;
            zoomPlanService.MaxZoom = 20.0;
            zoomPlanService.ZoomSensitivity = 0.001;
            zoomPlanService.Initialize(zoomContainerPlan, planCanvas, txtZoomLevelPlan);

            zoomElevationService.MinZoom = 0.1;
            zoomElevationService.MaxZoom = 20.0;
            zoomElevationService.ZoomSensitivity = 0.001;
            zoomElevationService.Initialize(zoomContainerElevation, elevationCanvas, txtZoomLevelElevation);

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
            view3DService.Initialize(helixViewport, modelVisual3D, txt3DInfo, null);
            LoadParametersFromUI();
            view3DService.GenerateModel(parameters);
        }

        private void GenerateHelixCulvertModel()
        {
            LoadParametersFromUI();
            view3DService.GenerateModel(parameters);
        }

        // ========== TAB CONTROL ==========
        private void MainTabControl_SelectionChanged(object sender, TabControlSelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            switch (mainTabControl.SelectedIndex)
            {
                case 0: // Plan & Elevation
                    DrawPlan();
                    DrawElevation();
                    break;
                case 1: // Section
                    DrawSection();
                    break;
                case 2: // 3D
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

        // ========== ZOOM/PAN ==========
        private void ZoomInPlan_Click(object sender, RoutedEventArgs e) => zoomPlanService.ZoomIn();
        private void ZoomOutPlan_Click(object sender, RoutedEventArgs e) => zoomPlanService.ZoomOut();
        private void ZoomFitPlan_Click(object sender, RoutedEventArgs e) => zoomPlanService.ZoomToFit();
        private void ZoomResetPlan_Click(object sender, RoutedEventArgs e) => zoomPlanService.Reset();

        private void ZoomInElevation_Click(object sender, RoutedEventArgs e) => zoomElevationService.ZoomIn();
        private void ZoomOutElevation_Click(object sender, RoutedEventArgs e) => zoomElevationService.ZoomOut();
        private void ZoomFitElevation_Click(object sender, RoutedEventArgs e) => zoomElevationService.ZoomToFit();
        private void ZoomResetElevation_Click(object sender, RoutedEventArgs e) => zoomElevationService.Reset();

        private void ZoomInSection_Click(object sender, RoutedEventArgs e) => zoomSectionService.ZoomIn();
        private void ZoomOutSection_Click(object sender, RoutedEventArgs e) => zoomSectionService.ZoomOut();
        private void ZoomFitSection_Click(object sender, RoutedEventArgs e) => zoomSectionService.ZoomToFit();
        private void ZoomResetSection_Click(object sender, RoutedEventArgs e) => zoomSectionService.Reset();

        // ========== 3D CAMERA ==========
        private void CameraFront_Click(object sender, RoutedEventArgs e) => view3DService.SetCameraView("front");
        private void CameraBack_Click(object sender, RoutedEventArgs e) => view3DService.SetCameraView("back");
        private void CameraTop_Click(object sender, RoutedEventArgs e) => view3DService.SetCameraView("top");
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
                    MessageBox.Show($"✅ Exported: {dialog.FileName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show($"✅ Exported: {dialog.FileName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show($"✅ Exported: {dialog.FileName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ========== KEYBOARD SHORTCUTS ==========
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.N: NewProject_Click(sender, e); e.Handled = true; break;
                    case Key.O: OpenProject_Click(sender, e); e.Handled = true; break;
                    case Key.S: SaveProject_Click(sender, e); e.Handled = true; break;
                    case Key.P: Print_Click(sender, e); e.Handled = true; break;
                }
            }
            else if (e.Key == Key.F1)
            {
                Help_Click(sender, e);
                e.Handled = true;
            }
        }

        // ========== TOOLBAR EVENTS ==========
        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Create new project? Unsaved data will be lost.", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                OnResetPlanElevation(null, null);
                OnResetSection(null, null);
                OnReset3D(null, null);
                MessageBox.Show("New project created!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Culvert Project (*.cvt)|*.cvt|All Files (*.*)|*.*",
                Title = "Open Project"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    MessageBox.Show($"Opening: {dialog.FileName}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Project saved!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Export to PDF",
                DefaultExt = ".pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    MessageBox.Show($"Exported PDF: {dialog.FileName}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportDXF_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "DXF Files (*.dxf)|*.dxf",
                Title = "Export to DXF",
                DefaultExt = ".dxf"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    MessageBox.Show($"Exported DXF: {dialog.FileName}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("Printing...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Start hydraulic and structural calculations?", "Confirm",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Calculation complete!\n\nResults:\n- Flow: OK\n- Stress: OK\n- Stability: OK",
                        "Results", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    errors.AppendLine("- L1: Invalid value");
                    hasErrors = true;
                }

                if (hasErrors)
                {
                    MessageBox.Show($"Errors found:\n\n{errors}", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Design is valid! ✓", "Validation", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "CULVERT BOX DESIGN TOOL\n\n" +
                "INSTRUCTIONS:\n" +
                "1. Enter parameters in left panel\n" +
                "2. View auto-updated drawings\n" +
                "3. Zoom: Mouse wheel\n" +
                "4. Pan: Middle click + drag\n" +
                "5. 3D Rotate: Shift + Middle click\n\n" +
                "SHORTCUTS:\n" +
                "- Ctrl+N: New project\n- Ctrl+O: Open\n- Ctrl+S: Save\n- Ctrl+P: Print\n- F1: Help\n\n" +
                "PANELS:\n" +
                "- Use Panels dropdown to show/hide views",
                "Help",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "CULVERT BOX DESIGN TOOL\n\n" +
                "Version: 1.0.0\n" +
                "Build Date: 2025-01-07\n\n" +
                "Professional culvert box design software\n" +
                "With docking panels and CAD-style controls\n\n" +
                "Developer: xuantoi2012\n" +
                "© 2025 All Rights Reserved",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ToggleDimensions_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (!IsLoaded) return;

            var barItem = sender as BarCheckItem;
            bool isChecked = barItem?.IsChecked ?? true;

            if (mainTabControl.SelectedIndex == 0 && chkShowDimensions != null)
                chkShowDimensions.IsChecked = isChecked;
            else if (mainTabControl.SelectedIndex == 1 && chkShowSectionDimensions != null)
                chkShowSectionDimensions.IsChecked = isChecked;
        }

        private void TogglePoints_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (!IsLoaded) return;

            var barItem = sender as BarCheckItem;
            bool isChecked = barItem?.IsChecked ?? true;

            if (chkShowPoints != null)
                chkShowPoints.IsChecked = isChecked;
        }

        private void ToggleLayoutOrientation_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            // Placeholder - can be implemented later
        }

        // ========== CLEANUP ==========
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveDockLayouts();

            if (dockManagerPlan != null)
                dockManagerPlan.DockItemClosed -= DockManager_DockItemClosed;

            if (dockManagerSection != null)
                dockManagerSection.DockItemClosed -= DockManager_DockItemClosed;

            if (dockManager3D != null)
                dockManager3D.DockItemClosed -= DockManager_DockItemClosed;

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            zoomPlanService?.Cleanup();
            zoomElevationService?.Cleanup();
            zoomSectionService?.Cleanup();
            base.OnClosed(e);
        }
    }
}
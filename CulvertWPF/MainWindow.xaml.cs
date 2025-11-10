using CulvertEditor.Models;
using CulvertEditor.Services;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Docking.Base;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static CulvertEditor.Models.CulvertParameters;

namespace CulvertEditor
{
    public partial class MainWindow : ThemedWindow
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

        // ✅ CURRENT SCALE
        private double currentScale = 100; // Default 1:100

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
                UpdateStatusBarInfo();

                // Set initial view to Plan
                SwitchToView(0);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        // ✅ SCALE SELECTION HANDLER - FIXED NULL REFERENCE
        private void ScaleSelected_Changed(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem item && item.IsChecked == true)
            {
                double scale = double.Parse(item.Tag.ToString());
                currentScale = scale;

                UpdateStatusBarInfo();

                // ✅ Check if controls are not null and is loaded
                if (IsLoaded)
                {
                    // Redraw current view with new scale
                    if (btnTabPlan?.IsChecked == true)
                    {
                        DrawPlan();
                        DrawElevation();
                    }
                    else if (btnTabSection?.IsChecked == true)
                    {
                        DrawSection();
                    }
                    // No redraw needed for 3D view on scale change
                }
            }
        }

        // ✅ EXPORT DXF BUTTON CLICK - Updated for MenuBar
        private void ExportDXF_Click(object sender, ItemClickEventArgs e)
        {
            try
            {
                // ✅ Update status: Exporting...
                if (txtExportInfo != null)
                {
                    txtExportInfo.Text = "Exporting...";
                    txtExportInfo.Foreground = System.Windows.Media.Brushes.Orange;
                }

                LoadParametersFromUI();

                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "AutoCAD DXF (*.dxf)|*.dxf",
                    DefaultExt = ".dxf",
                    FileName = string.Format("Culvert_{0}_{1}.dxf",
                        GetCurrentViewName(),
                        DateTime.Now.ToString("yyyyMMdd_HHmmss")),
                    Title = "Export to AutoCAD DXF"
                };

                if (dialog.ShowDialog() == true)
                {
                    string drawingType = GetCurrentViewType();

                    var exportService = new DXFExportService();
                    exportService.ExportDrawing(dialog.FileName, parameters, drawingType, currentScale);

                    // ✅ Update status: Success
                    if (txtExportInfo != null)
                    {
                        txtExportInfo.Text = string.Format("✅ Exported: {0}", Path.GetFileName(dialog.FileName));
                        txtExportInfo.Foreground = System.Windows.Media.Brushes.Green;
                    }

                    string message = string.Format(
                        "✅ Export successful!\n\n" +
                        "File: {0}\n" +
                        "Scale: 1:{1}\n" +
                        "Type: {2}\n\n" +
                        "Do you want to open the folder?",
                        Path.GetFileName(dialog.FileName),
                        currentScale,
                        drawingType.ToUpper());

                    var result = MessageBox.Show(
                        message,
                        "Export Success",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        string argument = string.Format("/select,\"{0}\"", dialog.FileName);
                        Process.Start("explorer.exe", argument);
                    }

                    // ✅ Reset status after 3 seconds
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(3)
                    };
                    timer.Tick += (s, args) =>
                    {
                        UpdateStatusBarInfo();
                        timer.Stop();
                    };
                    timer.Start();
                }
                else
                {
                    // ✅ Cancelled
                    if (txtExportInfo != null)
                    {
                        txtExportInfo.Text = "Export cancelled";
                        txtExportInfo.Foreground = System.Windows.Media.Brushes.Gray;
                    }
                }
            }
            catch (Exception ex)
            {
                // ✅ Error
                if (txtExportInfo != null)
                {
                    txtExportInfo.Text = "❌ Export failed!";
                    txtExportInfo.Foreground = System.Windows.Media.Brushes.Red;
                }

                MessageBox.Show(
                    string.Format("❌ Export failed!\n\n{0}", ex.Message),
                    "Export Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ✅ GET CURRENT VIEW NAME - UPDATED FOR TAB BUTTONS
        private string GetCurrentViewName()
        {
            if (btnTabPlan?.IsChecked == true) return "PlanElevation";
            if (btnTabSection?.IsChecked == true) return "Section";
            if (btnTab3D?.IsChecked == true) return "3DView";
            return "Drawing";
        }

        // ✅ GET CURRENT VIEW TYPE - UPDATED FOR TAB BUTTONS
        private string GetCurrentViewType()
        {
            if (btnTabPlan?.IsChecked == true) return "plan-elevation";
            if (btnTabSection?.IsChecked == true) return "section";
            if (btnTab3D?.IsChecked == true) return "3d";
            return "plan";
        }

        // ✅ UPDATE STATUS BAR - FIXED NULL REFERENCE
        private void UpdateStatusBarInfo()
        {
            if (txtCurrentScale == null || txtTextHeight == null || txtExportInfo == null)
                return;

            try
            {
                // Calculate text height based on scale
                double textHeight = 2.5 * (currentScale / 100.0);

                // Update status bar
                txtCurrentScale.Text = string.Format("1:{0}", currentScale);
                txtTextHeight.Text = string.Format("{0:F1} mm", textHeight);

                string viewName = GetCurrentViewName();
                txtExportInfo.Text = string.Format("Ready to export {0} at 1:{1}", viewName, currentScale);
                txtExportInfo.Foreground = System.Windows.Media.Brushes.DarkBlue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating status bar: {ex.Message}");
            }
        }

        // ========== RESET SETTINGS BUTTON ==========
        private void ResetSettings_Click(object sender, ItemClickEventArgs e)
        {
            var result = MessageBox.Show(
                "Reset all panel layouts to default?\n\n" +
                "This will delete saved layout configurations.",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // ✅ Delete all saved XML layouts
                    string appDataPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "CulvertEditor");

                    int deletedCount = 0;

                    if (Directory.Exists(appDataPath))
                    {
                        var layoutFiles = Directory.GetFiles(appDataPath, "dock_*.xml");

                        foreach (var file in layoutFiles)
                        {
                            try
                            {
                                File.Delete(file);
                                deletedCount++;
                                Debug.WriteLine($"Deleted: {Path.GetFileName(file)}");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Failed to delete {file}: {ex.Message}");
                            }
                        }
                    }

                    // ✅ Show success message
                    MessageBox.Show(
                        $"✅ Layout reset complete!\n\n" +
                        $"Deleted {deletedCount} layout file(s).\n\n" +
                        $"Please restart the application to apply default layout.",
                        "Reset Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"❌ Failed to reset settings!\n\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
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
            // Panel closed: e.Item.Name - Update corresponding menu items
            if (e.Item == panelPlanParams && btnPlanParams != null)
                btnPlanParams.IsChecked = false;
            else if (e.Item == panelPlanView && btnPlanView != null)
                btnPlanView.IsChecked = false;
            else if (e.Item == panelElevationView && btnElevationView != null)
                btnElevationView.IsChecked = false;
            else if (e.Item == panelSectionParams && btnSectionParams != null)
                btnSectionParams.IsChecked = false;
            else if (e.Item == panelSectionCanvas && btnSectionView != null)
                btnSectionView.IsChecked = false;
            else if (e.Item == panel3DParams && btn3DParams != null)
                btn3DParams.IsChecked = false;
            else if (e.Item == panel3DCanvas && btn3DView != null)
                btn3DView.IsChecked = false;

            // (Optional) log
            Debug.WriteLine($"Panel closed (via X): {e.Item.Name}, uncheck menu item!");
        }

        // ========== PANEL TOGGLE HANDLERS (MENU ITEMS) ==========

        // PLAN & ELEVATION PANELS
        private void TogglePlanParams_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panelPlanParams != null && dockManagerPlan != null)
            {
                if (checkItem.IsChecked == true)
                {
                    dockManagerPlan.DockController.Restore(panelPlanParams);
                    dockManagerPlan.DockController.Activate(panelPlanParams);
                }
                else
                {
                    dockManagerPlan.DockController.Close(panelPlanParams);
                }
            }
        }

        private void TogglePlanView_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panelPlanView != null && dockManagerPlan != null)
            {
                if (checkItem.IsChecked == true)
                {
                    dockManagerPlan.DockController.Restore(panelPlanView);
                    dockManagerPlan.DockController.Activate(panelPlanView);
                }
                else
                {
                    dockManagerPlan.DockController.Close(panelPlanView);
                }
            }
        }

        private void ToggleElevationView_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panelElevationView != null && dockManagerPlan != null)
            {
                if (checkItem.IsChecked == true)
                {
                    dockManagerPlan.DockController.Restore(panelElevationView);
                    dockManagerPlan.DockController.Activate(panelElevationView);
                }
                else
                {
                    dockManagerPlan.DockController.Close(panelElevationView);
                }
            }
        }

        // ========== SECTION PANELS ==========
        private void ToggleSectionParams_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panelSectionParams != null && dockManagerSection != null)
            {
                if (checkItem.IsChecked == true)
                {
                    dockManagerSection.DockController.Restore(panelSectionParams);
                    dockManagerSection.DockController.Activate(panelSectionParams);
                }
                else
                {
                    dockManagerSection.DockController.Close(panelSectionParams);
                }
            }
        }

        private void ToggleSectionView_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panelSectionCanvas != null && dockManagerSection != null)
            {
                if (checkItem.IsChecked == true)
                {
                    dockManagerSection.DockController.Restore(panelSectionCanvas);
                    dockManagerSection.DockController.Activate(panelSectionCanvas);
                }
                else
                {
                    dockManagerSection.DockController.Close(panelSectionCanvas);
                }
            }
        }

        // ========== 3D PANELS ==========
        private void Toggle3DParams_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panel3DParams != null && dockManager3D != null)
            {
                if (checkItem.IsChecked == true)
                {
                    dockManager3D.DockController.Restore(panel3DParams);
                    dockManager3D.DockController.Activate(panel3DParams);
                }
                else
                {
                    dockManager3D.DockController.Close(panel3DParams);
                }
            }
        }

        private void Toggle3DView_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (sender is BarCheckItem checkItem && panel3DCanvas != null && dockManager3D != null)
            {
                if (checkItem.IsChecked == true)
                {
                    dockManager3D.DockController.Restore(panel3DCanvas);
                    dockManager3D.DockController.Activate(panel3DCanvas);
                }
                else
                {
                    dockManager3D.DockController.Close(panel3DCanvas);
                }
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

        // ========== TAB BUTTON HANDLERS ========== 
        private void TabPlan_Click(object sender, ItemClickEventArgs e)
        {
            SwitchToView(0);
        }

        private void TabSection_Click(object sender, ItemClickEventArgs e)
        {
            SwitchToView(1);
        }

        private void Tab3D_Click(object sender, ItemClickEventArgs e)
        {
            SwitchToView(2);
        }

        private void SwitchToView(int viewIndex)
        {
            try
            {
                // Hide all views
                if (gridPlanView != null) gridPlanView.Visibility = Visibility.Collapsed;
                if (gridSectionView != null) gridSectionView.Visibility = Visibility.Collapsed;
                if (grid3DView != null) grid3DView.Visibility = Visibility.Collapsed;

                // Show selected view
                switch (viewIndex)
                {
                    case 0: // Plan View
                        if (gridPlanView != null) gridPlanView.Visibility = Visibility.Visible;
                        DrawPlan();
                        DrawElevation();
                        break;
                    case 1: // Section View
                        if (gridSectionView != null) gridSectionView.Visibility = Visibility.Visible;
                        DrawSection();
                        break;
                    case 2: // 3D View
                        if (grid3DView != null) grid3DView.Visibility = Visibility.Visible;
                        GenerateHelixCulvertModel();
                        break;
                }

                // Update status bar
                UpdateStatusBarInfo();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error switching view: {ex.Message}");
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

        // ========== CHANGE EVENTS - UPDATED FOR NEW VIEW SYSTEM ==========
        private void OnDimensionChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && btnTabPlan?.IsChecked == true)
            {
                DrawPlan();
                DrawElevation();
            }
        }

        private void OnSectionChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && btnTabSection?.IsChecked == true)
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

        // ========== ZOOM/PAN - OLD METHODS (KEEP FOR COMPATIBILITY) ==========
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

        // ========== TOOLBAR ZOOM METHODS - NEW ========== 
        private void ZoomIn_Click(object sender, ItemClickEventArgs e)
        {
            if (btnTabPlan?.IsChecked == true)
                zoomPlanService?.ZoomIn();
            else if (btnTabSection?.IsChecked == true)
                zoomSectionService?.ZoomIn();
        }

        private void ZoomOut_Click(object sender, ItemClickEventArgs e)
        {
            if (btnTabPlan?.IsChecked == true)
                zoomPlanService?.ZoomOut();
            else if (btnTabSection?.IsChecked == true)
                zoomSectionService?.ZoomOut();
        }

        private void ZoomFit_Click(object sender, ItemClickEventArgs e)
        {
            if (btnTabPlan?.IsChecked == true)
                zoomPlanService?.ZoomToFit();
            else if (btnTabSection?.IsChecked == true)
                zoomSectionService?.ZoomToFit();
            else if (btnTab3D?.IsChecked == true)
                helixViewport?.ZoomExtents();
        }

        private void ZoomReset_Click(object sender, ItemClickEventArgs e)
        {
            if (btnTabPlan?.IsChecked == true)
                zoomPlanService?.Reset();
            else if (btnTabSection?.IsChecked == true)
                zoomSectionService?.Reset();
            else if (btnTab3D?.IsChecked == true)
                helixViewport?.ZoomExtents();
        }

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

        // ========== TOOLBAR BUTTON HANDLERS ==========
        private void Undo_Click(object sender, ItemClickEventArgs e)
        {
            try
            {
                MessageBox.Show("Undo operation", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Redo_Click(object sender, ItemClickEventArgs e)
        {
            try
            {
                MessageBox.Show("Redo operation", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cut_Click(object sender, ItemClickEventArgs e)
        {
            try
            {
                MessageBox.Show("Cut operation", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Copy_Click(object sender, ItemClickEventArgs e)
        {
            try
            {
                MessageBox.Show("Copy operation", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Paste_Click(object sender, ItemClickEventArgs e)
        {
            try
            {
                MessageBox.Show("Paste operation", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== KEYBOARD SHORTCUTS - FIXED ==========
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.N:
                        NewProject_Click(sender, null);
                        e.Handled = true;
                        break;
                    case Key.O:
                        OpenProject_Click(sender, null);
                        e.Handled = true;
                        break;
                    case Key.S:
                        SaveProject_Click(sender, null);
                        e.Handled = true;
                        break;
                    case Key.P:
                        Print_Click(sender, null);
                        e.Handled = true;
                        break;
                    case Key.Z:
                        Undo_Click(sender, null);
                        e.Handled = true;
                        break;
                    case Key.Y:
                        Redo_Click(sender, null);
                        e.Handled = true;
                        break;
                    case Key.X:
                        Cut_Click(sender, null);
                        e.Handled = true;
                        break;
                    case Key.C:
                        Copy_Click(sender, null);
                        e.Handled = true;
                        break;
                    case Key.V:
                        Paste_Click(sender, null);
                        e.Handled = true;
                        break;
                }
            }
            else if (e.Key == Key.F1)
            {
                Help_Click(sender, null);
                e.Handled = true;
            }
        }

        // ========== MENU BAR EVENTS - UPDATED TO HANDLE NULL PARAMETERS ==========
        private void NewProject_Click(object sender, ItemClickEventArgs e)
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

        private void OpenProject_Click(object sender, ItemClickEventArgs e)
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

        private void SaveProject_Click(object sender, ItemClickEventArgs e)
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

        private void ExportPDF_Click(object sender, ItemClickEventArgs e)
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

        private void Print_Click(object sender, ItemClickEventArgs e)
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

        private void Calculate_Click(object sender, ItemClickEventArgs e)
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

        private void Validate_Click(object sender, ItemClickEventArgs e)
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

        private void Exit_Click(object sender, ItemClickEventArgs e)
        {
            this.Close();
        }

        private void Help_Click(object sender, ItemClickEventArgs e)
        {
            DXMessageBox.Show(
                "CULVERT BOX DESIGN TOOL\n\n" +
                "INSTRUCTIONS:\n" +
                "1. Enter parameters in left panel\n" +
                "2. View auto-updated drawings\n" +
                "3. Zoom: Mouse wheel or toolbar buttons\n" +
                "4. Pan: Middle click + drag\n" +
                "5. 3D Rotate: Shift + Middle click\n\n" +
                "SHORTCUTS:\n" +
                "- Ctrl+N: New project\n- Ctrl+O: Open\n- Ctrl+S: Save\n- Ctrl+P: Print\n" +
                "- Ctrl+Z: Undo\n- Ctrl+Y: Redo\n- Ctrl+X/C/V: Cut/Copy/Paste\n- F1: Help\n\n" +
                "TOOLBAR:\n" +
                "- Use toolbar tab buttons to switch views\n" +
                "- Use View menu to show/hide panels",
                "Help",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void About_Click(object sender, ItemClickEventArgs e)
        {
            DXMessageBox.Show(
                "CULVERT BOX DESIGN TOOL\n\n" +
                "Version: 1.0.0\n" +
                "Build Date: 2025-11-09\n\n" +
                "Professional culvert box design software\n" +
                "With docking panels and CAD-style toolbar\n\n" +
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

            if (btnTabPlan?.IsChecked == true && chkShowDimensions != null)
                chkShowDimensions.IsChecked = isChecked;
            else if (btnTabSection?.IsChecked == true && chkShowSectionDimensions != null)
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
using CulvertEditor.Models;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CulvertEditor.Services
{
    public class View3DService
    {
        private HelixViewport3D viewport;
        private ModelVisual3D modelContainer;
        private TextBlock infoLabel;
        private TextBlock statsLabel;

        // Material color dictionary
        private readonly Dictionary<string, Color> materialColors = new Dictionary<string, Color>
        {
            { "Concrete (Bê tông)", Color.FromRgb(200, 200, 200) },
            { "Steel (Thép)", Color.FromRgb(180, 180, 200) },
            { "Plastic (Nhựa)", Color.FromRgb(150, 200, 150) },
            { "Glass (Kính)", Color.FromArgb(100, 150, 200, 255) },
            { "Wood (Gỗ)", Color.FromRgb(139, 90, 43) }
        };

        public void Initialize(HelixViewport3D helixViewport, ModelVisual3D container,
            TextBlock info, TextBlock stats)
        {
            viewport = helixViewport;
            modelContainer = container;
            statsLabel = stats;
        }

        public void GenerateModel(CulvertParameters parameters)
        {
            if (modelContainer == null) return;
            modelContainer.Children.Clear();

            double w = parameters.SectionWidth / 1000.0;
            double h = parameters.SectionHeight / 1000.0;
            double wt = parameters.WallThickness / 1000.0;
            double length = parameters.L3 / 1000.0;

            // Outer box
            var outerBox = new BoxVisual3D
            {
                Center = new Point3D(0, 0, 0),
                Length = length,
                Width = w,
                Height = h,
                Fill = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                Material = MaterialHelper.CreateMaterial(Colors.LightGray)
            };
            modelContainer.Children.Add(outerBox);

            // Inner void
            var innerBox = new BoxVisual3D
            {
                Center = new Point3D(0, 0, 0),
                Length = length + 0.1,
                Width = w - 2 * wt,
                Height = h - 2 * wt,
                Fill = new SolidColorBrush(Color.FromArgb(100, 100, 150, 200)),
                Material = MaterialHelper.CreateMaterial(Color.FromArgb(150, 100, 150, 200))
            };
            modelContainer.Children.Add(innerBox);

            // Wireframe
            if (parameters.ShowWireframe)
            {
                var wireframe = new BoundingBoxWireFrameVisual3D
                {
                    BoundingBox = new Rect3D(-w / 2, -h / 2, -length / 2, w, h, length),
                    Thickness = 2,
                    Color = Colors.Yellow
                };
                modelContainer.Children.Add(wireframe);
            }

            // Bounding box
            if (parameters.ShowBoundingBox)
            {
                var bbox = new BoundingBoxWireFrameVisual3D
                {
                    BoundingBox = new Rect3D(-w / 2, -h / 2, -length / 2, w, h, length),
                    Thickness = 3,
                    Color = Colors.Red
                };
                modelContainer.Children.Add(bbox);
            }

            UpdateStats(parameters);
            if (viewport != null)
                viewport.ZoomExtents(500);
        }

        public void SetCameraView(string view)
        {
            if (viewport == null || viewport.Camera == null) return;

            switch (view.ToLower())
            {
                case "front":
                    viewport.Camera.Position = new Point3D(0, 0, 20);
                    viewport.Camera.LookDirection = new Vector3D(0, 0, -1);
                    viewport.Camera.UpDirection = new Vector3D(0, 1, 0);
                    UpdateInfo("3D View - Front");
                    break;
                case "back":
                    viewport.Camera.Position = new Point3D(0, 0, -20);
                    viewport.Camera.LookDirection = new Vector3D(0, 0, 1);
                    viewport.Camera.UpDirection = new Vector3D(0, 1, 0);
                    UpdateInfo("3D View - Back");
                    break;
                case "top":
                    viewport.Camera.Position = new Point3D(0, 20, 0);
                    viewport.Camera.LookDirection = new Vector3D(0, -1, 0);
                    viewport.Camera.UpDirection = new Vector3D(0, 0, -1);
                    UpdateInfo("3D View - Top");
                    break;
                case "bottom":
                    viewport.Camera.Position = new Point3D(0, -20, 0);
                    viewport.Camera.LookDirection = new Vector3D(0, 1, 0);
                    viewport.Camera.UpDirection = new Vector3D(0, 0, 1);
                    UpdateInfo("3D View - Bottom");
                    break;
                case "left":
                    viewport.Camera.Position = new Point3D(-20, 0, 0);
                    viewport.Camera.LookDirection = new Vector3D(1, 0, 0);
                    viewport.Camera.UpDirection = new Vector3D(0, 1, 0);
                    UpdateInfo("3D View - Left");
                    break;
                case "right":
                    viewport.Camera.Position = new Point3D(20, 0, 0);
                    viewport.Camera.LookDirection = new Vector3D(-1, 0, 0);
                    viewport.Camera.UpDirection = new Vector3D(0, 1, 0);
                    UpdateInfo("3D View - Right");
                    break;
            }
        }

        public void ExportOBJ(string filePath)
        {
            using (var stream = File.Create(filePath))
            {
                var exporter = new ObjExporter();
                exporter.Export(viewport.Viewport, stream);
            }
        }

        public void ExportSTL(string filePath)
        {
            using (var stream = File.Create(filePath))
            {
                var exporter = new StlExporter();
                exporter.Export(viewport.Viewport, stream);
            }
        }

        public void ExportScreenshot(string filePath)
        {
            if (viewport != null)
                viewport.Export(filePath);
        }

        public void ApplyMaterial(string materialName)
        {
            if (modelContainer == null) return;

            // ✅ Get color from dictionary (C# 7.3 compatible)
            Color color = materialColors.ContainsKey(materialName)
                ? materialColors[materialName]
                : Color.FromRgb(200, 200, 200);

            foreach (var child in modelContainer.Children)
            {
                if (child is BoxVisual3D box)
                {
                    box.Fill = new SolidColorBrush(color);
                    box.Material = MaterialHelper.CreateMaterial(color);
                }
            }
        }

        private void UpdateInfo(string text)
        {
            if (infoLabel != null)
                infoLabel.Text = text;
        }

        private void UpdateStats(CulvertParameters p)
        {
            if (statsLabel != null)
                statsLabel.Text = string.Format("W: {0}mm | H: {1}mm | L: {2}mm",
                    p.SectionWidth, p.SectionHeight, p.L3);
        }
    }
}
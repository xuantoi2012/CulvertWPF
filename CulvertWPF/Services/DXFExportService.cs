using System;
using CulvertEditor.Models;

namespace CulvertEditor.Services
{
    public class DXFExportService
    {
        public void ExportDrawing(string filename, CulvertParameters parameters, string drawingType, double scale)
        {
            // TODO: Implement actual DXF export
            // For now, create empty file to test
            System.IO.File.WriteAllText(filename,
                string.Format("DXF Export\nScale: 1:{0}\nType: {1}\n", scale, drawingType));
        }
    }
}
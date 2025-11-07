namespace CulvertEditor.Models
{
    public class CulvertParameters
    {
        // Bản quá độ
        public double L1 { get; set; } = 24500;
        public double W1 { get; set; } = 4000;

        // Sân cống - Tường đầu
        public double L2 { get; set; } = 5000;
        public double W2 { get; set; } = 1800;
        public double W3 { get; set; } = 2000;

        // Cống hộp
        public double W { get; set; } = 5000;
        public double L3 { get; set; } = 30000;

        // Tường cánh
        public double Alpha { get; set; } = 20;

        // Mặt cắt
        public double SectionWidth { get; set; } = 5000;
        public double SectionHeight { get; set; } = 3000;
        public double WallThickness { get; set; } = 300;
        public double ExcavationDepth { get; set; } = 2000;
        public double SlopeRatio { get; set; } = 1.5;

        // Display options
        public bool ShowDimensions { get; set; } = true;
        public bool ShowPoints { get; set; } = true;
        public bool ShowExcavation { get; set; } = true;
        public bool ShowWireframe { get; set; } = false;
        public bool ShowBoundingBox { get; set; } = false;

        public class ScaleOption
        {
            public string Display { get; set; }
            public double Value { get; set; }
            public string Description { get; set; }

            public override string ToString() => Display;
        }
    }
}
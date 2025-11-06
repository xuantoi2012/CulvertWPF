using CulvertEditor.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CulvertEditor.Services
{
    public class SectionViewService
    {
        public void Draw(Canvas canvas, CulvertParameters parameters)
        {
            if (canvas == null) return;
            canvas.Children.Clear();

            double centerX = canvas.Width / 2;
            double centerY = canvas.Height / 2;

            double boxWidth = parameters.SectionWidth * DrawingHelpers.SCALE;
            double boxHeight = parameters.SectionHeight * DrawingHelpers.SCALE;

            // Outer box
            Point topLeftOuter = new Point(centerX - boxWidth / 2, centerY - boxHeight / 2);
            Point topRightOuter = new Point(centerX + boxWidth / 2, centerY - boxHeight / 2);
            Point botRightOuter = new Point(centerX + boxWidth / 2, centerY + boxHeight / 2);
            Point botLeftOuter = new Point(centerX - boxWidth / 2, centerY + boxHeight / 2);

            DrawingHelpers.DrawLine(canvas, topLeftOuter, topRightOuter, DrawingHelpers.COLOR_MAIN_LINE, 2);
            DrawingHelpers.DrawLine(canvas, topRightOuter, botRightOuter, DrawingHelpers.COLOR_MAIN_LINE, 2);
            DrawingHelpers.DrawLine(canvas, botRightOuter, botLeftOuter, DrawingHelpers.COLOR_MAIN_LINE, 2);
            DrawingHelpers.DrawLine(canvas, botLeftOuter, topLeftOuter, DrawingHelpers.COLOR_MAIN_LINE, 2);

            // Inner void
            double innerWidth = boxWidth - 2 * parameters.WallThickness * DrawingHelpers.SCALE;
            double innerHeight = boxHeight - 2 * parameters.WallThickness * DrawingHelpers.SCALE;

            Point topLeftInner = new Point(centerX - innerWidth / 2, centerY - innerHeight / 2);
            Point topRightInner = new Point(centerX + innerWidth / 2, centerY - innerHeight / 2);
            Point botRightInner = new Point(centerX + innerWidth / 2, centerY + innerHeight / 2);
            Point botLeftInner = new Point(centerX - innerWidth / 2, centerY + innerHeight / 2);

            DrawingHelpers.DrawLine(canvas, topLeftInner, topRightInner, DrawingHelpers.COLOR_MAIN_LINE, 1.5);
            DrawingHelpers.DrawLine(canvas, topRightInner, botRightInner, DrawingHelpers.COLOR_MAIN_LINE, 1.5);
            DrawingHelpers.DrawLine(canvas, botRightInner, botLeftInner, DrawingHelpers.COLOR_MAIN_LINE, 1.5);
            DrawingHelpers.DrawLine(canvas, botLeftInner, topLeftInner, DrawingHelpers.COLOR_MAIN_LINE, 1.5);

            // Excavation
            if (parameters.ShowExcavation)
            {
                DrawExcavation(canvas, centerX, centerY, boxWidth, boxHeight, parameters);
            }

            // Label
            DrawingHelpers.AddLabel("MẶT CẮT CỐNG HỘP", centerX, centerY,
                new SolidColorBrush(DrawingHelpers.COLOR_TEXT), 14, canvas);

            // Dimensions
            if (parameters.ShowDimensions)
            {
                var dimBrush = new SolidColorBrush(DrawingHelpers.COLOR_DIMENSION);
                DrawingHelpers.AddHorizontalDimension(topLeftOuter.X, topLeftOuter.Y - 40,
                    parameters.SectionWidth, "W=" + parameters.SectionWidth, dimBrush, canvas);
                DrawingHelpers.AddVerticalDimension(topLeftOuter.X - 40, topLeftOuter.Y,
                    parameters.SectionHeight, "H=" + parameters.SectionHeight, dimBrush, canvas);
            }
        }

        private void DrawExcavation(Canvas canvas, double centerX, double centerY,
            double boxWidth, double boxHeight, CulvertParameters p)
        {
            double excavationTop = centerY - boxHeight / 2 - p.ExcavationDepth * DrawingHelpers.SCALE;
            double excavationWidth = boxWidth + 2 * p.ExcavationDepth * p.SlopeRatio * DrawingHelpers.SCALE;

            Point excav1 = new Point(centerX - excavationWidth / 2, excavationTop);
            Point excav2 = new Point(centerX + excavationWidth / 2, excavationTop);
            Point excav3 = new Point(centerX + boxWidth / 2, centerY + boxHeight / 2);
            Point excav4 = new Point(centerX - boxWidth / 2, centerY + boxHeight / 2);

            DrawingHelpers.DrawDashedLine(canvas, excav1, excav4,
                new SolidColorBrush(DrawingHelpers.COLOR_EXCAVATION), new DoubleCollection { 5, 3 });
            DrawingHelpers.DrawDashedLine(canvas, excav2, excav3,
                new SolidColorBrush(DrawingHelpers.COLOR_EXCAVATION), new DoubleCollection { 5, 3 });
            DrawingHelpers.DrawDashedLine(canvas, excav1, excav2,
                new SolidColorBrush(DrawingHelpers.COLOR_EXCAVATION), new DoubleCollection { 5, 3 });

            DrawingHelpers.AddLabel($"PHUI ĐÀO 1:{p.SlopeRatio}", centerX, excavationTop - 20,
                new SolidColorBrush(DrawingHelpers.COLOR_DIMENSION), 12, canvas);
        }
    }
}
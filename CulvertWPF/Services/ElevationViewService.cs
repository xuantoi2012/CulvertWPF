using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CulvertEditor.Services
{
    public class ElevationViewService
    {
        public void Draw(Canvas canvas)
        {
            if (canvas == null) return;
            canvas.Children.Clear();

            double centerX = canvas.Width / 2;
            double centerY = canvas.Height / 2;

            DrawingHelpers.AddLabel("MẶT ĐỨNG - ĐANG PHÁT TRIỂN",
                centerX, centerY, Brushes.Gray, 16, canvas);
        }
    }
}
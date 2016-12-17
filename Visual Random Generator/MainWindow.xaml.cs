using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Visual_Random_Generator
{
    public partial class MainWindow
    {
        [DllImport("../../Assemblies/CPU Analyser.dll")]
        private static extern uint GetCurrentCpuRate();

        private System.Windows.Threading.DispatcherTimer ZoomTimer { get; } = new System.Windows.Threading.DispatcherTimer();
        private bool IsZoomed { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            DrawLines();

            ZoomTimer.Tick += TimerMethod;
            ZoomTimer.Interval = TimeSpan.FromSeconds(1);
            ZoomTimer.Start();
        }

        #region Canvas
        private void TimerMethod(object sender, EventArgs e)
        {
            const double from = 1;
            const double to = 1.1;
            ZoomAnimation(IsZoomed ? to : from, IsZoomed ? from : to);
            IsZoomed = !IsZoomed;
        }

        private void DrawLines()
        {
            const int thikness = 4;
            const int starter = 4;
            const int length = 50;
            var bootomX = ClickCanvas.Width;
            var bootomY = ClickCanvas.Height;

            //  _
            // |
            AddLine(starter - thikness, starter, starter + length, starter);
            AddLine(starter, starter, starter, starter + length);

            // _|
            AddLine(bootomX - starter, bootomY - starter, bootomX - starter, bootomY - starter - length);
            AddLine(bootomX - starter + thikness, bootomY - starter, bootomX - starter - length, bootomY - starter);

            // |_
            AddLine(starter, bootomY - starter, starter, bootomY - starter - length);
            AddLine(starter - thikness, bootomY - starter, starter + length, bootomY - starter);

            // _
            //  |
            AddLine(bootomX - starter + thikness, starter, bootomX - starter - length, starter);
            AddLine(bootomX - starter, starter, bootomX - starter, starter + length);
        }

        private void AddLine(double x1, double y1, double x2, double y2)
        {
            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                StrokeThickness = 8,
                Stroke = Brushes.Black
            };

            ClickCanvas.Children.Add(line);
        }

        private void ZoomAnimation(double from, double to)
        {
            ScaleTransform trans = new ScaleTransform();
            ClickCanvas.RenderTransform = trans;

            DoubleAnimation anim = new DoubleAnimation(from, to, TimeSpan.FromSeconds(1));
            trans.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            trans.BeginAnimation(ScaleTransform.ScaleYProperty, anim);

        }

        private async Task CanvasFlash()
        {
            const int animationWait = 150;
            var cb = ClickCanvas.Background;
            // ReSharper disable once PossibleNullReferenceException
            var convertFromString = (Color)ColorConverter.ConvertFromString("#FF007ACC");
            var da = new ColorAnimation
            {
                To = convertFromString,
                Duration = new Duration(TimeSpan.FromMilliseconds(animationWait))
            };
            cb.BeginAnimation(SolidColorBrush.ColorProperty, da);
            await Task.Delay(animationWait);
            var da1 = new ColorAnimation
            {
                To = Colors.AliceBlue,
                Duration = new Duration(TimeSpan.FromMilliseconds(animationWait))
            };
            cb.BeginAnimation(SolidColorBrush.ColorProperty, da1);
        }

        private void ClickCanvas_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ClickCanvas.Background = new SolidColorBrush(Colors.AliceBlue);
        }

        private void ClickCanvas_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ClickCanvas.Background = new SolidColorBrush(Colors.White);
        }

        private async void ClickCanvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBoxCpu.AppendText($"{GetCurrentCpuRate()}\n");
            TextBoxCpu.ScrollToEnd();
            //TextBoxCpu.Text += $"{GetCurrentCpuRate()}\n";

            await CanvasFlash();
        }
        #endregion
    }
}

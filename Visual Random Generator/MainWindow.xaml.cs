using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Numerics;
using Microsoft.Win32;

namespace Visual_Random_Generator
{
    public partial class MainWindow
    {
        [DllImport("../../Assemblies/CPU Analyser.dll")]
        private static extern uint GetCurrentCpuRate();

        private System.Windows.Threading.DispatcherTimer ZoomTimer { get; } = new System.Windows.Threading.DispatcherTimer();
        private bool IsZoomed { get; set; }
        private bool IsAlreadyRun { get; set; }
        private bool IsTimeToStopZoomTimer { get; set; }

        private Algorithm Algorithm { get; } = new Algorithm();

        private uint PrevCpuRate { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            DrawLines();

            ZoomTimer.Tick += TimerMethod;
            ZoomTimer.Interval = TimeSpan.FromSeconds(1);
            ZoomTimer.Start();

            TextBoxS.Visibility = Visibility.Hidden;
            LabelS.Visibility = Visibility.Hidden;

            TextBoxD.Visibility = Visibility.Hidden;
            LabelD.Visibility = Visibility.Hidden;

            TextBoxK.Visibility = Visibility.Hidden;
            LabelK.Visibility = Visibility.Hidden;

            TextBoxRandom.Visibility = Visibility.Hidden;
            LabelRandom.Visibility = Visibility.Hidden;

            TextBoxBitK.Visibility = Visibility.Hidden;
            TextBoxBitS.Visibility = Visibility.Hidden;
            ButtonK.Visibility = Visibility.Hidden;
            ButtonS.Visibility = Visibility.Hidden;
        }

        #region Canvas
        private void TimerMethod(object sender, EventArgs e)
        {
            if (IsTimeToStopZoomTimer && !IsZoomed)
            {
                ZoomTimer.Stop();
                ClickCanvas.Background = new SolidColorBrush(Colors.White);
                ClickCanvas.Visibility = Visibility.Hidden;
            }

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
            if (IsAlreadyRun)
            {
                IsTimeToStopZoomTimer = true;
                await RunAlgorithm();
                return;
            }

            var cpuRate = GetCurrentCpuRate();
            TextBoxCpu.AppendText($"{cpuRate}\n");
            TextBoxCpu.ScrollToEnd();

            if (PrevCpuRate == 0)
            {
                TextBoxS.Visibility = Visibility.Visible;
                LabelS.Visibility = Visibility.Visible;
            }
            else
            {
                var bi = new BigInteger(Algorithm.S.Data.ToArray());
                bi *= Math.Abs(cpuRate - PrevCpuRate);
                if (bi < new BigInteger(Math.Pow(2, 128)))
                    Algorithm.S.Data = new List<byte>(bi.ToByteArray());
                else
                {
                    Algorithm.S.Data = new List<byte>(bi.ToByteArray().Where((t, idx) => idx < 16));
                    IsTimeToStopZoomTimer = true;
                    await RunAlgorithm();
                }
                TextBoxS.Text = new BigInteger(Algorithm.S.Data.ToArray()).ToString("X32");
            }
            PrevCpuRate = cpuRate;

            if (!IsTimeToStopZoomTimer)
                await CanvasFlash();
        }
        #endregion

        private async Task RunAlgorithm()
        {
            //ClickCanvas.Visibility = Visibility.Hidden;
            //ZoomTimer.Stop();

            if (!IsAlreadyRun)
            {
                LabelMain.Content = "Thank you";
                LabelMain.Foreground = Brushes.Green;

                TextBoxD.Visibility = Visibility.Visible;
                LabelD.Visibility = Visibility.Visible;
                TextBoxK.Visibility = Visibility.Visible;
                LabelK.Visibility = Visibility.Visible;
                TextBoxRandom.Visibility = Visibility.Visible;
                LabelRandom.Visibility = Visibility.Visible;

                Algorithm.D.Data = new List<byte>(new BigInteger(DateTime.UtcNow.Ticks).ToByteArray());

                var howMuchMore = 16 - Algorithm.D.Data.Count;
                for (var i = 0; i < howMuchMore; i++)
                    Algorithm.D.Data.Add(0);
                TextBoxD.Text = new BigInteger(Algorithm.D.Data.ToArray()).ToString("X32");
                TextBoxK.Text = new BigInteger(Algorithm.K.Data.ToArray()).ToString("X32");

                await Task.Delay(1000);
                LabelMain.Foreground = Brushes.Black;
            }
            IsAlreadyRun = true;

            await GenerateRandomValue();
        }

        private async Task GenerateRandomValue()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Algorithm.Stage3();
            while (!Algorithm.IsCompleted)
            {
                Algorithm.GenerateRandomBit();

                var randomValueText = Algorithm.RandomValue.Data.Aggregate("", (current, b) => b.ToString("X2") + current);
                TextBoxRandom.Text = randomValueText;
                //TextBoxRandom.Text = new BigInteger(Algorithm.RandomValue.Data.ToArray()).ToString("X");

                if (Algorithm.CurrentByte != 0)
                {
                    var estimated = watch.Elapsed.TotalSeconds / Algorithm.CurrentByte *
                                    (Algorithm.RandomValueSize - Algorithm.CurrentByte);
                    LabelMain.Content = $"Generated {Algorithm.CurrentByte} bytes." +
                                        $"\nTime left: {TimeSpan.FromSeconds(Math.Ceiling(estimated))}";
                }
                else
                    LabelMain.Content = $"Generated {Algorithm.CurrentByte} bytes.";

                await Task.Delay(1);
            }
            watch.Stop();

            var dlg = new SaveFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Text file (.txt)|*.txt"
            };
            if (dlg.ShowDialog() == true)
                using (var writer = new BinaryWriter(File.Open(dlg.FileName, FileMode.Create)))
                {
                    writer.Write(Algorithm.RandomValue.Data.ToArray());
                }

            IsTimeToStopZoomTimer = false;
            ClickCanvas.Background = new SolidColorBrush(Colors.White);
            ClickCanvas.Visibility = Visibility.Visible;
            LabelOnCanvas.Content = "Run again";
            LabelMain.Content = "Random value generated";
            ZoomTimer.Start();
            Algorithm.Reset();

            TextBoxBitK.Visibility = Visibility.Visible;
            TextBoxBitS.Visibility = Visibility.Visible;
            ButtonK.Visibility = Visibility.Visible;
            ButtonS.Visibility = Visibility.Visible;
        }

        private void TextBoxBitS_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            int result;
            e.Handled = !int.TryParse(e.Text, out result);
        }

        private void ButtonS_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(TextBoxBitS.Text);
            if (128 <= value)
                return;
            var bitN = value % 8;
            var byteN = value / 8;
            Algorithm.S.Data[byteN] ^= (byte) (1 << bitN);
            TextBoxS.Text = new BigInteger(Algorithm.S.Data.ToArray()).ToString("X32");
        }

        private void ButtonK_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(TextBoxBitK.Text);
            if (128 <= value)
                return;
            var bitN = value % 8;
            var byteN = value / 8;
            Algorithm.K.Data[byteN] ^= (byte)(1 << bitN);
            TextBoxK.Text = new BigInteger(Algorithm.K.Data.ToArray()).ToString("X32");
        }
    }
}

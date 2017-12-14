using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Un4seen.Bass.Misc;

namespace WpfControlLibraryBass.Elements
{
    public class SpectrumAnalyzer : Image
    {
        #region Fields

        //Timer _timer = null;
        DispatcherTimer _timer = new DispatcherTimer();
        //private int _imgWidth = 0;
        //private int _imgHeight = 0;

        //private int _stream;

        private System.Drawing.Color _color;

        private System.Drawing.Color _color2;

        //private bool _isStreamPlaying = false;

        //private DISPLAY _display = DISPLAY.SPECTRUM_LINE;
        
        Visuals vis = new Visuals();

        public const int TIMER_INTERVAL = 50;

        public enum DISPLAY { NONE, SPECTRUM_LINE, WAVE_FORM }

        #endregion Fields

        #region Constructor

        public SpectrumAnalyzer()
        {
            _timer.Interval = new TimeSpan(0, 0, 0, 0, TIMER_INTERVAL);
            _timer.Tick += _timer_Tick;

            this.Loaded += SpectrumAnalyzer_Loaded;
            this.IsVisibleChanged += SpectrumAnalyzer_IsVisibleChanged;
        }

        #endregion Constructor

        #region DependencyProperty

        public static readonly DependencyProperty StreamProperty =
            DependencyProperty.Register("Stream", typeof(int), typeof(SpectrumAnalyzer), new PropertyMetadata(IsStreamChanged));

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool), typeof(SpectrumAnalyzer), new PropertyMetadata(false, IsPlayingChanged));

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(SpectrumAnalyzer), new PropertyMetadata(new SolidColorBrush(Colors.Green), IsColorChanged));

        public static readonly DependencyProperty DisplayProperty =
           DependencyProperty.Register("Display", typeof(DISPLAY), typeof(SpectrumAnalyzer), new PropertyMetadata(DISPLAY.SPECTRUM_LINE, OnDisplayChanged));

        #endregion DependencyProperty

        #region Properties

        public int Stream
        {
            get { return (int)GetValue(StreamProperty); }
            set { SetValue(StreamProperty, value); }
        }

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public DISPLAY Display
        {
            get { return (DISPLAY)GetValue(DisplayProperty); }
            set { SetValue(DisplayProperty, value); }
        }

        #endregion Properties

        #region PropertyMetadata

        static void SpectrumAnalyzer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer s = sender as SpectrumAnalyzer;

            s.CheckOnStateChanged();
        }

        void SpectrumAnalyzer_Loaded(object sender, RoutedEventArgs e)
        {
            //_imgWidth = (int)this.Width;
            //_imgHeight = (int)this.Height;            
        }

        static void OnDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DISPLAY newVal = (DISPLAY)e.NewValue;

            SpectrumAnalyzer s = d as SpectrumAnalyzer;

            //s._display = newVal;

            s.CheckOnStateChanged();
        }

        static void IsColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SolidColorBrush newVal = null;

            SpectrumAnalyzer s = d as SpectrumAnalyzer;
            if (e.NewValue.GetType() == typeof(SolidColorBrush))
                newVal = (SolidColorBrush)e.NewValue;
            else if (e.NewValue.GetType() == typeof(LinearGradientBrush))
            {
                LinearGradientBrush lgb = (LinearGradientBrush)e.NewValue;
                newVal = new SolidColorBrush(lgb.GradientStops[lgb.GradientStops.Count - 1].Color);
            }

            s._color = System.Drawing.Color.FromArgb(newVal.Color.A, newVal.Color.R, newVal.Color.G, newVal.Color.B);

            Func<int, int, int> funcDarken = new Func<int, int, int>((color, dark) => {
                int val = color - dark ;
                return val < 0 ? 0 : val;
            });

            s._color2 = System.Drawing.Color.FromArgb(
                funcDarken(newVal.Color.A, 128),
                funcDarken(newVal.Color.R, 128),
                funcDarken(newVal.Color.G, 128),
                funcDarken(newVal.Color.B, 128));
        }

        static void IsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool newVal = (bool)e.NewValue;
            SpectrumAnalyzer s = d as SpectrumAnalyzer;
            
            //s._isStreamPlaying = newVal;

            s.CheckOnStateChanged();
                
        }

        static void IsStreamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int newVal = (int)e.NewValue;
            SpectrumAnalyzer s = d as SpectrumAnalyzer;

            //s._stream = newVal;

            s.CheckOnStateChanged();
        }

        

        void _timer_Tick(object sender, EventArgs e)
        {
            int stream = Stream;
            int width = (int)Width;
            int height = (int)Height;
            DISPLAY display = Display;

            if (stream != 0)
            {
                System.Drawing.Bitmap bmp = null;

                switch (display)
                {
                    case DISPLAY.SPECTRUM_LINE:
                        bmp = vis.CreateSpectrumLine(stream, width, height, _color, _color2, System.Drawing.Color.Transparent, 5, 3, false, false, false);
                        break;
                    case DISPLAY.WAVE_FORM:
                        bmp = vis.CreateSpectrumWave(stream, width, height, _color, _color2, System.Drawing.Color.Transparent, 1, false, false, false);
                        break;
                }

                if (bmp != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        System.Windows.Media.Imaging.BitmapImage bImg = new System.Windows.Media.Imaging.BitmapImage();
                        bImg.BeginInit();
                        bImg.StreamSource = new MemoryStream(ms.ToArray());
                        bImg.EndInit();
                        this.Source = bImg;
                    }
                }
            }
            else
                this.Source = null;
        }

        //public void CallTimerCallback(Object stateInfo)
        //{
        //    if (_stream != 0)
        //    {

        //        System.Drawing.Bitmap bmp = vis.CreateSpectrumLine(_stream, _imgWidth, _imgHeight, System.Drawing.Color.Green, System.Drawing.Color.Red, System.Drawing.Color.Transparent, 2, 3, false, false, false);

        //        this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(delegate()
        //        {
        //            ImageSource imgSrc = IMPUtils.ImageToBitmapImage(bmp);

        //            this.Source = imgSrc;
        //        }));
        //    }
        //    else
        //        this.Source = null;
        //}

        #endregion Events

        #region Methods

        private void CheckOnStateChanged()
        {
            DISPLAY display = Display;
            int width = (int)Width;
            int height = (int)Height;
            bool isStreamPlaying = IsPlaying;

            if (isStreamPlaying && IsVisible && display != DISPLAY.NONE && _timer.IsEnabled == false)
                _timer.Start();
            else if ((isStreamPlaying == false || IsVisible == false || display == DISPLAY.NONE) && _timer.IsEnabled == true)
            {
                _timer.Stop();
                int stride = width * 3;
                byte[] pixels = new byte[height * stride];
                
                List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
                colors.Add(System.Windows.Media.Colors.Transparent);
                BitmapPalette myPalette = new BitmapPalette(colors);

                // make an empty image to make it clickable
                this.Source = BitmapSource.Create(
                                         width, height,
                                         96, 96,
                                         PixelFormats.Indexed1,
                                         myPalette,
                                         pixels,
                                         stride);
            }
        }

        #endregion Methods

    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Un4seen.Bass;
using Un4seen.Bass.Misc;

namespace WpfControlLibraryBass.Elements
{
    /// <summary>
    /// Interaction logic for PeakLevel.xaml
    /// </summary>
    public partial class PeakLevel : UserControl
    {
        #region Constructor

        public PeakLevel()
        {
            InitializeComponent();

            timer.Interval = new TimeSpan(0, 0, 0, 0, TIMER_INTERVAL);
            timer.Tick += _timer_Tick;

            ChangeColor(new SolidColorBrush(Colors.Transparent));
        } 

        #endregion

        #region Fields

        DispatcherTimer timer = new DispatcherTimer();
        int rectangleWidth = 0;
        int stream;

        Visuals vis = new Visuals();

        public const int TIMER_INTERVAL = 50;

        #endregion Fields

        #region DependencyProperty

        public static readonly DependencyProperty StreamProperty =
            DependencyProperty.Register("Stream", typeof(int), typeof(PeakLevel), new PropertyMetadata(IsStreamChanged));

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool), typeof(PeakLevel), new PropertyMetadata(false, IsPlayingChanged));

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(PeakLevel), new PropertyMetadata(new SolidColorBrush(Colors.LightGray), IsColorChanged));

        public static readonly DependencyProperty BarDistanceProperty =
            DependencyProperty.Register("BarDistance", typeof(int), typeof(PeakLevel));

        #endregion


        #region Properties

        public int Stream
        {
            get { return (int)GetValue(StreamProperty); }
            set { SetValue(StreamProperty, value); }
        }

        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public int BarDistance
        {
            get { return (int)GetValue(BarDistanceProperty); }
            set { SetValue(BarDistanceProperty, value); }
        }

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        #endregion


        #region Events

        static void IsStreamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int newVal = (int)e.NewValue;
            PeakLevel s = d as PeakLevel;

            s.stream = newVal;
        }

        static void IsColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SolidColorBrush newVal = null;

            PeakLevel s = d as PeakLevel;
            if (e.NewValue.GetType() == typeof(SolidColorBrush))
                newVal = (SolidColorBrush)e.NewValue;
            else if (e.NewValue.GetType() == typeof(LinearGradientBrush))
            {
                LinearGradientBrush lgb = (LinearGradientBrush)e.NewValue;
                newVal = new SolidColorBrush(lgb.GradientStops[lgb.GradientStops.Count - 1].Color);
            }

            if (s.IsPlaying)
                s.ChangeColor(newVal);
        }

        static void IsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool newVal = (bool)e.NewValue;
            PeakLevel s = d as PeakLevel;
            if (newVal)
            {
                s.timer.Start();
                s.ChangeColor(s.Color);
            }
            else
            {
                s.timer.Stop();
                s.ChangeColor(new SolidColorBrush(Colors.Transparent));
            }

        }


        void _timer_Tick(object sender, EventArgs e)
        {
            rectangleWidth = (int)this.bglbar.ActualWidth;

            if (stream != 0)
            {
                int level = 0;

                level = Bass.BASS_ChannelGetLevel(stream);

                int left = Utils.LowWord32(level); // the left level
                int right = Utils.HighWord32(level); // the right level

                this.lbar.Width = rectangleWidth * left >> 15;
                this.rbar.Width = rectangleWidth * right >> 15;
            }
        }

        #endregion

        #region Methods

        private void ChangeColor(Brush color)
        {
            this.lbar.Fill = color;
            this.bglbar.Fill = color;
            this.rbar.Fill = color;
            this.bgrbar.Fill = color;
        }

        #endregion
    }
}

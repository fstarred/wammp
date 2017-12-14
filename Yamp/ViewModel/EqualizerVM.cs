using MvvmFoundation.Wpf;
using System;
using System.Windows.Input;
using TinyIoC;
using Yemp.Naming;
using Yemp.Services;
using YempCommons.Utils;
using YempCommons.ViewModel;

namespace Yemp.ViewModel
{
    public class EqualizerVM : BaseViewModel
    {
        #region Fields

        readonly float[] EQ_BANDS = new float[] { 125, 250, 500, 1000, 2000, 4000, 8000, 16000 };
        
        #endregion

        #region Constructor

        public EqualizerVM()
        {
            if (!Utility.IsDesignMode())
            {
                AudioControllerService.Instance.StreamCreated += Instance_StreamCreated;

                Reload();
            }
        }

        #endregion Constructor

        #region Events
        
        void Instance_StreamCreated(int channel)
        {
            AudioControllerService acservice = AudioControllerService.Instance;

            acservice.InitEQ(EQ_BANDS);

            acservice.SetEQ(0, Eq0);
            acservice.SetEQ(1, Eq1);
            acservice.SetEQ(2, Eq2);
            acservice.SetEQ(3, Eq3);
            acservice.SetEQ(4, Eq4);
            acservice.SetEQ(5, Eq5);
            acservice.SetEQ(6, Eq6);
            acservice.SetEQ(7, Eq7);
        }
        

        #endregion Events

        #region Properties

        private float eq0;
        public float Eq0
        {
            get
            {
                return eq0;
            }
            set
            {
                eq0 = value;
                AudioControllerService.Instance.SetEQ(0, eq0);
                RaisePropertyChanged(() => Eq0);                
            }
        }

        private float eq1;
        public float Eq1
        {
            get
            {
                return eq1;
            }
            set
            {
                eq1 = value;
                AudioControllerService.Instance.SetEQ(1, eq1);
                RaisePropertyChanged(() => Eq1);
            }
        }

        private float eq2;
        public float Eq2
        {
            get
            {
                return eq2;
            }
            set
            {
                eq2 = value;
                AudioControllerService.Instance.SetEQ(2, eq2);
                RaisePropertyChanged(() => Eq2);
            }
        }


        private float eq3;
        public float Eq3
        { get { return eq3; } 
            set
            {
                eq3 = value;
                AudioControllerService.Instance.SetEQ(3, eq3);
                RaisePropertyChanged(() => Eq3);
            }
        }

        private float eq4;
        public float Eq4
        {
            get { return eq4; }
            set
            {
                eq4 = value;
                AudioControllerService.Instance.SetEQ(4, eq4);
                RaisePropertyChanged(() => Eq4);
            }
        }

        private float eq5;
        public float Eq5
        {
            get { return eq5; }
            set
            {
                eq5 = value;
                AudioControllerService.Instance.SetEQ(5, eq5);
                RaisePropertyChanged(() => Eq5);
            }
        }

        private float eq6;
        public float Eq6
        {
            get { return eq6; }
            set
            {
                eq6 = value;
                AudioControllerService.Instance.SetEQ(6, eq6);
                RaisePropertyChanged(() => Eq6);
            }
        }


        private float eq7;
        public float Eq7
        {
            get { return eq7; }
            set
            {
                eq7 = value;
                AudioControllerService.Instance.SetEQ(7, eq7);
                RaisePropertyChanged(() => Eq7);
            }
        }

        #endregion Properties

        #region Methods

        void Reload()
        {
            var container = TinyIoC.TinyIoCContainer.Current;

            IAudioConfigProvider service = container.Resolve<IAudioConfigProvider>(Naming.ContainerNSR.AUDIO_SETTINGS);

            service.Load();

            service.EqValues = service.EqValues;

            if (service.EqValues.Length > 0) this.Eq0 = service.EqValues[0];
            if (service.EqValues.Length > 1) this.Eq1 = service.EqValues[1];
            if (service.EqValues.Length > 2) this.Eq2 = service.EqValues[2];
            if (service.EqValues.Length > 3) this.Eq3 = service.EqValues[3];
            if (service.EqValues.Length > 4) this.Eq4 = service.EqValues[4];
            if (service.EqValues.Length > 5) this.Eq5 = service.EqValues[5];
            if (service.EqValues.Length > 6) this.Eq6 = service.EqValues[6];
            if (service.EqValues.Length > 7) this.Eq7 = service.EqValues[7];
        }

        void ResetEqualizer()
        {
            Eq0 = 0;
            Eq1 = 0;
            Eq2 = 0;
            Eq3 = 0;
            Eq4 = 0;
            Eq5 = 0;
            Eq6 = 0;
            Eq7 = 0;
        }

        void SaveAudioSettings()
        {
            System.Diagnostics.Debug.WriteLine("Saving mixer settings");

            var container = TinyIoCContainer.Current;

            IAudioConfigProvider serviceAudio = container.Resolve<IAudioConfigProvider>(ContainerNSR.AUDIO_SETTINGS);

            serviceAudio.Load();

            int cnvEq0 = (int)Math.Ceiling(this.eq0);
            int cnvEq1 = (int)Math.Ceiling(this.eq1);
            int cnvEq2 = (int)Math.Ceiling(this.eq2);
            int cnvEq3 = (int)Math.Ceiling(this.eq3);
            int cnvEq4 = (int)Math.Ceiling(this.eq4);
            int cnvEq5 = (int)Math.Ceiling(this.eq5);
            int cnvEq6 = (int)Math.Ceiling(this.eq6);
            int cnvEq7 = (int)Math.Ceiling(this.eq7);

            int[] allEqValues = new int[] {
                cnvEq0,
                cnvEq1,
                cnvEq2,
                cnvEq3,
                cnvEq4,
                cnvEq5,
                cnvEq6,
                cnvEq7
            };

            serviceAudio.EqValues = allEqValues;

            serviceAudio.Save();
        }

        #endregion

        #region Commands

        public ICommand ResetEqualizerCommand { get { return new RelayCommand(ResetEqualizer); } }
        public ICommand SaveAudioSettingsCommand { get { return new RelayCommand(SaveAudioSettings); } }

        #endregion Commands
    }
}

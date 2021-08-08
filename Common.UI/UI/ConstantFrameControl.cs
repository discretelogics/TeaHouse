using System;
using System.Windows.Controls;
using System.Timers;

namespace TeaTime.UI
{
    public class ConstantFrameControl : UserControl, IDisposable
    {
        #region protected properties
        protected double MaxFrameRate
        {
            get
            {
                return GetFrameRateFromFrameSpan(minFrameSpan.Milliseconds);
            }
            set
            {
                minFrameSpan = TimeSpan.FromMilliseconds(GetFrameSpanFromFrameRate(value));
            }
        }
        protected double MinFrameRate
        {
            get
            {
                return GetFrameRateFromFrameSpan(pendingFrameUpdateTimer.Interval);
            }
            set
            {
                pendingFrameUpdateTimer.Interval = GetFrameSpanFromFrameRate(value);
            }
        }
        #endregion

        #region ctor
        public ConstantFrameControl()
        {
            frameStart = DateTime.MinValue;
            pendingFrameUpdateTimer = new Timer();
            pendingFrameUpdateTimer.AutoReset = false;
            pendingFrameUpdateTimer.Elapsed += pendingFrameUpdateTimer_Elapsed;

            MaxFrameRate = 50;
            MinFrameRate = 25;
        }
        #endregion

        #region public methods
        public virtual void Dispose()
        {
            pendingFrameUpdateTimer.Stop();
        }

        /// <summary>
        /// Updates all chart-elements that will be redrawn on a scroll-/resize- etc. event
        /// This update is limited to a specified framerate, 
        /// because else we would update always our own elements (even when passed to the dispatcher) that often,
        /// that child-controls will not have a chance to update themselves.
        /// </summary>
        public void UpdateFrame()
        {
            DateTime now = DateTime.Now;
            if ((now - frameStart) > minFrameSpan)
            {
                pendingFrameUpdateTimer.Stop();
                frameStart = now;

                UpdateFrameElements();
            }
            else
            {
                pendingFrameUpdateTimer.Start();
            }
        }
        #endregion

        #region protected methods
        protected virtual void UpdateFrameElements()
        {
        }
        #endregion

        #region private methods
        private double GetFrameSpanFromFrameRate(double frameRate)
        {
            return 1000.0 / frameRate;
        }
        private double GetFrameRateFromFrameSpan(double frameSpan)
        {
            return 1000.0 / frameSpan;
        }
        #endregion

        #region fields
        private TimeSpan minFrameSpan;

        private DateTime frameStart;
        private Timer pendingFrameUpdateTimer;
        #endregion

        #region eventhandler
        private void pendingFrameUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            pendingFrameUpdateTimer.Stop();

            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,
                new Action(() => UpdateFrameElements()));
        }
        #endregion
    }
}

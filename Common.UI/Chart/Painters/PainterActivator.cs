using System;
using TeaTime.API;
using TeaTime.Chart.Settings;

namespace TeaTime.Chart.Painters
{
    /// <summary>
    /// PainterActivator holds one default-instance of the respective Painter and creates a Painter for an <see cref="ITsVisualization"/> on demand.
    /// </summary>
    /// <remarks>
    /// In future, the Activator will keep the AddIn-tokens to generate painters from AddIns.
    /// </remarks>
    internal class PainterActivator
    {
        #region properties
        public IPainter DefaultInstance { get { return defaultInstance; } }
        public Type PainterType { get { return painterType; } }
        #endregion

        #region public methods
        public IPainter CreateInstance(ITapeView tapeView, ITsVisualization tv)
        {
            Guard.ArgumentNotNull(tapeView, "tapeView");
            Guard.ArgumentNotNull(tv, "tv");

            var p = (IPainter)Activator.CreateInstance(painterType);
            p.InitializeInternal(tapeView, tv);

            return p;
        }
		public override string ToString()
		{
			return painterType.ToSafeString("type is not set");
		}
        #endregion

        #region ctor
        public PainterActivator(IPainter defaultInstance)
        {
            Guard.ArgumentNotNull(defaultInstance, "defaultInstance");

            this.defaultInstance = defaultInstance;
            this.painterType = defaultInstance.GetType();
        }
        #endregion

        #region fields
        private readonly Type painterType;
        private readonly IPainter defaultInstance;
        #endregion
    }
}

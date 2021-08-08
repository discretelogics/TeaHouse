using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using TeaTime.API;
using TeaTime.Base;

namespace TeaTime.Chart.Painters
{
    /// <summary>
    /// Holds <see cref="PainterActivator"/>s for all supported painters.
    /// </summary>
    /// <remarks>
    /// New painters can be registered at runtime (important for CustomPainters loaded an AddIn-mechanism).
    /// Existing one can again be unregistered, which will prohibit the creation of new Painters of the removed type, but won't unload the Painter.
    /// </remarks>
    public class PainterManager
    {
        #region properties
        public static PainterManager Instance
        {
            get
            {
                return Singleton.Instance;
            }
        }

        internal IEnumerable<PainterActivator> RegisteredPainters { get { return registeredPainters; } }
        #endregion

        #region ctor
        private PainterManager()
        {
            registeredPainters = new List<PainterActivator>();

            // MEF
            CompositionContainer container = GetExportContainer();

            ImportPainters(container);
        }

        internal static CompositionContainer GetExportContainer()
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            container.Compose(batch);
            return container;
        }

        #endregion

        #region public methods
        public void ImportPainters(ExportProvider exportProvider)
        {
            Guard.ArgumentNotNull(exportProvider, "exportProvider");

            foreach (var painter in exportProvider.GetExportedValues<IPainter>())
            {
                var activator = new PainterActivator(painter);
                registeredPainters.Add(activator);
            }
        }
        #endregion

        #region internal methods
        internal IEnumerable<PainterActivator> FindPaintersByItemType(Type tsItemType)
        {
            var tsItemField = tsItemType.GetAllInstanceFields();
            var painters = new List<PainterActivator>();
            foreach(var pa in registeredPainters)
            {
                var painterItemFields = pa.DefaultInstance.ItemType.GetAllInstanceFields();
                if (painterItemFields.All(pif => tsItemField.Any(tif => tif.Is(pif.Name))))
                    painters.Add(pa);
            }
            return painters.OrderBy(p => p.DefaultInstance.Order);
        }
        internal PainterActivator GetPainterByItemType(Type tsItemType)
        {
            var painter = FindPaintersByItemType(tsItemType).FirstOrDefault();
            if (painter == null)
            {
                throw new Exception(String.Format("No painter registered for the Event-Type '{0}'.", tsItemType.Name));
            }
            return painter;
        }

        internal PainterActivator FindPainterByType(StorableType painterType)
        {
            Guard.ArgumentNotNull(painterType, "painterType");
            return registeredPainters.SingleOrDefault(p => painterType.Equals(p.PainterType));
        }
        #endregion

        #region fields
        private IList<PainterActivator> registeredPainters;
        #endregion

        #region Singleton
        private class Singleton
        {
            static Singleton()
            {
            }
            internal static readonly PainterManager Instance = new PainterManager();
        }
        #endregion
    }
}

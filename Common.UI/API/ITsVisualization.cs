using TeaTime.Data;
using System.Windows.Ink;
using System.Collections.Generic;

namespace TeaTime.API
{
    public interface ITsVisualization
    {
        ITeaFile TimeSeries { get; }
        IYScaler YScaler { get; }
        DrawingAttributes DrawingAttributes { get; }
        IEnumerable<IFieldMapping> FieldMappings { get; }
    }
}

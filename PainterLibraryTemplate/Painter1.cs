using System;
using System.Windows;
using System.Windows.Media;
using TeaTime.Data;
using DiscreteLogics.Common.GUI.API;

namespace $safeprojectname$
{
	public class Painter1 : SlicePainter<EventType>
	{
        protected override string MinFieldName
        {
            get { throw new NotImplementedException(); }
        }
        protected override string MaxFieldName
        {
            get { throw new NotImplementedException(); }
        }
        protected override string CloseFieldName
        {
            get { throw new NotImplementedException(); }
        }

        protected override Geometry DrawSlice(EventType item, double left, double width)
        {
            throw new NotImplementedException();
        }
	}
}

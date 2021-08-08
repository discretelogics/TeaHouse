using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeaTime.Diagnostics
{
	class ResourceMonitor
	{
		Dictionary<string, DateTime> resources;

		public ResourceMonitor()
		{
			this.resources = new Dictionary<string, DateTime>();
		}

		public void Created(string s)
		{
			
		}
	}
}

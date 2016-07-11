using System;
using System.Linq;
using System.Collections.Generic;

namespace FiredTVLauncher
{
	public class AppOrder
	{
		public AppOrder ()
		{
		}

		public string PackageName { get;set; }
		public int Order { get;set; }

		public override string ToString ()
		{
			return string.Format ("[AppOrder: PackageName={0}, Order={1}]", PackageName, Order);
		}
	}
}


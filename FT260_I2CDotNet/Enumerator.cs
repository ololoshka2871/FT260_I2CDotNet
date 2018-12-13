using HidSharp;
using System.Collections.Generic;
using System.Linq;

namespace FT260_I2CDotNet
{
	public static class Enumerator
	{
		#region Methods

		public static IList<HidDevice> Enumerate() 
			=> Enumerate(FT260.DEFAULT_VID, FT260.DEFAULT_PID);

		public static IList<HidDevice> Enumerate(int vid, int pid) 
			=> DeviceList.Local.GetHidDevices(vid, pid).ToList();

		#endregion Methods
	}
}
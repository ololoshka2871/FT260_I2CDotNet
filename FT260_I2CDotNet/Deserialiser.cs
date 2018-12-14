using System.Linq;
using System.Runtime.InteropServices;

namespace FT260_I2CDotNet
{
	internal static class Deserialiser
	{
		#region Methods

		// https://stackoverflow.com/a/2887
		public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
		{
			T stuff;
			GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			try
			{
				stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			}
			finally
			{
				handle.Free();
			}
			return stuff;
		}

		public static T ReportToStructure<T>(byte[] bytes) where T : struct =>
			ByteArrayToStructure<T>(bytes.Skip(1).ToArray());

		#endregion Methods
	}
}
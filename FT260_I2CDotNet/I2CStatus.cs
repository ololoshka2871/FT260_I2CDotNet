using System.Runtime.InteropServices;

namespace FT260_I2CDotNet
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct I2CStatus
	{
		#region Fields

		/// <summary>
		/// Bitfield
		/// </summary>
		public byte I2CBusStatus;

		/// <summary>
		/// The speed of I2C transmission. It ranges from 60K bps to 3400K bps
		/// </summary>
		//[MarshalAs(UnmanagedType.U2, )]
		public ushort I2CSpeedraw;

		/// <summary>
		/// Reserved
		/// </summary>
		public byte RESERVED;

		#endregion Fields

		#region Properties

		/// <summary>
		/// Controller busy: all other status bits invalid
		/// </summary>
		public bool Busy => (I2CBusStatus & (1 << 0)) != 0;

		/// <summary>
		/// Error condition
		/// </summary>
		public bool Error => (I2CBusStatus & (1 << 1)) != 0;

		/// <summary>
		/// Slave address was not acknowledged during last operation
		/// </summary>
		public bool ANAK => (I2CBusStatus & (1 << 2)) != 0;

		/// <summary>
		/// Data not acknowledged during last operation
		/// </summary>
		public bool DNAK => (I2CBusStatus & (1 << 3)) != 0;

		/// <summary>
		/// Arbitration lost during last operation
		/// </summary>
		public bool Arbitration => (I2CBusStatus & (1 << 4)) != 0;

		/// <summary>
		/// Controller idle
		/// </summary>
		public bool IDLE => (I2CBusStatus & (1 << 5)) != 0;

		/// <summary>
		/// Bus busy
		/// </summary>
		public bool BusBusy => (I2CBusStatus & (1 << 6)) != 0;

		/// <summary>
		/// I2C speed value
		/// </summary>
		public ushort I2CSpeed => (ushort)((I2CSpeedraw >> 8) | (I2CSpeedraw << 8));

		#endregion Properties
	}
}
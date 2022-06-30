using System;

namespace FT260_I2CDotNet
{
	public class BusNotReadyException : FT260Exception
	{
		#region Constructors

		public BusNotReadyException(uint waitingTimeout)
			: base("Bas not ready")
		{
			WaitingTimeout = waitingTimeout;
		}

		#endregion Constructors

		#region Properties

		public bool Arbitration { get; set; }
		public bool BusBusy { get; set; }
		public bool Busy { get; set; }
		public bool Error { get; set; }
		public uint WaitingTimeout { get; private set; }

		#endregion Properties

		#region Methods

		public override string ToString()
		{
			return $"Status:\n\tBusy={Busy}\n\tError={Error}\n\tArbitration={Arbitration}\n\tBusBusy={BusBusy}";
		}

		#endregion Methods
	}

	public class DeviceConfigurationsIncorrect : FT260Exception
	{
		#region Constructors

		public DeviceConfigurationsIncorrect(string messgae) : base(messgae)
		{
		}

		#endregion Constructors
	}

	public class DeviceNotFoundException : FT260Exception
	{
		#region Fields

		public readonly int pid = FT260.DEFAULT_VID;
		public readonly int vid = FT260.DEFAULT_PID;

		#endregion Fields

		#region Constructors

		public DeviceNotFoundException() : base()
		{
		}

		public DeviceNotFoundException(int vid, int pid) : base()
		{
			this.vid = vid;
			this.pid = pid;
		}

		#endregion Constructors

		#region Methods

		public override string ToString()
		{
			return $"Can't find FT260 device with VID={vid}, PID={pid}";
		}

		#endregion Methods
	}

	public class FT260Exception : Exception
	{
		#region Constructors

		public FT260Exception() : base()
		{
		}

		public FT260Exception(string message) : base(message)
		{
		}

		#endregion Constructors
	}

	public class ReadException : FT260Exception
	{
		#region Constructors

		public ReadException(int ExpectedSize, int ActualSize)
			: base($"Read size unexpected: Requested={ExpectedSize}, Readed={ActualSize}") { }

		#endregion Constructors
	}

	public class TransactionException : FT260Exception
	{
		#region Fields

		public readonly bool ANAK;
		public readonly bool Arbitration;
		public readonly bool BusBusy;
		public readonly bool DNAK;

		#endregion Fields

		#region Constructors

		internal TransactionException(I2CStatus status) : base()
		{
			ANAK = status.ANAK;
			DNAK = status.DNAK;
			Arbitration = status.Arbitration;
			BusBusy = status.BusBusy;
		}

		#endregion Constructors
	}

	public class Timeout : FT260Exception
	{
		#region Constructors

		internal Timeout(TimeoutException ex) : base(ex.Message)
		{
		}

		#endregion Constructors
	}
}
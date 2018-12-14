using System;

namespace FT260_I2CDotNet
{
	public class BusNotReadyException : FT260Exception
	{
		#region Constructors

		public BusNotReadyException(uint waitingTimeout) : base("Bas not ready")
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

	/*
	public class CommandException : WriteException
	{
		#region Constructors

		public CommandException(ErrorCode code) : base(code)
		{
		}

		#endregion Constructors

		#region Methods

		public override string ToString()
		{
			return string.Format("Write Command exception with code {0}", Code.ToString("D"));
		}

		#endregion Methods
	}
	*/
	/*
	public class ReadException : CH341Exception
	{
		#region Constructors

		public ReadException(ErrorCode code) : base()
		{
			this.Code = code;
		}

		#endregion Constructors

		#region Properties

		public ErrorCode Code { get; }

		#endregion Properties

		#region Methods

		public override string ToString()
		{
			return string.Format("I2C Read exception with code {0}", Code.ToString("D"));
		}

		#endregion Methods
	}
	*/

	public class WriteException : FT260Exception
	{
		#region Constructors

		public WriteException(Exception innerException) : base(innerException.Message)
		{
		}

		#endregion Constructors
	}
}
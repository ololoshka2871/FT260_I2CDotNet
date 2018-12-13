using System;

namespace FT260_I2CDotNet
{
	public class CH341Exception : Exception
	{
		#region Constructors

		public CH341Exception() : base()
		{
		}

		public CH341Exception(string message) : base(message)
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
	public class DeviceConfigurationsIncorrect : CH341Exception
	{
		#region Constructors

		public DeviceConfigurationsIncorrect(string messgae) : base(messgae)
		{
		}

		#endregion Constructors
	}

	public class DeviceNotFoundException : CH341Exception
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
	public class WriteException : CH341Exception
	{
		#region Constructors

		public WriteException(Exception innerException) : base(innerException.Message)
		{
		}

		#endregion Constructors
	}
}
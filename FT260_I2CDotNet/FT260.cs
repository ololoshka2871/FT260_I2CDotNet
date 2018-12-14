using HidSharp;
using System;
using System.Threading;

namespace FT260_I2CDotNet
{
	public class FT260 : IDisposable
	{
		#region Fields

		public const int DEFAULT_PID = 0x6030;
		public const int DEFAULT_VID = 0x0403;
		public const int I2C_AddressMax = 0x7F;
		public const int I2C_AddressMin = 0;

		private readonly HidDevice FT260Device;
		private readonly RequestBuilder RequestBuilder;
		private HidStream Stream = null;

		#endregion Fields

		public uint Timeout_ms { get; set; } = 10;

		#region Methods

		public void Close()
		{
			if (Stream != null)
			{
				Stream.Close();
			}
		}

		public void Dispose() => Close();

		private I2CStatus GetI2CStatus()
		{
			var req = RequestBuilder.BuildI2CStatusRequest();
			Stream.GetFeature(req);

			return Deserialiser.ReportToStructure<I2CStatus>(req);
		}

		private void WaitBusReady()
		{
			I2CStatus status = default(I2CStatus);
			for (int ms = 0; ms < Timeout_ms; ++ms)
			{
				status = GetI2CStatus();
				if (status.Error || status.Arbitration)
				{
					I2C_Reset();
				}
				else if (status.IDLE && !status.BusBusy)
				{
					return;
				}
				Thread.Sleep(1);
			}

			throw new BusNotReadyException(Timeout_ms)
			{
				Arbitration = status.Arbitration,
				BusBusy = status.BusBusy,
				Busy = status.BusBusy,
				Error = status.Error
			};
		}

		/// <summary>
		/// Use the single byte write style to get an ack bit from writing to an address with no commands.
		/// </summary>
		/// <returns>true, is ACK found</returns>
		public bool I2C_Detect(int i2c_addr)
		{
			WaitBusReady();

			return false;
		}

		/// <summary>
		/// Reset I2C module
		/// </summary>
		public void I2C_Reset() => Stream.SetFeature(RequestBuilder.BuildI2CReset());

		/// <summary>
		/// Set the i2c speed desired
		/// </summary>
		/// <param name="speed_khz">speed: in khz, will round down to 20, 100, 400, 750
		/// 750 is closer to 1000 for bytes, but slower around acks and each byte start.
		/// </param>
		public void SetSpeedAsync(uint speed_khz = 100)
			=> Stream.SetFeature(RequestBuilder.BuildI2CSpeedRequest(speed_khz));

		public bool TryOpen()
		{
			if (Stream == null)
			{
				var options = new OpenConfiguration();
				options.SetOption(OpenOption.Exclusive, true);
				if (!FT260Device.TryOpen(options, out Stream))
				{
					return false;
				}
			}

			EnableI2C();
			return true;
		}

		private static bool IsI2CEnabled(byte[] state) => state[5] == 1;

		private void EnableI2C()
		{
			var state = RequestBuilder.BuildGetStateRequest();

			Stream.GetFeature(state);
			if (!IsI2CEnabled(state))
			{
				Stream.SetFeature(RequestBuilder.BuildEnableI2C());
			}
		}

		#endregion Methods

		#region Constructors

		public FT260(HidDevice device)
		{
			FT260Device = device;
			RequestBuilder = new RequestBuilder()
			{
				GetMaxFeatureReportLength = FT260Device.GetMaxFeatureReportLength()
			};
		}

		public FT260() : this(DEFAULT_VID, DEFAULT_PID)
		{
		}

		public FT260(int vid, int pid)
		{
			if (!DeviceList.Local.TryGetHidDevice(out FT260Device, vid, pid))
			{
				throw new DeviceNotFoundException(vid, pid);
			}
			RequestBuilder = new RequestBuilder()
			{
				GetMaxFeatureReportLength = FT260Device.GetMaxFeatureReportLength()
			};
		}

		#endregion Constructors
	}
}
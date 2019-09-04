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

		internal const int MaxTransactionPyload = 64 - 4 
			- 1; // (windows raises exception on 60)

		private readonly HidDevice FT260Device;
		private readonly RequestBuilder RequestBuilder;
		private HidStream Stream = null;

		#endregion Fields

		#region Properties

		public uint Timeout_ms { get; set; } = 10;

		#endregion Properties

		#region Methods

		public void Close()
		{
			if (Stream != null)
			{
				Stream.Close();
				Stream = null;
			}
		}

		public void Dispose() => Close();

		/// <summary>
		/// Use the single byte write style to get an ack bit from writing to an address with no commands.
		/// </summary>
		/// <returns>true, is ACK found</returns>
		public bool I2C_Detect(int i2c_addr)
		{
			WaitBusReady();

			var scanRequest = RequestBuilder.BuildScanRequest(i2c_addr);
			Stream.Write(scanRequest);
			I2CStatus status;
			do
			{
				status = GetI2CStatus();
			} while (status.Busy);
			return !(status.ANAK || status.Error || status.Arbitration);
		}

		/// <summary>
		/// Reset I2C module
		/// </summary>
		public void I2C_Reset()
		{
			Stream.SetFeature(RequestBuilder.BuildI2CReset());
			FlistRead();
		}

		public void Read(int i2c_addr, out byte[] data, int size, bool RepStart = true, bool Stop = true)
		{
			data = new byte[size];
			int transmitted = 0;

			while (transmitted != size)
			{
				var this_transaction_size = Math.Min(size - transmitted, MaxTransactionPyload);

				var condition = GenerateCondition(RepStart, Stop, size, transmitted, this_transaction_size);

				var read_req = RequestBuilder.BuildReadRequest(i2c_addr, condition, this_transaction_size);

				Stream.Write(read_req);
				var status = WaitTransactionComplead();
				if (status.Error)
				{
					throw new TransactionException(status);
				}

				var result = Stream.Read();
				var res_info = Deserialiser.ReportToStructure<I2CInputReportInfo>(result);
				if (res_info.length == this_transaction_size)
				{
					Buffer.BlockCopy(result, 2, data, transmitted, res_info.length);
				}
				else
				{
					throw new ReadException(this_transaction_size, res_info.length);
				}

				transmitted += this_transaction_size;
			}
		}

		/// <summary>
		/// Set the i2c speed desired
		/// </summary>
		/// <param name="speed_khz">speed: in khz, will round down to 20, 100, 400, 750
		/// 750 is closer to 1000 for bytes, but slower around acks and each byte start.
		/// </param>
		public void SetSpeed(uint speed_khz = 100)
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

		public void Write(int i2c_addr, byte[] v, bool Start = true, bool Stop = true)
		{
			var data_size = v.Length;
			int transmitted = 0;

			if (Start)
			{
				WaitBusReady();
			}

			while (transmitted != data_size)
			{
				var this_transaction_size = Math.Min(data_size - transmitted, MaxTransactionPyload);

				var condition = GenerateCondition(Start, Stop, data_size, transmitted, this_transaction_size);

				var wtite_req = RequestBuilder.BuildWriteRequest(i2c_addr, condition, v, transmitted,
					this_transaction_size);

				Stream.Write(wtite_req);
				var status = WaitTransactionComplead();
				if (status.Error)
				{
					throw new TransactionException(status);
				}

				transmitted += this_transaction_size;
			}
		}

		private static RequestBuilder.I2CConditions GenerateCondition(bool Start, bool Stop, int data_size,
			int transmitted, int transaction_size)
		{
			RequestBuilder.I2CConditions condition = RequestBuilder.I2CConditions.None;
			if (transmitted == 0 && Start)
			{
				condition = RequestBuilder.I2CConditions.START;
				if (data_size == transaction_size && Stop)
				{
					condition = RequestBuilder.I2CConditions.START_AND_STOP;
				}
			}
			else if (transmitted + transaction_size == data_size && Stop)
			{
				condition = RequestBuilder.I2CConditions.STOP;
			}

			return condition;
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
			SetSpeed();
		}

		private void FlistRead()
		{
			var readTimeout = Stream.ReadTimeout;
			Stream.ReadTimeout = 2;
			try
			{
				Stream.Read();
			}
			catch (TimeoutException)
			{
				Stream.ReadTimeout = readTimeout;
			}
		}

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

		private I2CStatus WaitTransactionComplead()
		{
			I2CStatus status = default(I2CStatus);
			for (int ms = 0; ms < Timeout_ms; ++ms)
			{
				status = GetI2CStatus();
				if (!status.Busy) // Ожидается именно завершение работы FTшки, а не шины
				{
					return status;
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
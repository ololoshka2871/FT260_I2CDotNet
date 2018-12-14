namespace FT260_I2CDotNet
{
	internal class RequestBuilder
	{
		#region ReportIDs

		private const byte ControlReportID = 0xA1;
		private const byte I2CStatusReportID = 0xC0;

		#endregion ReportIDs

		#region Commands

		private const byte ConfiguerUart = 0x41;
		private const byte EnableInterruptWakeUp = 0x05;
		private const byte EnableUARTDCDRI = 0x07;
		private const byte EnableUartRIWakeup = 0x0C;
		private const byte GPIO2Function = 0x06;
		private const byte GPIOAFunction = 0x08;
		private const byte GPIOGFunction = 0x09;
		private const byte I2CReset = 0x20;
		private const byte SetI2CClockSpeed = 0x22;
		private const byte SetI2CMode = 0x02;
		private const byte SetInterruptTriggerCondition = 0x0A;
		private const byte SetSuspendOutPolarity = 0xA1;
		private const byte SetSystemClock = 0x01;
		private const byte SetUARTBaudRate = 0x42;
		private const byte SetUARTBreaking = 0x46;
		private const byte SetUARTDataBit = 0x43;
		private const byte SetUARTMode = 0x03;
		private const byte SetUARTParty = 0x44;
		private const byte SetUARTStopBit = 0x45;
		private const byte SetUARTXonXoff = 0x49;
		private const byte UARTReset = 0x40;
		private const byte UartRIWakeupConfig = 0x0D;

		#endregion Commands

		#region Properties

		public int GetMaxFeatureReportLength { get; set; }

		#endregion Properties

		#region Methods

		public byte[] BuildEnableI2C(bool enable = true)
			=> new byte[] { ControlReportID, SetI2CMode, enable ? (byte)1 : (byte)0 };

		public byte[] BuildGetStateRequest() => PrepareFeatureRequest(ControlReportID);

		public byte[] BuildI2CReset() => new byte[] { ControlReportID, I2CReset };

		public byte[] BuildI2CSpeedRequest(uint speed_khz)
			=> new byte[] { ControlReportID, SetI2CClockSpeed, (byte)(speed_khz >> 8), (byte)(speed_khz & 0xff) };

		public byte[] BuildI2CStatusRequest() => PrepareFeatureRequest(I2CStatusReportID);

		private byte[] PrepareFeatureRequest(byte ReportID)
		{
			var report = new byte[GetMaxFeatureReportLength];
			report[0] = ReportID;
			return report;
		}

		#endregion Methods
	}
}
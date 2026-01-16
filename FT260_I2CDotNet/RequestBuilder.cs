using System;
using System.Collections.Generic;

namespace FT260_I2CDotNet
{
	internal class RequestBuilder
	{
		#region ReportIDs

		private const byte ControlReportID = 0xA1;
		private const byte I2CStatusReportID = 0xC0;

		private const byte I2CReadRequest = 0xC2;
		private const byte I2CWriteRequest = 0xD0;

		private const byte GPIORequest = 0xB0;

		#endregion ReportIDs

		#region i2cConditions

		public enum I2CConditions : byte
		{
			None = 0,
			START = 2,
			Repeated_START = 3,
			STOP = 4,
			START_AND_STOP = 6,
		}

		#endregion i2cConditions

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
		{
			var enable_request = PrepareFeatureRequest(ControlReportID);
			enable_request[1] = SetI2CMode;
			enable_request[2] = enable ? (byte)1 : (byte)0;
			return enable_request;
		}

		public byte[] BuildReadRequest(int i2c_addr, I2CConditions condition, int this_transaction_size)
			=> new byte[] { I2CReadRequest, (byte)i2c_addr, (byte)condition, (byte)this_transaction_size };

		public byte[] BuildWriteRequest(int i2c_addr, I2CConditions condition, byte[] v, int offset, int size)
		{
			var result = new byte[4 + size];
			result[0] = (byte)(I2CWriteRequest + (size / 4));
			result[1] = (byte)i2c_addr;
			result[2] = (byte)condition;
			result[3] = (byte)size;
			Buffer.BlockCopy(v, offset, result, 4, size);
			return result;
		}

		public void BuildWriteRequest(int i2c_addr, I2CConditions condition, byte[] v)
			=> BuildWriteRequest(i2c_addr, condition, v, 0, v.Length);

		public byte[] BuildGetStateRequest() => PrepareFeatureRequest(ControlReportID);

		public byte[] BuildI2CReset()
		{
			var req = PrepareFeatureRequest(ControlReportID);
			req[1] = I2CReset;
			return req;
		}

		public byte[] BuildScanRequest(int i2c_addr)
			=> new byte[] { I2CWriteRequest, (byte)i2c_addr, (byte)I2CConditions.START_AND_STOP, 1 };

		public byte[] BuildI2CSpeedRequest(uint speed_khz)
		{
			var speed_request = PrepareFeatureRequest(ControlReportID);
			speed_request[1] = SetI2CClockSpeed;
			speed_request[2] = (byte)(speed_khz >> 8);
			speed_request[3] = (byte)(speed_khz & 0xff);
			return speed_request;
		}

		public byte[] BuildI2CStatusRequest() => PrepareFeatureRequest(I2CStatusReportID);

		private byte[] PrepareFeatureRequest(byte ReportID)
		{
			var report = new byte[GetMaxFeatureReportLength];
			report[0] = ReportID;
			return report;
		}

		public byte[] BuildSetupGPIOFunction(GPIOState pin)
		{
            var report = PrepareFeatureRequest(ControlReportID);

			var as_gpio = pin.State != PinState.DEFAULT;

            switch (pin.Id)
			{
				case PinID.DIO0_GPIOA:
					report[1] = GPIOAFunction;
					report[2] = as_gpio ? (byte)0 : (byte)3;
                    break;
                case PinID.DIO1_GPIOB:
                case PinID.DIO2_GPIOE:
                case PinID.DIO9_GPIOF:
                case PinID.DIO13_GPIOH:
                    report[1] = SetUARTMode;
					report[2] = as_gpio ? (byte)0 : (byte)4;
                    break;
                case PinID.DIO7_GPIO2:
					report[1] = GPIO2Function;
					report[2] = as_gpio ? (byte)0 : (byte)1;
                    break;
                case PinID.DIO8_GPIO3:
					report[1] = EnableInterruptWakeUp;
					report[2] = as_gpio ? (byte)0 : (byte)4;
                    break;
                case PinID.DIO10_GPIO4:
                case PinID.DIO11_GPIO5:
                    report[1] = EnableUARTDCDRI;
                    report[2] = as_gpio ? (byte)0 : (byte)1;
                    break;
                case PinID.DIO12_GPIOG:
					report[1] = GPIOGFunction;
					report[2] = as_gpio ? (byte)0 : (byte)6;
					break;
            }

			return report;
		}

		public byte[] BuildReadGPIO() => PrepareFeatureRequest(GPIORequest);

        public byte[] BuildWriteGPIO(byte[] state, IEnumerable<GPIOWriteState> pins)
		{
			if (state[0] != GPIORequest)
			{
				throw new GPIOException($"Incorrect prev GPIO state, {state[0]} != {GPIORequest}");
            }

			unsafe {
                //fixed (byte* gpio_05_vals = &state[1], byte* gpio_05_dirs = &state[2], byte* gpio_ah_vals = &state[3], byte* gpio_ah_dirs = &state[4]) 
                fixed (byte* start = &state[1]) 
                {
					foreach (var pin in pins)
					{
						var mask = pin.Id.ToMask();
						var v = pin.Id.IsExtension() ? start + 2 : start;
						var dir = pin.Id.IsExtension() ? start + 3 : start + 1;

						if (pin.State == PinState.OUTPUT)
						{
							*dir |= mask;
						}
						else
						{
							*dir &= (byte)~mask;
						}

						if (pin.Value)
						{
							*v |= mask;
						}
						else
						{
							*v &= (byte)~mask;
						}
					}
				}
			}

            return state;
        }

        #endregion Methods
    }
}
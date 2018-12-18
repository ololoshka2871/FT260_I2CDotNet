using FT260_I2CDotNet;
using HidSharp;
using NUnit.Framework;
using System;
using System.Linq;

namespace Tests
{
	[TestFixture]
	public class TestReadWrite
	{
		#region Fields

		private readonly HidDevice FT260Dev;

		#endregion Fields

		#region Constructors

		public TestReadWrite()
		{
			FT260Dev = Enumerator.Enumerate().First();
		}

		#endregion Constructors

		#region Methods

		[Test]
		public void Reset()
		{
			var ft260 = new FT260(FT260Dev);
			Assert.True(ft260.TryOpen());

			ft260.I2C_Reset();
		}

		[Test]
		public void Scantest()
		{
			var ft260 = new FT260(FT260Dev);
			Assert.True(ft260.TryOpen());

			for (int i = FT260.I2C_AddressMin; i <= FT260.I2C_AddressMax; ++i)
			{
				var res = ft260.I2C_Detect(i);
				if (res)
				{
					Console.WriteLine($"Found device at address {i:X}");
				}
			}
		}

		[Test]
		public void Read()
		{
			var ft260 = new FT260(FT260Dev);
			Assert.True(ft260.TryOpen());

			for (int i = FT260.I2C_AddressMin; i <= FT260.I2C_AddressMax; ++i)
			{
				var res = ft260.I2C_Detect(i);
				if (res)
				{
					ft260.Write(i, new[] { (byte)0 }, Stop: false);
					ft260.Read(i, out byte[] data, 65);
				}
			}
		}

		#endregion Methods
	}
}
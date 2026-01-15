using FT260_I2CDotNet;
using HidSharp;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
	[TestFixture]
	public class TestGPIO
	{
        #region Fields

        private readonly HidDevice FT260Dev;

        #endregion Fields

        #region Constructors

        public TestGPIO()
        {
            FT260Dev = Enumerator.Enumerate().First();
        }

        #endregion Constructors

        #region Methods

        private static IEnumerable<TestCaseData> GPIOInitData()
        {
            var result = new List<(PinID, bool)>()
            {
                (PinID.DIO0_GPIOA, true),
                (PinID.DIO1_GPIOB, true),
                (PinID.DIO2_GPIOE, true),
                (PinID.DIO7_GPIO2, true),
                (PinID.DIO8_GPIO3, true),
                (PinID.DIO9_GPIOF, true),
                (PinID.DIO10_GPIO4, true),
                (PinID.DIO11_GPIO5, true),
                (PinID.DIO12_GPIOG, true),
                (PinID.DIO13_GPIOH, true),
            };

            return result.Select(x => new TestCaseData(x.Item1).Returns(x.Item2));
        }

        [Test, TestCaseSource(nameof(GPIOInitData))]
		public bool InitGPIO(PinID pin)
		{
            var ft260 = new FT260(FT260Dev);
            Assert.True(ft260.TryOpen());

            try
            {
                ft260.InitGPIO(new List<GPIOWriteState>() { new GPIOWriteState() { Id = pin, State = PinState.OUTPUT, Value = false } });
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                ft260.Close();
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void WriteCheck(bool initial_state)
        {
            var all_pins = new List<PinID>()
            {
                PinID.DIO0_GPIOA,
                PinID.DIO1_GPIOB,
                PinID.DIO2_GPIOE,
                PinID.DIO7_GPIO2,
                PinID.DIO8_GPIO3,
                PinID.DIO9_GPIOF,
                PinID.DIO10_GPIO4,
                PinID.DIO11_GPIO5,
                PinID.DIO12_GPIOG,
                PinID.DIO13_GPIOH,
            };

            var ft260 = new FT260(FT260Dev);
            Assert.True(ft260.TryOpen());

            try
            {
                foreach (var pin_id in all_pins)
                {
                    var s = new GPIOWriteState() { Id = pin_id, State = PinState.OUTPUT, Value = initial_state };

                    ft260.InitGPIO(new List<GPIOWriteState>() { s });

                    var before = ft260.ReadGPIO(new List<PinID>() { pin_id });

                    Assert.AreEqual(before.Count, 1);

                    var before_pin = before[0];
                    Assert.AreEqual(before_pin.Id, pin_id);
                    Assert.AreEqual(before_pin.State, PinState.OUTPUT);
                    Assert.AreEqual(before_pin.Value, initial_state);

                    s.Value = !initial_state;
                    ft260.WriteGPIO(new List<GPIOWriteState>() { s });

                    var after = ft260.ReadGPIO(new List<PinID>() { pin_id });

                    Assert.AreEqual(before.Count, 1);

                    var after_pin = after[0];
                    Assert.AreEqual(after_pin.Id, pin_id);
                    Assert.AreEqual(after_pin.State, PinState.OUTPUT);
                    Assert.AreEqual(after_pin.Value, !initial_state);
                }
            }
            finally
            {
                ft260.Close();
            }
        }

		#endregion Methods
	}
}
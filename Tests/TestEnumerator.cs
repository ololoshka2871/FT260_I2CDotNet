using FT260_I2CDotNet;
using NUnit.Framework;
using System.Linq;

namespace Tests
{
	[TestFixture]
	public class TestEnumerator
	{
		#region Methods

		[Test]
		public void Enumerate()
		{
			var result = Enumerator.Enumerate();
			Assert.NotNull(result);
			Assert.IsNotEmpty(result);
		}

		[Test]
		public void OpenTwice()
		{
			var dev = Enumerator.Enumerate().First();

			var d1 = new FT260(dev);
			var d2 = new FT260(dev);

			Assert.True(d1.TryOpen());
			Assert.False(d2.TryOpen());

			d1.Close();
		}

		#endregion Methods
	}
}
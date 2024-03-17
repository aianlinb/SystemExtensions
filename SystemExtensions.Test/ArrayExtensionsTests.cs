namespace SystemExtensions.Tests {
	[TestClass]
	public class ArrayExtensionsTests {
		private static readonly int[] array = { 1, 2, 3, 4, 5 };

		[TestMethod]
		[DataRow(-1)]
		[DataRow(0)]
		[DataRow(1)]
		[DataRow(3)]
		[DataRow(5)]
		[DataRow(6)]
		public void Insert_Test(int index) {
			const int item = 0;
			if (index < 0 || index > array.Length) {
				Assert.ThrowsException<ArgumentOutOfRangeException>(() => array.Insert(index, item));
				return;
			}

			// Arrange
			var expected = new List<int>(array);
			expected.Insert(index, item);

			// Act
			var result = array.Insert(index, item);

			// Assert
			CollectionAssert.AreEqual(expected, result);
		}

		[TestMethod]
		[DataRow(-1)]
		[DataRow(0)]
		[DataRow(1)]
		[DataRow(3)]
		[DataRow(5)]
		[DataRow(6)]
		public void RemoveAt_Test(int index) {
			if (index < 0 || index >= array.Length) {
				Assert.ThrowsException<ArgumentOutOfRangeException>(() => array.RemoveAt(index));
				return;
			}

			// Act
			var expected = new List<int>(array);
			expected.RemoveAt(index);

			// Act
			var result = array.RemoveAt(index);

			// Assert
			CollectionAssert.AreEqual(expected, result);
		}
	}
}
namespace SystemExtensions.Tests {
	[TestClass]
	public class UtilsTests {
		private static readonly Range[] ranges = [default, Range.All, ^0..^0, 40..^41, 5.., (..3)];

		[TestMethod]
		[DataRow(0L)]
		[DataRow(10L)]
		[DataRow(1000L)]
		[DataRow(long.MaxValue)]
		public void GetOffset_LongTest(long length) {
			foreach (var index in (ReadOnlySpan<Index>)[0, 1, ^2, int.MaxValue, ^int.MaxValue]) {
				var expected = index.IsFromEnd ? length - index.Value : index.Value;

				var actual = index.GetOffset(length);

				Assert.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		[DataRow(100)]
		[DataRow(int.MaxValue)]
		public void GetOffsetAndEnd_Test(int length) {
			foreach (var range in ranges) {
				// Arrange
				var (Offset, Length) = range.GetOffsetAndLength(length);

				// Act
				var (Offset2, End) = range.GetOffsetAndEnd(length);

				// Assert
				Assert.AreEqual(Offset, Offset2);
				Assert.AreEqual(Offset + Length, End);
			}
		}

		[TestMethod]
		[DataRow(100L)]
		[DataRow(long.MaxValue)]
		public void GetOffsetAndLength_LongTest(long length) {
			foreach (var range in ranges) {
				// Arrange
				var Offset = range.Start.IsFromEnd ? length - range.Start.Value : range.Start.Value;
				var End = range.End.IsFromEnd ? length - range.End.Value : range.End.Value;

				// Act
				var (Offset2, Length) = range.GetOffsetAndLength(length);

				// Assert
				Assert.AreEqual(Offset, Offset2);
				Assert.AreEqual(End - Offset, Length);
			}
		}

		[TestMethod]
		[DataRow(100L)]
		[DataRow(long.MaxValue)]
		public void GetOffsetAndEnd_LongTest(long length) {
			foreach (var range in ranges) {
				// Arrange
				var Offset = range.Start.IsFromEnd ? length - range.Start.Value : range.Start.Value;
				var End = range.End.IsFromEnd ? length - range.End.Value : range.End.Value;

				// Act
				var (Offset2, End2) = range.GetOffsetAndEnd(length);

				// Assert
				Assert.AreEqual(Offset, Offset2);
				Assert.AreEqual(End, End2);
			}
		}
	}
}
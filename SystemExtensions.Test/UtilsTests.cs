namespace SystemExtensions.Tests {
	[TestClass]
	public class UtilsTests {
		private readonly Range[] ranges = [default, Range.All, ^0..^0, 50..^50, 5.., (..4)];

		[TestMethod]
		[DataRow(100)]
		[DataRow(int.MaxValue)]
		[DataRow(-100)]
		[DataRow(int.MinValue)]
		public void GetOffsetAndEnd_Test(int length) {
			foreach (var range in ranges) {
				// Arrange
				var (Offset, Length) = range.GetOffsetAndLength(length);

				// Act
				var (Offset2, End) = range.GetOffsetAndEnd(length);
				var (Offset3, End3) = range.GetOffsetAndEndUnchecked(length);

				// Assert
				Assert.AreEqual(Offset, Offset2);
				Assert.AreEqual(Offset + Length, End);
				Assert.AreEqual(Offset, Offset3);
				Assert.AreEqual(End, End3);
			}
		}

		[TestMethod]
		[DataRow(100)]
		[DataRow(long.MaxValue)]
		[DataRow(-100)]
		[DataRow(long.MinValue)]
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
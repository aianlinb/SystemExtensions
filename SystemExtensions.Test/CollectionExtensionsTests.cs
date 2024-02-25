namespace SystemExtensions.Tests {
	[TestClass]
	public class CollectionExtensionsTests {
		[TestMethod]
		public void IndexOf_Test() {
			// Arrange
			var enumerable = new[] { 1, 2, 3, 4, 5 };

			// Act + Assert
			for (var i = 0; i < enumerable.Length; ++i)
				Assert.AreEqual(i, Collections.CollectionExtensions.IndexOf(enumerable, enumerable[i]));
		}
	}
}
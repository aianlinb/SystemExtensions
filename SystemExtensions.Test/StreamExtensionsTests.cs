namespace SystemExtensions.Tests;
[TestClass]
public class StreamExtensionsTests {
	[TestMethod]
	[DataRow(0)]
	[DataRow(5)]
	[DataRow(99999)]
	public void ReadStringTest(int length) {
		// Arrange
		var stream = new MemoryStream();
		var expected = Random.Shared.NextString(length);
		var bytes = Encoding.Unicode.GetBytes(expected);
		stream.Write(bytes);
		stream.Position = 0;

		// Act
		var result = stream.ReadString(expected.Length);

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	[DataRow(0)]
	[DataRow(5)]
	[DataRow(99999)]
	public void WriteStringTest(int length) {
		// Arrange
		var stream = new MemoryStream();
		var expected = Random.Shared.NextString(length);

		// Act
		stream.Write(expected);

		// Assert
		var buffer = new byte[expected.Length * 2];
		stream.Position = 0;
		stream.Read(buffer, 0, buffer.Length);
		var result = Encoding.Unicode.GetString(buffer);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void ReadWriteStructTest() {
		// Arrange
		var stream = new MemoryStream();
		var expected = TestStruct.CreateRandom();
		stream.Write(expected);
		stream.Write(expected);
		stream.Position = 0;

		// Act
		var result1 = stream.Read<TestStruct>();
		stream.Read(out TestStruct result2);

		// Assert
		Assert.AreEqual(expected, result1);
		Assert.AreEqual(expected, result2);
	}

	[TestMethod]
	[DataRow(0)]
	[DataRow(5)]
	[DataRow(99999)]
	public void ReadWriteArrayOfStructTest(int length) {
		// Arrange
		var stream = new MemoryStream();
		var expected = Enumerable.Range(0, length).Select(_ => TestStruct.CreateRandom()).ToArray();
		stream.Write(expected);
		stream.Write(expected.AsReadOnlySpan());
		stream.Position = 0;

		// Act
		var result1 = new TestStruct[expected.Length];
		stream.Read(result1);

		var result2 = new TestStruct[expected.Length];
		stream.Read(result2.AsSpan());

		// Assert
		CollectionAssert.AreEqual(expected, result1);
		CollectionAssert.AreEqual(expected, result2);
	}

	[TestMethod]
	[DataRow(0, 0)]
	[DataRow(1, 1)]
	[DataRow(0, 50)]
	[DataRow(25, 100)]
	public void ReadWriteListOfStructTest(int offset, int count) {
		// Arrange
		var stream = new MemoryStream();
		var expected = Enumerable.Range(0, Random.Shared.Next(offset + count, Math.Max(256, offset + count))).Select(_ => TestStruct.CreateRandom()).ToList();
		stream.Write(expected, offset, count);
		expected = expected.GetRange(offset, count); // Trim for comparing

		// Act
		stream.Position = 0;
		var result1 = new List<TestStruct>(count);
		stream.Read(result1, 0, count);

		stream.Position = 0;
		var result2 = new List<TestStruct>(offset + count);
		stream.Read(result2, offset, count);
		result2.RemoveRange(0, offset); // TrimStart for comparing

		// Assert
		CollectionAssert.AreEqual(expected, result1);
		CollectionAssert.AreEqual(expected, result2);
	}

	[TestMethod]
	public void ReadToEndTest() {
		// Arrange
		var stream = new MemoryStream();
		Assert.AreEqual(0, stream.ReadToEnd().Length);
		Assert.AreEqual(0, stream.ReadToEndAsync().GetAwaiter().GetResult().Length);

		var expected = new byte[Random.Shared.Next(1, 2000000)];
		Random.Shared.NextBytes(expected);
		stream.Write(expected);

		var pos = Random.Shared.Next(0, 2);
		stream.Position = pos;

		// Act
		var result1 = stream.ReadToEnd();
		var result2 = stream.ReadToEndAsync().GetAwaiter().GetResult();

		// Assert
		new ReadOnlySpan<byte>(expected).Slice(pos).SequenceEqual(result1);
		new ReadOnlySpan<byte>(expected).Slice(pos).SequenceEqual(result2);
	}
}
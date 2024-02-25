using System.Runtime.InteropServices;

namespace SystemExtensions.Tests {
	[TestClass]
	public class SpanExtensionsTests {
		private static readonly int[] array = [5, 2, 3, 4];
		[TestMethod]
		public void AsReadOnlySpan_Test() => Assert.IsTrue(new ReadOnlySpan<int>(array) == array.AsReadOnlySpan());
		[TestMethod]
		public void AsReadOnlyMemory_Test() => Assert.AreEqual(new ReadOnlyMemory<int>(array), array.AsReadOnlyMemory());
		[TestMethod]
		public void AsReadOnly_SpanTest() => Assert.IsTrue(new ReadOnlySpan<int>(array) == new Span<int>(array).AsReadOnly());
		[TestMethod]
		public void AsReadOnly_MemoryTest() => Assert.AreEqual(new ReadOnlyMemory<int>(array), new Memory<int>(array).AsReadOnly());
		[TestMethod]
		public void AsWritable_SpanTest() => Assert.IsTrue(new Span<int>(array) == new ReadOnlySpan<int>(array).AsWritable());
		[TestMethod]
		public void AsWritable_MemoryTest() => Assert.AreEqual(new Memory<int>(array), new ReadOnlyMemory<int>(array).AsWritable());

		[TestMethod]
		[DataRow(0, 4)]
		[DataRow(0, 0)]
		[DataRow(4, 0)]
		[DataRow(1, 2)]
		public void SliceUnchecked_Test(int start, int length) {
			// Arrange
			var span1 = new ReadOnlySpan<int>(array);
			var span2 = new Span<int>(array);

			// Act
			var actual1 = span1.SliceUnchecked(start);
			var actual2 = span1.SliceUnchecked(start, length);
			var actual3 = span2.SliceUnchecked(start);
			var actual4 = span2.SliceUnchecked(start, length);

			// Assert
			Assert.IsTrue(span1[start..] == actual1);
			Assert.IsTrue(span1.Slice(start, length) == actual2);
			Assert.IsTrue(span2[start..] == actual3);
			Assert.IsTrue(span2.Slice(start, length) == actual4);
		}
		[TestMethod]
		public void CopyToUnchecked_Test() {
			// Arrange
			var excepted1 = new ReadOnlySpan<int>(array);
			var excepted2 = new Span<int>(array);
			Span<int> actual1 = stackalloc int[array.Length];
			Span<int> actual2 = stackalloc int[array.Length];

			// Act
			excepted1.CopyToUnchecked(ref MemoryMarshal.GetReference(actual1));
			excepted2.CopyToUnchecked(ref MemoryMarshal.GetReference(actual2));

			// Assert
			Assert.IsTrue(excepted1.SequenceEqual(actual1));
			Assert.IsTrue(excepted2.SequenceEqual(actual2));
		}

		[TestMethod]
		public void Contains_Test() {
			// Arrange
			var span = new ReadOnlySpan<int>(array);
			var excepted1 = array[Random.Shared.Next(array.Length)];
			var excepted2 = 9999;
			var excepted3 = new ReadOnlySpan<int>(array[2..]);
			var excepted4 = new ReadOnlySpan<int>([99, 999]);

			// Act
			var actual1 = SpanExtensions.Contains(span, excepted1);
			var actual2 = SpanExtensions.Contains(span, excepted2);
			var actual3 = SpanExtensions.Contains(span, excepted3);
			var actual4 = SpanExtensions.Contains(span, excepted4);
			var actual5 = SpanExtensions.ContainsAny(span, excepted3);
			var actual6 = SpanExtensions.ContainsAny(span, excepted4);

			// Assert
			Assert.AreEqual(MemoryExtensions.Contains(span, excepted1), actual1);
			Assert.AreEqual(MemoryExtensions.Contains(span, excepted2), actual2);
			Assert.AreEqual(MemoryExtensions.IndexOf(span, excepted3) >= 0, actual3);
			Assert.AreEqual(MemoryExtensions.IndexOf(span, excepted4) >= 0, actual4);
			Assert.AreEqual(MemoryExtensions.ContainsAny(span, excepted3), actual5);
			Assert.AreEqual(MemoryExtensions.ContainsAny(span, excepted4), actual6);
		}

		[TestMethod]
		public void IndexOf_Test() {
			// Arrange
			var span = new ReadOnlySpan<int>(array);
			var excepted1 = array[Random.Shared.Next(array.Length)];
			var excepted2 = array[2..];
			var excepted3 = new ReadOnlySpan<int>([99, 999]);

			// Act
			var actual1 = SpanExtensions.IndexOf(span, excepted1);
			var actual2 = SpanExtensions.IndexOf(span, excepted2);
			var actual3 = SpanExtensions.IndexOf(span, excepted3);
			var actual4 = SpanExtensions.IndexOfAny(span, excepted2);
			var actual5 = SpanExtensions.IndexOfAny(span, excepted3);
			var actual6 = SpanExtensions.LastIndexOf(span, excepted1);
			var actual7 = SpanExtensions.LastIndexOf(span, excepted2);
			var actual8 = SpanExtensions.LastIndexOf(span, excepted3);

			// Assert
			Assert.AreEqual(Array.IndexOf(array, excepted1), actual1);
			Assert.AreEqual(MemoryExtensions.IndexOf(span, excepted2), actual2);
			Assert.AreEqual(MemoryExtensions.IndexOf(span, excepted3), actual3);
			Assert.AreEqual(MemoryExtensions.IndexOfAny(span, excepted2), actual4);
			Assert.AreEqual(MemoryExtensions.IndexOfAny(span, excepted3), actual5);
			Assert.AreEqual(Array.LastIndexOf(array, excepted1), actual6);
			Assert.AreEqual(MemoryExtensions.LastIndexOf(span, excepted2), actual7);
			Assert.AreEqual(MemoryExtensions.LastIndexOf(span, excepted3), actual8);
		}
		[TestMethod]
		public void StartsWith_EndsWith_Test() {
			// Arrange
			var span = new ReadOnlySpan<int>(array);
			var excepted1 = array[..2];
			var excepted2 = array[2..];
			var excepted3 = new ReadOnlySpan<int>([99, 999]);

			// Act
			var actual1 = SpanExtensions.StartsWith(span, excepted1);
			var actual2 = SpanExtensions.StartsWith(span, excepted2);
			var actual3 = SpanExtensions.StartsWith(span, excepted3);
			var actual4 = SpanExtensions.EndsWith(span, excepted1);
			var actual5 = SpanExtensions.EndsWith(span, excepted2);
			var actual6 = SpanExtensions.EndsWith(span, excepted3);

			// Assert
			Assert.AreEqual(MemoryExtensions.StartsWith(span, excepted1), actual1);
			Assert.AreEqual(MemoryExtensions.StartsWith(span, excepted2), actual2);
			Assert.AreEqual(MemoryExtensions.StartsWith(span, excepted3), actual3);
			Assert.AreEqual(MemoryExtensions.EndsWith(span, excepted1), actual4);
			Assert.AreEqual(MemoryExtensions.EndsWith(span, excepted2), actual5);
			Assert.AreEqual(MemoryExtensions.EndsWith(span, excepted3), actual6);
		}
		[TestMethod]
		public void Replace_Test() {
			// Arrange
			var span1 = new Span<int>([1, 2, 3, 4, 5, 6, 7, 8]);
			var span2 = new Span<int>([1, 2, 3, 4, 5, 6, 7, 8]);
			var span3 = new Span<int>([1, 2, 3, 4, 5, 6, 7, 8]);
			var span4 = new Span<int>([1, 2, 3, 4, 5, 6, 7, 8]);
			var from1 = span1[Random.Shared.Next(span1.Length)];
			var from2 = 99;
			var to = 9999;
			Span<int> excepted1 = stackalloc int[span1.Length];
			Span<int> excepted2 = stackalloc int[span2.Length];

			// Act + Assert
			MemoryExtensions.Replace(span1, excepted1, from1, to);
			MemoryExtensions.Replace(span2, excepted2, from2, to);
			SpanExtensions.Replace(span1, from1, to);
			SpanExtensions.Replace(span2, from2, to);
			Assert.IsTrue(excepted1.SequenceEqual(span1));
			Assert.IsTrue(excepted2.SequenceEqual(span2));

			SpanExtensions.Replace(span3, excepted1, from1, to);
			SpanExtensions.Replace(span4, excepted2, from2, to);
			MemoryExtensions.Replace(span3, from1, to);
			MemoryExtensions.Replace(span4, from2, to);
			Assert.IsTrue(span3.SequenceEqual(excepted1));
			Assert.IsTrue(span4.SequenceEqual(excepted2));
		}
		[TestMethod]
		public void Count_Test() {
			// Arrange
			var span = new ReadOnlySpan<int>(array);
			var excepted1 = array[Random.Shared.Next(array.Length)];
			var excepted2 = 9999;
			var excepted3 = new ReadOnlySpan<int>(array[2..]);
			var excepted4 = new ReadOnlySpan<int>([99, 999]);

			// Act
			var actual1 = SpanExtensions.Count(span, excepted1);
			var actual2 = SpanExtensions.Count(span, excepted2);
			var actual3 = SpanExtensions.Count(span, excepted3);
			var actual4 = SpanExtensions.Count(span, excepted4);

			// Assert
			Assert.AreEqual(MemoryExtensions.Count(span, excepted1), actual1);
			Assert.AreEqual(MemoryExtensions.Count(span, excepted2), actual2);
			Assert.AreEqual(MemoryExtensions.Count(span, excepted3), actual3);
			Assert.AreEqual(MemoryExtensions.Count(span, excepted4), actual4);
		}

		[TestMethod]
		[DataRow(" A,b ,c", (char[])[','])]
		[DataRow(" A,b ,c", (char[])[',', ' ', '.', '`'])]
		[DataRow(" A,b ,c", (char[])[])]
		[DataRow("", (char[])[',', ' '])]
		[DataRow("", (char[])[])]
		[DataRow(" A,b ,c", (char[])['.'])]
		[DataRow("A. ,v.e,r.y, .l,o.n,g. ,s.t,r.i,n.g,", (char[])[',', '.', ' '])]
		public void Split_Test(string source, char[] separators) {
			foreach (var option in (ReadOnlySpan<StringSplitOptions>)[
					StringSplitOptions.None,
					StringSplitOptions.RemoveEmptyEntries,
					StringSplitOptions.TrimEntries,
					StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries]) {
				// Act
				var expected1 = source.Split(separators, option);
				var expected2 = source.Split(new string(separators), option);
				var actual1 = source.AsSpan().SplitAny(separators, option);
				var actual2 = source.AsSpan().Split(separators, option);

				// Assert
				for (var i = 0; i < expected1.Length; i++) {
					Assert.IsTrue(actual1.MoveNext());
					Assert.AreEqual(expected1[i], actual1.Current.ToString());
				}
				for (var i = 0; i < expected2.Length; i++) {
					Assert.IsTrue(actual2.MoveNext());
					Assert.AreEqual(expected2[i], actual2.Current.ToString());
				}
			}
		}

		[TestMethod]
		[DataRow(0)]
		[DataRow(5)]
		[DataRow(99999)]
		public unsafe void PointerMemoryManager_Test(int length) {
			// Arrange
			int* p = stackalloc int[length];

			// Act
			using var result = new PointerMemoryManager<int>(p, length);

			// Assert
			Assert.IsTrue(result.Memory.Span == new Span<int>(p, length));
			var i = Random.Shared.Next(length);
			using var pin = result.Pin(i);
			Assert.AreEqual((nint)pin.Pointer, (nint)(p + i));
		}
	}
}
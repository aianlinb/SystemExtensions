using System.Runtime.InteropServices;

namespace SystemExtensions.Tests;

[TestClass]
public class CollectionExtensionsTests {
	private static readonly int[] enumerable = [1, 2, 3, 4, 5];
	[TestMethod]
	public void IndexOf_Test() {
		// Act + Assert
		for (var i = 0; i < enumerable.Length; ++i)
			Assert.AreEqual(i, Collections.CollectionExtensions.IndexOf(enumerable, enumerable[i]));
	}

	[TestMethod]
	public void PickFirstEnumerator_Test() {
		// Arrange
		using var pfe = new PickFirstEnumerator<int>(enumerable);

		// Act + Assert
		Assert.AreEqual(enumerable[0], pfe.First);
		using var et = enumerable.AsEnumerable().GetEnumerator();
		while (pfe.MoveNext()) {
			Assert.IsTrue(et.MoveNext());
			Assert.AreEqual(et.Current, pfe.Current);
		}
		pfe.Reset();
		Assert.AreEqual(enumerable[0], pfe.First);
		Assert.IsTrue(enumerable.SequenceEqual(pfe));
	}

	[TestMethod]
	public unsafe void ListAsSpanAsMemory_Test() {
		// Arrange
		var list = new List<int>(enumerable);

		// Act
		var span = list.AsSpan();
		var memory = list.AsMemory();

		// Assert
		Assert.AreEqual(list.Count, span.Length);
		Assert.AreEqual(list.Count, memory.Length);
		fixed (int* expected = CollectionsMarshal.AsSpan(list), actual1 = span) {
			var actual2 = (int*)memory.Pin().Pointer;
			Assert.IsTrue(expected == actual1);
			Assert.IsTrue(expected == actual2);
		}
	}
}
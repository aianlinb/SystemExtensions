using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SystemExtensions.Spans;

namespace SystemExtensions.Benchmark;

public static class Program {
	public static void Main() {
		BenchmarkRunner.Run<SpanHelpersBenchmark>();
		Console.ReadLine();
	}
}

public class EquatableClass(int value) : IEquatable<EquatableClass?> {
	private readonly int value = value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(EquatableClass? other) => value == other?.value;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object? obj) => Equals(obj as EquatableClass);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode() => value;
}

public class SpanHelpersBenchmark {
	private const int Length = 256;
	public static Guid[] EqualtableStructs { get; set; } = new Guid[Length];
	public static EquatableClass[] EqualtableObjects { get; set; } = new EquatableClass[Length];
	static SpanHelpersBenchmark() {
		for (var i = 0; i < Length; ++i) {
			EqualtableStructs[i] = Guid.NewGuid();
			EqualtableObjects[i] = new EquatableClass(i);
		}
	}
#pragma warning disable CA1822
	[Benchmark]
	public int IndexOf_Struct_Original() {
		return MemoryExtensions.IndexOf(EqualtableStructs, EqualtableStructs[Length / 2]);
	}
	[Benchmark]
	public int IndexOf_Struct_WithoutIEqualable() {
		return SpanExtensions.IndexOf(EqualtableStructs, EqualtableStructs[Length / 2]);
	}
	[Benchmark]
	public int IndexOf_Object_Original() {
		return MemoryExtensions.IndexOf(EqualtableObjects, EqualtableObjects[Length / 2]);
	}
	[Benchmark]
	public int IndexOf_Object_WithoutIEqualable() {
		return SpanExtensions.IndexOf(EqualtableObjects, EqualtableObjects[Length / 2]);
	}

	[Benchmark]
	public int LastIndexOf_Struct_Original() {
		return MemoryExtensions.LastIndexOf(EqualtableStructs, EqualtableStructs[Length / 2]);
	}
	[Benchmark]
	public int LastIndexOf_Struct_WithoutIEqualable() {
		return SpanExtensions.LastIndexOf(EqualtableStructs, EqualtableStructs[Length / 2]);
	}
	[Benchmark]
	public int LastIndexOf_Object_Original() {
		return MemoryExtensions.LastIndexOf(EqualtableObjects, EqualtableObjects[Length / 2]);
	}
	[Benchmark]
	public int LastIndexOf_Object_WithoutIEqualable() {
		return SpanExtensions.LastIndexOf(EqualtableObjects, EqualtableObjects[Length / 2]);
	}

	[Benchmark]
	public int IndexOfAny_Struct_Original() {
		return MemoryExtensions.IndexOfAny(EqualtableStructs, new ReadOnlySpan<Guid>(EqualtableStructs, Length / 2, 6));
	}
	[Benchmark]
	public int IndexOfAny_Struct_WithoutIEqualable() {
		return SpanExtensions.IndexOfAny(EqualtableStructs, new ReadOnlySpan<Guid>(EqualtableStructs, Length / 2, 6));
	}
	[Benchmark]
	public int IndexOfAny_Object_Original() {
		return MemoryExtensions.IndexOfAny(EqualtableObjects, new ReadOnlySpan<EquatableClass>(EqualtableObjects, Length / 2, 6));
	}
	[Benchmark]
	public int IndexOfAny_Object_WithoutIEqualable() {
		return SpanExtensions.IndexOfAny(EqualtableObjects, new ReadOnlySpan<EquatableClass>(EqualtableObjects, Length / 2, 6));
	}

	[Benchmark] // StartsWith has almost the same implementation as EndsWith
	public bool EndsWith_Struct_Original() {
		return MemoryExtensions.EndsWith(EqualtableStructs, new ReadOnlySpan<Guid>(EqualtableStructs, Length / 2, Length / 2));
	}
	[Benchmark]
	public bool EndsWith_Struct_WithoutIEqualable() {
		return SpanExtensions.EndsWith(EqualtableStructs, new ReadOnlySpan<Guid>(EqualtableStructs, Length / 2, Length / 2));
	}
	[Benchmark]
	public bool EndsWith_Object_Original() {
		return MemoryExtensions.EndsWith(EqualtableObjects, new ReadOnlySpan<EquatableClass>(EqualtableObjects, Length / 2, Length / 2));
	}
	[Benchmark]
	public bool EndsWith_Object_WithoutIEqualable() {
		return SpanExtensions.EndsWith(EqualtableObjects, new ReadOnlySpan<EquatableClass>(EqualtableObjects, Length / 2, Length / 2));
	}

	[Benchmark]
	public void Replace_Struct_Original() {
		MemoryExtensions.Replace(EqualtableStructs, stackalloc Guid[Length], EqualtableStructs[Length / 2], default);
	}
	[Benchmark]
	public void Replace_Struct_WithoutIEqualable() {
		SpanExtensions.Replace(EqualtableStructs, stackalloc Guid[Length], EqualtableStructs[Length / 2], default);
	}
	[Benchmark]
	public void Replace_Object_Original() {
		MemoryExtensions.Replace(EqualtableObjects, new EquatableClass[Length], EqualtableObjects[Length / 2], default);
	}
	[Benchmark]
	public void Replace_Object_WithoutIEqualable() {
		SpanExtensions.Replace(EqualtableObjects, new EquatableClass[Length], EqualtableObjects[Length / 2], default);
	}

	[Benchmark]
	public int Count_Struct_Original() {
		return MemoryExtensions.Count(EqualtableStructs, new ReadOnlySpan<Guid>(EqualtableStructs, Length / 2, 6));
	}
	[Benchmark]
	public int Count_Struct_WithoutIEqualable() {
		return SpanExtensions.Count(EqualtableStructs, new ReadOnlySpan<Guid>(EqualtableStructs, Length / 2, 6));
	}
	[Benchmark]
	public int Count_Object_Original() {
		return MemoryExtensions.Count(EqualtableObjects, new ReadOnlySpan<EquatableClass>(EqualtableObjects, Length / 2, 6));
	}
	[Benchmark]
	public int Count_Object_WithoutIEqualable() {
		return SpanExtensions.Count(EqualtableObjects, new ReadOnlySpan<EquatableClass>(EqualtableObjects, Length / 2, 6));
	}
}
/*
BenchmarkDotNet v0.13.11, Windows 11 (10.0.22621.2861/22H2/2022Update/SunValley2)
Intel Core i5-10300H CPU 2.50GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.100
  [Host]     : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2


| Method                               | Mean         | Error      | StdDev     |
|------------------------------------- |-------------:|-----------:|-----------:|
| IndexOf_Struct_Original              |    59.328 ns |  0.6468 ns |  0.5401 ns |
| IndexOf_Struct_WithoutIEqualable     |    58.586 ns |  0.4149 ns |  0.3881 ns |
| IndexOf_Object_Original              |   276.991 ns |  2.6876 ns |  2.3824 ns |
| IndexOf_Object_WithoutIEqualable     |   274.819 ns |  1.9654 ns |  1.7423 ns |
| LastIndexOf_Struct_Original          |    63.798 ns |  0.8095 ns |  0.7572 ns |
| LastIndexOf_Struct_WithoutIEqualable |    63.706 ns |  0.5947 ns |  0.5563 ns |
| LastIndexOf_Object_Original          |   309.757 ns |  3.3865 ns |  3.1677 ns |
| LastIndexOf_Object_WithoutIEqualable |   304.721 ns |  2.3520 ns |  2.2001 ns |
| IndexOfAny_Struct_Original           |   488.691 ns |  3.2857 ns |  2.7437 ns |
| IndexOfAny_Struct_WithoutIEqualable  |   492.000 ns |  3.2906 ns |  3.0781 ns |
| IndexOfAny_Object_Original           | 2,147.426 ns | 18.6345 ns | 16.5190 ns |
| IndexOfAny_Object_WithoutIEqualable  | 1,776.086 ns |  8.5701 ns |  7.5972 ns |
| EndsWith_Struct_Original             |     1.513 ns |  0.0188 ns |  0.0167 ns |
| EndsWith_Struct_WithoutIEqualable    |     1.576 ns |  0.0407 ns |  0.0381 ns |
| EndsWith_Object_Original             |     2.611 ns |  0.0325 ns |  0.0304 ns |
| EndsWith_Object_WithoutIEqualable    |     1.935 ns |  0.0354 ns |  0.0313 ns |
| Replace_Struct_Original              |   281.802 ns |  5.2589 ns |  4.6618 ns |
| Replace_Struct_WithoutIEqualable     |   275.874 ns |  3.3917 ns |  3.0066 ns |
| Replace_Object_Original              | 1,030.462 ns | 20.2600 ns | 16.9180 ns |
| Replace_Object_WithoutIEqualable     | 1,066.005 ns | 18.5185 ns | 17.3222 ns |
| Count_Struct_Original                |   111.412 ns |  0.5216 ns |  0.4072 ns |
| Count_Struct_WithoutIEqualable       |   118.573 ns |  0.9390 ns |  0.8324 ns |
| Count_Object_Original                |   549.830 ns |  6.5711 ns |  5.1303 ns |
| Count_Object_WithoutIEqualable       |   597.282 ns |  4.3582 ns |  3.8635 ns |

// * Hints *
Outliers
  SpanHelpersBenchmark.IndexOf_Struct_Original: Default             -> 2 outliers were removed (65.02 ns, 65.60 ns)
  SpanHelpersBenchmark.IndexOf_Object_Original: Default             -> 1 outlier  was  removed (292.57 ns)
  SpanHelpersBenchmark.IndexOf_Object_WithoutIEqualable: Default    -> 1 outlier  was  removed (288.14 ns)
  SpanHelpersBenchmark.IndexOfAny_Struct_Original: Default          -> 2 outliers were removed (503.08 ns, 503.89 ns)
  SpanHelpersBenchmark.IndexOfAny_Object_Original: Default          -> 1 outlier  was  removed (2.24 us)
  SpanHelpersBenchmark.IndexOfAny_Object_WithoutIEqualable: Default -> 1 outlier  was  removed (1.81 us)
  SpanHelpersBenchmark.EndsWith_Struct_Original: Default            -> 1 outlier  was  removed (3.15 ns)
  SpanHelpersBenchmark.EndsWith_Object_WithoutIEqualable: Default   -> 1 outlier  was  removed (3.81 ns)
  SpanHelpersBenchmark.Replace_Struct_Original: Default             -> 1 outlier  was  removed (306.86 ns)
  SpanHelpersBenchmark.Replace_Struct_WithoutIEqualable: Default    -> 1 outlier  was  removed (285.77 ns)
  SpanHelpersBenchmark.Replace_Object_Original: Default             -> 2 outliers were removed (1.10 us, 1.10 us)
  SpanHelpersBenchmark.Count_Struct_Original: Default               -> 3 outliers were removed (115.49 ns..116.36 ns)
  SpanHelpersBenchmark.Count_Struct_WithoutIEqualable: Default      -> 1 outlier  was  removed (123.34 ns)
  SpanHelpersBenchmark.Count_Object_Original: Default               -> 3 outliers were removed (581.58 ns..706.66 ns)
  SpanHelpersBenchmark.Count_Object_WithoutIEqualable: Default      -> 1 outlier  was  removed (613.60 ns)
*/
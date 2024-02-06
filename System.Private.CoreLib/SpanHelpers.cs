﻿using System.Numerics;

namespace System {
	/// <summary>
	/// Forwarded to System.Private.CoreLib.dll at runtime
	/// </summary>
	public static class SpanHelpers {
		public static int IndexOf(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength) => throw null!;
		public static int IndexOf(ref char searchSpace, int searchSpaceLength, ref char value, int valueLength) => throw null!;
		public static int LastIndexOf(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength) => throw null!;
		public static int LastIndexOf(ref char searchSpace, int searchSpaceLength, ref char value, int valueLength) => throw null!;
		public static int IndexOfValueType<T>(ref T searchSpace, T value, int length) where T : struct, INumber<T> => throw null!;
		public static int IndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, int length) where T : struct, INumber<T> => throw null!;
		public static int IndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : struct, INumber<T> => throw null!;
		public static int IndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, int length) where T : struct, INumber<T> => throw null!;
		public static int IndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, T value4, int length) where T : struct, INumber<T> => throw null!;
		public static int LastIndexOfValueType<T>(ref T searchSpace, T value, int length) where T : struct, INumber<T> => throw null!;
		public static int LastIndexOfAnyExceptValueType<T>(ref T searchSpace, T value, int length) where T : struct, INumber<T> => throw null!;
		public static void ReplaceValueType<T>(ref T src, ref T dst, T oldValue, T newValue, nuint length) where T : struct => throw null!;
		public static int CountValueType<T>(ref T current, T value, int length) where T : struct, IEquatable<T>? => throw null!;
		public static bool SequenceEqual(ref byte first, ref byte second, nuint length) => throw null!;
	}
}
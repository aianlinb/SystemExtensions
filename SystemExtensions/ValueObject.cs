extern alias corelib;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SystemExtensions.Spans;
using MethodTable = corelib::System.Runtime.CompilerServices.MethodTable;
using RawData = corelib::System.Runtime.CompilerServices.RawData;
using RawArrayData = corelib::System.Runtime.CompilerServices.RawArrayData;

namespace SystemExtensions {
	/// <summary>
	/// Helper methods to treat a space on stack as an <see cref="object"/> to avoid heap allocation.
	/// <br />*Haven't tested any yet. Be careful using this class*
	/// </summary>
	/// <remarks>
	/// <para>The buffer should be allocated via <see langword="stackalloc"/> with length of at least <see cref="Size(Type)"/> / <see cref="SizeOfString"/> / <see cref="SizeOfArray{T}"/>.</para>
	/// <para>The object simulated by this class have a lifetime same as the buffer passed to the funtions, don't store it anywhere else like a field of other object, a collection or captured by a lambda expression.</para>
	/// <para>The finalizer <see cref="object.Finalize"/> of the object won't be called cause that it's not managed by GC.</para>
	/// <para>This class is experimental and may not work as expected.</para>
	/// </remarks>
	public static class ValueObject {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static unsafe MethodTable* GetMethodTable(Span<byte> buffer) =>
			(MethodTable*)Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(buffer), nint.Size));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static unsafe MethodTable* GetMethodTable(Type type) => (MethodTable*)type.TypeHandle.Value;
		/// <summary>
		/// Returns an simulated object of type <typeparamref name="T"/> that points to the <paramref name="buffer"/><br />
		/// The first two <see cref="nint"/>s of the <paramref name="buffer"/> must be reserved for ObjectHeader and MethodTable, which will be set by this method.<br />
		/// </summary>
		/// <remarks>
		/// <para>Note that this won't check the contents in <paramref name="buffer"/>, carefullly use it or it may cause undefined behavior.</para>
		/// <para>See remarks of <see cref="ValueObject"/></para>
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe T AsObject<T>(Span<byte> buffer) where T : class {
			if (buffer.Length < Size<T>())
				ThrowHelper.Throw<ArgumentException>("The length of the buffer is less than ValueObject.Size(Type)");
			
			Unsafe.As<byte, nint>(ref MemoryMarshal.GetReference(buffer)) = 0; // ObjectHeader
			var pMT = (nint)GetMethodTable(buffer);
			*(MethodTable**)pMT = GetMethodTable(typeof(T));
			return Unsafe.As<nint, T>(ref pMT);
		}

		/// <summary>
		/// Instantiates an simulated object of type <typeparamref name="T"/> to the <paramref name="buffer"/>
		/// </summary>
		/// <param name="constructor">
		/// Constructor of <typeparamref name="T"/> to call,
		/// or <see langword="null"/> to return an uninitialized object which like calling <see cref="RuntimeHelpers.GetUninitializedObject"/>
		/// </param>
		/// <remarks>
		/// See remarks of <see cref="ValueObject"/>
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe T CreateObject<T>(Span<byte> buffer, ConstructorInfo? constructor, object?[]? parameters) where T : class {
			if (buffer.Length < Size<T>())
				ThrowHelper.Throw<ArgumentException>("The length of the buffer is less than ValueObject.Size(Type)");
			buffer.Clear();

			var pMT = (nint)GetMethodTable(buffer);
			*(MethodTable**)pMT = GetMethodTable(typeof(T));
			constructor?.Invoke(Unsafe.As<nint, T>(ref pMT), parameters);
			return Unsafe.As<nint, T>(ref pMT);
		}

		/// <summary>
		/// Memberwise copies an existing <paramref name="instance"/> of type <typeparamref name="T"/> to the <paramref name="buffer"/>
		/// and returns an simulated object that points to it
		/// </summary>
		/// <remarks>
		/// See remarks of <see cref="ValueObject"/>
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe T FromObject<T>(Span<byte> buffer, T instance) where T : class {
			if (instance.GetType() != typeof(T))
				ThrowHelper.Throw<ArgumentException>("The type of the instance is not the same as the generic type parameter");
			var size = Size(instance);
			if (buffer.Length < size)
				ThrowHelper.Throw<ArgumentException>("The length of the buffer is less than ValueObject.Size(object)");
			buffer.Clear();

			var pMT = (nint)GetMethodTable(buffer);
			MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<RawData>(instance).Data, size)
				.CopyToUnchecked(ref Unsafe.AsRef<byte>((void*)pMT));
			return Unsafe.As<nint, T>(ref pMT);
		}

		/// <summary>
		/// Box a value type <typeparamref name="T"/> to the <paramref name="buffer"/>
		/// and returns an simulated object that points to it
		/// </summary>
		/// <remarks>
		/// See remarks of <see cref="ValueObject"/>
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe object Box<T>(Span<byte> buffer, in T value) where T : struct {
#pragma warning disable CS8500
			var size = sizeof(nint) * 2 + sizeof(T);
			if (buffer.Length < size)
				ThrowHelper.Throw<ArgumentException>("The length of the buffer is less than ValueObject.Size(Type)");
			buffer.Clear();

			var pMT = (nint)GetMethodTable(buffer);
			MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in value)), size)
				.CopyToUnchecked(ref Unsafe.AsRef<byte>((void*)pMT));
			return Unsafe.As<nint, T>(ref pMT);
		}

		/// <summary>
		/// Get the size of a instance of <paramref name="type"/> on heap in bytes
		/// </summary>
		/// <remarks>
		/// For <see cref="string"/>, use <see cref="SizeOfString"/> instead.<br />
		/// For <see cref="Array"/>, use <see cref="SizeOfArray"/> instead.<br />
		/// Or use <see cref="Size(object)"/> to handle all types with an existing instance.
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[RequiresDynamicCode("Use the Size<T> overload instead.")]
		public static unsafe int Size(Type type) => (int)GetMethodTable(type)->BaseSize; // MethodTable.BaseSize
		/// <summary>
		/// Get the size of a instance of <typeparamref name="T"/> on heap in bytes
		/// </summary>
		/// <remarks>
		/// For <see cref="string"/>, use <see cref="SizeOfString"/> instead.<br />
		/// For <see cref="Array"/>, use <see cref="SizeOfArray"/> instead.<br />
		/// Or use <see cref="Size(object)"/> to handle all types with an existing instance.
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.")]
		public static unsafe int Size<T>() => Size(typeof(T));

		/// <summary>
		/// Get the size of <paramref name="instance"/> on heap in bytes
		/// </summary>
		/// <remarks>
		/// For types other than <see cref="string"/> and <see cref="Array"/>, equivalent to <see cref="Size(Type)"/>.<br />
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe int Size(object instance) {
			if (instance is string str)
				return checked(nint.Size * 2 + sizeof(int) + sizeof(char) + sizeof(char) * str.Length); // == SizeOfString(str.Length)
			else if (instance is Array) {
				var mt = corelib::System.Runtime.CompilerServices.RuntimeHelpers.GetMethodTable(instance);
				return checked((int)(mt->BaseSize + mt->ComponentSize * Unsafe.As<RawArrayData>(instance).Length));
			} else
				return (int)corelib::System.Runtime.CompilerServices.RuntimeHelpers.GetMethodTable(instance)->BaseSize;
		}

		/// <summary>
		/// <see cref="Size(Type)"/> for <see cref="string"/> type
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe int SizeOfString(int length) {
			ArgumentOutOfRangeException.ThrowIfNegative(length);
			// ObjectHeader + MethodTable + Length(int) + Chars + NullTerminator
			return checked(nint.Size * 2 + sizeof(int) + sizeof(char) + sizeof(char) * length);
			/* Equivalent to:
			var methodTable = (MethodTable*)typeof(string).TypeHandle.Value;
			return (int)methodTable->BaseSize + methodTable->ComponentSize * length; // 22 + sizeof(char) * length
			*/
		}
		/// <summary>
		/// <see cref="Size(Type)"/> for <see cref="Array"/> type (<typeparamref name="T"/>[], <typeparamref name="T"/>[,] ...)
		/// <para>All reference types of <typeparamref name="T"/> with the same parameters return the same value.</para>
		/// </summary>
		/// <typeparam name="T">Element type of the array</typeparam>
		/// <param name="length">Length of the array (product of length of all dimension)</param>
		/// <param name="dimension">Number of dimensions of the array</param>
		/// <remarks>
		/// Note that for one-dimensional array created from <see cref="Array.CreateInstance(Type, int[], int[])"/>, if the lowerbound is not 0,
		/// the size should be the result of this method plus <c><see langword="sizeof"/>(int) * 2</c><br />
		/// Or a better way is to use <see cref="Size(object)"/> instead.
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe int SizeOfArray<T>(int length, int dimension = 1) {
			ArgumentOutOfRangeException.ThrowIfNegative(length);
			ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);
			checked {
				// ObjectHeader + MethodTable + Length(nint) + Elements
				var size = nint.Size * 3 + sizeof(T) * length;
				if (dimension != 1)
					size += sizeof(int) * 2 * dimension; // (DimensionLength + LowerBound) * Rank
				return size;
			}
			/* Equivalent to:
			var methodTable = (MethodTable*)typeof(T).MakeArrayType(dimension).TypeHandle.Value;
			return (int)methodTable->BaseSize + methodTable->ComponentSize * length;
			*/
		}
	}
}
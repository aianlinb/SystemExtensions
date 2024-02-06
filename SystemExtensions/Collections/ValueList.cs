using System.Buffers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SystemExtensions.Spans;

namespace SystemExtensions.Collections {
	/// <summary>
	/// <see cref="List{T}"/> without heap allocations
	/// </summary>
	/// <remarks>
	/// Do not make any copies of this struct, pass it by reference instead.<br />
	/// If the buffer isn't provided to the constructor, or if the capacity grows after that,
	/// make sure to call <see cref="Dispose"/> to return the buffer to the <see cref="ArrayPool{T}"/>
	/// </remarks>
	public ref struct ValueList<T> { // TODO: Analyzer to prevent from copying
		public static ValueList<T> Empty => default;

		private T[]? rentedBuffer;
		private Span<T> Buffer;
		private ref T[]? self;
		public int Count { readonly get; private set; }
		public readonly int Capacity => Buffer.Length;

		public readonly T this[int index] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => AsSpan()[index];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => AsSpan()[index] = value;
		}

		/// <summary>
		/// Use <paramref name="initialBuffer"/> as the backing buffer for the list,
		/// and use <see cref="ArrayPool{T}.Shared"/> if more capacity is needed.
		/// </summary>
		/// <param name="initialCount">
		/// The number of items in <paramref name="initialBuffer"/> that are already in use.
		/// </param>
		public ValueList(Span<T> initialBuffer, int initialCount = 0) {
			ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)initialCount, (uint)initialBuffer.Length);
			Buffer = initialBuffer;
			Count = initialCount;
			self = ref Unsafe.AsRef(ref rentedBuffer);
		}

		public ValueList(int initialCapacity) {
			Buffer = rentedBuffer = ArrayPool<T>.Shared.Rent(initialCapacity);
			self = ref Unsafe.AsRef<T[]?>(ref rentedBuffer);
		}

		public ValueList() : this(0) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item) {
			EnsureRemainingCapacity(1);
			Unsafe.Add(ref GetPinnableReference(), Count) = item;
			++Count;
		}
		public void AddRange(scoped ReadOnlySpan<T> items) {
			EnsureRemainingCapacity(items.Length);
			items.CopyToUnchecked(ref Unsafe.Add(ref GetPinnableReference(), Count));
			Count += items.Length;
		}
		public void AddRange(scoped in ReadOnlySequence<T> items) {
			EnsureRemainingCapacity(checked((int)items.Length));
			foreach (var segment in items) {
				segment.Span.CopyToUnchecked(ref Unsafe.Add(ref GetPinnableReference(), Count));
				Count += segment.Length;
			}
		}
		public void AddRange(IEnumerable<T> collection) {
			if (typeof(T) == typeof(char) && collection is string str) {
				AddRange(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<char, T>(ref Unsafe.AsRef(in str.GetPinnableReference())), str.Length));
				return;
			}
			if (rentedBuffer is not null && collection is ICollection<T> c) {
				EnsureRemainingCapacity(c.Count);
				c.CopyTo(rentedBuffer, Count);
				Count += c.Count;
				return;
			}
			if (collection is List<T> list) {
				EnsureRemainingCapacity(list.Count);
				list.CopyTo(Buffer[Count..]);
				Count += list.Count;
				return;
			}
			if (collection is T[] array) {
				AddRange(new ReadOnlySpan<T>(array));
				return;
			}
			if (collection.TryGetNonEnumeratedCount(out var count))
				EnsureRemainingCapacity(count);

			using IEnumerator<T> en = collection.GetEnumerator();
			while (en.MoveNext())
				Add(en.Current);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureRemainingCapacity(int capacity) {
			EnsureCapacity(Count + capacity);
		}
		public void EnsureCapacity(int capacity) {
			if (capacity > Capacity) {
				if (!Unsafe.AreSame(ref self, ref rentedBuffer))
					ThrowHelper.Throw<InvalidOperationException>("A copy of ValueList<T> is detected, this type can only be passed by reference");
				var newBuffer = ArrayPool<T>.Shared.Rent(capacity);
				AsSpan().CopyTo(newBuffer);
				Buffer = newBuffer;
				if (rentedBuffer is not null)
					ArrayPool<T>.Shared.Return(rentedBuffer);
				rentedBuffer = newBuffer;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Span<T>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref GetPinnableReference(), Count); // Skip bounds check
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Span<T> AsSpan(int index) => AsSpan()[index..];
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Span<T> AsSpan(int index, int length) => AsSpan().Slice(index, length);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool Contains(T item) => Count != 0 && IndexOf(item) >= 0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly int IndexOf(T item) => SpanExtensions.IndexOf(AsSpan(), item);
		public readonly void CopyTo(T[] array, int arrayIndex) => CopyTo(array.AsSpan(arrayIndex));
		public readonly void CopyTo(scoped Span<T> span) => AsSpan().CopyTo(span);

		public void Insert(int index, T item) {
			EnsureRemainingCapacity(1);
			AsSpan(index).CopyToUnchecked(ref Unsafe.Add(ref GetPinnableReference(), index + 1));
			Unsafe.Add(ref GetPinnableReference(), index) = item;
			++Count;
		}
		/// <remarks>
		/// The <paramref name="items"/> should not overlap with the backing buffer of this list.
		/// </remarks>
		public void InsertRange(int index, scoped ReadOnlySpan<T> items) {
			EnsureRemainingCapacity(items.Length);
			// There's a rare case that the `items` overlaps with the destination below line copying to,
			// but the memory there is uninitialized and shouldn't be used by the user,
			// so we treat it as an unsafe behavior caused by the user themselves and don't check it.
			AsSpan(index).CopyToUnchecked(ref Unsafe.Add(ref GetPinnableReference(), index + items.Length));
			items.CopyToUnchecked(ref Unsafe.Add(ref GetPinnableReference(), index));
			Count += items.Length;
		}
		/// <remarks>
		/// The <paramref name="items"/> should not overlap with the backing buffer of this list.
		/// </remarks>
		public void InsertRange(int index, scoped in ReadOnlySequence<T> items) {
			var len = checked((int)items.Length);
			EnsureRemainingCapacity(len);
			AsSpan(index).CopyToUnchecked(ref Unsafe.Add(ref GetPinnableReference(), index + len));
			foreach (var segment in items) {
				segment.Span.CopyToUnchecked(ref Unsafe.Add(ref GetPinnableReference(), index));
				index += segment.Length;
			}
			Count += len;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(T item) {
			var index = IndexOf(item);
			if (index < 0)
				return false;
			RemoveAt(index);
			return true;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAt(int index) {
			AsSpan(index + 1).CopyToUnchecked(ref Unsafe.Add(ref GetPinnableReference(), index));
			--Count;

			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				Unsafe.Add(ref GetPinnableReference(), Count) = default!;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveRange(int startIndex) {
			var span = AsSpan(startIndex); // Bounds check here
			Count = startIndex;
			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				span.Clear();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveRange(int startIndex, int length) {
			if (length == 0)
				return;

			AsSpan(startIndex + length).CopyToUnchecked(ref Unsafe.Add(ref GetPinnableReference(), startIndex));
			Count -= length;

			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				Buffer.Slice(Count, length).Clear();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveRange(Range range) {
			var (offset, length) = range.GetOffsetAndLength(Count);
			RemoveRange(offset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear() {
			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				AsSpan().Clear();
			Count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose() {
			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				Buffer.Clear();
			if (rentedBuffer is not null && Unsafe.AreSame(ref self, ref rentedBuffer))
				ArrayPool<T>.Shared.Return(rentedBuffer);
			this = default;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetPinnableReference() => ref MemoryMarshal.GetReference(Buffer);
	}
}
extern alias corelib;

using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SystemExtensions.Spans {
	/// <remarks>
	/// Some of the methods in this class have the same signature as the methods in <see cref="MemoryExtensions"/> without the <see cref="IEquatable{T}"/> limitation.<br />
	/// Use those in <see cref="MemoryExtensions"/> for T that implements <see cref="IEquatable{T}"/> instead, for better performance.
	/// </remarks>
	public static class SpanExtensions {
		public static ReadOnlySpan<T> AsReadOnly<T>(this Span<T> span) => span;
		public static ReadOnlyMemory<T> AsReadOnly<T>(this Memory<T> memory) => memory;
		public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] array) => array;
		public static ReadOnlyMemory<T> AsReadOnlyMemory<T>(this T[] array) => array;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> AsWritable<T>(this ReadOnlySpan<T> span) => MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(span), span.Length);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Memory<T> AsWritable<T>(this ReadOnlyMemory<T> memory) => MemoryMarshal.AsMemory(memory);

		#region Unsafe
		/// <summary>
		/// <see cref="Span{T}.Slice(int)"/> without bounds checking
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> SliceUnchecked<T>(this Span<T> source, int start) {
			return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(source), (nint)(uint)start /* force zero-extension */), source.Length - start);
		}
		/// <summary>
		/// <see cref="Span{T}.Slice(int, int)"/> without bounds checking
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> SliceUnchecked<T>(this Span<T> source, int start, int length) {
			return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(source), (nint)(uint)start /* force zero-extension */), length);
		}
		/// <summary>
		/// <see cref="Span{T}"/>[<see cref="Range"/>] without bounds checking
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> SliceUnchecked<T>(this Span<T> source, in Range range) {
			var offset = range.Start.GetOffset(source.Length);
			return MemoryMarshal.CreateSpan(
				ref Unsafe.Add(ref MemoryMarshal.GetReference(source), (nint)(uint)offset /* force zero-extension */),
				range.End.GetOffset(source.Length) - offset
			);
		}
		/// <summary>
		/// <see cref="ReadOnlySpan{T}.Slice(int)"/> without bounds checking
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlySpan<T> SliceUnchecked<T>(this ReadOnlySpan<T> source, int start) {
			return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(source), (nint)(uint)start /* force zero-extension */), source.Length - start);
		}
		/// <summary>
		/// <see cref="ReadOnlySpan{T}.Slice(int, int)"/> without bounds checking
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlySpan<T> SliceUnchecked<T>(this ReadOnlySpan<T> source, int start, int length) {
			return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(source), (nint)(uint)start /* force zero-extension */), length);
		}
		/// <summary>
		/// <see cref="ReadOnlySpan{T}"/>[<see cref="Range"/>] without bounds checking
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlySpan<T> SliceUnchecked<T>(this ReadOnlySpan<T> source, in Range range) {
			var offset = range.Start.GetOffset(source.Length);
			return MemoryMarshal.CreateReadOnlySpan(
				ref Unsafe.Add(ref MemoryMarshal.GetReference(source), (nint)(uint)offset /* force zero-extension */),
				range.End.GetOffset(source.Length) - offset
			);
		}

		/// <summary>
		/// <see cref="ReadOnlySpan{T}.CopyTo(Span{T})"/> without bounds checking
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyToUnchecked<T>(this scoped ReadOnlySpan<T> source, scoped ref T destination) {
			corelib::System.Buffer.Memmove(ref destination, ref MemoryMarshal.GetReference(source), (nuint)source.Length);
		}
		/// <summary>
		/// <see cref="Span{T}.CopyTo(Span{T})"/> without bounds checking
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyToUnchecked<T>(this scoped Span<T> source, scoped ref T destination) {
			corelib::System.Buffer.Memmove(ref destination, ref MemoryMarshal.GetReference(source), (nuint)source.Length);
		}
		#endregion

		#region MemoryExtensions
		// Most of the code in this region was copied from System.MemoryExtensions and removed the IEquatable<T> limitation.
		// Licensed to the .NET Foundation under the MIT license.

		/// <summary>
		/// Searches for the specified <paramref name="value"/> and returns <see langword="true"/> if found. If not found, returns <see langword="false"/>.
		/// </summary>
		/// <param name="span">The span to search.</param>
		/// <param name="value">The value to search for.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(this scoped ReadOnlySpan<T> span, T value) => IndexOf(span, value) >= 0;
		/// <inheritdoc cref="MemoryExtensions.Contains{T}(ReadOnlySpan{T}, T)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(this scoped ReadOnlySpan<T> span, scoped ReadOnlySpan<T> value) => IndexOf(span, value) >= 0;
		/// <summary>
		/// Searches for any occurance of any of the specified <paramref name="values"/> and returns <see langword="true"/> if found. If not found, returns <see langword="false"/>.
		/// </summary>
		/// <param name="span">The span to search.</param>
		/// <param name="values">The set of values to search for.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ContainsAny<T>(this scoped ReadOnlySpan<T> span, scoped ReadOnlySpan<T> values) => IndexOfAny(span, values) >= 0;

		/// <summary>
		/// Searches for the specified <paramref name="value"/> and returns the index of its first occurrence. If not found, returns -1.
		/// </summary>
		/// <param name="span">The span to search.</param>
		/// <param name="value">The value to search for.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe int IndexOf<T>(this scoped ReadOnlySpan<T> span, T value) {
			if (corelib::System.Runtime.CompilerServices.RuntimeHelpers.IsBitwiseEquatable<T>()) {
#pragma warning disable CS8500 // BitwiseEquatable is always unmanged
				if (sizeof(T) == sizeof(byte))
					return corelib::System.SpanHelpers.IndexOfValueType(
						ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
						Unsafe.As<T, byte>(ref value),
						span.Length);
				else if (sizeof(T) == sizeof(short))
					return corelib::System.SpanHelpers.IndexOfValueType(
						ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
						Unsafe.As<T, short>(ref value),
						span.Length);
				else if (sizeof(T) == sizeof(int))
					return corelib::System.SpanHelpers.IndexOfValueType(
						ref Unsafe.As<T, int>(ref MemoryMarshal.GetReference(span)),
						Unsafe.As<T, int>(ref value),
						span.Length);
				else if (sizeof(T) == sizeof(long))
					return corelib::System.SpanHelpers.IndexOfValueType(
						ref Unsafe.As<T, long>(ref MemoryMarshal.GetReference(span)),
						Unsafe.As<T, long>(ref value),
						span.Length);
#pragma warning restore CS8500
			}
			return SpanHelpersWithoutIEquatable.IndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
		}
		/// <inheritdoc cref="MemoryExtensions.IndexOf{T}(ReadOnlySpan{T}, T)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe int IndexOf<T>(this scoped ReadOnlySpan<T> span, scoped ReadOnlySpan<T> value) {
			if (corelib::System.Runtime.CompilerServices.RuntimeHelpers.IsBitwiseEquatable<T>()) {
#pragma warning disable CS8500 // BitwiseEquatable is always unmanged
				if (sizeof(T) == sizeof(byte))
					return corelib::System.SpanHelpers.IndexOf(
						ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
						span.Length,
						ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)),
						value.Length);
				else if (sizeof(T) == sizeof(char))
					return corelib::System.SpanHelpers.IndexOf(
						ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)),
						span.Length,
						ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(value)),
						value.Length);
#pragma warning restore CS8500
			}
			return SpanHelpersWithoutIEquatable.IndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
		}
		/// <summary>
		/// Searches for the first index of any of the specified <paramref name="values"/> similar to calling <see cref="IndexOf{T}(ReadOnlySpan{T}, T)"/> several times with the logical OR operator. If not found, returns -1.
		/// </summary>
		/// <param name="span">The span to search.</param>
		/// <param name="values">The set of values to search for.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe int IndexOfAny<T>(this scoped ReadOnlySpan<T> span, scoped ReadOnlySpan<T> values) {
			if (corelib::System.Runtime.CompilerServices.RuntimeHelpers.IsBitwiseEquatable<T>()) {
#pragma warning disable CS8500 // BitwiseEquatable is always unmanged
				if (sizeof(T) == sizeof(byte)) {
					ref byte spanRef = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span));
					ref byte valueRef = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values));
					switch (values.Length) {
						case 0:
							return -1;
						case 1:
							return corelib::System.SpanHelpers.IndexOfValueType(ref spanRef, valueRef, span.Length);
						case 2:
							return corelib::System.SpanHelpers.IndexOfAnyValueType(
								ref spanRef,
								valueRef,
								Unsafe.Add(ref valueRef, 1),
								span.Length);
						case 3:
							return corelib::System.SpanHelpers.IndexOfAnyValueType(
								ref spanRef,
								valueRef,
								Unsafe.Add(ref valueRef, 1),
								Unsafe.Add(ref valueRef, 2),
								span.Length);
						case 4:
							return corelib::System.SpanHelpers.IndexOfAnyValueType(
								ref spanRef,
								valueRef,
								Unsafe.Add(ref valueRef, 1),
								Unsafe.Add(ref valueRef, 2),
								Unsafe.Add(ref valueRef, 3),
								span.Length);
						case 5:
							return corelib::System.SpanHelpers.IndexOfAnyValueType(
								ref spanRef,
								valueRef,
								Unsafe.Add(ref valueRef, 1),
								Unsafe.Add(ref valueRef, 2),
								Unsafe.Add(ref valueRef, 3),
								Unsafe.Add(ref valueRef, 4),
								span.Length);
					}
				} else if (sizeof(T) == sizeof(short)) {
					ref short spanRef = ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span));
					ref short valueRef = ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(values));
					return values.Length switch {
						0 => -1,
						1 => corelib::System.SpanHelpers.IndexOfValueType(ref spanRef, valueRef, span.Length),
						2 => corelib::System.SpanHelpers.IndexOfAnyValueType(
														ref spanRef,
														valueRef,
														Unsafe.Add(ref valueRef, 1),
														span.Length),
						3 => corelib::System.SpanHelpers.IndexOfAnyValueType(
														ref spanRef,
														valueRef,
														Unsafe.Add(ref valueRef, 1),
														Unsafe.Add(ref valueRef, 2),
														span.Length),
						4 => corelib::System.SpanHelpers.IndexOfAnyValueType(
														ref spanRef,
														valueRef,
														Unsafe.Add(ref valueRef, 1),
														Unsafe.Add(ref valueRef, 2),
														Unsafe.Add(ref valueRef, 3),
														span.Length),
						5 => corelib::System.SpanHelpers.IndexOfAnyValueType(
														ref spanRef,
														valueRef,
														Unsafe.Add(ref valueRef, 1),
														Unsafe.Add(ref valueRef, 2),
														Unsafe.Add(ref valueRef, 3),
														Unsafe.Add(ref valueRef, 4),
														span.Length),
						_ => corelib::System.Buffers.ProbabilisticMap.IndexOfAny(ref Unsafe.As<short, char>(ref spanRef), span.Length, ref Unsafe.As<short, char>(ref valueRef), values.Length),
					};
				}
			}
			return SpanHelpersWithoutIEquatable.IndexOfAny(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
		}

		/// <summary>
		/// Searches for the specified <paramref name="value"/> and returns the index of its last occurrence. If not found, returns -1.
		/// </summary>
		/// <param name="span">The span to search.</param>
		/// <param name="value">The value to search for.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe int LastIndexOf<T>(this scoped ReadOnlySpan<T> span, T value) {
			if (corelib::System.Runtime.CompilerServices.RuntimeHelpers.IsBitwiseEquatable<T>()) {
#pragma warning disable CS8500 // BitwiseEquatable is always unmanged
				if (sizeof(T) == sizeof(byte)) {
					return corelib::System.SpanHelpers.LastIndexOfValueType(
						ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
						Unsafe.As<T, byte>(ref value),
						span.Length);
				} else if (sizeof(T) == sizeof(short)) {
					return corelib::System.SpanHelpers.LastIndexOfValueType(
						ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
						Unsafe.As<T, short>(ref value),
						span.Length);
				} else if (sizeof(T) == sizeof(int)) {
					return corelib::System.SpanHelpers.LastIndexOfValueType(
						ref Unsafe.As<T, int>(ref MemoryMarshal.GetReference(span)),
						Unsafe.As<T, int>(ref value),
						span.Length);
				} else if (sizeof(T) == sizeof(long)) {
					return corelib::System.SpanHelpers.LastIndexOfValueType(
						ref Unsafe.As<T, long>(ref MemoryMarshal.GetReference(span)),
						Unsafe.As<T, long>(ref value),
						span.Length);
				}
#pragma warning restore CS8500
			}
			return SpanHelpersWithoutIEquatable.LastIndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
		}
		/// <summary>
		/// Searches for the specified sequence and returns the index of its last occurrence. If not found, returns -1.
		/// </summary>
		/// <param name="span">The span to search.</param>
		/// <param name="value">The sequence to search for.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe int LastIndexOf<T>(this scoped ReadOnlySpan<T> span, scoped ReadOnlySpan<T> value) {
			if (corelib::System.Runtime.CompilerServices.RuntimeHelpers.IsBitwiseEquatable<T>()) {
#pragma warning disable CS8500 // BitwiseEquatable is always unmanged
				if (sizeof(T) == sizeof(byte)) {
					return corelib::System.SpanHelpers.LastIndexOf(
						ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
						span.Length,
						ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)),
						value.Length);
				} else if (sizeof(T) == sizeof(char)) {
					return corelib::System.SpanHelpers.LastIndexOf(
						ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)),
						span.Length,
						ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(value)),
						value.Length);
				}
#pragma warning restore CS8500
			}
			return SpanHelpersWithoutIEquatable.LastIndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
		}

		/// <summary>
		/// Determines whether the specified sequence appears at the start of the <paramref name="span"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe bool StartsWith<T>(this scoped ReadOnlySpan<T> span, scoped ReadOnlySpan<T> value) {
			int valueLength = value.Length;
			if (corelib::System.Runtime.CompilerServices.RuntimeHelpers.IsBitwiseEquatable<T>())
				return valueLength <= span.Length &&
				corelib::System.SpanHelpers.SequenceEqual(
					ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
					ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)),
#pragma warning disable CS8500 // BitwiseEquatable is always unmanged
					((uint)valueLength) * (nuint)sizeof(T));  // If this multiplication overflows, the Span we got overflows the entire address range. There's no happy outcome for this api in such a case so we choose not to take the overhead of checking.
#pragma warning restore CS8500
			return valueLength <= span.Length && SpanHelpersWithoutIEquatable.SequenceEqual(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(value), valueLength);
		}
		/// <summary>
		/// Determines whether the specified sequence appears at the end of the <paramref name="span"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe bool EndsWith<T>(this scoped ReadOnlySpan<T> span, scoped ReadOnlySpan<T> value) {
			int spanLength = span.Length;
			int valueLength = value.Length;
			if (corelib::System.Runtime.CompilerServices.RuntimeHelpers.IsBitwiseEquatable<T>())
				return valueLength <= spanLength &&
					corelib::System.SpanHelpers.SequenceEqual(
					ref Unsafe.As<T, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), (nint)(uint)(spanLength - valueLength) /* force zero-extension */)),
					ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)),
#pragma warning disable CS8500 // BitwiseEquatable is always unmanged
					((uint)valueLength) * (nuint)sizeof(T));  // If this multiplication overflows, the Span we got overflows the entire address range. There's no happy outcome for this api in such a case so we choose not to take the overhead of checking.
#pragma warning restore CS8500
			return valueLength <= spanLength &&
				SpanHelpersWithoutIEquatable.SequenceEqual(
					ref Unsafe.Add(ref MemoryMarshal.GetReference(span), (nint)(uint)(spanLength - valueLength) /* force zero-extension */),
					ref MemoryMarshal.GetReference(value),
					valueLength);
		}

		/// <summary>
		/// Replaces all occurrences of <paramref name="oldValue"/> with <paramref name="newValue"/>.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the span.</typeparam>
		/// <param name="span">The span in which the elements should be replaced.</param>
		/// <param name="oldValue">The value to be replaced with <paramref name="newValue"/>.</param>
		/// <param name="newValue">The value to replace all occurrences of <paramref name="oldValue"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void Replace<T>(this scoped Span<T> span, T oldValue, T newValue) where T : IEquatable<T>? {
			nuint length = (uint)span.Length;

			if (corelib::System.Runtime.CompilerServices.RuntimeHelpers.IsBitwiseEquatable<T>()) {
#pragma warning disable CS8500 // BitwiseEquatable is always unmanged
				if (sizeof(T) == sizeof(byte)) {
					ref byte src = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span));
					corelib::System.SpanHelpers.ReplaceValueType(
						ref src,
						ref src,
						Unsafe.As<T, byte>(ref oldValue),
						Unsafe.As<T, byte>(ref newValue),
						length);
					return;
				} else if (sizeof(T) == sizeof(ushort)) {
					// Use ushort rather than short, as this avoids a sign-extending move.
					ref ushort src = ref Unsafe.As<T, ushort>(ref MemoryMarshal.GetReference(span));
					corelib::System.SpanHelpers.ReplaceValueType(
						ref src,
						ref src,
						Unsafe.As<T, ushort>(ref oldValue),
						Unsafe.As<T, ushort>(ref newValue),
						length);
					return;
				} else if (sizeof(T) == sizeof(int)) {
					ref int src = ref Unsafe.As<T, int>(ref MemoryMarshal.GetReference(span));
					corelib::System.SpanHelpers.ReplaceValueType(
						ref src,
						ref src,
						Unsafe.As<T, int>(ref oldValue),
						Unsafe.As<T, int>(ref newValue),
						length);
					return;
				} else if (sizeof(T) == sizeof(long)) {
					ref long src = ref Unsafe.As<T, long>(ref MemoryMarshal.GetReference(span));
					corelib::System.SpanHelpers.ReplaceValueType(
						ref src,
						ref src,
						Unsafe.As<T, long>(ref oldValue),
						Unsafe.As<T, long>(ref newValue),
						length);
					return;
				}
#pragma warning restore CS8500
			}
			SpanHelpersWithoutIEquatable.Replace(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(span), oldValue, newValue, length);
		}
		/// <summary>
		/// Copies <paramref name="source"/> to <paramref name="destination"/>, replacing all occurrences of <paramref name="oldValue"/> with <paramref name="newValue"/>.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the spans.</typeparam>
		/// <param name="source">The span to copy.</param>
		/// <param name="destination">The span into which the copied and replaced values should be written.</param>
		/// <param name="oldValue">The value to be replaced with <paramref name="newValue"/>.</param>
		/// <param name="newValue">The value to replace all occurrences of <paramref name="oldValue"/>.</param>
		/// <exception cref="ArgumentException">The <paramref name="destination"/> span was shorter than the <paramref name="source"/> span.</exception>
		/// <exception cref="ArgumentException">The <paramref name="source"/> and <paramref name="destination"/> were overlapping but not referring to the same starting location.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void Replace<T>(this scoped ReadOnlySpan<T> source, scoped Span<T> destination, T oldValue, T newValue) where T : IEquatable<T>? {
			nuint length = (uint)source.Length;
			if (length == 0)
				return;

			if (length > (uint)destination.Length)
				ThrowHelper.Throw<ArgumentException>("Destination is too short", nameof(destination));

			ref T src = ref MemoryMarshal.GetReference(source);
			ref T dst = ref MemoryMarshal.GetReference(destination);

			nint byteOffset = Unsafe.ByteOffset(ref src, ref dst);
			unchecked {
				if (byteOffset != 0 &&
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
					((nuint)byteOffset < (nuint)((nint)source.Length * sizeof(T)) ||
					 (nuint)byteOffset > (nuint)(-((nint)destination.Length * sizeof(T))))) {
#pragma warning restore CS8500
					ThrowHelper.Throw<ArgumentException>("Source and destination span overlapped", nameof(destination));
				}
			}

			if (corelib::System.Runtime.CompilerServices.RuntimeHelpers.IsBitwiseEquatable<T>()) {
#pragma warning disable CS8500 // BitwiseEquatable is always unmanged
				if (sizeof(T) == sizeof(byte)) {
					corelib::System.SpanHelpers.ReplaceValueType(
						ref Unsafe.As<T, byte>(ref src),
						ref Unsafe.As<T, byte>(ref dst),
						Unsafe.As<T, byte>(ref oldValue),
						Unsafe.As<T, byte>(ref newValue),
						length);
					return;
				} else if (sizeof(T) == sizeof(ushort)) {
					// Use ushort rather than short, as this avoids a sign-extending move.
					corelib::System.SpanHelpers.ReplaceValueType(
						ref Unsafe.As<T, ushort>(ref src),
						ref Unsafe.As<T, ushort>(ref dst),
						Unsafe.As<T, ushort>(ref oldValue),
						Unsafe.As<T, ushort>(ref newValue),
						length);
					return;
				} else if (sizeof(T) == sizeof(int)) {
					corelib::System.SpanHelpers.ReplaceValueType(
						ref Unsafe.As<T, int>(ref src),
						ref Unsafe.As<T, int>(ref dst),
						Unsafe.As<T, int>(ref oldValue),
						Unsafe.As<T, int>(ref newValue),
						length);
					return;
				} else if (sizeof(T) == sizeof(long)) {
					corelib::System.SpanHelpers.ReplaceValueType(
						ref Unsafe.As<T, long>(ref src),
						ref Unsafe.As<T, long>(ref dst),
						Unsafe.As<T, long>(ref oldValue),
						Unsafe.As<T, long>(ref newValue),
						length);
					return;
				}
#pragma warning restore CS8500
			}
			SpanHelpersWithoutIEquatable.Replace(ref src, ref dst, oldValue, newValue, length);
		}

		/// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
		/// <typeparam name="T">The element type of the span.</typeparam>
		/// <param name="span">The span to search.</param>
		/// <param name="value">The value for which to search.</param>
		/// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe int Count<T>(this scoped ReadOnlySpan<T> span, T value) {
			if (corelib::System.Runtime.CompilerServices.RuntimeHelpers.IsBitwiseEquatable<T>()) {
#pragma warning disable CS8500 // BitwiseEquatable is always unmanged
				if (sizeof(T) == sizeof(byte)) {
					return corelib::System.SpanHelpers.CountValueType(
						ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
						Unsafe.As<T, byte>(ref value),
						span.Length);
				} else if (sizeof(T) == sizeof(short)) {
					return corelib::System.SpanHelpers.CountValueType(
						ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
						Unsafe.As<T, short>(ref value),
						span.Length);
				} else if (sizeof(T) == sizeof(int)) {
					return corelib::System.SpanHelpers.CountValueType(
						ref Unsafe.As<T, int>(ref MemoryMarshal.GetReference(span)),
						Unsafe.As<T, int>(ref value),
						span.Length);
				} else if (sizeof(T) == sizeof(long)) {
					return corelib::System.SpanHelpers.CountValueType(
						ref Unsafe.As<T, long>(ref MemoryMarshal.GetReference(span)),
						Unsafe.As<T, long>(ref value),
						span.Length);
				}
#pragma warning restore CS8500
			}
			return SpanHelpersWithoutIEquatable.Count(ref MemoryMarshal.GetReference(span), value, span.Length);
		}
		/// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
		/// <typeparam name="T">The element type of the span.</typeparam>
		/// <param name="span">The span to search.</param>
		/// <param name="value">The value for which to search.</param>
		/// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
		public static int Count<T>(this scoped ReadOnlySpan<T> span, scoped ReadOnlySpan<T> value) {
			switch (value.Length) {
				case 0:
					return 0;
				case 1:
					return Count(span, value[0]);
				default:
					int count = 0;
					int pos;
					while ((pos = IndexOf(span, value)) >= 0) {
						span = span[(pos + value.Length)..];
						count++;
					}
					return count;
			}
		}
		#endregion MemoryExtensions

		public static bool Any<T>(this ReadOnlySpan<T> span, Predicate<T> predicate) {
			foreach (var item in span)
				if (predicate(item))
					return true;
			return false;
		}

		public static bool All<T>(this ReadOnlySpan<T> span, Predicate<T> predicate) {
			foreach (var item in span)
				if (!predicate(item))
					return false;
			return true;
		}

		/// <summary>
		/// See <see cref="SplitEnumerator"/>
		/// </summary>
		public static SplitEnumerator Split(this ReadOnlySpan<char> source, in char separator, StringSplitOptions options = StringSplitOptions.None) => Split(source, new ReadOnlySpan<char>(in separator), options);
		/// <summary>
		/// See <see cref="SplitEnumerator"/>
		/// </summary>
		public static SplitEnumerator Split(this ReadOnlySpan<char> source, ReadOnlySpan<char> separator, StringSplitOptions options = StringSplitOptions.None) => new(source, separator, false, options);
		/// <summary>
		/// See <see cref="SplitEnumerator"/>
		/// </summary>
		public static SplitEnumerator SplitAny(this ReadOnlySpan<char> source, ReadOnlySpan<char> separator, StringSplitOptions options = StringSplitOptions.None) => new(source, separator, true, options);
		/// <summary>
		/// Free allocation implementation of <see cref="string.Split(char, StringSplitOptions)"/>
		/// </summary>
		/// <param name="source">The source span to parse.</param>
		/// <param name="separator">A character(s) that delimits the regions in this instance.</param>
		/// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim whitespace and include empty ranges.</param>
		/// <param name="isAny">
		/// Whether to split on any of the characters in <paramref name="separator"/>.<br />
		/// <see langword="true"/> if the <paramref name="separator"/> is a set; <see langword="false"/> if <paramref name="separator"/> should be treated as a single separator.
		/// </param>
		/// <remarks>
		/// This underlying uses the <see cref="MemoryExtensions.Split(ReadOnlySpan{char}, Span{Range}, ReadOnlySpan{char}, StringSplitOptions)"/> or <see cref="MemoryExtensions.SplitAny(ReadOnlySpan{char}, Span{Range}, ReadOnlySpan{char}, StringSplitOptions)"/> but handles the <see cref="Range"/> things for you.<br />
		/// Note that each instance of <see cref="SplitEnumerator"/> can be enumerated only once.<br />
		/// Do not modify the <paramref name="source"/> and <paramref name="separator"/> during the lifetime of this instance.
		/// </remarks>
		[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref struct SplitEnumerator(ReadOnlySpan<char> source, ReadOnlySpan<char> separator, bool isAny, StringSplitOptions options = StringSplitOptions.None) {
			private ReadOnlySpan<char> source = source;
			private readonly ReadOnlySpan<char> separator = separator;
			private readonly StringSplitOptions options = options;
			private readonly bool isAny = isAny || separator.Length == 1; // MemoryExtensions.Split pass true to isAny of SplitCore for single char separator parameter

			/// <remarks><see href="https://github.com/dotnet/runtime/issues/96579"/></remarks>
			private readonly bool specialCase = options == StringSplitOptions.TrimEntries && (isAny ? separator.Any(c => char.IsWhiteSpace(c)) : separator.IsWhiteSpace());

			/// <inheritdoc cref="Ranges"/>
			private Ranges ranges;
			private int index = -1;
			private int count; // assumed never negative

			public bool MoveNext() {
				var i = index + 1;

				if (i == 0) // First element
					goto Spliting;
				else if ((uint)i >= (uint)count) // Reached end or constructor not called
					return false;
				else if (count == stackAllocationLength && i == count - 1 /*Last element*/) { // More spliting needed
					if (specialCase) { // https://github.com/dotnet/runtime/issues/96579
						source = source.SliceUnchecked(ranges.UnsafeAt(i - 1).End.GetOffset(source.Length));
						source = source.SliceUnchecked(
							isAny ?
								MemoryExtensions.IndexOfAny(source, separator) + 1
								: MemoryExtensions.IndexOf(source, separator) + separator.Length
						);
					} else
						source = source.SliceUnchecked(in ranges.UnsafeAt(i));
					goto Spliting;
				}

				// Other elements
				index = i;
				return true;

			Spliting:
				index = 0;
				count = isAny ?
					MemoryExtensions.SplitAny(source, ranges.AsSpan(), separator, options) :
					MemoryExtensions.Split(source, ranges.AsSpan(), separator, options);
				return count != 0;
			}

			public readonly ReadOnlySpan<char> Current {
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get {
					if ((uint)index >= (uint)count)
						ThrowHelper.Throw<IndexOutOfRangeException>();
					return source.SliceUnchecked(in ranges.UnsafeAt(index));
				}
			}

			/// <returns><see langword="this"/></returns>
			public readonly SplitEnumerator GetEnumerator() => this;

			private const int stackAllocationLength = 8; // Question: what's the best number here?
			/// <summary>
			/// Inline array of <see cref="Range"/> with <see cref="stackAllocationLength"/> elements.
			/// </summary>
			[InlineArray(stackAllocationLength)]
			private struct Ranges {
				private Range firstRange;

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public Span<Range> AsSpan() => MemoryMarshal.CreateSpan(ref firstRange, stackAllocationLength);

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public ref Range UnsafeAt(int index) => ref Unsafe.Add(ref Unsafe.AsRef(ref firstRange), index);
			}
		}

		public static MemoryEnumerable<T> AsEnumerable<T>(this ReadOnlyMemory<T> memory) => new(memory);
		public static MemoryEnumerable<T> AsEnumerable<T>(this Memory<T> memory) => new(memory);
		/// <summary>
		/// <see cref="IEnumerable{T}"/> implementation of <see cref="ReadOnlyMemory{T}"/>
		/// </summary>
		/// <remarks>
		/// Use <see cref="ReadOnlySpan{T}.Enumerator"/> from <see cref="ReadOnlyMemory{T}.Span"/> instead for better performance
		/// </remarks>
		public sealed class MemoryEnumerable<T>(ReadOnlyMemory<T> memory) : IEnumerable<T>, IEnumerator<T> {
			private int index = -1;

			public IEnumerator<T> GetEnumerator() => this;
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			public T Current => memory.Span[index];
			object? IEnumerator.Current => Current;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext() {
				var i = index + 1;
				if (i < memory.Length) {
					index = i;
					return true;
				}
				return false;
			}
			public void Reset() {
				index = -1;
			}
			public void Dispose() {
				memory = default;
			}
		}
	}
}
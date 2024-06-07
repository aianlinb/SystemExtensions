using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SystemExtensions.Streams;
/// <summary>
/// A <see cref="SubStream"/> that can only be read,
/// and disallow to seek beyond <see cref="SubStream.EndPosition"/>.
/// </summary>
public class ReadOnlySubStream : SubStream, IEquatable<ReadOnlySubStream> {
	/// <summary>
	/// See <see cref="SubStream(Stream, long)"/>
	/// </summary>
	public ReadOnlySubStream(Stream baseStream, long offset) : base(baseStream, offset) { }
	/// <summary>
	/// See <see cref="SubStream(Stream, long, long)"/>
	/// </summary>
	public ReadOnlySubStream(Stream baseStream, long offset, long length) : base(baseStream, offset, length) { }
	/// <summary>
	/// See <see cref="SubStream(Stream, Range)"/>
	/// </summary>
	public ReadOnlySubStream(Stream baseStream, Range range) : base(baseStream, range) { }

	#region Block_Writing
	/// <returns><see langword="false"/></returns>
	public override bool CanWrite => false;
	private const string ReadOnlyMessage = "Attempted to write to a ReadOnlySubStream";
	[DoesNotReturn]
	public override void SetLength(long value) => throw new NotSupportedException(ReadOnlyMessage);
	[DoesNotReturn]
	public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException(ReadOnlyMessage);
	[DoesNotReturn]
	public override void Write(scoped ReadOnlySpan<byte> buffer) => throw new NotSupportedException(ReadOnlyMessage);
	[DoesNotReturn]
	public override void WriteByte(byte value) => throw new NotSupportedException(ReadOnlyMessage);
	[DoesNotReturn]
	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException(ReadOnlyMessage);
	[DoesNotReturn]
	public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException(ReadOnlyMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override long Seek(long offset, SeekOrigin origin) {
		var basePos = origin switch {
			SeekOrigin.Begin => Offset,
			SeekOrigin.Current => BaseStream.Position,
			SeekOrigin.End => EndPosition,
			_ => throw ThrowHelper.ArgumentOutOfRange(origin)
		} + offset;
		if (basePos < Offset || basePos > EndPosition)
			ThrowHelper.ThrowArgumentOutOfRange(offset);
		BaseStream.Position = basePos;
		return Position;
	}
	#endregion Block_Writing

	#region IEquatable
	public virtual bool Equals(ReadOnlySubStream? other) {
		return other is not null
			&& BaseStream == other.BaseStream
			&& Offset == other.Offset
			&& EndPosition == other.EndPosition;
	}
	public override bool Equals(SubStream? other) => other is ReadOnlySubStream r && Equals(r);
	public override bool Equals(object? obj) => Equals(obj as ReadOnlySubStream);
	public override int GetHashCode() => ~base.GetHashCode();
	#endregion IEquatable
}
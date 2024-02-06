using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using SystemExtensions.Tasks;

namespace SystemExtensions.Streams {
	/// <summary>
	/// Wrap a <see cref="Stream"/> to represent a sub-range of it
	/// </summary>
	/// <remarks>
	/// This class won't limit the length that can be written to the <see cref="BaseStream"/>,
	/// so the <see cref="Length"/> may increase after writing or seeking (but not reading).
	/// <br />
	/// Note that the <see cref="Position"/> is dependent on the one of the <see cref="BaseStream"/>,
	/// if it is changed to before <see cref="Offset"/>, the read/write operations of this class may be performed outside the range.
	/// <br />
	/// Disposing this class is not necessary, and it won't dispose the underlying <see cref="BaseStream"/>,
	/// but only block the operations on this <see cref="SubStream"/>.
	/// </remarks>
	public class SubStream : Stream, IEquatable<SubStream> {
		/// <summary>
		/// The underlying <see cref="Stream"/> being wrapped
		/// </summary>
		[MemberNotNull(nameof(_baseStream))]
		public Stream BaseStream {
			get {
				ObjectDisposedException.ThrowIf(_baseStream is null, this);
				return _baseStream;
			}
		}
		/// <summary>
		/// Set to <see langword="null"/> after disposing
		/// </summary>
		private Stream? _baseStream;
		/// <summary>
		/// The offset in <see cref="BaseStream"/> at which this <see cref="SubStream"/> begins
		/// </summary>
		public long Offset { get; private set; } // Setter for Rescope(long, long)
		public sealed override long Length => EndPosition - Offset;
		/// <summary>
		/// The maximum position in <see cref="BaseStream"/> that can be read from this <see cref="SubStream"/>.<br />
		/// Equals to <see cref="Offset"/> + <see cref="Length"/>.
		/// </summary>
		public long EndPosition {
			get {
				if (BaseStream.Length < _lazyEndPosition)
					_lazyEndPosition = BaseStream.Length;
				return _lazyEndPosition;
			}
		}
		/// <remarks>
		/// If the <see cref="BaseStream"/>.Length is changed to less than <see cref="_lazyEndPosition"/>,
		/// calling <see cref="EndPosition"/> will update it to the <see cref="BaseStream"/>.Length.
		/// <br />
		/// If the updating is unneeded, use this field (<see cref="_lazyEndPosition"/>) instead.
		/// </remarks>
		private long _lazyEndPosition;

		/// <summary>
		/// Create a <see cref="SubStream"/> of the <paramref name="baseStream"/>
		/// </summary>
		/// <param name="baseStream">The underlying <see cref="Stream"/> to wrap</param>
		/// <param name="offset">
		/// The offset in <paramref name="baseStream"/> at which the <see cref="SubStream"/> begins.
		/// If it is greater than the <see cref="Stream.Position"/> of the <paramref name="baseStream"/>,
		/// this will seek the <paramref name="baseStream"/> to the <paramref name="offset"/>, or throw if not <see cref="Stream.CanSeek"/>.
		/// </param>
		public SubStream(Stream baseStream, long offset) : this(baseStream, offset, baseStream.Length - offset) { }

		/// <inheritdoc cref="SubStream(Stream, long)"/>
		/// <param name="length">
		/// The length of the <see cref="SubStream"/>.
		/// The max(default if not provided) value is <paramref name="baseStream"/>.<see cref="Stream.Length"/> - <paramref name="offset"/>
		/// </param>
		public SubStream(Stream baseStream, long offset, long length) {
			ArgumentNullException.ThrowIfNull(baseStream);
			_baseStream = baseStream;
			Rescope(offset, length);
		}

		/// <summary>
		/// Create a <see cref="SubStream"/> of the <paramref name="baseStream"/>
		/// </summary>
		/// <param name="baseStream">The underlying <see cref="Stream"/> to wrap</param>
		/// <param name="range">
		/// The <see cref="Range"/> of this <see cref="SubStream"/> in <paramref name="baseStream"/>.
		/// If the <see cref="Range.Start"/> is greater than the <see cref="Stream.Position"/> of the <paramref name="baseStream"/>,
		/// this will seek the <paramref name="baseStream"/> to the <see cref="Range.Start"/>, or throw if not <see cref="Stream.CanSeek"/>.
		/// </param>
		public SubStream(Stream baseStream, Range range) {
			ArgumentNullException.ThrowIfNull(baseStream);
			_baseStream = baseStream;
			Rescope(range);
		}

		/// <summary>
		/// Change the scope of this <see cref="SubStream"/> relative to the <see cref="BaseStream"/>, and return self
		/// </summary>
		/// <remarks>
		/// If the <paramref name="offset"/> is greater than the <see cref="Stream.Position"/> of the <see cref="BaseStream"/>,
		/// this will seek the <see cref="BaseStream"/> to the <paramref name="offset"/>, or throw if not <see cref="Stream.CanSeek"/>.
		/// </remarks>
		/// <returns><see langword="this"/></returns>
		public SubStream Rescope(long offset, long length) {
			if (offset < 0 || offset > BaseStream.Length)
				ThrowHelper.ThrowArgumentOutOfRange(offset);
			var endPos = offset + length;
			if (length < 0 || endPos > _baseStream.Length)
				ThrowHelper.ThrowArgumentOutOfRange(length);

			Offset = offset;
			_lazyEndPosition = endPos;

			if (_baseStream.Position < offset)
				_baseStream.Position = offset; // throw if not seekable

			return this;
		}

		/// <summary>
		/// Change the scope of this <see cref="SubStream"/> relative to the <see cref="BaseStream"/>, and return self
		/// </summary>
		/// <remarks>
		/// If the <see cref="Range.Start"/> is greater than the <see cref="Stream.Position"/> of the <see cref="BaseStream"/>,
		/// this will seek the <see cref="BaseStream"/> to the <see cref="Range.Start"/>, or throw if not <see cref="Stream.CanSeek"/>.
		/// </remarks>
		/// <returns><see langword="this"/></returns>
		public SubStream Rescope(Range range) {
			Offset = range.Start.IsFromEnd ? BaseStream.Length - range.Start.Value : range.Start.Value;
			_lazyEndPosition = range.End.IsFromEnd ? BaseStream.Length - range.End.Value : range.End.Value;

			if (BaseStream.Position < Offset)
				_baseStream.Position = Offset; // throw if not seekable

			return this;
		}

		public override int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, CheckLengthBeforeRead(count));
		public override int Read(scoped Span<byte> buffer) => BaseStream.Read(buffer[..CheckLengthBeforeRead(buffer.Length)]);
		public override int ReadByte() => BaseStream.Position >= _lazyEndPosition ? -1 : _baseStream.ReadByte();
		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => BaseStream.ReadAsync(buffer, offset, CheckLengthBeforeRead(count), cancellationToken);
		public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => BaseStream.ReadAsync(buffer[..CheckLengthBeforeRead(buffer.Length)], cancellationToken);

		public override long Position { get => BaseStream.Position - Offset; set => Seek(value, SeekOrigin.Begin); }
		public override long Seek(long offset, SeekOrigin origin) {
			var basePos = origin switch {
				SeekOrigin.Begin => Offset,
				SeekOrigin.Current => BaseStream.Position,
				SeekOrigin.End => EndPosition,
				_ => throw ThrowHelper.ArgumentOutOfRange(origin)
			} + offset;
			if (basePos < Offset)
				ThrowHelper.ThrowArgumentOutOfRange(offset);
			BaseStream.Position = basePos;
			return Position;
		}

		/// <summary>
		/// Sets the length of the current stream to <paramref name="value"/>,
		/// and sets the length of the <see cref="BaseStream"/> to <see cref="Offset"/> + <paramref name="value"/>.<br />
		/// Throws if <paramref name="value"/> is negative.
		/// </summary>
		/// <remarks>
		/// To avoid affecting the <see cref="BaseStream"/>, use <see cref="Rescope(long, long)"/> instead.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is negative</exception>
		public override void SetLength(long value) {
#if NET8_0_OR_GREATER
			ArgumentOutOfRangeException.ThrowIfNegative(value);
#else
			ThrowHelper.ThrowArgumentOutOfRangeIf(value < 0, value);
#endif
			BaseStream.SetLength(Offset + value);
			_lazyEndPosition = _baseStream.Length;
		}

		public override void Write(byte[] buffer, int offset, int count) {
			BaseStream.Write(buffer, offset, count);
			CheckLengthAfterWrite();
		}
		public override void Write(scoped ReadOnlySpan<byte> buffer) {
			BaseStream.Write(buffer);
			CheckLengthAfterWrite();
		}
		public override void WriteByte(byte value) {
			BaseStream.WriteByte(value);
			CheckLengthAfterWrite();
		}
		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => BaseStream.WriteAsync(buffer, offset, count, cancellationToken).ContinueWith(CheckLengthAfterWrite, cancellationToken);
		public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => BaseStream.WriteAsync(buffer, cancellationToken).ContinueWith(action: CheckLengthAfterWrite, cancellationToken);

		public override void CopyTo(Stream destination, int bufferSize) {
			if (_lazyEndPosition >= BaseStream.Length) // not limiting reading length
				_baseStream.CopyTo(destination, bufferSize);
			else
				base.CopyTo(destination, bufferSize);
		}
		public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) {
			if (_lazyEndPosition >= BaseStream.Length) // not limiting reading length
				return _baseStream.CopyToAsync(destination, bufferSize, cancellationToken);
			else
				return base.CopyToAsync(destination, bufferSize, cancellationToken);
		}

		protected override void Dispose(bool disposing) {
			if (disposing)
				_baseStream = null;
			base.Dispose(disposing);
		}

		public ReadOnlySubStream AsReadOnly() => new(BaseStream, Offset, Length);

		#region IEquatable
		public virtual bool Equals(SubStream? other) {
			return other is not null
				&& BaseStream == other.BaseStream // Throw if disposed
				&& Offset == other.Offset
				&& EndPosition == other.EndPosition;
		}
		public override bool Equals(object? obj) => Equals(obj as SubStream);
		public override int GetHashCode() => BaseStream.GetHashCode() ^ (int)Offset ^ (int)EndPosition;
		#endregion IEquatable

		#region Bridge only
		public override bool CanRead => _baseStream?.CanRead ?? false;
		public override bool CanSeek => _baseStream?.CanSeek ?? false;
		public override bool CanWrite => _baseStream?.CanWrite ?? false;
		public override bool CanTimeout => _baseStream?.CanTimeout ?? false;
		public override int ReadTimeout { get => BaseStream.ReadTimeout; set => BaseStream.ReadTimeout = value; }
		public override int WriteTimeout { get => BaseStream.WriteTimeout; set => BaseStream.WriteTimeout = value; }
		public override void Flush() => BaseStream.Flush();
		public override Task FlushAsync(CancellationToken cancellationToken) => BaseStream.FlushAsync(cancellationToken);
		#endregion Bridge only

		#region Helpers
		/// <summary>
		/// Limit the <paramref name="count"/> to prevent reading beyond <see cref="EndPosition"/>
		/// </summary>
		/// <param name="count">The length the caller wants to read</param>
		/// <returns>The length should be read from the <see cref="BaseStream"/></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int CheckLengthBeforeRead(int count) {
			var remain = _lazyEndPosition - BaseStream.Position;
			return remain > count ? count : (int)remain;
		}
		/// <inheritdoc cref="CheckLengthAfterWrite()"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CheckLengthAfterWrite(object? _) => CheckLengthAfterWrite();
		/// <summary>
		/// Expand <see cref="Length"/> if written beyond it
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CheckLengthAfterWrite() {
			if (_lazyEndPosition < BaseStream.Position)
				_lazyEndPosition = _baseStream.Position;
		}
		#endregion Helpers
	}
}
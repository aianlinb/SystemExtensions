namespace System.IO {
	using System.IO.Strategies;
	using System.Threading;
	using System.Threading.Tasks;
	using Microsoft.Win32.SafeHandles;

	public static class RandomAccess {
		public static unsafe int ReadAtOffset(SafeFileHandle handle, Span<byte> buffer, long fileOffset) => throw null!;
		public static ValueTask<int> ReadAtOffsetAsync(SafeFileHandle handle, Memory<byte> buffer, long fileOffset, CancellationToken cancellationToken, OSFileStreamStrategy? strategy = null) => throw null!;
		public static unsafe void WriteAtOffset(SafeFileHandle handle, ReadOnlySpan<byte> buffer, long fileOffset) => throw null!;
		public static ValueTask WriteAtOffsetAsync(SafeFileHandle handle, ReadOnlyMemory<byte> buffer, long fileOffset, CancellationToken cancellationToken, OSFileStreamStrategy? strategy = null) => throw null!;
	}
}

namespace System.IO.Strategies {
	public abstract class OSFileStreamStrategy { }
}
﻿using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SystemExtensions {
	public static class Utils {
		/// <summary>
		/// <see cref="Index.GetOffset"/> but with <see cref="long"/> <paramref name="length"/>
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long GetOffset(this scoped in Index index, long length) {
			Debug.Assert(length >= 0);
			long value = Unsafe.As<Index, int>(ref Unsafe.AsRef(in index)); // local copy
			if (value < 0) // index.IsFromEnd
				unchecked {
					value += length + 1; // length - ~value == value + length + 1
				} 
			return value;
		}

		/// <summary>
		/// <see cref="Range.GetOffsetAndLength"/> but with <see cref="long"/> <paramref name="length"/>
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (long Offset, long Length) GetOffsetAndLength(this scoped in Range range, long length) {
			Debug.Assert(length >= 0);
			// Since Range is actually two Index (which is actually an int)
			long start = Unsafe.As<Range, int>(ref Unsafe.AsRef(in range)); // local copy and cast to long
			long end = Unsafe.Add(ref Unsafe.As<Range, int>(ref Unsafe.AsRef(in range)), 1);

			unchecked {
				// See Index.GetOffset
				// We don't cache length + 1 here because it's rare that both Start and End are IsFromEnd
				if (start < 0)
					start += length + 1; // length - ~start == start + length + 1
				if (end < 0)
					end += length + 1;

				if ((ulong)end > (ulong)length || (ulong)start > (ulong)end)
					ThrowHelper.ThrowArgumentOutOfRange(length);
			}
			return (start, end - start);
		}

		/// <summary>
		/// <see cref="Range.GetOffsetAndLength"/> but returns End position instead of Length (which is End - Offset)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (int Offset, int End) GetOffsetAndEnd(this Range range, int length) {
			Debug.Assert(length >= 0);
			// Since Range is actually two Index (which is actually an int)
			ref int start = ref Unsafe.As<Range, int>(ref range);
			ref int end = ref Unsafe.Add(ref start, 1);

			unchecked {
				// See Index.GetOffset
				// We don't cache length + 1 here because it's rare that both Start and End are IsFromEnd
				if (start < 0)
					start += length + 1; // length - ~start == start + length + 1
				if (end < 0)
					end += length + 1;

				if ((uint)end > (uint)length || (uint)start > (uint)end)
					ThrowHelper.ThrowArgumentOutOfRange(length);
			}
			return Unsafe.As<int, (int, int)>(ref start);
		}

		/// <summary>
		/// <see cref="GetOffsetAndEnd(Range, int)"/> but with <see cref="long"/> <paramref name="length"/>
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (long Offset, long End) GetOffsetAndEnd(this scoped in Range range, long length) {
			Debug.Assert(length >= 0);
			// Since Range is actually two Index (which is actually an int)
			long start = Unsafe.As<Range, int>(ref Unsafe.AsRef(in range)); // local copy and cast to long
			long end = Unsafe.Add(ref Unsafe.As<Range, int>(ref Unsafe.AsRef(in range)), 1);

			unchecked {
				// See Index.GetOffset
				// We don't cache length + 1 here because it's rare that both Start and End are IsFromEnd
				if (start < 0)
					start += length + 1; // length - ~start == start + length + 1
				if (end < 0)
					end += length + 1;

				if ((ulong)end > (ulong)length || (ulong)start > (ulong)end)
					ThrowHelper.ThrowArgumentOutOfRange(length);
			}
			return (start, end);
		}

		/// <summary>
		/// On Windows, equivalent to <see cref="Environment.ExpandEnvironmentVariables"/>.<br />
		/// On Unix, at the beginning of the <paramref name="path"/>, expands "~" or "~currentUsername" to the user's home directory,
		/// or "~+" to the <see cref="Environment.CurrentDirectory"/>.<br />
		/// Otherwise (for example: Browser), return <paramref name="path"/> as is.
		/// </summary>
		public static string ExpandPath(string path) {
			if (OperatingSystem.IsWindows())
				return Environment.ExpandEnvironmentVariables(path);
			else if (path.StartsWith('~') && Environment.OSVersion.Platform == PlatformID.Unix) {
				if (path.Length == 1 || path[1] == Path.DirectorySeparatorChar || path.AsSpan(1).SequenceEqual(Environment.UserName)) { // "~" || "~/" || "~currentUsername"
					var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
					if (userProfile != "/") // When failed, it fallbacks to "/" in above line
						return string.Concat(userProfile, path.AsSpan(1));
				} else if (path[1] == '+') {
					if (path.Length == 2 || path[2] == Path.DirectorySeparatorChar) // "~+" || "~+/"
						return string.Concat(Environment.CurrentDirectory, path.AsSpan(2));
					// "~+......" => return as is
				}/* else // "~otherUsername/..."
					try {
						using var p = Process.Start(new ProcessStartInfo("sh") {
							CreateNoWindow = true,
							RedirectStandardInput = true,
							RedirectStandardOutput = true,
							WindowStyle = ProcessWindowStyle.Hidden
						});
						if (p is not null) {
							p.StandardInput.WriteLine("echo " + path);
							p.StandardInput.Flush();
							var tmp = p.StandardOutput.ReadLine();
							p.Kill();
							if (Path.Exists(tmp))
								return tmp;
						}
					} catch { }
				*/
			}
			return path;
		}
	}
}
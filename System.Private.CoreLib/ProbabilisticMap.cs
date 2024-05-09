using System.Runtime.InteropServices;

namespace System.Buffers {
	/// <summary>
	/// Forwarded to System.Private.CoreLib.dll at runtime
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct ProbabilisticMap {
#pragma warning disable IDE0051 // Remove unused private member
		private readonly uint _e0, _e1, _e2, _e3, _e4, _e5, _e6, _e7;
#pragma warning restore IDE0051
		public static extern int IndexOfAny(ref char searchSpace, int searchSpaceLength, ref char values, int valuesLength);
	}
}
using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices {
	/// <summary>
	/// Forwarded to System.Private.CoreLib.dll at runtime
	/// </summary>
	public static class RuntimeHelpers {
		public static bool IsBitwiseEquatable<T>() => throw null!;
		public static unsafe MethodTable* GetMethodTable(object obj) => throw null!;
	}

	/// <summary>
	/// Forwarded to System.Private.CoreLib.dll at runtime
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct MethodTable { // Subset of src\vm\methodtable.h
		/// <summary>
		/// The low WORD of the first field is the component size for array and string types.
		/// </summary>
		[FieldOffset(0)]
		public ushort ComponentSize;

		/// <summary>
		/// The flags for the current method table (only for not array or string types).
		/// </summary>
		[FieldOffset(0)]
		public uint Flags;

		/// <summary>
		/// The base size of the type (used when allocating an instance on the heap).
		/// </summary>
		[FieldOffset(4)]
		public uint BaseSize;

		// See additional native members in methodtable.h, not needed here yet.
		// 0x8: m_dwFlags2 (additional flags and token in upper 24 bits)
		// 0xC: m_wNumVirtuals

		/// <summary>
		/// The number of interfaces implemented by the current type.
		/// </summary>
		[FieldOffset(0x0E)]
		public ushort InterfaceCount;
		/*
		// For DEBUG builds, there is a conditional field here (see methodtable.h again).
		// 0x10: debug_m_szClassName (display name of the class, for the debugger)
		
        private const int ParentMethodTableOffset = 0x10 + DebugClassNamePtr;
		/// <summary>
		/// A pointer to the parent method table for the current one.
		/// </summary>
		[FieldOffset(ParentMethodTableOffset)]

		// Additional conditional fields (see methodtable.h).
		// m_pModule
		// m_pAuxiliaryData
		// union {
		//   m_pEEClass (pointer to the EE class)
		//   m_pCanonMT (pointer to the canonical method table)
		// }

		/// <summary>
		/// This element type handle is in a union with additional info or a pointer to the interface map.
		/// Which one is used is based on the specific method table being in used (so this field is not
		/// always guaranteed to actually be a pointer to a type handle for the element type of this type).
		/// </summary>
		[FieldOffset(ElementTypeOffset)]
		public unsafe void* ElementType;

		/// <summary>
		/// This interface map used to list out the set of interfaces. Only meaningful if InterfaceCount is non-zero.
		/// </summary>
		[FieldOffset(InterfaceMapOffset)]
		public MethodTable** InterfaceMap;

		private const int DebugClassNamePtr = // adjust for debug_m_szClassName
#if DEBUG
#if TARGET_64BIT
            8
#else
			4
#endif
#else
            0
#endif
			;

		private const int ParentMethodTableOffset = 0x10 + DebugClassNamePtr;

#if TARGET_64BIT
        private const int ElementTypeOffset = 0x30 + DebugClassNamePtr;
#else
		private const int ElementTypeOffset = 0x20 + DebugClassNamePtr;
#endif

#if TARGET_64BIT
        private const int InterfaceMapOffset = 0x38 + DebugClassNamePtr;
#else
		private const int InterfaceMapOffset = 0x24 + DebugClassNamePtr;
#endif
		*/
	}

	/// <summary>
	/// Forwarded to System.Private.CoreLib.dll at runtime
	/// </summary>
	public sealed class RawData {
		public byte Data;
	}

	/// <summary>
	/// Forwarded to System.Private.CoreLib.dll at runtime
	/// </summary>
	public sealed class RawArrayData {
		public uint Length; // Array._numComponents padded to IntPtr
#if TARGET_64BIT // Without this define is ok, cause it will be forwarded to the one in System.Private.CoreLib.dll at runtime
		public uint Padding;
#endif
		public byte Data;
	}
}
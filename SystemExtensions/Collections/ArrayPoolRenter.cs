using System.Buffers;

namespace SystemExtensions.Collections;

/// <summary>
/// Rents an array from <see cref="ArrayPool{T}.Shared"/> and returns it when disposed.
/// </summary>
public sealed class ArrayPoolRenter<T>(int minimumLength) : IDisposable { // Must be a class to prevent duplicate
	/// <summary>
	/// The rented array. Will be <see langword="null"/> after <see cref="Return"/> or <see cref="Dispose"/>.
	/// </summary>
	public T[] Array { get; private set; } = ArrayPool<T>.Shared.Rent(minimumLength);

	/// <summary>
	/// Rents a array with <paramref name="minimumLength"/> and returns the old one.
	/// </summary>
	/// <remarks>
	/// The new array will be stored in <see cref="Array"/>.
	/// </remarks>
	public void Resize(int minimumLength) {
		lock (this) {
			var newArray = ArrayPool<T>.Shared.Rent(minimumLength); // Rent first to check the argument
			Return();
			Array = newArray;
		}
	}

	/// <summary>
	/// Rents a array with <paramref name="minimumLength"/>
	/// and copies the first <paramref name="copyLength"/> elements to the new array.
	/// And then returns the old one.
	/// </summary>
	/// <remarks>
	/// The new array will be stored in <see cref="Array"/>.
	/// </remarks>
	public void Resize(int minimumLength, int copyLength) {
		lock (this) {
			var newArray = ArrayPool<T>.Shared.Rent(minimumLength);
			try {
				new ReadOnlySpan<T>(Array, 0, copyLength).CopyTo(newArray);
			} catch {
				ArrayPool<T>.Shared.Return(newArray);
				throw;
			}
			Return();
			Array = newArray;
		}
	}

	/// <summary>
	/// Returns the <see cref="Array"/> to the pool, and sets it to <see langword="null"/>.
	/// </summary>
	public void Return(bool clearArray = false) {
		lock (this) {
			if (Array is not null) {
				ArrayPool<T>.Shared.Return(Array, clearArray);
				Array = null!;
			}
		}
	}

	/// <summary>
	/// Calls <see cref="Return"/>.
	/// </summary>
	public void Dispose() => Return();
}
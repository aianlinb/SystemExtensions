using System.Collections;

namespace SystemExtensions.Collections;

/// <summary>
/// Wrap a <see cref="IEnumerator{T}"/> to pick its first value and save to <see cref="First"/>,
/// but still can iterate all values including the first one without calling <see cref="IEnumerator.Reset"/>.
/// </summary>
/// <remarks>
/// <para>
/// This is useful when you want to get the first value before a foreach loop,
/// but don't want to re-enumerate the collection.
/// </para>
/// <para>
/// Note that the <see cref="GetEnumerator"/> returns the <see cref="PickFirstEnumerator{T}"/> itself,
/// so this <see cref="IEnumerable{T}"/> can't be used again before calling <see cref="Reset"/>.
/// </para>
/// </remarks>
public struct PickFirstEnumerator<T> : IEnumerator<T>, IEnumerable<T> {
	public PickFirstEnumerator(IEnumerable<T> enumerable) : this(enumerable.GetEnumerator()) { }
	public PickFirstEnumerator(IEnumerator<T> enumerator) {
		if (enumerator.MoveNext())
			First = enumerator.Current;
		else
			State = 1;
		BaseEnumerator = enumerator;
	}

	/// <summary>
	/// The base <see cref="IEnumerator{T}"/> that was passed to the constructor.
	/// </summary>
	public readonly IEnumerator<T> BaseEnumerator { get; }
	/// <summary>
	/// The first value of <see cref="BaseEnumerator"/>, or <see langword="default"/> if it's empty.
	/// </summary>
	public readonly T? First { get; }
	private byte State; // 0: Initial, 1: Initial (Empty), 2: First moved, 3: Other

	public readonly T Current => State switch {
		0 or 1 => default!,
		2 => First!,
		_ => BaseEnumerator.Current
	};
	readonly object IEnumerator.Current => Current!;

	public bool MoveNext() {
		switch (State) {
			case 0:
				State = 2;
				return true;
			case 1:
				State = 3;
				return false;
			case 2:
				State = 3;
				break;
		}
        return BaseEnumerator.MoveNext();
	}

	public void Reset() {
		BaseEnumerator.Reset();
		this = new(BaseEnumerator);
	}

	public readonly void Dispose() => BaseEnumerator.Dispose();

	/// <summary>
	/// Returns <see langword="this"/> as is.
	/// </summary>
	/// <remarks>
	/// See the remarks of <see cref="PickFirstEnumerator{T}"/>.
	/// </remarks>
	public readonly IEnumerator<T> GetEnumerator() => this;
	readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
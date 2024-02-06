using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace SystemExtensions {
	[DebuggerNonUserCode]
	public static class ThrowHelper {
		/// <summary>
		/// Returns an <see cref="Exception"/> of the specified type <typeparamref name="T"/> with its parameterless constructor
		/// </summary>
		/// <returns><typeparamref name="T"/> with its parameterless constructor</returns>
		public static T Create<T>() where T : new() => new();
		/// <summary>
		/// Returns an <see cref="Exception"/> of the specified type <typeparamref name="T"/> instantiated with <paramref name="arguments"/>
		/// </summary>
		/// <returns><typeparamref name="T"/> with its parameterless constructor</returns>
		public static T Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params object?[] arguments) where T : Exception => (Activator.CreateInstance(typeof(T), arguments) as T)!;
		/// <summary>
		/// Throws an <see cref="Exception"/> of the specified type <typeparamref name="T"/> with its parameterless constructor
		/// </summary>
		[DoesNotReturn]
		public static void Throw<T>() where T : Exception, new() => throw Create<T>();
		/// <summary>
		/// Throws an <see cref="Exception"/> of the specified type <typeparamref name="T"/> instantiated with <paramref name="arguments"/>
		/// </summary>
		[DoesNotReturn]
		public static void Throw<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params object?[] arguments) where T : Exception => throw Create<T>(arguments);

		/// <summary>
		/// Throws an <see cref="ArgumentOutOfRangeException"/>
		/// </summary>
		/// <remarks>
		/// When using C# 10 and above, the <paramref name="paramName"/> will be set to the expression passed to <paramref name="actualValue"/> if not provided.
		/// See <see cref="CallerArgumentExpressionAttribute"/>.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException"/>
		[DoesNotReturn]
		public static void ThrowArgumentOutOfRange(object? actualValue = null, string? message = null, [CallerArgumentExpression(nameof(actualValue))] string? paramName = null) {
			throw ArgumentOutOfRange(actualValue, message, paramName);
		}
		/// <summary>
		/// Returns a new <see cref="ArgumentOutOfRangeException"/>
		/// </summary>
		/// <remarks>
		/// When using C# 10 and above, the <paramref name="paramName"/> will be set to the expression passed to <paramref name="actualValue"/> if not provided.
		/// See <see cref="CallerArgumentExpressionAttribute"/>.
		/// </remarks>
		public static ArgumentOutOfRangeException ArgumentOutOfRange(object? actualValue, string? message = null, [CallerArgumentExpression(nameof(actualValue))] string? paramName = null) => new(paramName, actualValue, message);

		/// <summary>
		/// Throws a thrown <paramref name="exception"/> without losing its original stack trace
		/// </summary>
		/// <param name="exception">
		/// The <see cref="Exception"/> catched to rethrow
		/// </param>
		[DoesNotReturn]
		public static void ThrowKeepStackTrace(this Exception exception) {
			ExceptionDispatchInfo.Capture(exception).Throw();
		}
	}
}
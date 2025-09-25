#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

namespace System.Reflection;

/// <summary>
/// An interface for method accessor that provides late binding access to method invocation.
/// </summary>
/// <remarks>
/// This interface provides a high-performance way to invoke methods using compiled expressions
/// rather than traditional reflection, while maintaining type safety and flexibility.
/// </remarks>
#if PUBLIC_EXTENSIONS
public
#endif
interface IMethodAccessor
{
    /// <summary>
    /// Gets the <see cref="System.Reflection.MethodInfo"/> that describes this method.
    /// </summary>
    /// <value>The <see cref="System.Reflection.MethodInfo"/> instance containing reflection metadata for this method.</value>
    MethodInfo MethodInfo { get; }

    /// <summary>
    /// Gets the name of the method.
    /// </summary>
    /// <value>The name of the method as defined in the source code.</value>
    string Name { get; }

    /// <summary>
    /// Invokes the method on the specified instance with the provided arguments.
    /// </summary>
    /// <param name="instance">The object on which to invoke the method. Can be <see langword="null"/> for static methods.</param>
    /// <param name="arguments">An argument list for the invoked method. The arguments must match the method's parameter types and count.</param>
    /// <returns>
    /// An object containing the return value of the invoked method, or <see langword="null"/> if the method returns <see langword="void"/>
    /// or if the method's return value is <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the number of arguments doesn't match the method's parameter count, or when argument types are incompatible with the method's parameter types.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to invoke an instance method with a <see langword="null"/> instance, or when the instance type is incompatible with the method's declaring type.
    /// </exception>
    /// <exception cref="TargetInvocationException">
    /// Thrown when the invoked method throws an exception. The original exception can be found in the <see cref="Exception.InnerException"/> property.
    /// </exception>
    object? Invoke(object? instance, params object?[] arguments);
}

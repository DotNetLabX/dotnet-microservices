using Blocks.Exceptions;

namespace Blocks.Core;

public static class GuardExtensions
{
    //insight - use this method as well so we can see the difference between Guard.NotFound and the extension method.
    public static T OrThrowNotFound<T>(this T? value, string? message = null) where T : class
        => value ?? throw new NotFoundException(message ?? $"{typeof(T).Name} not found");
}

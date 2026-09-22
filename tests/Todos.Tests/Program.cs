using System.Reflection;

namespace Todos.Tests;

internal static class Program
{
    [STAThread]
    private static async Task<int> Main()
    {
        var testMethods = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.IsClass)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(method => method.GetCustomAttribute<TestAttribute>() is not null)
            .OrderBy(method => method.DeclaringType?.Name)
            .ThenBy(method => method.Name)
            .ToArray();

        var failures = new List<string>();
        var startedAt = DateTimeOffset.UtcNow;

        foreach (var method in testMethods)
        {
            var name = $"{method.DeclaringType?.Name}.{method.Name}";
            try
            {
                var result = method.Invoke(null, null);
                if (result is Task task)
                {
                    await task;
                }
                Console.WriteLine($"PASS {name}");
            }
            catch (TargetInvocationException exception)
            {
                var message = exception.InnerException?.Message ?? exception.Message;
                failures.Add($"FAIL {name}: {message}");
                Console.WriteLine(failures[^1]);
            }
            catch (Exception exception)
            {
                failures.Add($"FAIL {name}: {exception.Message}");
                Console.WriteLine(failures[^1]);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(null);
            }
        }

        var elapsed = DateTimeOffset.UtcNow - startedAt;
        Console.WriteLine();
        Console.WriteLine($"Tests: {testMethods.Length}, passed: {testMethods.Length - failures.Count}, failed: {failures.Count}, time: {elapsed.TotalMilliseconds:N0} ms");
        return failures.Count == 0 ? 0 : 1;
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class TestAttribute : Attribute;

internal static class Assert
{
    public static void True(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message ?? "Expected true, but was false.");
        }
    }

    public static void False(bool condition, string? message = null) =>
        True(!condition, message ?? "Expected false, but was true.");

    public static void Equal<T>(T expected, T actual, string? message = null)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException(message ?? $"Expected <{expected}> but was <{actual}>.");
        }
    }

    public static void NotNull(object? value, string? message = null)
    {
        if (value is null)
        {
            throw new InvalidOperationException(message ?? "Expected a non-null value.");
        }
    }

    public static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string? message = null)
    {
        if (!expected.SequenceEqual(actual))
        {
            throw new InvalidOperationException(message ?? "The sequences are different.");
        }
    }

    public static T Throws<T>(Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch (T exception)
        {
            return exception;
        }

        throw new InvalidOperationException($"Expected exception {typeof(T).Name}.");
    }

    public static async Task<T> ThrowsAsync<T>(Func<Task> action) where T : Exception
    {
        try
        {
            await action();
        }
        catch (T exception)
        {
            return exception;
        }

        throw new InvalidOperationException($"Expected exception {typeof(T).Name}.");
    }
}

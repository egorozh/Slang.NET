namespace Slang.Generator.Models;


internal sealed record Result<TValue>(TValue Value) where TValue : IEquatable<TValue>?;
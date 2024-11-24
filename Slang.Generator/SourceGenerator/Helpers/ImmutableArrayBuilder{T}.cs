using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Slang.Generator.SourceGenerator.Helpers;

/// <summary>
/// A helper type to build sequences of values with pooled buffers.
/// </summary>
/// <typeparam name="T">The type of items to create sequences for.</typeparam>
internal ref struct ImmutableArrayBuilder<T>
{
    /// <summary>
    /// The rented <see cref="Writer"/> instance to use.
    /// </summary>
    private Writer? writer;

    /// <summary>
    /// Creates a <see cref="ImmutableArrayBuilder{T}"/> value with a pooled underlying data writer.
    /// </summary>
    /// <returns>A <see cref="ImmutableArrayBuilder{T}"/> instance to write data to.</returns>
    public static ImmutableArrayBuilder<T> Rent()
    {
        return new ImmutableArrayBuilder<T>(new Writer());
    }

    /// <summary>
    /// Creates a new <see cref="ImmutableArrayBuilder{T}"/> object with the specified parameters.
    /// </summary>
    /// <param name="writer">The target data writer to use.</param>
    private ImmutableArrayBuilder(Writer writer)
    {
        this.writer = writer;
    }


    /// <inheritdoc cref="ImmutableArray{T}.Builder.Add(T)"/>
    public readonly void Add(T item)
    {
        writer!.Add(item);
    }

    /// <summary>
    /// Adds the specified items to the end of the array.
    /// </summary>
    /// <param name="items">The items to add at the end of the array.</param>
    public readonly void AddRange(scoped ReadOnlySpan<T> items)
    {
        writer!.AddRange(items);
    }

    /// <inheritdoc/>
    public readonly override string ToString()
    {
        return writer!.WrittenSpan.ToString();
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        writer?.Dispose();
        writer = null;
    }

    /// <summary>
    /// A class handling the actual buffer writing.
    /// </summary>
    private sealed class Writer : IDisposable
    {
        private T?[]? array;

        /// <summary>
        /// The starting offset within <see cref="array"/>.
        /// </summary>
        private int index;

        /// <summary>
        /// Creates a new <see cref="Writer"/> instance with the specified parameters.
        /// </summary>
        public Writer()
        {
            array = ArrayPool<T?>.Shared.Rent(typeof(T) == typeof(char) ? 1024 : 8);
            index = 0;
        }

        public ReadOnlySpan<T> WrittenSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(array!, 0, index);
        }

        /// <inheritdoc cref="ImmutableArrayBuilder{T}.Add"/>
        public void Add(T value)
        {
            EnsureCapacity(1);

            array![index++] = value;
        }

        /// <inheritdoc cref="ImmutableArrayBuilder{T}.AddRange"/>
        public void AddRange(ReadOnlySpan<T> items)
        {
            EnsureCapacity(items.Length);

            items.CopyTo(array.AsSpan(index)!);

            index += items.Length;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (array is not null)
            {
                ArrayPool<T?>.Shared.Return(array, clearArray: typeof(T) != typeof(char));
            }

            array = null;
        }

        /// <summary>
        /// Ensures that <see cref="array"/> has enough free space to contain a given number of new items.
        /// </summary>
        /// <param name="requestedSize">The minimum number of items to ensure space for in <see cref="array"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int requestedSize)
        {
            if (requestedSize > array!.Length - index)
            {
                ResizeBuffer(requestedSize);
            }
        }

        /// <summary>
        /// Resizes <see cref="array"/> to ensure it can fit the specified number of new items.
        /// </summary>
        /// <param name="sizeHint">The minimum number of items to ensure space for in <see cref="array"/>.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ResizeBuffer(int sizeHint)
        {
            int minimumSize = index + sizeHint;

            T?[] oldArray = array!;
            T?[] newArray = ArrayPool<T?>.Shared.Rent(minimumSize);

            Array.Copy(oldArray, newArray, index);

            array = newArray;

            ArrayPool<T?>.Shared.Return(oldArray, clearArray: typeof(T) != typeof(char));
        }
    }
}
using System.Buffers.Binary;
using System.Collections;
using System.Runtime.InteropServices;

namespace EDMS1.CommandLog.Primitives.Sequences;

/// <summary>
/// Represents a sequential <see cref="IGuidSequence"/>.
/// </summary>
/// <remarks>Copy of: https://github.com/dotnet/efcore/blob/main/src/EFCore/ValueGeneration/SequentialGuidValueGenerator.cs</remarks>
/// <remarks>
/// Initializes a new instance of <see cref="SequentialGuidSequence"/>.
/// </remarks>
/// <param name="initialCounter">Initialization counter.</param>
public class SequentialGuidSequence(long initialCounter) : ISequence<Guid>
{
    private long _counter = initialCounter;
    private readonly bool _isLittleEndian = BitConverter.IsLittleEndian;

    /// <summary>
    /// Initializes a new instance of <see cref="SequentialGuidSequence"/>.
    /// </summary>
    /// <param name="timeProvider">Instance of <see cref="TimeProvider"/>.</param>
    public SequentialGuidSequence(TimeProvider timeProvider)
        : this(timeProvider.GetUtcNow().UtcTicks)
    { }

    /// <inheritdoc/>
    public bool IsRepeat => false;

    /// <inheritdoc/>
    /// <para>
    /// Значение счётчика внутри каждого экземпляра увеличивается атомарно, но 
    /// <b>порядок возврата GUID между потоками не гарантированно монотонный</b>:
    /// поток, получивший большее значение счётчика, может вернуть свой GUID раньше
    /// потока с меньшим значением.
    /// </para>
    /// <para>
    /// Для определения хронологического порядка записей используйте отдельную
    /// временную метку (например, <see cref="DateTime.UtcNow"/>) при их сохранении.
    /// </para>
    public Guid Next()
    {
        var guid = Guid.NewGuid();

        var counter = _isLittleEndian
            ? Interlocked.Increment(ref _counter)
            : BinaryPrimitives.ReverseEndianness(Interlocked.Increment(ref _counter));

        var counterBytes = MemoryMarshal.AsBytes(new ReadOnlySpan<long>(ref counter));

        
        // Guid uses a sequential layout where the first 8 bytes (_a, _b, _c)
        // are subject to byte-swapping on big-endian systems when reading from
        // or writing to a byte array (e.g., via MemoryMarshal or Guid constructors).
        // The remaining 8 bytes (_d through _k) are interpreted as-is,
        // regardless of endianness.
        //
        // Since we only modify the last 8 bytes of the Guid (bytes 8–15),
        // byte order does not affect the result.
        //
        // This allows us to safely use MemoryMarshal.AsBytes to directly access
        // and modify the Guid's underlying bytes without any extra conversions,
        // which also slightly improves performance on big-endian architectures.
        var guidBytes = MemoryMarshal.AsBytes(new Span<Guid>(ref guid));

        guidBytes[8] = counterBytes[1];
        guidBytes[9] = counterBytes[0];
        guidBytes[10] = counterBytes[7];
        guidBytes[11] = counterBytes[6];
        guidBytes[12] = counterBytes[5];
        guidBytes[13] = counterBytes[4];
        guidBytes[14] = counterBytes[3];
        guidBytes[15] = counterBytes[2];

        return guid;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// <b>Внимание:</b> перечислитель бесконечный — он никогда не выдаст 
    /// <c>MoveNext() == false</c>. Использовать с <c>foreach</c>, 
    /// <c>ToList()</c>, <c>ToArray()</c>, <c>Count()</c> и другими операторами, 
    /// требующими полного перебора, нельзя - это приведёт к зависанию или 
    /// <c>OutOfMemoryException</c>.
    /// </para>
    /// </remarks>
    public IEnumerator<Guid> GetEnumerator()
    {
        while (true)
        {
            yield return Next();
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
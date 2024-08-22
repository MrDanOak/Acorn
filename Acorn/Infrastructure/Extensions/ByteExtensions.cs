namespace Acorn.Infrastructure.Extensions;

public static class ByteExtensions
{
    public static ReadOnlyMemory<byte> AsReadOnly(this IEnumerable<byte> bytes)
    {
        byte[] byteArray = bytes.ToArray();
        return new ReadOnlyMemory<byte>(byteArray);
    }
}
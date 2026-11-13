using System.Buffers.Binary;
using System.Text;

namespace ChangeMetadata.Tests;

/// <summary>Small video files for the tests, made on the spot so that none is kept in the repository.</summary>
internal static class SampleVideo
{
    /// <summary>The least an MPEG-4 file can be: its type, and a movie header with no tracks. Enough to carry a title.</summary>
    public static byte[] Mp4()
    {
        var fileType = Box("ftyp", Encoding.ASCII.GetBytes("isom"), Numbers(0x200), Encoding.ASCII.GetBytes("isommp41"));
        var movieHeader = Box(
            "mvhd",
            Numbers(0, 0, 0, 1000, 0, 0x00010000),                         // version and flags, created, modified, timescale, duration, rate
            [0x01, 0x00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],                    // volume, reserved
            Numbers(0x00010000, 0, 0, 0, 0x00010000, 0, 0, 0, 0x40000000), // the identity matrix
            Numbers(0, 0, 0, 0, 0, 0, 1));                                 // pre-defined, next track id
        return [.. fileType, .. Box("moov", movieHeader)];
    }

    /// <summary>What a file with a video's name holds when it is no video at all.</summary>
    public static byte[] Garbage() => "this is not a video"u8.ToArray();

    private static byte[] Box(string type, params byte[][] parts)
    {
        var body = parts.SelectMany(part => part).ToArray();
        var box = new byte[8 + body.Length];
        BinaryPrimitives.WriteUInt32BigEndian(box, (uint)box.Length);
        Encoding.ASCII.GetBytes(type).CopyTo(box, 4);
        body.CopyTo(box, 8);
        return box;
    }

    private static byte[] Numbers(params uint[] values)
    {
        var bytes = new byte[values.Length * 4];
        for (var i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(i * 4), values[i]);
        }

        return bytes;
    }
}

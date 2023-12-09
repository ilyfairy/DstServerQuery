using System.Buffers;

namespace Ilyfairy.DstServerQuery.Web.Helpers;

public static class Helper
{
    public static string GetRandomColor(int minGray, int maxGray)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minGray);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxGray, 255);

        var temp = ArrayPool<byte>.Shared.Rent(3);
        int gray;

        do
        {
            Random.Shared.NextBytes(temp.AsSpan()[0..3]);
            gray = (int)(0.299f * temp[0] + 0.587f * temp[1] + 0.114f * temp[2]);
        } while (gray < minGray || gray > maxGray);

        var colorHex = $"{temp[0]:X2}{temp[1]:X2}{temp[2]:X2}";
        ArrayPool<byte>.Shared.Return(temp);

        return colorHex;
    }

}

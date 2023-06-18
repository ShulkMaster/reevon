using System.Text;

namespace Reevon.Api.Extension;

public static class StringBuilderExtension
{
    public static void Pop(this StringBuilder sb)
    {
        if (sb.Length == 0) return;
        sb.Remove(sb.Length - 1, 1);
    }
}
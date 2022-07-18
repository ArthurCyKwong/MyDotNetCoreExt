using System.Text;
using System.Linq;

namespace ObjectExt;
public static class StringExt{
    public static string TrimByByte(this string inStr, int StartIndex, int length  = 0, Encoding encoding = null){
        if(inStr.IsNullOrEmpty()) return inStr;
        if (encoding is null) encoding = Encoding.UTF8;
        
        IEnumerable<byte> strByte = encoding.GetBytes(SplitedStr);
        strByte =strByte.Skip(StartIndex);
        if(length > 0)
            strByte = strByte.Take(length);
        
        return encoding.GetString(strByte.ToArray());
    }
}
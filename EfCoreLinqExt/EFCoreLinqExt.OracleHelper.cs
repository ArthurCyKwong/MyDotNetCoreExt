using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;

namespace EfCoreLinqExt;
public static partial class EFCoreLinqExt
{
    public const int OracleSearchLimit = 1000;
    public static List<T[]> SplitOracleSearchList<T>(this T[] inList, int SplitLimit = OracleSearchLimit)
    {
        List<T[]> result = new List<T[]>();
        for (int i = 0; i < inList.Length; i += SplitLimit)
        {
            result.Add(inList.Skip(i).Take(Math.Min(SplitLimit, inList.Length - i)).ToArray());
        }
        return result;
    }

    public static List<T[]> SplitOracleSearchList<T>(this IEnumerable<T> inList, int SplitLimit = OracleSearchLimit)
    {
        List<T[]> result = new List<T[]>();
        for (int i = 0; i < inList.Count(); i += SplitLimit)
        {
            result.Add(inList.Skip(i).Take(Math.Min(SplitLimit, inList.Count() - i)).ToArray());
        }
        return result;
    }

    public static string GetTableName<T>()
    {
        var customeAttributes = typeof(T).GetTypeInfo().GetCustomAttribute<TableAttribute>();
        if (customeAttributes != null)
        {
            return (string.IsNullOrEmpty(customeAttributes.Schema) ? "" : customeAttributes.Schema + ".") + customeAttributes.Name;
        }
        return typeof(T).Name;
    }

    public static string GetFieldName<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> property)
    {
        MemberExpression memberExp = property.Body as MemberExpression;
        if (memberExp == null)
            return "";

        PropertyInfo propInfo = memberExp.Member as PropertyInfo;
        if (propInfo == null)
            return "";

        if (typeof(TSource) != propInfo.ReflectedType && typeof(TSource).IsSubclassOf(propInfo.ReflectedType))
        {
            return "";
        }

        var columnAttr = propInfo.GetCustomAttribute<ColumnAttribute>();
        return columnAttr.Name;
    }
}
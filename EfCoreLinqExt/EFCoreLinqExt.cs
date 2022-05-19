using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;
using EnumerableExtension;

namespace EfCoreLinqExt;
public static partial class EFCoreLinqExt
{
    private static Dictionary<Type, Dictionary<Type, PropertyInfo>> TableSetDictionary;
    public static DbSet<Table> GetTable<Table, Context>(this Context dbcontext) where Context : DbContext where Table : class
    {
        if (TableSetDictionary == null)
            TableSetDictionary = new Dictionary<Type, Dictionary<Type, PropertyInfo>>();
        if (!TableSetDictionary.ContainsKey(typeof(Context)))
        {
            var contextDbSetDict = typeof(Context).GetProperties().Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)).ToDictionary(x => x.PropertyType, y => y);
            TableSetDictionary.Add(typeof(Context), contextDbSetDict);
        }

        var contextTableSet = TableSetDictionary[typeof(Context)];
        if (!contextTableSet.ContainsKey(typeof(DbSet<Table>)))
            return null;

        return (DbSet<Table>)contextTableSet[typeof(DbSet<Table>)].GetValue(dbcontext);
    }


    public static async Task<List<TSource>> RetrieveQueryToListAsync<TSource>(this IEnumerable<IQueryable<TSource>> queryList, CancellationToken cancellationToken = default(CancellationToken))
    {
        List<TSource> Result = new List<TSource>();
        foreach (var query in queryList)
        {
            var tempResult = await query.ToListAsync(cancellationToken);
            if (!tempResult.IsNullOrEmpty())
                Result.AddRange(tempResult);
        }
        return Result;
    }

    public static async Task<List<TResult>> RetrieveQueryToListAsync<TSource, TResult>(this IEnumerable<IQueryable<TSource>> queryList, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
    {
        List<TResult> Result = new List<TResult>();
        foreach (var query in queryList)
        {
            var tempResult = await query.Select(selector).ToListAsync(cancellationToken);
            if (!tempResult.IsNullOrEmpty())
                Result.AddRange(tempResult);
        }
        return Result;
    }


    public delegate TField FieldSelector<TSoruce, TField>(TSoruce DbTable);
    public static IEnumerable<IQueryable<TSource>> InList<TSource, TFilter>(this IEnumerable<IQueryable<TSource>> queryList, IEnumerable<TFilter> filterList, Expression<Func<TSource, TFilter>> fieldSelector, int SplitLimit = OracleSearchLimit)
    {
        var SplitedFilterList = SplitOracleSearchList(filterList, SplitLimit);
        // Field Selector
        MemberExpression body = fieldSelector.Body as MemberExpression;
        if (body == null) return queryList;

        ParameterExpression paramSource = Expression.Parameter(typeof(TSource), "field");
        Expression DataselectExpression = Expression.PropertyOrField(paramSource, body.Member.Name);


        var result = new List<IQueryable<TSource>>();
        foreach (var splitedFilter in SplitedFilterList)
        {
            //List Init Expression
            //Expression listfilter = Expression.ListBind(,)
            Expression paramFilter = Expression.Constant(splitedFilter, typeof(TFilter[]));
            MethodCallExpression method = Expression.Call(typeof(Enumerable), "Contains", new Type[] { typeof(TFilter) }, paramFilter, DataselectExpression);
            foreach (var query in queryList)
            {
                result.Add(query.Where(Expression.Lambda<Func<TSource, bool>>(method, paramSource)));
            }
        }

        return result;
    }

    public static IEnumerable<IQueryable<TSource>> GeneralQuery<TSource, TFilter>(this IEnumerable<IQueryable<TSource>> queryList, TFilter filter, ExpressionType expressionType, Expression<Func<TSource, TFilter>> fieldSelector)
    {
        //FieldSelector
        MemberExpression body = fieldSelector.Body as MemberExpression;
        if (body == null) return queryList;

        ParameterExpression paramSource = Expression.Parameter(typeof(TSource), "field");
        Expression DataSelectionExpression = Expression.PropertyOrField(paramSource, body.Member.Name);

        // Filter Value Expression
        Expression paramFilter = Expression.Constant(filter, typeof(TFilter));
        Expression expression = Expression.MakeBinary(expressionType, paramFilter, DataSelectionExpression);

        return queryList.Select(x => x.Where(Expression.Lambda<Func<TSource, bool>>(expression, paramSource)));
    }

    public static IEnumerable<IQueryable<TSource>> Include<TSource, TProperties>(this IEnumerable<IQueryable<TSource>> queryList, Expression<Func<TSource, TProperties>> includeField) where TSource : class
    {
        return queryList.Select(x => x.Include(includeField));
    }
    public static IEnumerable<IQueryable<TSource>> Where<TSource>(this IEnumerable<IQueryable<TSource>> queryList, Expression<Func<TSource, bool>> whereClause)
    {
        return queryList.Select(x => x.Where(whereClause));
    }
}

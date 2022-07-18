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
    public static IEnumerable<IQueryable<TSource>> IsNull<TSource, TFilter>(this IEnumerable<IQueryable<TSource>> queryList, bool isNull, Expression<Func<TSource, TFilter>> fieldSelector)
    {
        //FieldSelector
        MemberExpression body = fieldSelector.Body as MemberExpression;
        if (body == null) return queryList;

        ParameterExpression paramSource = Expression.Parameter(typeof(TSource), "field");
        Expression DataSelectionExpression = Expression.PropertyOrField(paramSource, body.Member.Name);

        // Filter Value Expression
        Expression paramFilter = Expression.Constant(null, typeof(TFilter));
        Expression expression = Expression.MakeBinary(isNull ? ExpressionType.Equal : ExpressionType.NotEqual, paramFilter, DataSelectionExpression);

        return queryList.Select(x => x.Where(Expression.Lambda<Func<TSource, bool>>(expression, paramSource)));
    }

    private static IEnumerable<IQueryable<TSource>> GeneralQuery<TSource>(this IEnumerable<IQueryable<TSource>> queryList, string? filter, ExpressionType expressionType, Expression<Func<TSource, string?>> fieldSelector)
    {
        switch (expressionType)
        {
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.LessThan:
                // Field Selector
                MemberExpression body = fieldSelector.Body as MemberExpression;
                if (body == null) return queryList;

                ParameterExpression paramSource = Expression.Parameter(typeof(TSource), "field");
                Expression DataselectExpression = Expression.PropertyOrField(paramSource, body.Member.Name);
                var CompareMethod = typeof(string).GetMethod("CompareTo",new[]{typeof(string)});
                Expression paramFilter = Expression.Constant(filter, typeof(string));
                MethodCallExpression method = Expression.Call(paramFilter, CompareMethod, DataselectExpression);

                Expression Zero = Expression.Constant(0, typeof(int));
                Expression expression = Expression.MakeBinary(expressionType, method, Zero);

                return queryList.Select(x=>x.Where(Expression.Lambda<Func<TSource, bool>>(expression, paramSource)));
                break;
            default:
                return queryList.GeneralQuery<TSource, string?>(filter, expressionType, fieldSelector);
        }
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
    public static IEnumerable<IQueryable<TSource>> Filter<TSource>(this IEnumerable<IQueryable<TSource>> queryList, Expression<Func<TSource, string?>> fieldSelector, EFCoreFilter<string?> filter)
    {
        if (!(filter.GreaterThan is null))
            queryList = queryList.GeneralQuery(filter.GreaterThan, ExpressionType.LessThan, fieldSelector);
        if (!(filter.GreaterThanOrEqual is null))
            queryList = queryList.GeneralQuery(filter.GreaterThanOrEqual, ExpressionType.LessThanOrEqual, fieldSelector);
        if (!(filter.LessThan is null))
            queryList = queryList.GeneralQuery(filter.LessThan, ExpressionType.GreaterThan, fieldSelector);
        if (!(filter.LessThanOrEqual is null))
            queryList = queryList.GeneralQuery(filter.LessThanOrEqual, ExpressionType.GreaterThanOrEqual, fieldSelector);
        if (!(filter.Equal is null))
            queryList = queryList.GeneralQuery(filter.Equal, ExpressionType.Equal, fieldSelector);
        if (!filter.In.IsNullOrEmpty())
            queryList = queryList.InList(filter.In, fieldSelector);
        if (filter.isNull.HasValue)
            queryList = queryList.IsNull(filter.isNull.Value, fieldSelector);
        return queryList;
    }
    public static IEnumerable<IQueryable<TSource>> Filter<TSource, TFilter>(this IEnumerable<IQueryable<TSource>> queryList, Expression<Func<TSource, TFilter>> fieldSelector, EFCoreFilter<TFilter> filter)
    {
        if (!(filter.GreaterThan is null))
            queryList = queryList.GeneralQuery(filter.GreaterThan, ExpressionType.LessThan, fieldSelector);
        if (!(filter.GreaterThanOrEqual is null))
            queryList = queryList.GeneralQuery(filter.GreaterThanOrEqual, ExpressionType.LessThanOrEqual, fieldSelector);
        if (!(filter.LessThan is null))
            queryList = queryList.GeneralQuery(filter.LessThan, ExpressionType.GreaterThan, fieldSelector);
        if (!(filter.LessThanOrEqual is null))
            queryList = queryList.GeneralQuery(filter.LessThanOrEqual, ExpressionType.GreaterThanOrEqual, fieldSelector);
        if (!(filter.Equal is null))
            queryList = queryList.GeneralQuery(filter.Equal, ExpressionType.Equal, fieldSelector);
        if (!filter.In.IsNullOrEmpty())
            queryList = queryList.InList(filter.In, fieldSelector);
        if (filter.isNull.HasValue)
            queryList = queryList.IsNull(filter.isNull.Value, fieldSelector);
        return queryList;
    }

    public static IEnumerable<IQueryable<TSource>> Filter<TSource, TFilter>(this IEnumerable<IQueryable<TSource>> queryList, Expression<Func<TSource, TFilter>> fieldSelector, NullableEFCoreFilter<TFilter> filter) where TFilter : struct
    {
        if (!(filter.GreaterThan is null))
            queryList = queryList.GeneralQuery(filter.GreaterThan.Value, ExpressionType.LessThan, fieldSelector);
        if (!(filter.GreaterThanOrEqual is null))
            queryList = queryList.GeneralQuery(filter.GreaterThanOrEqual.Value, ExpressionType.LessThanOrEqual, fieldSelector);
        if (!(filter.LessThan is null))
            queryList = queryList.GeneralQuery(filter.LessThan.Value, ExpressionType.GreaterThan, fieldSelector);
        if (!(filter.LessThanOrEqual is null))
            queryList = queryList.GeneralQuery(filter.LessThanOrEqual.Value, ExpressionType.GreaterThanOrEqual, fieldSelector);
        if (!(filter.Equal is null))
            queryList = queryList.GeneralQuery(filter.Equal.Value, ExpressionType.Equal, fieldSelector);
        if (!filter.In.IsNullOrEmpty())
            queryList = queryList.InList(filter.In.Select(x => x.Value), fieldSelector);
        if (filter.isNull.HasValue)
            queryList = queryList.IsNull(filter.isNull.Value, fieldSelector);
        return queryList;
    }
}

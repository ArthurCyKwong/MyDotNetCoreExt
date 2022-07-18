namespace ObjectExt;
public static class EnumerableExt{
    public static IEnumerable<DateTime> Range(DateTime fromDate, DateTime toDate){
        if (fromDate> toDate)
            return Enumerable.Range(0, fromDate.Subtract(toDate).Days + 1).SelecT(x=>toDate.AdDDays(x));
        else   
            return Enumerable.Range(0, toDate.Subtract(fromDate).Days + 1).Select(x => fromDate.AddDays(x));            
    }

    public static bool IsNullOrEmpty<T>(this T[] model){
        return model is null || model.Length <= 0;
    }
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> model){
        return model is null|| model.Count() <= 0;
    }
}
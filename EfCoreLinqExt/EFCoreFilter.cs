using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EfCoreLinqExt;
public class NullableEFCoreFilter<TFilter>: EFCoreFilter<Nullable<TFilter>> where TFilter : struct{
    
}

public class EFCoreFilter<TFilter>{    
    public TFilter GreaterThan{get;set;}    
    public TFilter LessThan{get;set;}
    public TFilter GreaterThanOrEqual{get;set;}
    public TFilter LessThanOrEqual{get;set;}
    public TFilter Equal{get;set;}
    public bool? isNull{get;set;}
    public TFilter[]? In{get;set;}
}
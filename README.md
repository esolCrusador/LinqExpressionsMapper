# LinqExpressionsMapper
**LinqExpressionsMapper** is LINQ extensions library for EntityFramework and other .NET LINQ ORMs.
Here is sample project [LinqExpressionsMapper.Samples](https://github.com/esolCrusador/LinqExpressionsMapper.Samples)
You can find more info in [wiki](https://github.com/esolCrusador/LinqExpressionsMapper/wiki).
Project is available on [nuget.org](https://www.nuget.org/packages/LinqExpressionsMapper).

## Inerfaces
1. **Projection Expression Factory** 
<br/>
`ISelectExpression<TSource, TDest>`
<br/>
 — Class implementing this interface implements `Expression<Func<TSource, TDest>> GetExpression()` method and in fact is factory of projection expression. If this method is used via _Mapper_ Class its result executed only _once_ because it is cached.
`IDynamicSelectExpression<TSource, TDest>`
<br/>
 — Class implementing this interface also implements `Expression<Func<TSource, TDest>> GetExpression()`. If this method is used via _Mapper_ Class than only _Factory_ _Delegate_ is cached.
<br/>
`ISelectExpression<TSource, TDest, TParam>`
<br/>
 — Class implementing this interface implements `Expression<Func<TSource, TDest>> GetExpression(TParam param)` method and in fact is factory of projection expression. If this method is used via _Mapper_ Class than _Factory_ _Delegate_ is cached. And this Expression can be received from _Mapper_ via `WithParam(param)` methods.
2. **Properties Mapping Delegate**
`IPropertiesMapper<TSource, TDest>`
 — Class implementing this interface implements `void MapProperties(TSource source, TDest dest)` method. If this method is used via _Mapper_ Class than mapping _Delegate_ is cached.

## Mapper _Cache_ API

1. **Mapper.Register()** 
<br/>
`Mapper.Register(/*IPropertiesMapper<TSource, TDest>*/mapper);` or `Mapper.Register(/*ISelectExpression<TSource, TDest>*/mapper);` or `Mapper.Register(/*IDynamicSelectExpression<TSource, TDest>*/mapper);`
<br/>
 — Registers PropertiesMapper or SelectExpression in _Cache_, registered Mapping Delegates and Projection Expression Factories can be used later.
2. **Mapper.RegisterAll()**
<br/>
`Mapper.RegisterAll(/*IMultipleMappings*/mapper);`
<br/>
 — Registers all implemented by _mapper_ _Type_ `IPropertiesMapper<TSource,TDest>`,<br/> `ISelectExpression<TSource, TSelect>`, ... interfaces in _Cache_.
3. **Mapper...GetExpression()**
<br/>
`var expr = Mapper.From<TSource>().To<TDest>().Using<TMapper>().GetExpression();` or
<br/>
`Expression<Func<TSrc, TDest>> e = Mapper.From<TSrc>().To<TDest>().Using<TSelect>();`
<br/> 
 — Gets Expression from _Cache_ or receives _Expression_ _Factory_ from _TSelect_ instance. _TSelect_ should implement `ISelectExpression<TSource, TDest>`.
<br/>
`var expr = Mapper.From<TSource>().To<TDest>().GetExpression();` or 
<br/>
`Expression<Func<TSource, TDest>> expr = Mapper.From<TSource>().To<TDest>();`
<br/>
 — Gets Expression from _Cache_ or tries to receive _Expression_ _Factory_ from _TDest_ instance (if `ISelectExpression<TSource, TDest>` is implemented).
4. **Mapper...Map()**
<br/>
`var dest = Mapper.From<TSource>(source).To<TDest>().Using<TMapper>().Map();` or
<br/>
`TDest dest = Mapper.From<TSource>(source).To<TDest>().Using<TMapper>();` or
<br/>
`Mapper.From<TSource>(source).To<TDest>(dest).Using<TMapper>().Map();`
<br/> 
 — Maps source to dest. Gets mapping Delegate from _Cache_ or receives it _Delegate_ from _TMapper_ instance. _TMapper_ should implement `IPropertiesMapper<TSource, TDest>`.
<br/>
`var dest = Mapper.From<TSource>(source).To<TDest>().Map();` or
<br/>
`TDest dest = Mapper.From<TSource>(source).To<TDest>();` or
<br/>
`Mapper.From<TSource>(source).To<TDest>(dest).Map();`
<br/>
 — Maps source to dest. Gets mapping Delegate from _Cache_ or tries to receive it from _TSource_ or _TDest_ instance (if `IPropertiesMapper<TSource, TDest>` is implemented).
5. **Enumerable.Map().To()**
<br/>
`IEnumerable<TDest> result = enumerable.Map().To<TDest>(m=>m.Using<TMapper>());`
<br/>
 — Maps IEnumerable of sources to IEnumerables of destanations. Gets mapping Delegate from _Cache_ or receives it from _TMapper_ instance. _TMapper_ should implement `IPropertiesMapper<TSource, TDest>`.
<br/>
`IEnumerable<TDest> result = enumerable.Map().To<TDest>();`
<br/>
 — Maps IEnumerable of sources to IEnumerables of destanations. Gets mapping Delegate from _Cache_ or tries to receive it from _TSource_ or _TDest_ instance (if `IPropertiesMapper<TSource, TDest>` is implemented).
6. **Queryable.Project().To()**
<br/>
`IQueryable<TDest> reult = queryable.Project().To<TDest>(p=>p.Using<TSelect>());`
<br/>
 — Projects IQueryable of sources to IQueryable of destanations. Gets Projection expression from _Cache_ or receives it from _TSelect_ instance.  _TSelect_ should implement `ISelectExpression<TSource, TDest>`.
<br/>
`IQueryable<TDest> result = queryable.Project().To<TDest>();`
<br/>
 — Projects IQueryable of sources to IQueryable of destanations. Gets Projection expression from _Cache_ or tries to receive it from _TDest_ instance (if `ISelectExpression<TSource, TDest>` is implemented).


## Expressions Extensions API
**Example:**

    Expression<Func<Student, StudentWithCoursesModel>> select = student =>
        new StudentWithCoursesModel
        {
            MaxGade = student.Enrollments.Max(e => e.Grade),
            Courses = Mapper.From<Course>().To<CourseBaseModel>().GetExpression()
                .InvokeEnumerable(student.Enrollments.Select(er => er.Course)),
        };
    select = select.ApplyExpressions();

    select = select.AddMemberInit(
        s => s.Enrollments.Where(e => e.Grade <= Grade.C).Select(e => e.Course),
        s => s.PositiveGradedCourses,
        Mapper.From<Course>().To<CourseBaseModel>().GetExpression());

    select = select.InheritInit(Mapper.From<Student>().To<StudentBaseModel>().GetExpression());

**Result**

    //select = student => new StudentWithCoursesModel
    //{
    //    StudentId = student.ID,
    //    FullName = student.FirstMidName + " " + student.LastName,
    //    MaxGade = student.Enrollments.Max(e => e.Grade),

    //    Courses = student.Enrollments.Select(er => er.Course)
    //        .Select(course => new CourseBaseModel
    //        {
    //            CourseId = course.CourseID,
    //            CourseName = course.Title,
    //            Credits = course.Credits
    //        }),
    //    PositiveGradedCourses = student.Enrollments.Where(e => e.Grade <= Grade.C)
    //        .Select(e => e.Course)
    //        .Select(course => new CourseBaseModel
    //        {
    //            CourseId = course.CourseID,
    //            CourseName = course.Title,
    //            Credits = course.Credits
    //        })
    //};

1. **InheritInit**
<br/>
`select = select.InheritInit(baseInit);`
<br/>
 — Combines two Projection expressions into one. Projection of child class is extended with Projection of entity (base entity) on base class. Projection of members initialized in `baseInit` expression and not not initialized in init expression will be copied. For example: `StudentId = student.ID` from `baseExpression`.
2. **AddMemberInit**
<br/>
`    select = select.AddMemberInit(`
<br/>
`        s => s.Enrollments.Select(e => e.Course),/*sourceMember*/`
<br/>
`        s => s.PositiveGradedCourses,/*member*/`
<br/>
`        Mapper.From<Course>().To<CourseBaseModel>().GetExpression()/*memberInit*/);`
 — Adds Target _Single_(_Enumerable_) `member` initialization from _Single_(_Enumerable_) `sourceMember` using `memberInit` projection expression.
3. **ApplyExpressions** expressions **Invoke**, **InvokeEnumerable**

<br/>

    select = student => new StudentWithCoursesModel
    {
        Courses = student.Enrollments.Select(er => memberInit.Invoke(er.Course))
    };
    select = select.ApplyExpressions();
 — `expr.Invoke(param)`(`expr.InvokeEnumerable(enumerableParam)`) creates placeholder for `expr` invokation. It means that when `ApplyExpressions()` method is called all `Invoke`, `InvokeEnumerable` methods are replaced with `expr` body with `param` argument.
 
 # LinqExpressionsMapper
**LinqExpressionsMapper** is LINQ extensions library for EntityFramework and other .NET LINQ ORMs.
Here is sample project [LinqExpressionsMapper.Samples](https://github.com/esolCrusador/LinqExpressionsMapper.Samples)
You can find more info in [wiki](https://github.com/esolCrusador/LinqExpressionsMapper/wiki).
Project is available on [nuget.org](https://www.nuget.org/packages/LinqExpressionsMapper).

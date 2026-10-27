## Compiled_and_inline_emit_identical_sql_for_update

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .Where(u => u.Id == @id)
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledSync_ExecuteUpdate_with_TagWith_updates_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledAsync_ExecuteUpdate_updates_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledAsync_ExecuteUpdate_with_filter_updates_only_target

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .Where(u => u.Id == @id)
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledSync_ExecuteUpdate_with_filter_updates_only_target

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .Where(u => u.Id == @id)
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledSync_ExecuteUpdate_updates_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledAsync_body_spelled_ExecuteDeleteAsync_removes_only_target

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .Where(u => u.Id == @id)
    .ExecuteDeleteAsync(CancellationToken)' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledAsync_body_ExecuteDelete_removes_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.ArgumentException
- InnerException is null: True
- Message:
  ```
  Expression of type 'System.Int32' cannot be used for return type 'System.Threading.Tasks.Task`1[System.Int32]'
  ```
- First three stack frames:
  ```
  at System.Linq.Expressions.Expression.ValidateLambdaArgs(Type delegateType, Expression& body, ReadOnlyCollection`1 parameters, String paramName)
  at System.Linq.Expressions.Expression.Lambda[TDelegate](Expression body, String name, Boolean tailCall, IEnumerable`1 parameters)
  at Microsoft.EntityFrameworkCore.Query.QueryCompilationContext.CreateQueryExecutorExpression[TResult](Expression query)
  ```

## Compiled_and_inline_emit_identical_sql_for_update

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .Where(u => u.Id == @id)
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledSync_ExecuteUpdate_with_TagWith_updates_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledAsync_ExecuteUpdate_updates_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledAsync_ExecuteUpdate_with_filter_updates_only_target

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .Where(u => u.Id == @id)
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledSync_ExecuteUpdate_with_filter_updates_only_target

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .Where(u => u.Id == @id)
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledSync_ExecuteUpdate_updates_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledAsync_body_spelled_ExecuteDeleteAsync_removes_only_target

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .Where(u => u.Id == @id)
    .ExecuteDeleteAsync(CancellationToken)' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledAsync_body_ExecuteDelete_removes_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.ArgumentException
- InnerException is null: True
- Message:
  ```
  Expression of type 'System.Int32' cannot be used for return type 'System.Threading.Tasks.Task`1[System.Int32]'
  ```
- First three stack frames:
  ```
  at System.Linq.Expressions.Expression.ValidateLambdaArgs(Type delegateType, Expression& body, ReadOnlyCollection`1 parameters, String paramName)
  at System.Linq.Expressions.Expression.Lambda[TDelegate](Expression body, String name, Boolean tailCall, IEnumerable`1 parameters)
  at Microsoft.EntityFrameworkCore.Query.QueryCompilationContext.CreateQueryExecutorExpression[TResult](Expression query)
  ```

## CompiledAsync_body_ExecuteDelete_removes_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.ArgumentException
- InnerException is null: True
- Message:
  ```
  Expression of type 'System.Int32' cannot be used for return type 'System.Threading.Tasks.Task`1[System.Int32]'
  ```
- First three stack frames:
  ```
  at System.Linq.Expressions.Expression.ValidateLambdaArgs(Type delegateType, Expression& body, ReadOnlyCollection`1 parameters, String paramName)
  at System.Linq.Expressions.Expression.Lambda[TDelegate](Expression body, String name, Boolean tailCall, IEnumerable`1 parameters)
  at Microsoft.EntityFrameworkCore.Query.QueryCompilationContext.CreateQueryExecutorExpression[TResult](Expression query)
  ```

## CompiledAsync_body_ExecuteDelete_removes_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.ArgumentException
- InnerException is null: True
- Message:
  ```
  Expression of type 'System.Int32' cannot be used for return type 'System.Threading.Tasks.Task`1[System.Int32]'
  ```
- First three stack frames:
  ```
  at System.Linq.Expressions.Expression.ValidateLambdaArgs(Type delegateType, Expression& body, ReadOnlyCollection`1 parameters, String paramName)
  at System.Linq.Expressions.Expression.Lambda[TDelegate](Expression body, String name, Boolean tailCall, IEnumerable`1 parameters)
  at Microsoft.EntityFrameworkCore.Query.QueryCompilationContext.CreateQueryExecutorExpression[TResult](Expression query)
  ```

## Compiled_and_inline_emit_identical_sql_for_update

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .Where(u => u.Id == @id)
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledSync_ExecuteUpdate_with_TagWith_updates_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledAsync_ExecuteUpdate_updates_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledAsync_ExecuteUpdate_with_filter_updates_only_target

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .Where(u => u.Id == @id)
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledSync_ExecuteUpdate_with_filter_updates_only_target

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .Where(u => u.Id == @id)
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledSync_ExecuteUpdate_updates_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .ExecuteUpdate(s => s.SetProperty<DateTime>(
        propertyExpression: u => u.LastActive, 
        valueExpression: 6/15/2024 12:00:00 PM))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledAsync_body_spelled_ExecuteDeleteAsync_removes_only_target

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.InvalidOperationException
- InnerException is null: True
- Message:
  ```
  The LINQ expression 'DbSet<User>()
    .Where(u => u.Id == @id)
    .ExecuteDeleteAsync(CancellationToken)' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
  ```
- First three stack frames:
  ```
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression)
  at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query)
  at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query)
  ```

## CompiledAsync_body_ExecuteDelete_removes_all_rows

- Provider: SQLite
- EF Core assembly version: 10.0.9
- Runtime: .NET 10.0.13
- OS: Ubuntu 24.04.3 LTS (kernel: Unix 7.0.0.34)
- Exception type: System.ArgumentException
- InnerException is null: True
- Message:
  ```
  Expression of type 'System.Int32' cannot be used for return type 'System.Threading.Tasks.Task`1[System.Int32]'
  ```
- First three stack frames:
  ```
  at System.Linq.Expressions.Expression.ValidateLambdaArgs(Type delegateType, Expression& body, ReadOnlyCollection`1 parameters, String paramName)
  at System.Linq.Expressions.Expression.Lambda[TDelegate](Expression body, String name, Boolean tailCall, IEnumerable`1 parameters)
  at Microsoft.EntityFrameworkCore.Query.QueryCompilationContext.CreateQueryExecutorExpression[TResult](Expression query)
  ```


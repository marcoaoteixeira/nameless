// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1827:Do not use Count() or LongCount() when Any() can be used", Justification = "NHibernate loads the entity if we use .Any(); .Count() just translates the query to a COUNT statement.", Scope = "member", Target = "~M:Nameless.WebApplication.Web.Persistence.Repository.DeleteAsync``1(``0,System.Threading.CancellationToken)~System.Threading.Tasks.Task{System.Boolean}")]
[assembly: SuppressMessage("Performance", "CA1827:Do not use Count() or LongCount() when Any() can be used", Justification = "NHibernate loads the entity if we use .Any(); .Count() just translates the query to a COUNT statement.", Scope = "member", Target = "~M:Nameless.WebApplication.Web.Persistence.Repository.ExistsAsync``1(System.Linq.Expressions.Expression{System.Func{``0,System.Boolean}},System.Threading.CancellationToken)~System.Threading.Tasks.Task{System.Boolean}")]

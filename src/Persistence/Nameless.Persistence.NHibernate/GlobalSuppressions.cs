// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1827:Do not use Count() or LongCount() when Any() can be used", Justification = "NHibernate loads all fields if we call .Any(). Instead, use .Count() to check how many records exists with the given query.", Scope = "member", Target = "~M:Nameless.Persistence.NHibernate.Writer.Exists``1(NHibernate.ISession,System.Linq.Expressions.Expression{System.Func{``0,System.Boolean}})~System.Boolean")]

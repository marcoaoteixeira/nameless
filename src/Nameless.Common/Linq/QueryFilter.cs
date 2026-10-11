using System.Linq.Expressions;

namespace Nameless.Linq;

/// <summary>
///     A fluent builder for composing an <see cref="Expression{TDelegate}"/>
///     predicate and applying it to an <see cref="IQueryable{T}"/>.
///     Use <see cref="And"/> and <see cref="Or"/> to incrementally combine
///     predicate expressions; call <see cref="Apply(IQueryable{T})"/> to
///     apply the resulting predicate to a query.
/// </summary>
/// <typeparam name="T">
///     The element type of the query the filter will be applied to.
/// </typeparam>
public sealed class QueryFilter<T> {
    // Backing expression that represents the composed predicate.
    // When null, no filter has been added.
    private Expression<Func<T, bool>>? _predicate;

    /// <summary>
    ///     Adds the specified <paramref name="predicate"/> to the current
    ///     filter by combining it with the existing predicate using a
    ///     logical <c>AND</c>. If no predicate has been added yet, the
    ///     provided one becomes the current predicate.
    /// </summary>
    /// <param name="predicate">
    ///     The predicate expression to add.
    /// </param>
    /// <param name="apply">
    ///     An optional function that determines whether the predicate
    ///     should be applied. If <c>null</c> or returns <c>true</c>, the
    ///     predicate is applied; if it returns <c>false</c>, the call
    ///     is ignored.
    /// </param>
    /// <returns>
    ///     The same <see cref="QueryFilter{T}"/> instance to allow fluent
    ///     chaining.
    /// </returns>
    public QueryFilter<T> And(Expression<Func<T, bool>> predicate, Func<bool>? apply = null) {
        if (apply is not null && !apply()) {
            return this;
        }

        _predicate = _predicate is null
            ? predicate
            : _predicate.And(predicate);

        return this;
    }

    /// <summary>
    ///     Adds the specified <paramref name="predicate"/> to the current
    ///     filter by combining it with the existing predicate using a logical
    ///     <c>OR</c>. If no predicate has been added yet, the provided one
    ///     becomes the current predicate.
    /// </summary>
    /// <param name="predicate">
    ///     The predicate expression to add.
    /// </param>
    /// <param name="apply">
    ///     An optional function that determines whether the predicate should
    ///     be applied. If <c>null</c> or returns <c>true</c>, the predicate
    ///     is applied; if it returns <c>false</c>, the call is ignored.
    /// </param>
    /// <returns>
    ///     The same <see cref="QueryFilter{T}"/> instance to allow fluent
    ///     chaining.
    /// </returns>
    public QueryFilter<T> Or(Expression<Func<T, bool>> predicate, Func<bool>? apply = null) {
        if (apply is not null && !apply()) {
            return this;
        }

        _predicate = _predicate is null
            ? predicate
            : _predicate.Or(predicate);

        return this;
    }

    /// <summary>
    ///     Applies the composed predicate to the provided
    ///     <paramref name="query"/>. If no predicate has been composed, the
    ///     original query is returned unchanged.
    /// </summary>
    /// <param name="query">
    ///     The query to which the filter should be applied.
    /// </param>
    /// <returns>
    ///     An <see cref="IQueryable{T}"/> that has the composed predicate
    ///     applied via <c>Where</c>, or the original <paramref name="query"/>
    ///     if no predicate exists.
    /// </returns>
    public IQueryable<T> Apply(IQueryable<T> query) {
        return _predicate is null ? query : query.Where(_predicate);
    }
}


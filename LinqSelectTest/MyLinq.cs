using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace LinqSelectTest
{
    public static partial class Enumerable
    {
        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return new SelectEnumerableIterator<TSource, TResult>(source, selector);
        }

        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
        {
            var list = new List<TSource>();
            foreach (var item in source)
            {
                list.Add(item);
            }

            return list;
        }
    }

    /// <summary>
    /// An iterator that maps each item of an <see cref="IEnumerable{TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source enumerable.</typeparam>
    /// <typeparam name="TResult">The type of the mapped items.</typeparam>
    public sealed class SelectEnumerableIterator<TSource, TResult> : Iterator<TResult>
    {
        private readonly IEnumerable<TSource> _source;
        private readonly Func<TSource, TResult> _selector;
        private IEnumerator<TSource> _enumerator;

        public SelectEnumerableIterator(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Debug.Assert(source != null);
            Debug.Assert(selector != null);
            _source = source;
            _selector = selector;
        }

        public override Iterator<TResult> Clone() =>
            new SelectEnumerableIterator<TSource, TResult>(_source, _selector);

        public override void Dispose()
        {
            if (_enumerator != null)
            {
                _enumerator.Dispose();
                _enumerator = null;
            }

            base.Dispose();
        }

        public override bool MoveNext()
        {
            switch (_state)
            {
                case 1:
                    _enumerator = _source.GetEnumerator();
                    _state = 2;
                    goto case 2;
                case 2:
                    if (_enumerator.MoveNext())
                    {
                        _current = _selector(_enumerator.Current);
                        return true;
                    }

                    Dispose();
                    break;
            }

            return false;
        }

        //public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector) =>
        //    new SelectEnumerableIterator<TSource, TResult2>(_source, CombineSelectors(_selector, selector));

        public TResult[] ToArray()
        {
            var builder = new List<TResult>();

            foreach (TSource item in _source)
            {
                builder.Add(_selector(item));
            }

            return builder.ToArray();
        }

        public List<TResult> ToList()
        {
            var list = new List<TResult>();

            foreach (TSource item in _source)
            {
                list.Add(_selector(item));
            }

            return list;
        }

        public int GetCount(bool onlyIfCheap)
        {
            // In case someone uses Count() to force evaluation of
            // the selector, run it provided `onlyIfCheap` is false.

            if (onlyIfCheap)
            {
                return -1;
            }

            int count = 0;

            foreach (TSource item in _source)
            {
                _selector(item);
                checked
                {
                    count++;
                }
            }

            return count;
        }
    }

    /// <summary>
    /// A base class for enumerables that are loaded on-demand.
    /// </summary>
    /// <typeparam name="TSource">The type of each item to yield.</typeparam>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>
    /// The value of an iterator is immutable; the operation it represents cannot be changed.
    /// </description></item>
    /// <item><description>
    /// However, an iterator also serves as its own enumerator, so the state of an iterator
    /// may change as it is being enumerated.
    /// </description></item>
    /// <item><description>
    /// Hence, state that is relevant to an iterator's value should be kept in readonly fields.
    /// State that is relevant to an iterator's enumeration (such as the currently yielded item)
    /// should be kept in non-readonly fields.
    /// </description></item>
    /// </list>
    /// </remarks>
    public abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerator<TSource>
    {
        private readonly int _threadId;
        internal int _state;
        internal TSource _current;

        /// <summary>
        /// Initializes a new instance of the <see cref="Iterator{TSource}"/> class.
        /// </summary>
        protected Iterator()
        {
            _threadId = Environment.CurrentManagedThreadId;
        }

        /// <summary>
        /// The item currently yielded by this iterator.
        /// </summary>
        public TSource Current => _current;

        /// <summary>
        /// Makes a shallow copy of this iterator.
        /// </summary>
        /// <remarks>
        /// This method is called if <see cref="GetEnumerator"/> is called more than once.
        /// </remarks>
        public abstract Iterator<TSource> Clone();

        /// <summary>
        /// Puts this iterator in a state whereby no further enumeration will take place.
        /// </summary>
        /// <remarks>
        /// Derived classes should override this method if necessary to clean up any
        /// mutable state they hold onto (for example, calling Dispose on other enumerators).
        /// </remarks>
        public virtual void Dispose()
        {
            _current = default(TSource);
            _state = -1;
        }

        /// <summary>
        /// Gets the enumerator used to yield values from this iterator.
        /// </summary>
        /// <remarks>
        /// If <see cref="GetEnumerator"/> is called for the first time on the same thread
        /// that created this iterator, the result will be this iterator. Otherwise, the result
        /// will be a shallow copy of this iterator.
        /// </remarks>
        public IEnumerator<TSource> GetEnumerator()
        {
            Iterator<TSource> enumerator = _state == 0 && _threadId == Environment.CurrentManagedThreadId ? this : Clone();
            enumerator._state = 1;
            return enumerator;
        }

        /// <summary>
        /// Retrieves the next item in this iterator and yields it via <see cref="Current"/>.
        /// </summary>
        /// <returns><c>true</c> if there was another value to be yielded; otherwise, <c>false</c>.</returns>
        public abstract bool MoveNext();

        /// <summary>
        /// Returns an enumerable that maps each item in this iterator based on a selector.
        /// </summary>
        /// <typeparam name="TResult">The type of the mapped items.</typeparam>
        /// <param name="selector">The selector used to map each item.</param>
        public virtual IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
        {
            return new SelectEnumerableIterator<TSource, TResult>(this, selector);
        }

        /// <summary>
        /// Returns an enumerable that filters each item in this iterator based on a predicate.
        /// </summary>
        /// <param name="predicate">The predicate used to filter each item.</param>
        //public virtual IEnumerable<TSource> Where(Func<TSource, bool> predicate)
        //{
        //    return new WhereEnumerableIterator<TSource>(this, predicate);
        //}

        object IEnumerator.Current => Current;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void IEnumerator.Reset() => throw new Exception("NotSupported");
    }
}

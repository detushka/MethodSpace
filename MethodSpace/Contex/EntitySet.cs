using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MethodSpace.Contex
{
    public class EntitySet<T> : IEnumerable<T> where T : class
    {
        private readonly Func<IEnumerable<T>> _enumerate;
        private readonly Func<int, T> _find;
        private readonly Action<T> _add;
        private readonly Func<T, bool> _remove;

        public EntitySet(List<T> items, Func<T, int> getId, Action<T> beforeAdd = null, Action<T> afterRemove = null)
            : this(
                () => items ?? new List<T>(),
                id => (items ?? new List<T>()).FirstOrDefault(item => getId(item) == id),
                item =>
                {
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }

                    beforeAdd?.Invoke(item);
                    (items ?? throw new ArgumentNullException("items")).Add(item);
                },
                item =>
                {
                    if (item == null || items == null)
                    {
                        return false;
                    }

                    bool removed = items.Remove(item);
                    if (removed)
                    {
                        afterRemove?.Invoke(item);
                    }

                    return removed;
                })
        {
        }

        public EntitySet(Func<IEnumerable<T>> enumerate, Func<int, T> find, Action<T> add, Func<T, bool> remove)
        {
            _enumerate = enumerate ?? throw new ArgumentNullException("enumerate");
            _find = find ?? throw new ArgumentNullException("find");
            _add = add ?? throw new ArgumentNullException("add");
            _remove = remove ?? throw new ArgumentNullException("remove");
        }

        public void Add(T item)
        {
            _add(item);
        }

        public bool Remove(T item)
        {
            return _remove(item);
        }

        public T Find(int id)
        {
            return _find(id);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

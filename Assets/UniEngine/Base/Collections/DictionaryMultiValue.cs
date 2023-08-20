using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UniEngine.Base.Collections
{
    /// <summary>
    /// 游戏框架多值字典类。
    /// </summary>
    /// <typeparam name="TKey">指定多值字典的主键类型。</typeparam>
    /// <typeparam name="TValue">指定多值字典的值类型。</typeparam>
    public sealed class DictionaryMultiValue<TKey, TValue> : IEnumerable<KeyValuePair<TKey, LinkedListRange<TValue>>>
    {
        private readonly LinkedListCached<TValue> _linkedListCached;
        private readonly Dictionary<TKey, LinkedListRange<TValue>> _dictionary;

        /// <summary>
        /// 初始化游戏框架多值字典类的新实例。
        /// </summary>
        public DictionaryMultiValue()
        {
            _linkedListCached = new LinkedListCached<TValue>();
            _dictionary = new Dictionary<TKey, LinkedListRange<TValue>>();
        }

        /// <summary>
        /// 获取多值字典中实际包含的主键数量。
        /// </summary>
        public int Count => _dictionary.Count;

        /// <summary>
        /// 获取多值字典中指定主键的范围。
        /// </summary>
        /// <param name="key">指定的主键。</param>
        /// <returns>指定主键的范围。</returns>
        public LinkedListRange<TValue> this[TKey key]
        {
            get
            {
                _dictionary.TryGetValue(key, out var linkedListRange);
                return linkedListRange;
            }
        }

        /// <summary>
        /// 清理多值字典。
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
            _linkedListCached.Clear();
        }

        /// <summary>
        /// 检查多值字典中是否包含指定主键。
        /// </summary>
        /// <param name="key">要检查的主键。</param>
        /// <returns>多值字典中是否包含指定主键。</returns>
        public bool Contains(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// 检查多值字典中是否包含指定值。
        /// </summary>
        /// <param name="key">要检查的主键。</param>
        /// <param name="value">要检查的值。</param>
        /// <returns>多值字典中是否包含指定值。</returns>
        public bool Contains(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var linkedListRange))
            {
                return linkedListRange.Contains(value);
            }

            return false;
        }

        /// <summary>
        /// 尝试获取多值字典中指定主键的范围。
        /// </summary>
        /// <param name="key">指定的主键。</param>
        /// <param name="linkedListRange">指定主键的范围。</param>
        /// <returns>是否获取成功。</returns>
        public bool TryGetValue(TKey key, out LinkedListRange<TValue> linkedListRange)
        {
            return _dictionary.TryGetValue(key, out linkedListRange);
        }

        /// <summary>
        /// 向指定的主键增加指定的值。
        /// </summary>
        /// <param name="key">指定的主键。</param>
        /// <param name="value">指定的值。</param>
        public void Add(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var linkedListRange))
            {
                _linkedListCached.AddBefore(linkedListRange.Terminal, value);
            }
            else
            {
                var first = _linkedListCached.AddLast(value);
                var terminal = _linkedListCached.AddLast(default(TValue));
                _dictionary.Add(key, new LinkedListRange<TValue>(first, terminal));
            }
        }

        /// <summary>
        /// 从指定的主键中移除指定的值。
        /// </summary>
        /// <param name="key">指定的主键。</param>
        /// <param name="value">指定的值。</param>
        /// <returns>是否移除成功。</returns>
        public bool Remove(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var linkedListRange))
            {
                for (var current = linkedListRange.First;
                     current != null && current != linkedListRange.Terminal;
                     current = current.Next)
                {
                    if (current.Value.Equals(value))
                    {
                        if (current == linkedListRange.First)
                        {
                            var next = current.Next;
                            if (next == linkedListRange.Terminal)
                            {
                                _linkedListCached.Remove(next);
                                _dictionary.Remove(key);
                            }
                            else
                            {
                                _dictionary[key] = new LinkedListRange<TValue>(next, linkedListRange.Terminal);
                            }
                        }

                        _linkedListCached.Remove(current);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 从指定的主键中移除所有的值。
        /// </summary>
        /// <param name="key">指定的主键。</param>
        /// <returns>是否移除成功。</returns>
        public bool RemoveAll(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var linkedListRange))
            {
                _dictionary.Remove(key);

                var current = linkedListRange.First;
                while (current != null)
                {
                    var next = current != linkedListRange.Terminal ? current.Next : null;
                    _linkedListCached.Remove(current);
                    current = next;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_dictionary);
        }

        /// <summary>
        /// 返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        IEnumerator<KeyValuePair<TKey, LinkedListRange<TValue>>>
            IEnumerable<KeyValuePair<TKey, LinkedListRange<TValue>>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 循环访问集合的枚举数。
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, LinkedListRange<TValue>>>
        {
            private Dictionary<TKey, LinkedListRange<TValue>>.Enumerator _enumerator;

            internal Enumerator(Dictionary<TKey, LinkedListRange<TValue>> dictionary)
            {
                if (dictionary == null)
                {
                    throw new Exception("Dictionary is invalid.");
                }

                _enumerator = dictionary.GetEnumerator();
            }

            /// <summary>
            /// 获取当前结点。
            /// </summary>
            public KeyValuePair<TKey, LinkedListRange<TValue>> Current => _enumerator.Current;

            /// <summary>
            /// 获取当前的枚举数。
            /// </summary>
            object IEnumerator.Current => _enumerator.Current;

            /// <summary>
            /// 清理枚举数。
            /// </summary>
            public void Dispose()
            {
                _enumerator.Dispose();
            }

            /// <summary>
            /// 获取下一个结点。
            /// </summary>
            /// <returns>返回下一个结点。</returns>
            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            /// <summary>
            /// 重置枚举数。
            /// </summary>
            void IEnumerator.Reset()
            {
                ((IEnumerator<KeyValuePair<TKey, LinkedListRange<TValue>>>)_enumerator).Reset();
            }
        }
    }
}
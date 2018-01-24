﻿using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.UserInput
{

    /// <summary>
    /// Store a group of IInputSignatures based on a mapping value instead of a hash. This mapping value should usually be an enum, you can also use an int/long/etc if you want.
    /// 
    /// When creating IInputSignatures to add to this, it's best if its hash is set to the enum value that it maps to. 
    /// It's not required, but the ICollection&ltT&gt.Add(IInputSignature) method will fail. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MappedInputSignaturCollection<T> : ICollection<IInputSignature> where T : struct, System.IConvertible
    {

        #region Fields

        private Dictionary<T, IInputSignature> _table = new Dictionary<T, IInputSignature>();
        private List<IInputSignature> _sortedList = new List<IInputSignature>();

        #endregion

        #region Methods

        public void Add(T mapping, IInputSignature item)
        {
            if (item == null) throw new System.ArgumentNullException("item");
            if (_table.ContainsKey(mapping)) throw new System.ArgumentException("A signature already exists with this mapping.", "item");
            
            _table[mapping] = item;
            _sortedList.Add(item);
        }

        public IInputSignature GetSignature(T mapping)
        {
            IInputSignature result;
            if (_table.TryGetValue(mapping, out result) && result != null)
            {
                return result;
            }
            return null;
        }

        public IInputSignature GetSignature(string id)
        {
            var e = _table.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.Id == id) return e.Current.Value;
            }
            return null;
        }

        /// <summary>
        /// MappedInputSignatureCollection favors the mapping over the hash. This method will run slower than GetSignature(T mapping).
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public IInputSignature GetSignature(int hash)
        {
            var e = _table.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.Hash == hash) return e.Current.Value;
            }
            return null;
        }

        public bool Contains(T mapping)
        {
            return _table.ContainsKey(mapping);
        }

        public bool Contains(string id)
        {
            var e = _table.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.Id == id) return true;
            }
            return false;
        }
        
        public bool Remove(T mapping)
        {
            return _table.Remove(mapping);
        }

        public bool Remove(string id)
        {
            var e = _table.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.Id == id)
                {
                    _table.Remove(e.Current.Key);
                    return true;
                }
            }

            return false;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void Sort()
        {
            _sortedList.Sort(SortOnPrecedence);
        }
        
        #endregion

        #region ICollection Interface

        public int Count
        {
            get { return _table.Count; }
        }

        void ICollection<IInputSignature>.Add(IInputSignature item)
        {
            if (item == null) return;

            T map;
            try
            {
                map = com.spacepuppy.Utils.ConvertUtil.ToPrim<T>(item.Hash);
            }
            catch (System.Exception)
            {
                throw new System.InvalidOperationException("MappedInputSignature generic type " + typeof(T).Name + " can not be coerced to an int. Your generic type should be an Enum or other integer type.");
            }
            this.Add(map, item);
        }

        public void Clear()
        {
            _table.Clear();
            _sortedList.Clear();
        }

        public bool Contains(IInputSignature item)
        {
            return _sortedList.Contains(item);
        }

        public void CopyTo(IInputSignature[] array, int arrayIndex)
        {
            _sortedList.CopyTo(array, arrayIndex);
        }

        bool ICollection<IInputSignature>.IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(IInputSignature item)
        {
            var e = _table.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value == item)
                {
                    if (_table.Remove(e.Current.Key))
                    {
                        _sortedList.Remove(item);
                        return true;
                    }
                    else
                        return false;
                }
            }

            return false;
        }

        IEnumerator<IInputSignature> IEnumerable<IInputSignature>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region Special Types

        public struct Enumerator : IEnumerator<IInputSignature>
        {

            private List<IInputSignature>.Enumerator _e;

            public Enumerator(MappedInputSignaturCollection<T> coll)
            {
                if (coll == null) throw new System.ArgumentNullException("coll");
                _e = coll._sortedList.GetEnumerator();
            }

            public IInputSignature Current
            {
                get { return _e.Current; }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _e.Current; }
            }

            public bool MoveNext()
            {
                return _e.MoveNext();
            }

            void System.Collections.IEnumerator.Reset()
            {
                (_e as System.Collections.IEnumerator).Reset();
            }

            public void Dispose()
            {
                _e.Dispose();
            }
        }

        #endregion

        #region Sort Methods

        private static System.Comparison<IInputSignature> _sortOnPrecedence;
        private static System.Comparison<IInputSignature> SortOnPrecedence
        {
            get
            {
                if (_sortOnPrecedence == null)
                {
                    _sortOnPrecedence = (a, b) =>
                    {
                        if (a.Precedence > b.Precedence)
                            return 1;
                        if (a.Precedence < b.Precedence)
                            return -1;
                        else
                            return 0;
                    };
                }
                return _sortOnPrecedence;
            }
        }

        #endregion

    }

}
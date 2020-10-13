using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DictionaryKeys
{
    interface IDoubleKeyDictionary<TKeyId, TKeyName, TValue>
    {
        int Count { get; }
        Tuple<TKeyId, TValue> GetById(TKeyId id);
        Tuple<TKeyName, TValue> GetByName(TKeyName name);
        bool Add(TKeyId id, TKeyName name, TValue value);
        bool Add(Tuple<TKeyId, TKeyName, TValue> elem);
        void Remove(TKeyId id);
        void Remove(TKeyName name);
        void Clear();
    }
}

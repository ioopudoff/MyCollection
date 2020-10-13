using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DictionaryKeys
{
    class Program
    {
        class UserType : IComparable<UserType>
        {
            public string _value;
            public UserType(string value)
            {
                _value = value;
            }

            public int CompareTo(UserType other)
            {
                return string.Compare(_value, other._value);
            }
        }
        static void Main(string[] args)
        {
            var dictionary = new DoubleKeyDictionary<UserType, int, int>();
            for(var i = 0; i<10; i++)
            {
                dictionary.Add(new Tuple<UserType, int, int>(new UserType(i.ToString()), i, i));
            }
            var th = dictionary.GetById(new UserType("5"));
            Console.WriteLine(th.ToString());
            Console.ReadKey();
        }
    }
}

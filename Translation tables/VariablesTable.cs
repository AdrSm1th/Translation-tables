using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Translation_tables
{
    //Убрать три хуйни. Сделать через несколько конструкторов, да прибудут с нами nullы
    interface IDynamicElement
    { 
        public string Name { get; set; }
        public string Key { get; set; }
    }

    struct Identificator(string name, string key, int scope) : IDynamicElement
    {
        public string Name { get; set; } = name;
        public string Key { get; set; } = key;
        public int Scope { get; set; } = scope;
    }

    struct Constant(string name, string key) : IDynamicElement
    {
        public string Name { get; set; } = name;
        public string Key { get; set; } = key;
    }

    class VariablesTable
    {
        private const int tableSize = 10000;
        private IDynamicElement[] dynamicElements = new IDynamicElement[tableSize];

        public uint Hash(string key)
        {
            uint h = 5371;
            foreach (char c in key)
            {
                h = (h * 33) ^ c;
            }
            return (h % tableSize);
        }

        public void InsertIdentificator(string name, string key, int scope)
        {
            uint hash = Hash(key);
            if (dynamicElements[hash] != null) dynamicElements[hash] = new Identificator(name, key, scope);
            else
            {
                int i = 0;
                while (dynamicElements[hash] != null)
                {
                    hash = (hash + 1) % tableSize;
                    if (++i == tableSize) return;
                }
                dynamicElements[hash] = new Identificator(name, key, scope);
            }
        }

        public void InsertConstant(string name, string key)
        {
            uint hash = Hash(key);
            if (dynamicElements[hash] != null) dynamicElements[hash] = new Constant(name, key);
            else
            {
                int i = 0;
                while (dynamicElements[hash] != null)
                {
                    hash = (hash + 1) % tableSize;
                    if (++i == tableSize) return;
                }
                dynamicElements[hash] = new Constant(name, key);
            }
        }

        public bool Search(string key)
        {
            uint hash = Hash(key);
            if (dynamicElements[hash] == null) return false;
            if (dynamicElements[hash].Key == key) return true;

            while (dynamicElements[hash].Key != key)
            {
                hash = (hash + 1) % tableSize;
                if (hash == tableSize && dynamicElements[hash] == null) return false;
            }
            return true;
        }
    }
}

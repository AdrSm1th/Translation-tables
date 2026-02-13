using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Translation_tables
{
    interface IDynamicElement
    {
        public string Key { get; set; }
    }

    struct Identificator(string key) : IDynamicElement
    {
        public string? Name { get; set; }
        public string Key { get; set; } = key;
        public int Scope { get; set; }
    }

    struct Constant(string key) : IDynamicElement
    {
        public int Value { get; set; }
        public string Key { get; set; } = key;
    }

    //class DynamicElement(string key)
    //{
    //    public string Key { get; set; } = key;
    //    public string? Name { get; set; }
    //    public int Value { get; set; }
    //    public int Scope { get; set; }
    //}

    class VariablesTable
    {
        private const int tableSize = 10000;
        public IDynamicElement[] dynamicElements = new IDynamicElement[tableSize];

        public int Hash(string key)
        {
            int h = 5371;
            foreach (char c in key)
            {
                h = (h * 33) ^ c;
            }
            return Math.Abs(h % tableSize);
        }

        public void InsertConstant(string key)
        {
            int hash = Hash(key);
            if (dynamicElements[hash] != null) dynamicElements[hash] = new Constant(key);
            else
            {
                int i = 0;
                while (dynamicElements[hash] != null)
                {
                    hash = (hash + 1) % tableSize;
                    if (++i == tableSize) return;
                }
                dynamicElements[hash] = new Constant(key);
            }
        }

        public void InsertIdentificator(string key)
        {
            int hash = Hash(key);
            if (dynamicElements[hash] != null) dynamicElements[hash] = new Identificator(key);
            else
            {
                int i = 0;
                while (dynamicElements[hash] != null)
                {
                    hash = (hash + 1) % tableSize;
                    if (++i == tableSize) return;
                }
                dynamicElements[hash] = new Identificator(key);
            }
        }

        public void AddAttribute(string key, string? name = null, int? value = null, int? scope = null)
        {
            int idx = Search(key);
            if (idx == -1)
            {
                Console.WriteLine($"Element with {key} not found");
                return;
            }

            if (dynamicElements[idx] is Constant constant)
            {
                constant.Value = value.Value;
                dynamicElements[idx] = constant;
            }

            if (dynamicElements[idx] is Identificator identificator)
            {
                identificator.Name = name;
                identificator.Scope = scope.Value;
                dynamicElements[idx] = identificator;
            }
        }

        public int Search(string key)
        {
            int hash = Hash(key);
            if (dynamicElements[hash] == null) return -1;
            if (dynamicElements[hash].Key == key) return hash;

            while (dynamicElements[hash].Key != key)
            {
                hash = (hash + 1) % tableSize;
                if (hash == tableSize && dynamicElements[hash] == null) return -1;
            }
            return hash;
        }
    }
}

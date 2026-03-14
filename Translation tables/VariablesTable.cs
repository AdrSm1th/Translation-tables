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
        public string Name { get; set; }
    }

    struct Lexeme(string name, int value, int scope): IDynamicElement
    {
        public string Name { get; set;  } = name;
        public int Value { get; set; } = value;
        public int Scope { get; set; } = scope;
    }

    struct Constant(string name, int value) : IDynamicElement
    {
        public string Name { get; set; } = name;
        public int Value { get; set; } = value;
    }

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

        public void InsertConstant(string name, int value)
        {
            int hash = Hash(name);
            if (dynamicElements[hash] != null) dynamicElements[hash] = new Constant(name, value);
            else
            {
                int i = 0;
                while (dynamicElements[hash] != null)
                {
                    hash = (hash + 1) % tableSize;
                    if (++i == tableSize) return;
                }
                dynamicElements[hash] = new Constant(name, value);
            }
        }

        public void InsertLexeme(string name, int value, int scope)
        {
            int hash = Hash(name);
            if (dynamicElements[hash] != null) dynamicElements[hash] = new Lexeme(name, value, scope);
            else
            {
                int i = 0;
                while (dynamicElements[hash] != null)
                {
                    hash = (hash + 1) % tableSize;
                    if (++i == tableSize) return;
                }
                dynamicElements[hash] = new Lexeme(name, value, scope);
            }
        }     

        public int Search(string key)
        {
            int hash = Hash(key);
            if (dynamicElements[hash] == null) return -1;
            if (dynamicElements[hash].Name == key) return hash;

            while (dynamicElements[hash].Name != key)
            {
                hash = (hash + 1) % tableSize;
                if (hash == tableSize && dynamicElements[hash] == null) return -1;
            }
            return hash;
        }
    }
}

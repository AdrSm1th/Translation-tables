//VariblesTable.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Translation_tables
{
    enum VarType { Int }

    struct Lexeme(string name, int value, bool constant, VarType type = VarType.Int, bool isInit = false)
    {
        public string Name { get; set; } = name;
        public int Value { get; set; } = value;
        public bool Const { get; set; } = constant;
        public VarType Type { get; set; } = type;
        public bool IsInitialized { get; set; } = isInit;
    }

    class VariablesTable
    {
        private const int tableSize = 200;
        public Lexeme[] dynamicElements = new Lexeme[tableSize];

        public int Hash(string key)
        {
            int h = 5371;
            foreach (char c in key)
            {
                h = (h * 33) ^ c;
            }
            return Math.Abs(h % tableSize);
        }

        public int InsertLexeme(string name, int value, bool constant, VarType type = VarType.Int, bool isInit = false)
        {
            int hash = Hash(name);
            if (dynamicElements[hash].Name == null) dynamicElements[hash] = new Lexeme(name, value, constant);
            else
            {
                int i = 0;
                while (dynamicElements[hash].Name != null)
                {
                    hash = (hash + 1) % tableSize;
                    if (++i == tableSize) return hash;
                }
                dynamicElements[hash] = new Lexeme(name, value, constant);
            }
            dynamicElements[hash] = new Lexeme(name, value, constant, type, isInit);
            return hash;
        }

        public void ChangeLexeme(string name, int value)
        {
            int idx = Search(name);
            if (idx != -1) dynamicElements[idx].Value = value;
        }

        public int Search(string key)
        {
            int hash = Hash(key), i = 0;
            if (dynamicElements[hash].Name == null) return -1;
            if (dynamicElements[hash].Name == key) return hash;

            while (dynamicElements[hash].Name != key)
            {
                hash = (hash + 1) % tableSize;
                if (i >= 10000) return -1;
                i++;
            }
            return hash;
        }

        public void UpdateLexemeAttributes(string name, VarType type, bool isInitialized, bool isConst)
        {
            int idx = Search(name);
            if (idx != -1)
            {
                var lex = dynamicElements[idx];
                lex.Type = type;
                lex.IsInitialized = isInitialized;
                lex.Const = isConst;
                dynamicElements[idx] = lex;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translation_tables
{
    struct Word
    {
        public string name;
        public string type;

        public Word(string _name, string _type)
        {
            name = _name;
            type = _type;
        }
    }

    class PermanentTable
    {
        public List<Word> Alphabet = [];
        public List<Word> Words = [];
        public List<Word> Operators = [];
        public List<Word> Separators = [];

        int i = 0;

        public PermanentTable()
        {
            string[] alphabet = File.ReadAllLines("alphabet.txt");
            foreach (string letter in alphabet)
            {
                Alphabet.Add(new Word(letter, "letter"));
            }

            string[] words = File.ReadAllLines("reserved_words.txt");
            foreach (string word in words)
            {
                Words.Add(new Word(word, "word"));
            }

            string[] operators = File.ReadAllLines("operators.txt");
            foreach (string oper in operators)
            {
                Operators.Add(new Word(oper, "operator"));
            }

            string[] separators = File.ReadAllLines("separators.txt");
            foreach (string separator in separators)
            {
                Separators.Add(new Word(separator, "separator"));
            }

            Alphabet.Sort((a, b) => a.name.CompareTo(b.name));
            Words.Sort((a, b) => a.name.CompareTo(b.name));
            Operators.Sort((a, b) => a.name.CompareTo(b.name));
            Separators.Sort((a, b) => a.name.CompareTo(b.name));
        }
    }
}

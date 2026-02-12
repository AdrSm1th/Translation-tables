using Microsoft.VisualBasic;
using System;
using System.Runtime.InteropServices;
using Translation_tables;

struct Word
{
    public string name;
    public int keyCode;

    public Word(string _name, int _keyCode)
    {
        name = _name;
        keyCode = _keyCode;
    }
}

class PermanentTable
{
    private List<Word> Alphabet = [];
    private List<Word> ReservedWords = [];
    private List<Word> Operators = [];
    private List<Word> Separators = [];

    public PermanentTable()
    {
        string[] alphabet = File.ReadAllLines("alphabet.txt");
        int i = 0;
        foreach (string letter in alphabet)
        {
            Alphabet.Add(new Word(letter, i++));
        }
        Alphabet.Sort((a, b) => a.name.CompareTo(b.name));

        string[] words = File.ReadAllLines("reserved_words.txt");
        foreach (string word in words)
        {
            ReservedWords.Add(new Word(word, i++));
        }
        ReservedWords.Sort((a,b) => a.name.CompareTo(b.name));

        string[] operators = File.ReadAllLines("operators.txt");
        foreach (string oper in operators)
        {
            Operators.Add(new Word(oper, i++));
        }
        Operators.Sort((a, b) => a.name.CompareTo(b.name));

        string[] separators = File.ReadAllLines("separators.txt");
        foreach (string separator in separators)
        {
            Separators.Add(new Word(separator, i++));
        }
        Separators.Sort((a, b) => a.name.CompareTo(b.name));
    }

    public bool Search(string elem, int key)
    {
        switch (key)
        {
            case 1: return BinarySearch.Search(elem, key, Alphabet);
            case 2: return BinarySearch.Search(elem, key, ReservedWords);
            case 3: return BinarySearch.Search(elem, key, Operators);
            case 4: return BinarySearch.Search(elem, key, Separators);
            default: throw new ArgumentOutOfRangeException("key");
        }
    }
}

class Program
{
    static void Main()
    {
        Directory.SetCurrentDirectory("C:\\Users\\sasha\\source\\repos\\Translation tables\\Translation tables");

        PermanentTable permanentTables = new PermanentTable();
        VariablesTable variablesTable = new VariablesTable();

        Console.WriteLine(permanentTables.Search("a", 1));

        variablesTable.InsertConstant("-5", "NEGR");
        variablesTable.InsertIdentificator("a", "sNegr", 1);

        Console.WriteLine(variablesTable.Search("NEGR"));
        Console.WriteLine(variablesTable.Search("NEGRi"));
        Console.WriteLine(variablesTable.Search("sNegr"));
    }
}
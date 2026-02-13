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
            case 1: return BinarySearch.Search(elem, Alphabet);
            case 2: return BinarySearch.Search(elem, ReservedWords);
            case 3: return BinarySearch.Search(elem, Operators);
            case 4: return BinarySearch.Search(elem, Separators);
            default: throw new ArgumentOutOfRangeException("key");
        }
    }
}

class Program
{
    static void Main()
    {
        Directory.SetCurrentDirectory("C:\\Users\\sasha\\source\\repos\\Translation tables\\Translation tables");

        PermanentTable permanentTable = new PermanentTable();
        VariablesTable variablesTable = new VariablesTable();

        while (true)
        {
            int choice = 0;
            string key = "";

            Console.WriteLine("<1> - Find element in permanent table");
            Console.WriteLine("<2> - Insert element");
            Console.WriteLine("<3> - Add attribute");
            Console.WriteLine("<4> - Find element in dynamic table");
            Console.WriteLine("<5> - Find attribute");
            Console.WriteLine("<6> - Exit");

            choice = int.Parse(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Console.WriteLine("<1> - Find letter");
                    Console.WriteLine("<2> - Find word");
                    Console.WriteLine("<3> - Find operator");
                    Console.WriteLine("<4> - Find separator");
                    choice = int.Parse(Console.ReadLine());

                    if(choice < 1 || choice > 4)
                    {
                        Console.WriteLine("Unknown command!");
                        break;
                    }

                    Console.WriteLine("Enter element name");
                    string name = Console.ReadLine();

                    Console.WriteLine(permanentTable.Search(name, choice));
                    break;

                case 2:
                    Console.WriteLine("<1> - Insert constant");
                    Console.WriteLine("<2> - Insert identificator");
                    choice = int.Parse(Console.ReadLine());


                    if (choice < 1 || choice > 2)
                    {
                        Console.WriteLine("Unknown command!");
                        break;
                    }

                    Console.WriteLine("Enter element key");
                    key = Console.ReadLine();

                    if (choice == 1) variablesTable.InsertConstant(key);
                    if (choice == 2) variablesTable.InsertIdentificator(key);

                    break;

                case 3:
                    Console.WriteLine("<1> - Insert constant attribute");
                    Console.WriteLine("<2> - Insert identificator attribute");
                    choice = int.Parse(Console.ReadLine());

                    if (choice < 1 || choice > 2)
                    {
                        Console.WriteLine("Unknown command!");
                        break;
                    }

                    if(choice == 1)
                    {
                        Console.WriteLine("Enter key and value");
                        string[] input = Console.ReadLine().Split(' ');
                        key = input[0];
                        int value = int.Parse(input[1]);
                        variablesTable.AddAttribute(key, value: value);
                    }

                    if(choice == 2)
                    {
                        Console.WriteLine("Enter key, name and scope");
                        string[] input = Console.ReadLine().Split(' ');
                        key = input[0];
                        name = input[1];
                        int scope = int.Parse(input[2]);
                        variablesTable.AddAttribute(key, name: name, scope: scope);
                    }
                    break;

                case 4:
                    Console.WriteLine("Enter key");
                    key = Console.ReadLine();
                    if (variablesTable.Search(key) != -1) Console.WriteLine("true");
                    else Console.WriteLine("false");
                    break;

                case 5:
                    Console.WriteLine("Enter key");
                    key = Console.ReadLine();
                    int idx = variablesTable.Search(key);
                    if (idx == -1)
                    {
                        Console.WriteLine("Element not fount");
                        break;
                    }

                    if (variablesTable.dynamicElements[idx] is Constant constant)
                    {
                        constant = (Constant)variablesTable.dynamicElements[idx];
                        Console.WriteLine($"Value = {constant.Value}");
                    }

                    if (variablesTable.dynamicElements[idx] is Identificator identificator)
                    {
                        identificator = (Identificator)variablesTable.dynamicElements[idx];
                        Console.WriteLine($"Name: {identificator.Name}\nScope: {identificator.Scope}");
                    }
                    break;

                default: Console.WriteLine("Unknown command!"); break;
            }
        }
    }
}
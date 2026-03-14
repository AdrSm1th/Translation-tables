using Microsoft.VisualBasic;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Translation_tables;

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
    public List<Word> DataStatic = [];

    public PermanentTable()
    {
        string[] alphabet = File.ReadAllLines("alphabet.txt");
        foreach (string letter in alphabet)
        {
            DataStatic.Add(new Word(letter, "letter"));
        }

        string[] words = File.ReadAllLines("reserved_words.txt");
        foreach (string word in words)
        {
            DataStatic.Add(new Word(word, "word"));
        }

        string[] operators = File.ReadAllLines("operators.txt");
        foreach (string oper in operators)
        {
            DataStatic.Add(new Word(oper, "operator"));
        }

        string[] separators = File.ReadAllLines("separators.txt");
        foreach (string separator in separators)
        {
            DataStatic.Add(new Word(separator, "separator"));
        }
        
        DataStatic.Sort((a, b) => a.name.CompareTo(b.name));
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
            int choice = 0, idx = 0;
            string name = "";

            Console.WriteLine("<1> - Find element in permanent table");
            Console.WriteLine("<2> - Insert lexeme in dynamic table");
            Console.WriteLine("<3> - Find lexeme in dynamic table");
            Console.WriteLine("<4> - Exit");

            choice = int.Parse(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Console.WriteLine("Enter element name");
                    name = Console.ReadLine();

                    Console.WriteLine(BinarySearch.Search(name, permanentTable.DataStatic));
                    break;

                case 2:
                    Console.WriteLine("<1> - Insert constant");
                    Console.WriteLine("<2> - Insert lexeme");
                    choice = int.Parse(Console.ReadLine());

                    if (choice < 1 || choice > 2)
                    {
                        Console.WriteLine("Unknown command!");
                        break;
                    }

                    if (choice == 1)
                    {
                        Console.WriteLine("Enter element name, value");
                        string[] input = Console.ReadLine().Split(' ');
                        if (input.Length != 2)
                        {
                            Console.WriteLine("Invalid input");
                            break;
                        }
                        name = input[0];
                        int value = 0;

                        if (!int.TryParse(input[1], out value))
                        {
                            Console.WriteLine("Invalid input");
                            break;
                        }

                        variablesTable.InsertConstant(name, value);
                    }

                    if (choice == 2)
                    {
                        Console.WriteLine("Enter element name, value and scope");
                        string[] input = Console.ReadLine().Split(' ');
                        if (input.Length != 3)
                        {
                            Console.WriteLine("Invalid input");
                            break;
                        }
                        name = input[0];
                        int value = 0, scope = 0;

                        
                        if (!int.TryParse(input[1], out value))
                        {
                            Console.WriteLine("Invalid input");
                            break;
                        }

                        if (!int.TryParse(input[2], out scope))
                        {
                            Console.WriteLine("Invalid input");
                            break;
                        }

                        variablesTable.InsertLexeme(name, value, scope);
                    }
                    break;

                case 3:
                    {
                        Console.WriteLine("Enter name");
                        name = Console.ReadLine();
                        idx = variablesTable.Search(name);
                        if (idx == -1)
                        {
                            Console.WriteLine("Element not found");
                            break;
                        }

                        if (variablesTable.dynamicElements[idx] is Constant constant)
                        {
                            constant = (Constant)variablesTable.dynamicElements[idx];
                            Console.WriteLine($"Value = {constant.Value}");
                        }

                        if (variablesTable.dynamicElements[idx] is Lexeme identificator)
                        {
                            identificator = (Lexeme)variablesTable.dynamicElements[idx];
                            Console.WriteLine($"Name: {identificator.Name} Value: {identificator.Value} Scope: {identificator.Scope}");
                        }
                        break;
                    }

                case 4:
                    return;

                default: Console.WriteLine("Unknown command!"); break;
            }
        }
    }
}
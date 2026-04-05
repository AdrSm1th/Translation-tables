using Microsoft.VisualBasic;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Translation_tables;

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

            Scanner scanner = new Scanner(permanentTable, variablesTable);
            scanner.Scan("program.txt");
            //scanner.Output();

            choice = int.Parse(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Console.WriteLine("Enter element name");
                    name = Console.ReadLine();

                    //Console.WriteLine(BinarySearch.Search(name, permanentTable.DataStatic));
                    break;

                case 2:
                    Console.WriteLine("Enter element name, value");
                    string[] input = Console.ReadLine().Split(' ');
                    if (input.Length != 2)
                    {
                        Console.WriteLine("Invalid input: wrong number of parameters");
                        break;
                    }
                    name = input[0];
                    int value = 0;

                    if (!int.TryParse(input[1], out value))
                    {
                        Console.WriteLine("Invalid input: value must be integer");
                        break;
                    }
                    //variablesTable.InsertLexeme(name, value);
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
                        Lexeme identificator = (Lexeme)variablesTable.dynamicElements[idx];
                        Console.WriteLine($"Name: {identificator.Name} Value: {identificator.Value}");
                        break;
                    }

                case 4:
                    return;

                default: Console.WriteLine("Unknown command!"); break;
            }
        }
    }
}
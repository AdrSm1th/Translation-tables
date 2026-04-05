using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translation_tables
{
    public enum State
    {
        START,
        IDENTIFIER,
        CONSTANT,
        OPERATOR,
        SEPARATOR,
        COMMENT,
        BLOCKCOMMENT,
        NAMED_CONSTANT
    }

    struct Token(int type, int id)
    {
        private int Type = type;
        //0 - Keyword
        //1 - Seporator
        //2 - Operator
        //3 - Named constant
        //4 - constant
        //5 - Identifier

        private int Id = id;

        public int GetId()
        {
            return Id;
        }

        public int GetTokenType()
        {
            return Type;
        }

        public string GetToken()
        {
            return $"({Type.ToString()}, {Id}) ";
        }
    }

    class Scanner(PermanentTable permanentTable, VariablesTable variablesTable)
    {
        private State CurrentState { get; set; } = State.START;
        private PermanentTable PermanentTable { get; set; } = permanentTable;
        private VariablesTable VariablesTable { get; set; } = variablesTable;
        private string Buffer { get; set; }

        private List<Token> Tokens = new List<Token>();

        private bool flag = true;
        private bool initialize = false;

        private int Line { get; set; } = 1;
        private int Pos { get; set; }

        public void Scan(string filename)
        {
            string programInput = File.ReadAllText(filename);
            programInput += "$";
            ReadOnlySpan<char> input = programInput.AsSpan();
            string output = string.Empty;
            int curTokenId = 0;

            for (int position = 0; position < input.Length; position++)
            {
                Pos++;
                char ch = input[position];
                int id = 0;
                Word elem = new Word();
                id = BinarySearch.Search(ch.ToString(), PermanentTable.Alphabet);
                if(id != -1) elem = PermanentTable.Alphabet[id];
                if (id == -1)
                {
                    id = BinarySearch.Search(ch.ToString(), PermanentTable.Operators);
                    if (id != -1) elem = PermanentTable.Operators[id];
                }
                if (id == -1)
                {
                    id = BinarySearch.Search(ch.ToString(), PermanentTable.Seporators);
                    if (id != -1) elem = PermanentTable.Seporators[id];
                }

                switch (CurrentState)
                {
                    case State.START:
                        {
                            int value = 0;
                            if (elem.type != null)
                            {
                                switch (elem.type)
                                {
                                    case "letter":
                                        CurrentState = State.IDENTIFIER;
                                        break;
                                    case "operator":
                                        CurrentState = State.OPERATOR;
                                        break;
                                    case "separator":
                                        if (elem.name == "/") CurrentState = State.COMMENT;
                                        else CurrentState = State.SEPARATOR;
                                        break;
                                }
                                Buffer += ch;
                            }

                            else if (ch == ' ' || ch == '\r' || ch == '\t')
                            {
                                continue;
                            }

                            else if (ch == '\n')
                            {
                                output += "\n";
                                Line++;
                                Pos = 0;
                            }

                            else if (int.TryParse(ch.ToString(), out value))
                            {
                                CurrentState = State.CONSTANT;
                                Buffer += value.ToString();
                            }

                            else
                                Console.WriteLine("Unknown symbol on position: ", position);
                        }
                        break;

                    case State.IDENTIFIER:
                        {
                            if (ch == ' ' || elem.type == "operator" || elem.type == "separator")
                            {
                                int wordId = BinarySearch.Search(Buffer, PermanentTable.Words);
                                if (wordId != -1)
                                {
                                    if(PermanentTable.Words[wordId].name != "main" && initialize)
                                    {
                                        output += $"reserved word can't be identifier line: {Line} position: {Pos} ";
                                        flag = false;
                                    }
                                    else
                                    {
                                        Tokens.Add(new Token(0, wordId));
                                        output += Tokens[curTokenId++].GetToken();
                                        if (PermanentTable.Words[wordId].name == "const")
                                        {
                                            CurrentState = State.NAMED_CONSTANT;
                                            Buffer = "";
                                            break;
                                        }
                                        if (PermanentTable.Words[wordId].name == "int")
                                        {
                                            initialize = true;
                                            Buffer = "";
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    int val = 0;
                                    foreach(char letter in Buffer)
                                    {
                                        if(BinarySearch.Search(letter.ToString(), PermanentTable.Alphabet) == -1 && !int.TryParse(letter.ToString(), out val))
                                        {
                                            output += $"Invalid name for identifier on line: {Line} position {Pos} ";
                                            flag = false;
                                            break;
                                        }    
                                    }

                                    if (flag)
                                    {
                                        int hash = VariablesTable.Search(Buffer);
                                        if (hash == -1)
                                        {
                                            hash = VariablesTable.InsertLexeme(Buffer, 0);
                                        }
                                        Tokens.Add(new Token(5, hash));
                                        output += Tokens[curTokenId++].GetToken();
                                    }
                                }
                                Buffer = "";
                                CurrentState = State.START;
                                position--;
                                Pos--;
                                flag = true;
                                initialize = false;
                            }
                            else
                            {
                                Buffer += ch;
                            }
                        }
                        break;

                    case State.OPERATOR:
                        {
                            int value = 0;
                            if (ch == ' ' || int.TryParse(ch.ToString(), out value))
                            {
                                Tokens.Add(new Token(2, BinarySearch.Search(Buffer, PermanentTable.Operators)));
                                output += Tokens[curTokenId++].GetToken();
                                Buffer = "";
                                CurrentState = State.START;
                                position--;
                                Pos--;
                            }
                            else Buffer += ch;
                        }
                        break;

                    case State.SEPARATOR:
                        {
                            CurrentState = State.START;
                            Tokens.Add(new Token(1, BinarySearch.Search(Buffer, PermanentTable.Seporators)));
                            output += Tokens[curTokenId++].GetToken();
                            Buffer = "";
                            position--;
                            Pos--;
                        }
                        break;

                    case State.CONSTANT:
                        {
                            int value = 0;
                            if (int.TryParse(ch.ToString(), out value))
                            {
                                Buffer += value.ToString();
                            }
                            else if (elem.type == "letter")
                            {
                                if(flag == true) output += $"Invalid name for identifier on line: {Line} position: {Pos} ";
                                flag = false;
                            }
                            else
                            {
                                if(flag)
                                {
                                    int hash = VariablesTable.Search(Buffer);
                                    if (hash == -1)
                                    {
                                        hash = VariablesTable.InsertLexeme(Buffer, int.Parse(Buffer));
                                    }
                                    Tokens.Add(new Token(4, hash));
                                    output += Tokens[curTokenId++].GetToken();
                                }

                                flag = true;
                                CurrentState = State.START;
                                Buffer = "";
                                position--;
                                Pos--;
                            }
                        }
                        break;

                    case State.COMMENT:
                        {
                            if (Buffer.Length > 2 && Buffer[0].ToString() + Buffer[1].ToString() == "/*")
                            {
                                CurrentState = State.BLOCKCOMMENT;
                            }
                            else if (ch == '\n' || ch == '$')
                            {
                                CurrentState = State.START;
                                Buffer = "";
                                position--;
                                Pos--;
                            }
                            else
                            {
                                Buffer += ch.ToString();
                            }
                        }
                        break;

                    case State.BLOCKCOMMENT:
                        {
                            int bufferSize = Buffer.Length;
                            if (Buffer[bufferSize - 2].ToString() + Buffer[bufferSize - 1].ToString() == "*/")
                            {
                                CurrentState = State.START;
                                Buffer = "";
                                position--;
                                Pos--;
                            }
                            else
                            {
                                Buffer += ch.ToString();
                            }
                        }
                        break;

                    case State.NAMED_CONSTANT:
                        {
                            if(ch != ';') Buffer += ch.ToString();
                            else
                            {
                                bool isCorrect = true;
                                CurrentState = State.START;
                                string[] parts = Buffer.Split(' ');
                                Buffer = "";
                                Tokens.Add(new Token(0, BinarySearch.Search(parts[0], PermanentTable.Words)));
                                output += Tokens[curTokenId++].GetToken();
                                foreach (char letter in parts[1])
                                {
                                    int lId = BinarySearch.Search(letter.ToString(), PermanentTable.Alphabet);
                                    if(char.IsLower(letter) || lId == -1)
                                    {
                                        output += ($" Invalid name for constant in line: {Line} ");
                                        break;
                                        isCorrect = false;
                                    }
                                }
                                if(isCorrect)
                                {
                                    int hash = VariablesTable.InsertLexeme(parts[1], int.Parse(parts[3]));
                                    Tokens.Add(new Token(3, hash));
                                    output += Tokens[curTokenId++].GetToken();
                                }
                                Tokens.Add(new Token(1, BinarySearch.Search(";", PermanentTable.Seporators)));
                                output += Tokens[curTokenId++].GetToken();
                            }
                        }
                        break;
                }
            }
            File.WriteAllText("output.txt", output);
        }

        //public void Output()
        //{
        //    //string output = string.Empty;
        //    //foreach (var token in Tokens)
        //    //{
        //    //    output += token.GetToken();
        //    //}
        //    //File.WriteAllText("output.txt", output);
        //}
    }
}
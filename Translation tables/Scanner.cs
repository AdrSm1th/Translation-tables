using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translation_tables
{
    public enum TokenType
    {
        KEYWORD,
        IDENTIFIER,
        CONSTANT,
        OPERATOR,
        SEPARATOR,
        COMMENT
    }

    public enum State
    {
        START,
        IDENTIFIER,
        CONSTANT,
        OPERATOR,
        SEPARATOR,
        COMMENT
    }

    struct Token(TokenType type, string name)
    {
        private TokenType Type = type;
        private string Name = name;

        public string GetName()
        {
            return Name;
        }
        public string GetToken()
        {
            return $"Type: {Type.ToString()} Name: {Name}\n";
        }
    }

    class Scanner(PermanentTable permanentTable, VariablesTable variablesTable)
    {
        private State CurrentState { get; set; } = State.START;
        PermanentTable PermanentTable { get; set; } = permanentTable;
        VariablesTable VariablesTable { get; set; } = variablesTable;
        string Buffer { get; set; }

        List<Token> Tokens = new List<Token>();

        public void Scan(string filename)
        {
            string programInput = File.ReadAllText(filename);
            programInput += "$";
            ReadOnlySpan<char> input = programInput.AsSpan();

            for(int position = 0; position < input.Length; position++)
            {
                char ch = input[position];
                Word elem = BinarySearch.Search(ch.ToString(), PermanentTable.DataStatic);
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

                            else if(ch == ' ' || ch == '\n' || ch == '\r' || ch == '\t')
                            {
                                continue;
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

                            Word chW = BinarySearch.Search(ch.ToString(), PermanentTable.DataStatic);
                            if(ch == ' ' || chW.type == "operator" || chW.type == "separator")
                            {
                                Word word = BinarySearch.Search(Buffer, PermanentTable.DataStatic);
                                if (word.type == "word")
                                {
                                    Tokens.Add(new Token(TokenType.KEYWORD, Buffer));
                                }
                                else 
                                {
                                    int idx = VariablesTable.Search(Buffer);
                                    if (idx == -1)
                                    {
                                        Tokens.Add(new Token(TokenType.IDENTIFIER, Buffer));
                                        //VariablesTable.InsertLexeme(Buffer, 0);
                                    }
                                }
                                Buffer = "";
                                CurrentState = State.START;
                                position--;
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
                            //if (ch == ' ') continue;
                            if (ch == ' ' || int.TryParse(ch.ToString(), out value))
                            {
                                //Token identifier = Tokens[Tokens.Count - 1];
                                //switch (Buffer)
                                //{
                                //    case "=":
                                //        {
                                //            VariablesTable.ChangeLexeme(identifier.GetName(), value);
                                //        }
                                //    break;

                                //    case "+":
                                //        {
                                //            int prevValue = VariablesTable.Search(identifier.GetName());
                                //            value += prevValue;
                                //            VariablesTable.ChangeLexeme(identifier.GetName(), value);
                                //        }
                                //    break;
                                //}
                                Tokens.Add(new Token(TokenType.OPERATOR, Buffer));
                                Buffer = "";
                                CurrentState = State.START;
                                position--;
                            }
                            else Buffer += ch;
                        }
                    break;

                    case State.SEPARATOR:
                        {
                            CurrentState = State.START;
                            Tokens.Add(new Token(TokenType.SEPARATOR, Buffer));
                            Buffer = "";
                            position--;
                        }
                    break;

                    case State.CONSTANT:
                        {
                            int value = 0;
                            if (int.TryParse(ch.ToString(), out value))
                            {
                                Buffer += value.ToString();
                            }
                            else if (BinarySearch.Search(ch.ToString(), PermanentTable.DataStatic).type == "alphabet")
                            {
                                Console.WriteLine("Error: unkwonw number on position: ", position);
                            }
                            else
                            {
                                CurrentState = State.START;
                                Tokens.Add(new Token(TokenType.CONSTANT, Buffer));
                                Buffer = "";
                                position--;
                            }
                        }
                    break;

                    case State.COMMENT:
                        {
                            if (ch == '\n' || ch == '$')
                            {
                                CurrentState = State.START;
                                Tokens.Add(new Token(TokenType.COMMENT, Buffer));
                                Buffer = "";
                                position--;
                            }
                            else
                            {
                                Buffer += ch.ToString();
                            };
                        }
                    break;
                }
            }
        }

        public void Output()
        {
            string output = string.Empty;
            foreach (var token in Tokens)
            {
                output += token.GetToken();
            }
            File.WriteAllText("output.txt", output);
        }
    }
}
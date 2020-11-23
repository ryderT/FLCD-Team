using System;
using System.Collections.Generic;
using System.Linq;
namespace LFTC___LABORATOR_4
{
    class Program
    {
        static Grammar grammar;
        static string inputSequence;

        static void MenuOption(int opt)
        {
            string grammarFile = "";

            switch (opt)
            {
                case 1:
                    grammarFile = "basicGrammar.txt";
                    string inputSequenceFile = "inputSequence.txt";

                    ReadGrammarFromFile(grammarFile);
                    ReadInputSequenceFromFile(inputSequenceFile);
                    break;
                case 2:
                    grammarFile = "minilanguageGrammar.txt";
                    string programInternalFormFile = "programInternalForm.txt";

                    ReadGrammarFromFile(grammarFile);
                    ReadProgramInternalFormFromFile(programInternalFormFile);
                    break;
                default:
                    break;
            }
        }

        static void ReadGrammarFromFile(string grammarFile)
        {
            string startingSymbol = "";
            List<string> nonTerminals = new List<string>();
            List<string> terminals = new List<string>();
            List<Production> productions = new List<Production>();

            string line;
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(grammarFile);
                int k = 0;

                while ((line = file.ReadLine()) != null)
                {
                    if (k == 0 && line != "")
                        startingSymbol = line.Trim();

                    if (k == 1 && line != "")
                    {
                        string[] strings = line.Split(',', ' ');
                        nonTerminals.AddRange(strings.ToList<string>());
                    }

                    if (k == 2 && line != "")
                    {
                        string[] strings = line.Split(',', ' ');
                        terminals.AddRange(strings.ToList<string>());
                    }

                    if (k > 2 & line != "")
                    {
                        string[] strings = line.Split('-');
                        string[] results = strings[1].Split(' ');
                        Production p = new Production(strings[0], results.ToList<string>());
                        productions.Add(p);
                    }

                    if (line == "")
                        k++;
                }

                grammar = new Grammar(startingSymbol, nonTerminals, terminals, productions);

                file.Close();
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
            }
        }

        static void ReadInputSequenceFromFile(string inputSequenceFile)
        {
            string line;
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(inputSequenceFile);

                while ((line = file.ReadLine()) != null)
                {
                    inputSequence = line;
                }

                file.Close();
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
            }
        }

        static void ReadProgramInternalFormFromFile(string programInternalFormFile)
        {
            string line;
            inputSequence = "";

            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(programInternalFormFile);

                while ((line = file.ReadLine()) != null)
                {
                    int code = Convert.ToInt32(line.Split(' ')[1]);

                    inputSequence = inputSequence + code + " ";
                }

                file.Close();

                Console.WriteLine(inputSequence);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
            }
        }

        static void WriteToConsole()
        {
            string startingSymbol = grammar.StartingSymbol;
            List<string> nonTerminals = grammar.NonTerminals;
            List<string> terminals = grammar.Terminals;
            List<Production> productions = grammar.Productions;

            Console.WriteLine("Initial Grammar:");
            Console.Write($"Starting symbol: {startingSymbol}");

            Console.WriteLine();

            Console.Write($"Non terminals: ");
            foreach (string s in nonTerminals)
                Console.Write(s + " ");

            Console.WriteLine();

            Console.Write("Terminals: ");
            foreach (string s in terminals)
                Console.Write(s + " ");

            Console.WriteLine();

            Console.WriteLine("Productions: ");
            foreach (Production p in productions)
            {
                Console.Write(p.LeftHandSide + "-");

                for (int i = 0; i < p.RightHandSide.Count; i++)
                {
                    if (i == p.RightHandSide.Count - 1)
                        Console.Write(p.RightHandSide[i]);
                    else
                        Console.Write(p.RightHandSide[i] + " ");
                }

                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Generated First Set: ");

            Dictionary<string, List<string>> firstSet = grammar.GetFirstUs();

            foreach (KeyValuePair<string, List<string>> kv in firstSet)
            {
                Console.Write(kv.Key + " - ");
                foreach (string v in kv.Value)
                    Console.Write(v + " ");
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Generated Follow Set: ");

            Dictionary<string, List<string>> followSet = grammar.GetFollowUs();

            foreach (KeyValuePair<string, List<string>> kv in followSet)
            {
                Console.Write(kv.Key + " - ");
                foreach (string v in kv.Value)
                    Console.Write(v + " ");
                Console.WriteLine();
            }

            Console.WriteLine("Terminal to show productions of : ");
            grammar.GetProductionContainingTerminal(Console.ReadLine());
            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            string option;

            Console.WriteLine("Options: ");
            Console.WriteLine("1 - Step 1 - Basic Grammar");
            Console.WriteLine("2 - Step 2 - Minilanguage Grammar");
            Console.WriteLine();
            Console.Write("Select an option: ");

            option = Console.ReadLine();
            Console.WriteLine();

            MenuOption(Int32.Parse(option));
            WriteToConsole();
        }
    }
}

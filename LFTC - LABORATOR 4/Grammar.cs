using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LFTC___LABORATOR_4
{
    public class Production
    {
        public string LeftHandSide { get; set; }
        public List<string> RightHandSide { get; set; }

        public Production(string leftHandSide, List<string> rightHandSide)
        {
            this.LeftHandSide = leftHandSide;
            this.RightHandSide = rightHandSide;
        }
    }

    public class Grammar
    {
        public string StartingSymbol { get; set; }
        public List<string> NonTerminals { get; set; }
        public List<string> Terminals { get; set; }
        public List<Production> Productions { get; set; }

        public Grammar(string startingSymbol, List<string> nonTerminals, List<string> terminals, List<Production> productions)
        {
            this.StartingSymbol = startingSymbol;
            this.NonTerminals = nonTerminals;
            this.Terminals = terminals;
            this.Productions = productions;
        }

        public Dictionary<string, List<string>> GetFirstUs()
        {
            var firstSet = new Dictionary<string, List<string>>();

            foreach (var terminal in this.Terminals)
            {
                firstSet.Add(terminal, new List<string> { terminal });
            }

            firstSet = this.ExecuteFirstIteration(firstSet, out var trimmedProductions);

            for (int i = 0; i < 2; i++)
            {
                // foreach remaining production
                trimmedProductions.ForEach(p =>
                {
                    var lastIndex = p.RightHandSide.Count() - 1;
                    var nonTerminal = p.LeftHandSide;

                    foreach (var element in p.RightHandSide)
                    {
                        var firstSetOfElement = firstSet[element];
                        
                        //
                        if (!firstSetOfElement.Any())
                        {
                            break;
                        }

                        if (!firstSetOfElement.Contains("epsilon"))
                        {

                            firstSetOfElement.ForEach(f =>
                            {
                                if (!firstSet[nonTerminal].Contains(f)) firstSet[nonTerminal].Add(f);
                            });

                            break;
                        }

                        if (p.RightHandSide.IndexOf(element) == lastIndex)
                        {
                            if (!firstSet[nonTerminal].Contains("epsilon")) firstSet[nonTerminal].Add("epsilon");
                        }
                    }
                });
            }

            return firstSet;
        }

        private Dictionary<string, List<string>> ExecuteFirstIteration(Dictionary<string, List<string>> firstSet, out List<Production> trimmedProductions)
        {
            var resultedList = new List<Production>();

            this.Productions.ForEach(p =>
            {
                var firstOfRightHandSide = p.RightHandSide.First();
                var nonTerminal = p.LeftHandSide;

                // Create entry for every non terminal.
                if (!firstSet.ContainsKey(nonTerminal))
                {
                    firstSet.Add(nonTerminal, new List<string>());
                }

                // Check whether the first symbol is terminal or epsilon.
                if (Terminals.Contains(firstOfRightHandSide)
                    || firstOfRightHandSide.Equals("epsilon"))
                {
                    // Check for duplicate symbol.
                    if (!firstSet[nonTerminal].Contains(firstOfRightHandSide))
                    {
                        firstSet[nonTerminal].Add(firstOfRightHandSide);
                    }
                }
                else
                {
                    resultedList.Add(p);
                }
            });

            trimmedProductions = resultedList;
            return firstSet;
        }

        public Dictionary<string, List<string>> GetFollowUs()
        {
            var firstSet = this.GetFirstUs();

            Dictionary<string, List<string>> followSet = new Dictionary<string, List<string>>
            {
                {StartingSymbol, new List<string>{"epsilon"}}
            };

            for (int i = 0; i < 1; i++)
            {
                foreach (var nonTerminal in this.NonTerminals)
                {
                    if (!followSet.ContainsKey(nonTerminal))
                    {
                        followSet.Add(nonTerminal, new List<string>());
                    }

                    foreach (var production in Productions)
                    {
                    
                        if (production.RightHandSide.Contains(nonTerminal))
                        {
                            List<string> toBeAdded = new List<string>();

                            var indexOfNonTerminalInProduction = production.RightHandSide.IndexOf(nonTerminal);
                            if (indexOfNonTerminalInProduction < production.RightHandSide.Count - 1)
                            {

                                var beta = production.RightHandSide[indexOfNonTerminalInProduction + 1];
                                if (beta != "epsilon")
                                {

                                    if (!firstSet[beta].Contains("epsilon"))
                                    {
                                        firstSet.TryGetValue(beta, out var set);
                                        if (set != null)
                                        {
                                            toBeAdded = set;
                                        }
                                    }
                                    else
                                    {
                                        toBeAdded = firstSet[beta].Select(s => s).Where(s => s != "epsilon").ToList();

                                        if (followSet.ContainsKey(production.LeftHandSide))
                                        {
                                            toBeAdded.AddRange(followSet[production.LeftHandSide]);
                                        }
                                    }
                                }

                            

                            }
                            else if (followSet.ContainsKey(production.LeftHandSide))
                            {
                                toBeAdded = followSet[production.LeftHandSide];
                            }

                            foreach (var element in toBeAdded)
                            {
                                if (!followSet[nonTerminal].Contains(element))
                                {
                                    followSet[nonTerminal].Add(element);
                                }
                            }
                        }
                    }
                }
            }

            followSet[StartingSymbol].Reverse();
            followSet[StartingSymbol].Add("$");
            followSet[StartingSymbol].Reverse();
            return followSet;
        }

        public Dictionary<Tuple<string, string>, int> GenerateParsingTable()
        {
            Dictionary<Tuple<string, string>, int> parsingTable = new Dictionary<Tuple<string, string>, int>();

            Dictionary<string, List<string>> firstSet = GetFirstUs();
            Dictionary<string, List<string>> followSet = GetFollowUs();

            Terminals.Add("$");

            foreach (string nonTerminal in NonTerminals)
            {
                bool conflicts = false;

                foreach (string terminal in Terminals)
                {
                    if (terminal != "epsilon")
                    {
                        Tuple<string, string> pair = new Tuple<string, string>(nonTerminal, terminal);

                        int index = 0;
                        foreach (Production production in Productions)
                        {
                            index++;
                            if (production.LeftHandSide == nonTerminal)
                            {
                                List<string> rightHandSide = production.RightHandSide;
                                List<string> firstSetForProduction = firstSet[rightHandSide[0]];
                                List<string> followSetForNonTerminal = followSet[nonTerminal];

                                if ((firstSetForProduction.Contains(terminal)))
                                {
                                    try
                                    {
                                        parsingTable.Add(pair, index);
                                    }
                                    catch
                                    {
                                        Console.WriteLine("This grammar contains conflicts: " + nonTerminal + " & " + terminal);
                                        conflicts = true;
                                        break;
                                    }
                                }

                                if (followSetForNonTerminal.Contains(terminal) && firstSetForProduction.Contains("epsilon"))
                                {
                                    try
                                    {
                                        parsingTable.Add(pair, index);
                                    }
                                    catch
                                    {
                                        Console.WriteLine("This grammar contains conflicts: " + nonTerminal + " & " + terminal);
                                        conflicts = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (conflicts) break;
                    }
                }

                if (conflicts) break;
            }

            Terminals.Remove("$");

            return parsingTable;
        }

        public void ApplyLL1(string inputSequence)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Dictionary<Tuple<string, string>, int> parsingTable = GenerateParsingTable();

            Stack<string> productionsStack = new Stack<string>();
            productionsStack.Push("$");
            productionsStack.Push(StartingSymbol);

            Stack<string> inputSequenceStack = new Stack<string>();
            inputSequenceStack.Push("$");

            List<string> tokens = inputSequence.Split(' ').ToList();
            tokens.Reverse();

            foreach (string token in tokens)
                if (token != "")
                    inputSequenceStack.Push(token);
            var productionString = new List<string>();
            while (inputSequenceStack.Count > 1)
            {
                Tuple<string, string> pair = new Tuple<string, string>(productionsStack.Peek(), inputSequenceStack.Peek());

                if (pair.Item1 == pair.Item2)
                {
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"({String.Join("", inputSequenceStack.ToArray())},\t{String.Join("", productionsStack.ToArray())},\t{String.Join("", productionString)})");

                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("POP " + productionsStack.Peek() + " " + inputSequenceStack.Peek());

                    productionsStack.Pop();
                    inputSequenceStack.Pop();

                }
                else
                {
                    if (parsingTable.ContainsKey(pair))
                    {
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"({String.Join("", inputSequenceStack.ToArray())},\t{String.Join("", productionsStack.ToArray())},\t{String.Join("", productionString)})");

                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.Write("POP " + productionsStack.Peek() + " and PUSH ");

                        productionsStack.Pop();

                        List<string> rightHandSide = Productions[parsingTable[pair] - 1].RightHandSide;
                        List<string> tokensToPush = new List<string>();

                        var production = Productions[parsingTable[pair] - 1];
                        productionString.Add(Productions.IndexOf(production) + 1 + " ");

                        foreach (string token in rightHandSide)
                        {
                            if (token != "epsilon")
                            {
                                tokensToPush.Add(token);
                                Console.Write(token + " ");
                            }
                        }
                        Console.WriteLine();

                        tokensToPush.Reverse();

                        foreach (string token in tokensToPush)
                            productionsStack.Push(token);
                    }
                    else
                    {
                        Console.WriteLine("Error for: " + productionsStack.Peek() + " " + inputSequenceStack.Peek());
                        break;
                    }
                }

                if (inputSequenceStack.Peek() == "$")
                {
                    while (productionsStack.Peek() != "$")
                    {
                        var nonTerminal = productionsStack.Pop();
                        var production = Productions.Single(p => p.LeftHandSide == nonTerminal && p.RightHandSide.Contains("epsilon"));
                        productionString.Add(Productions.IndexOf(production) + 1 + " ");
                    }

                    inputSequenceStack.Pop();
                    productionsStack.Pop();
                }
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write("Production String -> "); productionString.ForEach(s => Console.Write(s));
            if (productionsStack.Count == 0)
                Console.WriteLine("\nSuccess!");
        }
    }
}

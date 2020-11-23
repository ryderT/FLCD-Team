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
                if (Terminals.Contains(firstOfRightHandSide) || firstOfRightHandSide.Equals("epsilon"))
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
                    Console.WriteLine(String.Join(" ", p.RightHandSide));
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

            followSet[StartingSymbol].Reverse();
            followSet[StartingSymbol].Add("$");
            followSet[StartingSymbol].Reverse();
            return followSet;
        }


        public Production GetProductionContainingTerminal(string terminal)
        {
            foreach (var production in this.Productions)
            {
                if (production.RightHandSide.Contains(terminal))
                {
                    Console.WriteLine(production.LeftHandSide + " -> " + String.Join(" ", production.RightHandSide));
                }
            }

            return null;
        }
    }
}

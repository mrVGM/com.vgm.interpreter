﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ScriptingLaunguage.Tokenizer;

namespace ScriptingLaunguage.Parser
{
    public class Grammar
    {
        public Rule[] Rules;
        public Rule InitialRule;

        string[] alphabet = null;
        string[] terminals = null;
        string[] nonTerminals = null;
        public IEnumerable<string> Alphabet
        {
            get 
            {
                if (alphabet == null) 
                {
                    var allLHS = Rules.Select(x => x.LHS);
                    var allRHS = Rules.SelectMany(x => x.RHS);
                    alphabet = allLHS.Concat(allRHS).Distinct().ToArray();
                }
                return alphabet;
            }
        }
        public IEnumerable<string> NonTerminals
        {
            get
            {
                if (nonTerminals == null) 
                {
                    nonTerminals = Rules.Select(x => x.LHS).Distinct().ToArray();
                }
                return nonTerminals;
            }
        }

        public IEnumerable<string> Terminals 
        {
            get 
            {
                if (terminals == null) 
                {
                    terminals = Alphabet.Where(x => !NonTerminals.Contains(x)).Distinct().ToArray();
                }
                return terminals;
            }
        }

        public HashSet<ParserState> GenerateParserStates()
        {
            var initialState = new ParserState();
            initialState.Rules.Add(new AugmentedRule { Rule = InitialRule, DotPosition = 0, LookAheadSymbols = new HashSet<string>() });
            initialState.ExtendState(this);

            HashSet<ParserState> res = new HashSet<ParserState>();
            res.Add(initialState);
            bool buildStep()
            {
                bool changed = false;
                foreach (var parserState in res.ToArray())
                {
                    var transitionSymbols = parserState.GetTransitionSymbols();
                    foreach (var symbol in transitionSymbols) 
                    {
                        var newState = parserState.TransitionWith(symbol, this);
                        var associated = res.FirstOrDefault(x => x == newState);
                        if (associated != null)
                        {
                            changed |= associated.Merge(newState);
                        }
                        else 
                        {
                            res.Add(newState);
                            changed = true;
                        }
                    }
                }
                return changed;
            }

            while (buildStep()) { }

            return res;
        }

        public IEnumerable<string> ProducibleTerminalsFrom(string symbol) 
        {
            HashSet<string> processed = new HashSet<string>();
            IEnumerable<string> getProducibleTerminalsRecursive(string s)
            {
                if (Terminals.Contains(s))
                {
                    yield return s;
                    yield break;
                }
                if (processed.Contains(s)) 
                {
                    yield break;
                }

                var firstSymbols = Rules.Where(x => x.LHS == s).Select(x => x.RHS.First());
                processed.Add(s);
                var terminals = firstSymbols.SelectMany(x => getProducibleTerminalsRecursive(x));
                foreach (var terminal in terminals) 
                {
                    yield return terminal;
                }
            }

            return getProducibleTerminalsRecursive(symbol).Distinct();
        }

        public 
        static IEnumerable<IEnumerable<Token>> GetLines(IEnumerable<Token> grammarScript)
        {
            List<Token> curLine = new List<Token>();
            foreach (var token in grammarScript)
            {
                if (token.Name == Environment.NewLine) 
                {
                    if (curLine.Any()) 
                    {
                        yield return curLine;
                        curLine = new List<Token>();
                    }
                    continue;
                }
                curLine.Add(token);
            }
            if (curLine.Any()) 
            {
                yield return curLine;
            }
        }

        static Rule ReadRule(IEnumerable<Token> ruleTokens) 
        {
            var rule = new Rule();
            ruleTokens = ruleTokens.Where(x => x.Name == "String");
            rule.LHS = ruleTokens.First().Data as string;
            rule.RHS = ruleTokens.Skip(1).Select(x => x.Data as string).ToArray();
            return rule;
        }
        public static Grammar ReadGrammarFromString(string grammarJson)
        {
            var tokenizer = new CombinedTokenizer(new StringTokenizer(), new NewLineTokenizer());
            var grammarScript = Utils.TokenizeText(grammarJson);
            grammarScript = tokenizer.Tokenize(grammarScript);
            var lines = GetLines(grammarScript);

            var rules = new List<Rule>();
            foreach (var line in lines) 
            {
                rules.Add(ReadRule(line));
            }

            return new Grammar { Rules = rules.ToArray(), InitialRule = rules.First() };
        }

        public static Grammar ReadGrammarFromFile(string fileName) 
        {
            string grammarStr = System.IO.File.ReadAllText(fileName);
            return ReadGrammarFromString(grammarStr);
        }
    }
}
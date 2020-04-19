﻿using System;
using System.Collections.Generic;
using System.Text;
using ScriptingLaunguage.Tokenizer;

namespace ScriptingLaunguage.Parser
{
    public class ProgramNode
    {
        public int RuleId = -1;
        public Token Token;
        public List<ProgramNode> Children = new List<ProgramNode>();

        public bool MatchChildren(params string[] template) 
        {
            if (template.Length != Children.Count)
            {
                return false;
            }

            for (int i = 0; i < template.Length; ++i) 
            {
                if (Children[i].Token.Name != template[i]) 
                {
                    return false;
                }
            }
            return true;
        }
    }
}
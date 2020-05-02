using System.Collections.Generic;
using System.Linq;
using ScriptingLaunguage.Tokenizer;

namespace ScriptingLaunguage.Parser
{
    public class ProgramNode
    {
        public int RuleId = -1;
        public IToken Token;
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

        public int GetCodeIndex() 
        {
            var indexed = Token as IIndexed;
            if (indexed != null) 
            {
                return indexed.Index;
            }

            return Children.FirstOrDefault().GetCodeIndex();
        }
    }
}

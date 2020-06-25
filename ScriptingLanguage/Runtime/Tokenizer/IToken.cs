using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptingLaunguage.Tokenizer
{
	public interface IToken
	{
		string Name { get; }
		object Data { get; }
	}

	public interface IIndexed 
	{
		int Index { get; set; }
	}

	public interface IScriptSourceHolder
	{
		ScriptId ScriptSource { get; }
	}

	public class ScriptId 
	{
		public string Filename;
		public string Script;
	}

	public class SimpleToken : IToken
	{
		public string Name { get; set; }
		public object Data { get; set; }
	}

	public class IndexedToken : IToken, IIndexed, IScriptSourceHolder
	{
		public string Name { get; set; }
		public object Data { get; set; }
		public int Index { get; set; }

		public ScriptId ScriptSource { get; private set; }

		public IndexedToken(int index, ScriptId scriptSource) 
		{
			Index = index;
			ScriptSource = scriptSource;
		}
	}
}

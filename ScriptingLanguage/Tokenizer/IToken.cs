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

	public class SimpleToken : IToken
	{
		public string Name { get; set; }
		public object Data { get; set; }
	}

	public class IndexedToken : IToken, IIndexed
	{
		public string Name { get; set; }
		public object Data { get; set; }
		public int Index { get; set; }
		public IndexedToken(int index) 
		{
			Index = index;
		}
	}
}

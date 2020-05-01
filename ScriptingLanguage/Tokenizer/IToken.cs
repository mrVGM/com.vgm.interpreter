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

	public class SimpleToken : IToken
	{
		public string Name { get; set; }
		public object Data { get; set; }
	}

	public class IndexedToken : IToken 
	{
		public string Name { get; set; }
		public object Data { get; set; }
		public int Index;
		public IndexedToken(int index) 
		{
			Index = index;
		}
	}
}

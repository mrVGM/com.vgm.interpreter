using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptingLaunguage.Tokenizer
{
	public interface Token
	{
		string Name { get; }
		object Data { get; }
	}

	public class SimpleToken : Token
	{
		public string Name { get; set; }
		public object Data { get; set; }
	}

	public class IndexedToken : Token 
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

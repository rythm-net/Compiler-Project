using System;

namespace MTProject
{
	public abstract class TableSymbol: IdentToken
	{
		public TableSymbol(int line, int column, string value): base(line, column, value) {}
	}
}

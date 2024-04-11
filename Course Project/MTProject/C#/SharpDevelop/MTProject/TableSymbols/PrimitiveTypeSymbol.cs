using System;

namespace MTProject
{
	public class PrimitiveTypeSymbol: TypeSymbol
	{
		public PrimitiveTypeSymbol(IdentToken token, Type type): base(token, type) {}
	}
}

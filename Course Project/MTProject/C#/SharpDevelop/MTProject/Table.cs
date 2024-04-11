using MTProject.TableSymbols;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MTProject
{
	public class Table
	{
        // стек от речници където всеки речник съдържа идентификаторите и техните символи на определен обхват в прогрм
        public Stack<Dictionary<string, TableSymbol>> symbolTable;

        // речник който съдържа информация за полетата на класове или структури
        private Dictionary<string, TableSymbol> fieldScope;

        // речник който съдържа информация за символите от глобалния обхват
        private Dictionary<string, TableSymbol> universeScope;

        // списък от имена на пространства на имена, които се използват в прогрм
        private List<string> usingNamespaces = new List<string>();

        // списък от сборки, които се използват в програмата.
        private List<string> references;

		public Table(List<string> references)
		{
            this.symbolTable = new Stack<Dictionary<string, TableSymbol>>();
			this.universeScope = BeginScope();
			this.fieldScope = BeginScope();
			this.references = references;
			foreach (string assemblyRef in references)
			{
				Assembly.LoadWithPartialName(assemblyRef);
				//Assembly.Load(assemblyRef);
			}
		}

        // връща текстово представяне на символната таблица
        public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			int i = symbolTable.Count;
			s.AppendFormat("=========\n");

			foreach (Dictionary<string, TableSymbol> table in symbolTable)
			{
				s.AppendFormat("---[{0}]---\n", i--);
				foreach (KeyValuePair<string, TableSymbol> row in table)
				{
					s.AppendFormat("[{0}] {1}\n", row.Key, row.Value);
				}
			}
			s.AppendFormat("=========\n");
			return s.ToString();
		}

        // добавя пространство на имена към списъка с използвани пространства на имена
        public void AddUsingNamespace(string usingNamespace)
		{
			usingNamespaces.Add(usingNamespace);
		}

        // добавя символ към текущия обхват на символната таблица
        public TableSymbol Add(TableSymbol symbol)
		{
			symbolTable.Peek().Add(symbol.value, symbol);
			return symbol;
		}

        // добавя символ към глобалния обхват на символната таблица
        public TableSymbol AddToUniverse(TableSymbol symbol)
		{
			universeScope.Add(symbol.value, symbol);
			return symbol;
		}

        // добавя поле към обхвата на полетата.
        public FieldSymbol AddField(IdentToken token, FieldInfo field)
		{
			FieldSymbol result = new FieldSymbol(token, field);
			fieldScope.Add(token.value, result);
			return result;
		}

        // добавя локална променлива към текущия обхват на символната таблица
        public LocalVarSymbol AddLocalVar(IdentToken token, LocalBuilder localBuilder)
		{
			LocalVarSymbol result = new LocalVarSymbol(token, localBuilder);
			symbolTable.Peek().Add(token.value, result);
			return result;
		}

        // добавя формален параметър към текущия обхват на символната таблица
        public FormalParamSymbol AddFormalParam(IdentToken token, Type type, ParameterBuilder parameterInfo)
		{
			FormalParamSymbol result = new FormalParamSymbol(token, type, parameterInfo);
			symbolTable.Peek().Add(token.value, result);
			return result;
		}

        // добавя метод към текущия обхват на символната таблица
        public MethodSymbol AddMethod(IdentToken token, Type type, FormalParamSymbol[] formalParams, MethodInfo methodInfo)
		{
			MethodSymbol result = new MethodSymbol(token, type, formalParams, methodInfo);
			symbolTable.Peek().Add(token.value, result);
			return result;
		}

        // създава нов обхват на символната таблица и го добавя към стека от обхвати
        public Dictionary<string, TableSymbol> BeginScope()
		{
			symbolTable.Push(new Dictionary<string, TableSymbol>());
			return symbolTable.Peek();
		}

        // премахва текущия обхват на символната таблица от стека от обхвати.
        public void EndScope()
		{
			Debug.WriteLine(ToString());

			symbolTable.Pop();
		}

		// връща символа с дадено име от текущия обхват на символната таблица
		public TableSymbol GetSymbol(string ident)
		{
			TableSymbol result;

			foreach (Dictionary<string, TableSymbol> table in symbolTable)
			{
				if (table.TryGetValue(ident, out result))
				{
					return result;
				}
			}
			return ResolveExternalMember(ident);
		}

        // проверява дали даден идентификатор съществува в текущия обхват на символната таблица
        public bool ExistCurrentScopeSymbol(string ident)
		{
			return symbolTable.Peek().ContainsKey(ident);
		}

        // разрешава външен тип по даден идентификатор
        public Type ResolveExternalType(string ident)
		{
			Type type = Type.GetType(ident, false, false);

			if (type != null) return type;
			foreach (string ns in usingNamespaces)
			{
				string nsTypeName = ns + Type.Delimiter + ident;

				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					type = assembly.GetType(ident);
					if (type != null) return type;
					type = assembly.GetType(nsTypeName);
					if (type != null) return type;
				}
			}
			return null;
		}

        // разрешава външен член (поле или метод) по даден идентификатор
        public TableSymbol ResolveExternalMember(string ident)
		{
			int lastIx = ident.LastIndexOf(Type.Delimiter);

			if (lastIx > 0)
			{
				string memberName = ident.Substring(lastIx + 1);
				string typeName = ident.Substring(0, lastIx);
				Debug.WriteLine(string.Format("{0} -- {1}", typeName, memberName));
				Type type = ResolveExternalType(typeName);

				if (type == null)
				{
					foreach (string usingNamespace in usingNamespaces)
					{
						type = ResolveExternalType(usingNamespace + "." + typeName);
						if (type != null) break;
					}
				}

				if (type != null)
				{
					FieldInfo fi = type.GetField(memberName, BindingFlags.Public | BindingFlags.Static);
					if (fi != null) return new FieldSymbol(new IdentToken(0, 0, memberName), fi);
					MemberInfo[] mi = type.GetMember(memberName, MemberTypes.Method, BindingFlags.Public | BindingFlags.Static);
					if (mi != null) return new ExternalMethodSymbol(new IdentToken(0, 0, memberName), (MethodInfo[])mi);
				}
			}
			return null;
		}
	}
}
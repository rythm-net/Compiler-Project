using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTProject
{
    // Compiler - това е статичен клас, който съдържа методи за компилиране на изходен код на програма.
    public static class Compiler
	{
        // references - това е статично поле от тип List<string>,
		// което съдържа списък от референции към други сборни библиотеки или изходни файлове,
		// които са необходими по време на компилацията
        private static List<string> references = new List<string>();

        // публичен статичен метод, който приема път до файл с изходен код и име на изходен изпълним файл.
        public static bool Compile(string file, string assemblyName)
		{
			return Compiler.Compile(file, assemblyName, new DefaultDiagnostics());
		}

        // този метод извлича кода от файла, създава лексически анализатор (Scanner),
		// синтактичен анализатор (Parser) и генератор на изходен код (Emit)
        public static bool Compile(string file, string assemblyName, Diagnostics diag)
		{
			TextReader reader = new StreamReader(file);
			Scanner scanner	  = new Scanner(reader);
			Table symbolTable = new Table(references);
			Emit emit		  = new Emit(assemblyName, symbolTable);
			Parser parser     = new Parser(scanner, emit, symbolTable, diag);

			diag.BeginSourceFile(file);
			bool isProgram = parser.Parse();
			diag.EndSourceFile();

			if (isProgram)
			{
				emit.WriteExecutable();
				return true;
			}
			else
			{
				return false;
			}
		}

        // този метод позволява добавяне на референции към други сборни библиотеки или изходни файлове,
        public static void AddReferences(List<string> references)
		{
			Compiler.references.AddRange(references);
		}
	}
}
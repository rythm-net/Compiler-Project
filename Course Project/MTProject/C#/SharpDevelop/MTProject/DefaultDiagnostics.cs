using System;

namespace MTProject
{
    // Това е класът, който представя типичните диагностики по време на компилация.
	// Той наследява абстрактния клас Diagnostics.
    public class DefaultDiagnostics : Diagnostics
	{
		private int errorCount = 0;
		public int ErrorCount
		{
			get { return errorCount; } // property
		}

		private int warningCount = 0;
		public int WarningCount
		{
			get { return warningCount; } // property
		}

		private int noteCount = 0;
		public int NoteCount
		{
			get { return noteCount; }  // property
		}

        // property-та съхраняват броя на съответно грешките

        public override void Error(int line, int column, String message)
		{
			Console.WriteLine(string.Format("Error on Line: {0}, Column {1}: {2}", line, column, message));
			errorCount++;
		}

		public override void Warning(int line, int column, String message)
		{
			Console.WriteLine("Attention on Line: {0}, Column: {1}: {2}", line, column, message);
			warningCount++;
		}

		public override void Note(int line, int column, String message)
		{
			Console.WriteLine("Remark on Like: {0}, Column: {1}: {2}", line, column, message);
			noteCount++;
		}

        //  Връща броя на грешките.
        public override int GetErrorCount()
		{
			return errorCount;
		}

        // BeginSourceFile & EndSourceFile се извикват в началото и края на компилацията на изходния файл
        public override void BeginSourceFile(string sourceFile)
		{
			//no operations
		}

		public override void EndSourceFile()
		{
			//no operations
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTProject
{
	public abstract class Diagnostics
	{
        // този метод се извиква при откриване на грешка по време на компилацията
		// параметрите line и column указват позицията в изходния код, където е открита грешката,
		// а message съдържа съобщението за грешката.
        public abstract void Error(int line, int column, String message);

        // този метод се извиква при откриване на предупреждение по време на компилацията
		// параметрите са същите като за метода за грешки
        public abstract void Warning(int line, int column, String message);

        // този метод се извиква при откриване на бележка по време на компилацията
		// параметрите са същите като за методите за грешки и предупреждения.
        public abstract void Note(int line, int column, String message);

        // връща броя на откритите грешки по време на компилацията.
        public abstract int GetErrorCount();

        //  се извиква в началото на компилацията на конкретен изходен файл и приема пътя до файла
        public abstract void BeginSourceFile(string sourceFile);

        // се извиква в края на компилацията на конкретен изходен файл
		public abstract void EndSourceFile();
	}
}
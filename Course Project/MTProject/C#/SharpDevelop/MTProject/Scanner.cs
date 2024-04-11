using System;
using System.IO;
using System.Text;

namespace MTProject
{
    // този клас осигурява необходимата информация за създаване на абстрактно синтактично дърво и генериране на изпълним код
    public class Scanner
	{
        // тези константи представляват край на файла, символ за нов ред при Windows
        const char EOF = '\u001a';
		const char CR = '\r';
		const char LF = '\n';
		//const char Escape = '\\';	??????

		static readonly string keywords = " printf printd scanf scand return skip ";

        // стрингове, които съдържат специални символи (символи за пунктуация)
        static readonly string specialSymbols1 = "{}(),;~";
		static readonly string specialSymbols2 = "*%/!&|+-<=>";

        // стринг, който съдържа двойки от специални символи, които образуват нов специален символ.
        static readonly string specialSymbols2Pairs =" += -= *= /= %= != && || ++ -- <= == >= ";

		// променливи
		private TextReader reader;		  // за четене на символи от входящия текстов файл
        private char ch;				  // текущ символ, който се чете от входния файл
        private int line, column;		  // позицията на текущия символ
        private bool skipComments = true; // указва дали да пропускаме коментарите или да ги включваме

        public bool SkipComments
		{
			get { return skipComments; }
			set { skipComments = value; }
		}

		public Scanner(TextReader reader)
		{
			this.reader = reader;
			this.line = 1;
			this.column = 0;
			ReadNextChar();
		}

        // чете следващия символ от входния файл и обновява ch, line и column
        public void ReadNextChar()
		{
			int ch1 = reader.Read();
			column++;
			ch = (ch1 < 0) ? EOF : (char) ch1;

			if (ch == CR)
			{
				line++;
				column = 0;
			}
			else if (ch == LF)
			{
				column = 0;
			}
		}

        // генерира следващия токен от входния текст, използва се цикъл, който се върти докато има символи за сканиране
		// вътрешността на метода разпознава различни типове токени като ключови думи -
		// идентификатори, числа, специални символи, коментари и създава съответните обекти за тях.
        public Token Next()
		{
			int start_column;
			int start_line;
			while (true)
			{
				start_column = column;
				start_line = line;

				if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch == '.')
				{
					StringBuilder s = new StringBuilder();

					while (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch == '.' || ch >= '0' && ch <= '9')
					{
						s.Append(ch);
						ReadNextChar();
					}
					String id = s.ToString();

					if (keywords.Contains(" " + id + " "))
					{
						return new KeywordToken(start_line, start_column, id);
					}
					return new IdentToken(start_line, start_column, id);
				}
				else if (ch >= '0' && ch <= '9')
				{
					StringBuilder s = new StringBuilder();

                    if (ch == '.')
                    {
                        while (ch >= '0' && ch <= '9')
                        {
                            s.Append(ch);
                            ReadNextChar();
                        }
                        return new DoubleToken(start_line, start_column, Convert.ToDouble(s.ToString()));
                    }

                    while (ch >= '0' && ch <= '9')
					{
						s.Append(ch);
						ReadNextChar();
					}
					return new NumberToken(start_line, start_column, Convert.ToInt64(s.ToString()));
				}
				else if (specialSymbols1.Contains(ch.ToString()))
				{
					char ch1 = ch;
					ReadNextChar();
					return new SpecialSymbolToken(start_line, start_column, ch1.ToString());
				}
				else if (specialSymbols2.Contains(ch.ToString()))
				{
					char ch1 = ch;
					ReadNextChar();
					char ch2 = ch;

					if (specialSymbols2Pairs.Contains(" " + ch1 + ch2 + " "))
					{
						ReadNextChar();
						return new SpecialSymbolToken(start_line, start_column, ch1.ToString() + ch2);
					}
					return new SpecialSymbolToken(start_line, start_column, ch1.ToString());
				}
				else if (ch == ' ' || ch == '\t' || ch == CR || ch == LF)
				{
					ReadNextChar();
					continue;
				}
				else if (ch == '/')
				{
					char ch1 = ch;
					ReadNextChar();

					if (ch == '/')
					{
						if (skipComments)
						{
							while (ch != CR && ch != LF && ch != EOF)
							{
								ReadNextChar();
							}
							ReadNextChar();
						}
						else
						{
							StringBuilder s = new StringBuilder();
							while (ch != CR && ch != LF && ch != EOF)
							{
								ReadNextChar();
								s.Append(ch);
							}
							ReadNextChar();
							return new CommentToken(start_line, start_column, s.ToString(), true);
						}
					}
					else if (ch == '*')
					{
						if (skipComments)
						{
							ReadNextChar();
							do
							{
								while (ch != '*' && ch != EOF)
								{
									ReadNextChar();
								}
								ReadNextChar();
							} while (ch != '/' && ch != EOF);
							ReadNextChar();
						}
						else
						{
							StringBuilder s = new StringBuilder();
							ReadNextChar();
							do
							{
								while (ch != '*' && ch != EOF)
								{
									s.Append(ch);
									ReadNextChar();
								}
								ReadNextChar();
							} while (ch != '/' && ch != EOF);
							ReadNextChar();
							return new CommentToken(start_line, start_column, s.ToString(), false);
						}
					}
					else return new SpecialSymbolToken(start_line, start_column, ch1.ToString());
					continue;
				}
				else if (ch == EOF)
				{
					return new EOFToken(start_line, start_column);
				}
				else
				{
					string s = ch.ToString();
					ReadNextChar();
					return new OtherToken(start_line, start_column, s);
				}
			}
		}
	}
}
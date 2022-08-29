using Jmy.Parser.Models;
using Jmy.Parser.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser
{
	public class TokenizerSettings
	{
		public string StringCatalystExcluded { get; set; } = " \t\n\r";
		public string StringCatalystEscapable { get; set; } = "";
		public string WordIncluded { get; set; } = "";
		public string CatchAllType { get; set; } = "";
		public bool SkipWhiteSpace { get; set; } = true;
		public bool CommentsAsTokens { get; set; } = false;
		public bool IgnoreCase { get; set; } = false;

		/// <summary>
		/// When <value>true</value>, counts tabs and newlines as one char column and does not add to the row counter
		/// </summary>
		public bool AllOneLine { get; set; }
		public static TokenizerSettings Default { get { return new TokenizerSettings(); } }
	}


	public class Tokenizer
	{
		private bool _bAtEnd;
		private int _nIndex;
		private uint _nRow;
		private uint _nColumn;
		private char _cCurrent;
		private string _text = "";
		private IEnumerable<TokenizerRule> _rules = new List<TokenizerRule>();
		TokenizerSettings _settings;
		public Tokenizer(IEnumerable<TokenizerRule> rules, TokenizerSettings settings = null)
		{
			_rules = rules.OrderBy((rule) => rule.Value.Length).Reverse();
			_settings = settings ?? TokenizerSettings.Default;
		}


		public IEnumerable<Token> TokenizeFile(string filename, bool bDebug = false)
		{
			if (File.Exists(filename))
			{
				return Tokenize(File.ReadAllText(filename), bDebug);
			}
			throw new IOException($"unable to open file '{filename}'");
		}
		public IEnumerable<Token> Tokenize(string text, bool bDebug = false)
		{
			init(text);
			List<Token> tokens = new List<Token>();
			while (!_bAtEnd)
			{
				Token token = next();
				if (bDebug)
				{
					Console.Write($"{token}, ");
				}
				tokens.Add(token);
			}
			reset();
			return tokens;
		}


		private Token next()
		{
			bool bEolCommentFlag = false;
			bool bMlCommentFlag = false;

			while (!_bAtEnd)
			{

				if (_cCurrent == '\0')
				{
					advance();
				}
				if (char.IsWhiteSpace(_cCurrent))
				{
					if (_settings.SkipWhiteSpace)
					{
						advance();
						continue;
					}
					else
					{
						if (_cCurrent == ' ')
						{
							advance();
							return new Token(TokenTypes.WhiteSpaceSpace, _cCurrent.ToString(), _nRow, _nColumn);
						}
						if (_cCurrent == '\t')
						{
							advance();
							return new Token(TokenTypes.WhiteSpaceTab, _cCurrent.ToString(), _nRow, _nColumn);
						}
						if (_cCurrent == '\r')
						{
							advance();
							return new Token(TokenTypes.WhiteSpaceCR, _cCurrent.ToString(), _nRow, _nColumn);
						}
						if (_cCurrent == '\n')
						{
							advance();
							return new Token(TokenTypes.WhiteSpaceLF, _cCurrent.ToString(), _nRow, _nColumn);
						}
					}

				}

				if (_cCurrent == '~')
				{
					advance();
					return tokenliteral();
				}

				if (_cCurrent == '_' || Char.IsLetter(_cCurrent))
				{
					return word();
				}

				foreach (TokenizerRule rule in _rules)
				{
					if (CompareRule(lookAhead(Convert.ToUInt32(rule.Length)), rule.Value))
					{
						if (rule.Type == TokenTypes.EOLComment)
						{
							bEolCommentFlag = true;
							break;
						}

						advance(rule.Length);

						if (rule.Type == TokenTypes.StringEnclosing)
						{
							return str(rule.Value);
						}
						if (rule.Type == TokenTypes.StringCatalyst)
						{
							return strcatalyst(rule.Value);
						}

						if (rule.Type == TokenTypes.MLCommentStart)
						{
							bMlCommentFlag = true;
							break;
						}

						if (rule.IsEnclosed)
						{
							return enclosed(rule.Type, rule.Enclosing);
						}

						return new Token(rule, _nRow, _nColumn);
					}
				}

				if (bEolCommentFlag)
				{
					if (_settings.CommentsAsTokens)
					{
						bEolCommentFlag = false;
						return eolcomment(TokenTypes.EOLComment);
					}
					else
					{
						eolcomment(TokenTypes.EOLComment);
						bEolCommentFlag = false;
						continue;
					}
				}

				if (bMlCommentFlag)
				{
					mlcomment();
					bMlCommentFlag = false;
					continue;
				}

				if (Char.IsDigit(_cCurrent))
				{
					return number();
				}

				if (_cCurrent == '_' || Char.IsLetter(_cCurrent))
				{
					return word();
				}


				Token result = new Token(string.IsNullOrWhiteSpace(_settings.CatchAllType) ? _cCurrent.ToString() : _settings.CatchAllType, _cCurrent.ToString(), _nRow, _nColumn);
				advance();
				return result;

			}
			return new Token(TokenTypes.EOF, TokenTypes.EOF, _nRow, _nColumn);
		}

		private bool CompareRule(string a, string b)
        {
			if (_settings.IgnoreCase) return a?.ToLower() == b?.ToLower();
			return a == b;
        }
		private bool init(string text)
		{
			reset();
			_text = text;
			if (_text.Length == 0)
			{
				_bAtEnd = true;
				return false;
			}
			_cCurrent = _text[0];
			return true;
		}


		private void reset()
		{
			_text = "";
			_nIndex = 0;
			_nRow = 0;
			_nColumn = 0;
			_bAtEnd = false;
		}

		private void advance(int next)
		{
			for (int i = 0; i < next; i++)
			{
				advance();
			}
		}
		private void advance()
		{
			count();
			_nIndex++;
			if (_nIndex >= _text.Length)
			{
				_bAtEnd = true;
				return;
			}
			_cCurrent = _text[_nIndex];
		}

		private void count()
		{
			if (_settings.AllOneLine)
			{
				_nColumn++;
				return;
			}
			if (_cCurrent == '\n')
			{
				_nRow++;
				_nColumn = 0;
			}
			else if (_cCurrent == '\r')
			{
				_nColumn = 0;
			}
			else if (_cCurrent == '\t')
			{
				_nColumn += 4;
			}
			else
			{
				_nColumn++;
			}
		}

		private string lookAhead(uint peek)
		{
			string result = "";

			for (uint i = 0; i < peek; i++)
			{
				if (_nIndex + i < _text.Length)
				{
					result += _text[Convert.ToInt32(_nIndex + i)];
					continue;
				}
				break;
			}
			return result;
		}


		// Multi-Character token processing

		private Token str(string enclosing)
		{
			StringBuilder result = new StringBuilder();
			bool bSlash = false;
			while (!_bAtEnd && (lookAhead(Convert.ToUInt32(enclosing.Length)) != enclosing || bSlash))
			{
				if (_cCurrent == '\\' && !bSlash)
				{
					bSlash = true;
					advance();
					continue;
				}
				if (bSlash)
				{
					if (_cCurrent == 'n')
					{
						result.Append('\n');
						advance();
					}
					else if (_cCurrent == 't')
					{
						result.Append('\t');
						advance();
					}
					else if (_cCurrent == 'r')
					{
						result.Append('\r');
						advance();
					}
					else if (_cCurrent == 'a')
					{
						result.Append('\a');
						advance();
					}
					else if (_cCurrent == 'b')
					{
						result.Append('\b');
						advance();
					}
					else if (_cCurrent == 'v')
					{
						result.Append('\v');
						advance();
					}
					else if (_cCurrent == 'f')
					{
						result.Append('\f');
						advance();
					}
					else if (_cCurrent == '"')
					{
						result.Append('\"');
						advance();
					}
					else if (_cCurrent == '\'')
					{
						result.Append('\'');
						advance();
					}
					else if (_cCurrent == '0')
					{
						result.Append('\0');
						advance();
					}
					else if (_cCurrent == '\\')
					{
						result.Append('\\');
						advance();
					}
					else
					{
						result.Append('\\');
						result.Append(_cCurrent);
						advance();
					}
					bSlash = false;
					continue;
				}
				else
				{
					result.Append(_cCurrent);
					advance();
					bSlash = false;
				}
			}

			if (!_bAtEnd)
			{
				advance(enclosing.Length);
			}
			return new Token(TokenTypes.TTString, result.ToString(), _nRow, _nColumn, enclosing, enclosing);
		}

		private Token enclosed(string tokenType, string enclosing)
		{
			StringBuilder result = new StringBuilder();
			while (!_bAtEnd && lookAhead(Convert.ToUInt32(enclosing.Length)) != enclosing)
			{
				result.Append(_cCurrent);
				advance();
			}

			if (!_bAtEnd)
			{
				advance(enclosing.Length);
			}
			return new Token(tokenType, result.ToString(), _nRow, _nColumn, enclosing, enclosing);
		}

		private Token word()
		{
			StringBuilder result = new StringBuilder();

			string type = TokenTypes.TTWord;
			while (!_bAtEnd && (_cCurrent == '_' || Char.IsLetterOrDigit(_cCurrent) || _cCurrent == '\0' || _settings.WordIncluded.Contains(_cCurrent)))
			{
				if (_cCurrent != '\0')
				{
					result.Append(_cCurrent);
				}
				advance();
			}


			foreach (TokenizerRule rule in _rules)
			{
				if (CompareRule(result.ToString(), rule.Value))
				{
					type = rule.Type;
					result.Clear();
					result.Append(rule.GetValueOrMacro());
					break;
				}
			}

			return new Token(type, result.ToString(), _nRow, _nColumn);
		}

		private Token strcatalyst(string precursor)
		{
			StringBuilder result = new StringBuilder();
			bool bSlash = false;
			while (!_bAtEnd && (_settings.StringCatalystExcluded.Contains(_cCurrent) ? _settings.StringCatalystEscapable.Contains(_cCurrent) && bSlash : true))
			{
				if (_cCurrent == '\\' && !bSlash)
				{
					bSlash = true;
					advance();
					continue;
				}
				if (bSlash)
				{
					if (_settings.StringCatalystEscapable.Contains(_cCurrent))
					{
						result.Append(_cCurrent);
						advance();
					}
					else
					{
						result.Append('\\');
						result.Append(_cCurrent);
						advance();
					}

					bSlash = false;
					continue;
				}
				else
				{
					result.Append(_cCurrent);
					advance();
					bSlash = false;
				}
			}


			return new Token(TokenTypes.TTString, result.ToString(), _nRow, _nColumn, precursor);
		}

		private Token number()
		{
			StringBuilder result = new StringBuilder();

			bool bHadDecimal = false;
			bool bIsFloat = false;
			bool bIsDouble = false;
			bool bIsUnsigned = false;
			while (!_bAtEnd && (Char.IsDigit(_cCurrent) || (_cCurrent == '.' && !bHadDecimal) || (_cCurrent == 'f' && bHadDecimal) || (_cCurrent == 'd' && bHadDecimal) || (_cCurrent == 'u' && !bHadDecimal)))
			{
				if (_cCurrent == '.')
				{
					bHadDecimal = true;
				}
				if (_cCurrent == 'f')
				{
					bIsFloat = true;
					advance();
					break;
				}
				if (_cCurrent == 'd')
				{
					bIsDouble = true;
					advance();
					break;
				}
				if (_cCurrent == 'u')
				{
					bIsUnsigned = true;
					advance();
					break;
				}

				result.Append(_cCurrent);
				advance();
			}

			string type = TokenTypes.TTInteger;
			if (bHadDecimal)
			{
				if (bIsFloat)
				{
					type = TokenTypes.TTFloat;
				}
				else if (bIsDouble)
				{
					type = TokenTypes.TTDouble;
				}
				else
				{
					type = TokenTypes.TTDouble;
				}
			}
			else if (bIsUnsigned)
			{
				type = type = TokenTypes.TTUnsignedInteger;
			}
			return new Token(type, result.ToString(), _nRow, _nColumn);
		}


		private Token tokenliteral()
		{
			StringBuilder result = new StringBuilder();

			while (!_bAtEnd && _cCurrent != '~')
			{
				result.Append(_cCurrent);
				advance();
			}

			if (!_bAtEnd)
			{
				advance();
			}
			return new Token(TokenTypes.TTWord, result.ToString(), _nRow, _nColumn);
		}


		private Token eolcomment(string type)
		{
			StringBuilder result = new StringBuilder();
			while (!_bAtEnd && _cCurrent != '\n' && _cCurrent != '\r')
			{
				result.Append(_cCurrent);
				advance();
			}
			return new Token(type, result.ToString(), _nRow, _nColumn);
		}

		private void mlcomment()
		{
			// if rule for comment end is not found, default to newline
			string mlCommentEnclosing = "\n";
			var MlCommentEnd = _rules.Where(rule => { return rule.Type == TokenTypes.MLCommentEnd; }).FirstOrDefault();
			if (MlCommentEnd != null)
			{
				mlCommentEnclosing = MlCommentEnd.GetValueOrMacro();
			}

			while (!_bAtEnd && lookAhead(Convert.ToUInt32(mlCommentEnclosing.Length)) != mlCommentEnclosing)
			{
				advance();
			}
			if (!_bAtEnd)
			{
				advance(mlCommentEnclosing.Length);
			}
		}
	}
}

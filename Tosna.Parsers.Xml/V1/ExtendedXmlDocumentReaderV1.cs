using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using GrammarV1;
using Tosna.Core.Documents;

namespace Tosna.Parsers.Xml.V1
{
	public class ExtendedXmlDocumentReaderV1 : IDocumentReader
	{
		public Document ReadDocument(string fileName)
		{
			var content = File.ReadAllText(fileName);
			return ParseDocument(content, fileName);
		}

		public Document ParseDocument(string content, string fileName)
		{
			var stream = CharStreams.fromString(content);
			var lexer = new XMLLexer(stream);
			var lexerErrorListener = new LexerErrorListener();
			lexer.AddErrorListener(lexerErrorListener);
			var tokens = new CommonTokenStream(lexer);
			var parser = new XMLParser(tokens) { BuildParseTree = true };

			var parseTree = parser.document();

			var parseTreeListener = new Listener();

			ParseTreeWalker.Default.Walk(parseTreeListener, parseTree);
			
			if (parseTreeListener.TopElement != null)
			{
				parseTreeListener.TopElement.Errors.AddRange(lexerErrorListener.LexerErrors);
			}

			return new Document(parseTreeListener.TopElement ?? CreateInvalidTopElement(),
				new DocumentInfo(fileName, DocumentFormat.Xml));
		}

		private static DocumentElement CreateInvalidTopElement()
		{
			var invalidTopElement = new DocumentElement("Invalid");
			invalidTopElement.Errors.Add(new DocumentError("Root element not found",
				DocumentErrorCode.ParsingProblem, DocumentElementLocation.Unknown));
			return invalidTopElement;
		}

		#region Nested

		private class LexerErrorListener : IAntlrErrorListener<int>
		{
			public readonly IList<DocumentError> LexerErrors = new List<DocumentError>();

			public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line,
				int charPositionInLine,
				string msg, RecognitionException e)
			{
				LexerErrors.Add(new DocumentError(
					error: msg,
					code: DocumentErrorCode.LexerProblem,
					problemLocation: new DocumentElementLocation(lineStart: line, columnStart: charPositionInLine,
						lineEnd: line, columnEnd: charPositionInLine + 1)));
			}
		}
		
		private class Listener : XMLParserBaseListener
		{
			public DocumentElement TopElement;

			private readonly Stack<DocumentElement> elements = new Stack<DocumentElement>();

			public override void VisitErrorNode(IErrorNode node)
			{
				if (elements.Any())
				{
					elements.Peek().Errors.Add(new DocumentError($"Unexpected input '{node.GetText()}'",
						DocumentErrorCode.ParsingProblem, DocumentElementLocation.Unknown));
				}
			}

			public override void EnterValidElement(XMLParser.ValidElementContext context)
			{
				var elementLocation = CreateLocation(context);

				var validOpen = context.validOpen();
				var validClose = context.validClose();
				var validOpenShort = context.validOpenShort();

				var name = validOpen?.Name().GetText() ?? validOpenShort?.Name().GetText() ?? string.Empty;
				var newDocumentElement = new DocumentElement(name)
				{
					Location = elementLocation
				};
				
				if (validClose != null)
				{
					var name2 = validClose.Name().GetText();
					if (name != name2)
					{
						newDocumentElement.Errors.Add(new DocumentError(
							error: $"Invalid closing statement {name2}. Expected {name}",
							code: DocumentErrorCode.XmlOpenCloseTagsMismatch,
							problemLocation: CreateLocation(validClose),
							name, name2));
					}
				}

				if (elements.Any())
				{
					elements.Peek().Children.Add(newDocumentElement);
				}

				elements.Push(newDocumentElement);

				if (TopElement == null)
				{
					TopElement = newDocumentElement;
				}
			}

			public override void ExitValidElement(XMLParser.ValidElementContext context)
			{
				elements.Pop();
			}

			public override void EnterInvalidElement(XMLParser.InvalidElementContext context)
			{
				var name = context.validOpen()?.Name()?.GetText() ??
				           context.invalidOpen()?.Name()?.GetText() ?? string.Empty;
				
				var newDocumentElement = new DocumentElement(name)
				{
					Location = CreateLocation(context),
				};

				var invalidClose = context.invalidClose();
				if (invalidClose != null)
				{
					newDocumentElement.Errors.Add(new DocumentError(
						error: $"Unfinished element {name}",
						code: DocumentErrorCode.XmlUnfinishedElement,
						problemLocation: CreateLocation(invalidClose),
						name, invalidClose.Name()?.GetText() ?? string.Empty));
				}
				else
				{
					newDocumentElement.Errors.Add(new DocumentError(
						error: $"Unfinished element {name}",
						code: DocumentErrorCode.XmlUnfinishedElement,
						problemLocation: CreateLocation(context),
						name));
				}
				
				if (elements.Any())
				{
					elements.Peek().Children.Add(newDocumentElement);
				}

				elements.Push(newDocumentElement);

				if (TopElement == null)
				{
					TopElement = newDocumentElement;
				}
			}

			public override void ExitInvalidElement(XMLParser.InvalidElementContext context)
			{
				elements.Pop();
			}

			public override void EnterAttribute(XMLParser.AttributeContext context)
			{
				if (!elements.Any())
				{
					return;
				}

				var content = context.STRING().GetText();
				
				elements.Peek().Children.Add(new DocumentElement(context.Name().GetText())
				{
					Content = content.Substring(1, content.Length - 2), // removing quotes
					Location = CreateLocation(context)
				});
			}

			public override void EnterDuplicateElement(XMLParser.DuplicateElementContext context)
			{
				TopElement?.Errors.Add(new DocumentError(
					"Only one top element allowed on top of XML document",
					DocumentErrorCode.ParsingProblem,
					CreateLocation(context)));

				base.EnterDuplicateElement(context);
			}

			private static DocumentElementLocation CreateLocation(ParserRuleContext context)
			{
				return new DocumentElementLocation(
					lineStart: context.Start.Line,
					columnStart: context.Start.Column,
					lineEnd: context.Stop.Line,
					columnEnd: context.Stop.Column + context.Stop.Text.Length);
			}
		}

		#endregion
	}
}
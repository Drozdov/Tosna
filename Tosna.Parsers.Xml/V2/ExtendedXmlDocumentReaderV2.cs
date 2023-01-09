using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using GrammarV2;
using Tosna.Core.Documents;

namespace Tosna.Parsers.Xml.V2
{
	/// <summary>
	/// Extended XML document reader with more information providing on elements location, problems details
	/// (may be used for solutions finding)
	/// </summary>
	public class ExtendedXmlDocumentReaderV2 : IDocumentReader
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
			
			parseTreeListener.End();

			if (parseTreeListener.TopElement != null)
			{
				parseTreeListener.TopElement.Errors.AddRange(lexerErrorListener.LexerErrors);
				parseTreeListener.TopElement.Errors.AddRange(parseTreeListener.GlobalErrors);
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
			
			public readonly List<DocumentError> GlobalErrors =
				new List<DocumentError>();
			
			private readonly Stack<DocumentElement> elements = new Stack<DocumentElement>();

			public void End()
			{
				PopElement(null);
			}

			public override void EnterElement(XMLParser.ElementContext context)
			{
				base.EnterElement(context);
				
				var opening = context.opening().GetText();
				var closing = context.closing().GetText();

				var name = context.Name()?.GetText() ?? string.Empty;

				if (opening == "") // <SomeElement /> or <SomeElement>
				{
					var element = new DocumentElement(name)
					{
						Location = CreateLocation(context)
					};
					
					// <SomeUnfinishedElement
					if (context.CLOSE() == null)
					{
						element.Errors.Add(new DocumentError(error: $"Unfinished element '{name}'",
							code: DocumentErrorCode.XmlUnfinishedElement,
							problemLocation: CreateLocation(context),
							name));
					}
					// </>
					else if (name == string.Empty)
					{
						element.Errors.Add(new DocumentError(error: "Empty name",
							code: DocumentErrorCode.ParsingProblem,
							problemLocation: CreateLocation(context)));
					}

					foreach (var attributeContext in context.attribute() ?? new XMLParser.AttributeContext[]{})
					{
						var content = attributeContext.STRING().GetText();
						content = content.Substring(1, content.Length - 2); // Getting rid of quotes
						element.Children.Add(new DocumentElement(attributeContext.Name().GetText())
						{
							Content = content,
							Location = CreateLocation(attributeContext)
						});
					}
					
					PushElement(element);

					switch (closing)
					{
						case "/":
							PopElement(element);
							break;
						
						case "?":
						case "!":
							// <SomeElement?>
							element.Errors.Add(new DocumentError(
								error: $"Unexpected symbol {closing}",
								code: DocumentErrorCode.ParsingProblem,
								problemLocation: CreateLocation(context.closing())));
							break;
					}
				}
				else if (opening == "/") // </SomeElement>
				{
					var element = PeekElement(name);
					if (element == null)
					{
						GlobalErrors.Add(new DocumentError($"No opening tag <{name}> found",
							DocumentErrorCode.ParsingProblem, CreateLocation(context)));
						return;
					}
					
					// </SomeUnfinishedElement
					if (context.CLOSE() == null)
					{
						element.Errors.Add(new DocumentError(error: $"Unfinished element '{name}'",
							code: DocumentErrorCode.XmlUnfinishedElement,
							problemLocation: CreateLocation(context),
							name));
					}

					if (element.Name != name)
					{
						element.Errors.Add(new DocumentError(
							error: $"Invalid closing statement {name}. Expected {element.Name}",
							code: DocumentErrorCode.XmlOpenCloseTagsMismatch,
							problemLocation: CreateLocation(context),
							element.Name, name));
					}
					
					var attribute = context.attribute().FirstOrDefault();
					if (attribute != null)
					{
						element.Errors.Add(new DocumentError(
							error: "Attributes cannot be present in closing tag",
							code: DocumentErrorCode.ParsingProblem,
							problemLocation: CreateLocation(attribute)));
					}
					
					PopElement(element);
				}
			}

			public override void VisitErrorNode(IErrorNode node)
			{
				var location = new DocumentElementLocation(
					lineStart: node.Symbol.Line,
					columnStart: node.Symbol.Column,
					lineEnd: node.Symbol.Line,
					columnEnd: node.Symbol.Column + node.GetText().Length);
				var error = new DocumentError(node.GetText(),
					DocumentErrorCode.ParsingProblem, location);

				if (elements.Any())
				{
					var documentElement = elements.Peek();
					documentElement.Errors.Add(error);
				}
				else
				{
					GlobalErrors.Add(error);
				}

				base.VisitErrorNode(node);
			}

			private void PushElement(DocumentElement documentElement)
			{
				if (TopElement == null)
				{
					TopElement = documentElement;
				}
				else
				{
					var elementOnStack = elements.Peek();
					if (elementOnStack == null)
					{
						GlobalErrors.Add(new DocumentError(
							error: $"Multiple items on top level: {TopElement.Name}, {documentElement.Name}",
							code: DocumentErrorCode.ParsingProblem,
							problemLocation: DocumentElementLocation.Unknown));
					}
					else
					{
						elementOnStack.Children.Add(documentElement);
					}
				}
				
				elements.Push(documentElement);
			}

			private DocumentElement PeekElement(string name)
			{
				foreach (var documentElement in elements.Where(documentElement => documentElement.Name == name))
				{
					return documentElement;
				}

				return elements.FirstOrDefault();
			}

			private void PopElement(DocumentElement element)
			{
				while (elements.Any())
				{
					var pop = elements.Pop();
					if (pop != element)
					{
						pop.Errors.Add(new DocumentError($"Tag <{pop.Name}> not closed",
							DocumentErrorCode.XmlUnfinishedElement, pop.Location));
					}
					else
					{
						break;
					}
				}
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Grammar;
using Tosna.Core.Documents;

namespace Tosna.Parsers.Xml.DocumentReading
{
	public class ExtendedXmlDocumentReader : IDocumentReader
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
			var tokens = new CommonTokenStream(lexer);
			var parser = new XMLParser(tokens) { BuildParseTree = true };

			var parseTree = parser.document();

			var parseTreeListener = new Listener();

			try
			{
				ParseTreeWalker.Default.Walk(parseTreeListener, parseTree);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace);
			}

			return new Document(parseTreeListener.TopElement, new DocumentInfo(fileName, DocumentFormat.Xml));
		}

		#region Nested

		private class Listener : XMLParserBaseListener
		{
			public DocumentElement TopElement;

			private readonly Stack<DocumentElement> elements = new Stack<DocumentElement>();

			public override void VisitErrorNode(IErrorNode node)
			{
				if (elements.Any() && elements.Peek().ValidationInfo.IsValid)
				{
					elements.Peek().ValidationInfo =
						DocumentElementValidationInfo.CreateInvalid($"Unexpected input '{node.GetText()}'",
							DocumentValidationCode.ParsingProblem);
				}
			}

			public override void EnterValidElement(XMLParser.ValidElementContext context)
			{
				var names = context.Name().Select(n => n.GetText()).ToArray();
				var newDocumentElement = new DocumentElement(names.FirstOrDefault() ?? "Unknown")
				{
					Location = CreateLocation(context)
				};

				switch (names.Length)
				{
					case 0:
						newDocumentElement.ValidationInfo = DocumentElementValidationInfo.CreateInvalid(
							error: "Unknown error",
							code: DocumentValidationCode.ParsingProblem);
						break;

					case 2 when names[0] != names[1]:
						newDocumentElement.ValidationInfo = DocumentElementValidationInfo.CreateInvalid(
							$"Invalid closing statement {names[1]}. Expected {names[0]}",
							DocumentValidationCode.XmlOpenCloseTagsMismatch);
						break;
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
				var names = context.Name().Select(n => n.GetText()).ToArray();
				var name = names.FirstOrDefault() ?? "Unknown";
				var newDocumentElement = new DocumentElement(name)
				{
					Location = CreateLocation(context),
					ValidationInfo = DocumentElementValidationInfo.CreateInvalid(
						error: $"Unfinished element {name}",
						code: DocumentValidationCode.XmlUnfinishedElement,
						errorParameters: name)
				};

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

			private static DocumentElementLocation CreateLocation(ParserRuleContext context)
			{
				return new DocumentElementLocation(
					lineStart: context.Start.Line,
					columnStart: context.Start.Column,
					lineEnd: context.Stop.Line,
					columnEnd: context.Stop.Column);
			}
		}

		#endregion
	}
}
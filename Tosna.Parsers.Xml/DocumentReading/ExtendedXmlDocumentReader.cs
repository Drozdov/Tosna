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

			ParseTreeWalker.Default.Walk(parseTreeListener, parseTree);

			return new Document(parseTreeListener.TopElement ?? CreateInvalidTopElement(),
				new DocumentInfo(fileName, DocumentFormat.Xml));
		}

		private static DocumentElement CreateInvalidTopElement()
		{
			return new DocumentElement("Invalid")
			{
				ValidationInfo = DocumentElementValidationInfo.CreateInvalid("Root element not found",
					DocumentValidationCode.ParsingProblem, DocumentElementLocation.Unknown)
			};
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
							DocumentValidationCode.ParsingProblem, DocumentElementLocation.Unknown);
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
						newDocumentElement.ValidationInfo = DocumentElementValidationInfo.CreateInvalid(
							error: $"Invalid closing statement {name2}. Expected {name}",
							code: DocumentValidationCode.XmlOpenCloseTagsMismatch,
							problemLocation: CreateLocation(validClose),
							name, name2);
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
					newDocumentElement.ValidationInfo = DocumentElementValidationInfo.CreateInvalid(
						error: $"Unfinished element {name}",
						code: DocumentValidationCode.XmlUnfinishedElement,
						problemLocation: CreateLocation(invalidClose),
						name, invalidClose.Name()?.GetText() ?? string.Empty);
				}
				else
				{
					newDocumentElement.ValidationInfo = DocumentElementValidationInfo.CreateInvalid(
						error: $"Unfinished element {name}",
						code: DocumentValidationCode.XmlUnfinishedElement,
						problemLocation: CreateLocation(context),
						name);
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
				if (TopElement != null)
				{
					TopElement.ValidationInfo =
						DocumentElementValidationInfo.CreateInvalid(
							"Only one top element allowed on top of XML document",
							DocumentValidationCode.ParsingProblem,
							CreateLocation(context));
				}

				base.EnterDuplicateElement(context);
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Grammar;
using Tosna.Core.Documents;

namespace Tosna.Parsers.Xml.DocumentReading
{
	/// <summary>
	/// Extended XML document reader with more information providing on elements location, problems details
	/// (may be used for solutions finding)
	/// </summary>
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

			if (parseTreeListener.GlobalErrors.Any() && parseTreeListener.TopElement != null
			    && parseTreeListener.TopElement.ValidationInfo.IsValid)
			{
				parseTreeListener.TopElement.ValidationInfo = parseTreeListener.GlobalErrors.First();
			}

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

			public IReadOnlyCollection<DocumentElementValidationInfo> GlobalErrors => globalErrors;

			private readonly Stack<DocumentElement> elements = new Stack<DocumentElement>();
			private readonly Stack<ParserRuleContext> contexts = new Stack<ParserRuleContext>();

			private readonly List<DocumentElementValidationInfo> globalErrors =
				new List<DocumentElementValidationInfo>();

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
						element.ValidationInfo = DocumentElementValidationInfo.CreateInvalid(
							error: $"Unfinished element '{name}'",
							code: DocumentValidationCode.XmlUnfinishedElement,
							problemLocation: CreateLocation(context),
							name);
					}
					// </>
					else if (name == string.Empty)
					{
						element.ValidationInfo = DocumentElementValidationInfo.CreateInvalid(
							error: "Empty name",
							code: DocumentValidationCode.ParsingProblem,
							problemLocation: CreateLocation(context));
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
							element.ValidationInfo = DocumentElementValidationInfo.CreateInvalid(
								error: $"Unexpected symbol {closing}",
								code: DocumentValidationCode.ParsingProblem,
								problemLocation: CreateLocation(context.closing()));
							break;
					}
				}
				else if (opening == "/") // </SomeElement>
				{
					var element = PeekElement(name);
					if (element == null)
					{
						globalErrors.Add(DocumentElementValidationInfo.CreateInvalid($"No opening tag <{name}> found",
							DocumentValidationCode.ParsingProblem, CreateLocation(context)));
						return;
					}

					if (element.Name != name)
					{
						element.ValidationInfo = DocumentElementValidationInfo.CreateInvalid(
							error: $"Invalid closing statement {name}. Expected {element.Name}",
							code: DocumentValidationCode.XmlOpenCloseTagsMismatch,
							problemLocation: CreateLocation(context),
							element.Name, name);
					}
					
					var attribute = context.attribute().FirstOrDefault();
					if (attribute != null)
					{
						if (element.ValidationInfo.IsValid)
						{
							element.ValidationInfo = DocumentElementValidationInfo.CreateInvalid(
								"Attributes cannot be present in closing tag",
								DocumentValidationCode.ParsingProblem,
								CreateLocation(attribute));
						}
					}
					
					PopElement(element);
				}
			}

			public override void EnterEveryRule(ParserRuleContext context)
			{
				base.EnterEveryRule(context);
				contexts.Push(context);
			}

			public override void ExitEveryRule(ParserRuleContext context)
			{
				base.ExitEveryRule(context);
				contexts.Pop();
			}

			public override void VisitErrorNode(IErrorNode node)
			{
				var location = contexts.Any() ? CreateLocation(contexts.Peek()) : DocumentElementLocation.Unknown;
				var error = DocumentElementValidationInfo.CreateInvalid(node.GetText(),
					DocumentValidationCode.ParsingProblem, location);

				if (elements.Any())
				{
					var documentElement = elements.Peek();
					if (documentElement.ValidationInfo.IsValid)
					{
						documentElement.ValidationInfo = error;
					}
				}
				else
				{
					globalErrors.Add(error);
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
						globalErrors.Add(DocumentElementValidationInfo.CreateInvalid(
							$"Multiple items on top level: {TopElement.Name}, {documentElement.Name}",
							DocumentValidationCode.ParsingProblem, DocumentElementLocation.Unknown));
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
				if (!elements.Any())
				{
					return null;
				}
				
				// If top element satisfies condition 
				var documentElement = elements.Peek();
				if (documentElement.Name == name)
				{
					return documentElement;
				}

				if (elements.Count == 1)
				{
					return null;
				}
				elements.Pop();
				// If second from top element satisfies condition 
				var secondDocumentElement = elements.Peek();
				if (secondDocumentElement.Name == name)
				{
					return secondDocumentElement;
				}
				// else: return all like it was before
				elements.Push(documentElement);
				return documentElement;
			}

			private void PopElement(DocumentElement element)
			{
				DocumentElement lastPop;
				do
				{
					lastPop = elements.Pop();
				} while (lastPop != element);
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
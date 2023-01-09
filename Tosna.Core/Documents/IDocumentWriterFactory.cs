namespace Tosna.Core.Documents
{
	public interface IDocumentWriterFactory
	{
		IDocumentWriter CreateWriter();
	}
}
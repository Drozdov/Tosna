namespace Tosna.Core.Imprints.Generation
{
	public interface IObjectResolver
	{
		object Get(Imprint imprint);

		object Get(ImprintIdentifier identifier);
	}
}
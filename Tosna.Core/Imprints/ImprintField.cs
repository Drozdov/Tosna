using Tosna.Core.Imprints.Fields;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Imprints
{
	public abstract class ImprintField
	{
		protected ImprintField(SerializingElement info)
		{
			Info = info;
		}

		public SerializingElement Info { get; }

		public abstract void Accept(IImprintFieldVisitor visitor);
	}
	
	public interface IImprintFieldVisitor
	{
		void Visit(SimpleTypeImprintField field);

		void Visit(NestedImprintField field);

		void Visit(ArrayImprintField field);
	}
}
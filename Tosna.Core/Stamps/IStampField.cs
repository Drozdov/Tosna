using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Stamps
{
	public interface IStampField
	{
		SerializingElement SerializingElement { get; }

		void Accept(IStampFieldVisitor visitor);
	}
	
	public interface IStampFieldVisitor
	{
		void Visit(SimpleTypeField simpleTypeField);

		void Visit(StampField stampField);

		void Visit(ArrayStampField arrayStampField);
	}
}
using System.Collections.Generic;

namespace Tosna.Core.Stamps
{
	public class StampsDependenciesGetter : IStampFieldVisitor
	{
		private IReadOnlyCollection<Stamp> dependencies;

		private StampsDependenciesGetter()
		{
		}

		public static IReadOnlyCollection<Stamp> GetDependencies(IStampField field)
		{
			var visitor = new StampsDependenciesGetter();
			field.Accept(visitor);
			return visitor.dependencies;
		}

		void IStampFieldVisitor.Visit(SimpleTypeField simpleTypeField)
		{
			dependencies = new Stamp[] { };
		}

		void IStampFieldVisitor.Visit(StampField stampField)
		{
			dependencies = new[] {stampField.Value};
		}

		void IStampFieldVisitor.Visit(ArrayStampField arrayStampField)
		{
			dependencies = arrayStampField.Values;
		}
	}
}
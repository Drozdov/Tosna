using System.Collections.Generic;

namespace Tosna.Core.Common.Stamps
{
	public class StampsDependenciesGetter : IStampPropertyVisitor
	{
		private IReadOnlyCollection<Stamp> dependencies;

		private StampsDependenciesGetter()
		{
		}

		public static IReadOnlyCollection<Stamp> GetDependencies(IStampProperty property)
		{
			var visitor = new StampsDependenciesGetter();
			property.Visit(visitor);
			return visitor.dependencies;
		}

		void IStampPropertyVisitor.Visit(SimpleTypeProperty simpleTypeProperty)
		{
			dependencies = new Stamp[] { };
		}

		void IStampPropertyVisitor.Visit(StampProperty stampProperty)
		{
			dependencies = new[] {stampProperty.Value};
		}

		void IStampPropertyVisitor.Visit(ArrayStampProperty arrayStampProperty)
		{
			dependencies = arrayStampProperty.Values;
		}
	}
}
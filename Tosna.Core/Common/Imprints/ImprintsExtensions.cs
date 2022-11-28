using System;
using System.Collections.Generic;
using System.Linq;

namespace Tosna.Core.Common.Imprints
{
	public static class ImprintsExtensions
	{
		public static IEnumerable<Imprint> GetNestedImprints(this Imprint imprint)
		{
			return NestedImprintsGetter.GetNested(imprint);
		}

		public static IEnumerable<Imprint> GetNestedImprintsRecursively(this IEnumerable<Imprint> imprints)
		{
			return SelectRecursive(imprints, GetNestedImprints);
		}

		public static IEnumerable<Imprint> GetNestedImprintsRecursively(this Imprint imprint)
		{
			return GetNestedImprintsRecursively(new[] {imprint});
		}

		public static IEnumerable<ImprintIdentifier> GetExternalDependencies(this Imprint imprint)
		{
			return ImprintsDependenciesGetter.GetExternalDependencies(imprint);
		}

		public static IEnumerable<ImprintIdentifier> GetExternalDependenciesRecursively(this IEnumerable<Imprint> imprints)
		{
			return GetNestedImprintsRecursively(imprints).SelectMany(GetExternalDependencies).Distinct();
		}

		public static IEnumerable<ImprintIdentifier> GetExternalDependenciesRecursively(this Imprint imprint)
		{
			return GetExternalDependenciesRecursively(new[] { imprint });
		}

		public static string GetCompactDescription(this Imprint imprint)
		{
			if (imprint.TryGetPublicName(out var publicName))
			{
				return publicName;
			}

			if (imprint.TryGetId(out var id))
			{
				return id;
			}

			return imprint.Type.Name;
		}

		private static IEnumerable<T> SelectRecursive<T>(this IEnumerable<T> subjects, Func<T, IEnumerable<T>> selector)
		{
			if (subjects == null)
				yield break;

			var stack = new Stack<IEnumerator<T>>();

			stack.Push(subjects.GetEnumerator());

			while (stack.Count > 0)
			{
				var en = stack.Peek();
				if (en.MoveNext())
				{
					var subject = en.Current;
					yield return subject;

					stack.Push(selector(subject).GetEnumerator());
				}
				else
				{
					stack.Pop().Dispose();
				}
			}
		}
	}

	public class ImprintsDependenciesGetter : IImprintVisitor
	{
		private IEnumerable<ImprintIdentifier> dependencies;

		private ImprintsDependenciesGetter()
		{
		}

		public static IEnumerable<ImprintIdentifier> GetExternalDependencies(Imprint imprint)
		{
			var visitor = new ImprintsDependenciesGetter();
			imprint.Visit(visitor);
			return visitor.dependencies;
		}

		public void Visit(SimpleTypeImprint imprint)
		{
			dependencies = Enumerable.Empty<ImprintIdentifier>();
		}

		public void Visit(AggregateImprint imprint)
		{
			dependencies = Enumerable.Empty<ImprintIdentifier>();
		}

		public void Visit(ReferenceImprint imprint)
		{
			dependencies = new[] {new ImprintIdentifier(imprint.ReferenceId, imprint.AbsoluteReferencePath)};
		}
	}

	public class NestedImprintsGetter : IImprintVisitor, IImprintFieldVisitor
	{
		private IEnumerable<Imprint> dependencies;

		private NestedImprintsGetter()
		{
		}

		public static IEnumerable<Imprint> GetNested(Imprint imprint)
		{
			var visitor = new NestedImprintsGetter();
			imprint.Visit(visitor);
			return visitor.dependencies;
		}

		public static IEnumerable<Imprint> GetNested(ImprintField field)
		{
			var visitor = new NestedImprintsGetter();
			field.Visit(visitor);
			return visitor.dependencies;
		}

		void IImprintVisitor.Visit(SimpleTypeImprint imprint)
		{
			dependencies = new Imprint[]{};
		}

		void IImprintVisitor.Visit(AggregateImprint imprint)
		{
			dependencies = imprint.Fields.SelectMany(GetNested);
		}

		void IImprintVisitor.Visit(ReferenceImprint imprint)
		{
			dependencies = new Imprint[]{};
		}

		void IImprintFieldVisitor.Visit(SimpleTypeImprintField field)
		{
			dependencies = Enumerable.Empty<Imprint>();
		}

		void IImprintFieldVisitor.Visit(NestedImprintField field)
		{
			dependencies = new[] { field.NestedItem };
		}

		void IImprintFieldVisitor.Visit(ArrayImprintField field)
		{
			dependencies = field.Items;
		}
	}
}

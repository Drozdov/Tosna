using System;
using System.Runtime.Serialization;

namespace Tosna.Core.Common.Imprints.Generation
{
	public class ObjectFactory : IImprintVisitor, IImprintFieldVisitor
	{
		private object result;

		private readonly IObjectResolver resolver;

		private ObjectFactory(IObjectResolver resolver)
		{
			this.resolver = resolver;
		}

		public static object Create(Imprint imprint, IObjectResolver resolver)
		{
			var visitor = new ObjectFactory(resolver);
			imprint.Visit(visitor);
			return visitor.result;
		}

		public static object Create(ImprintField imprintField, IObjectResolver resolver)
		{
			var visitor = new ObjectFactory(resolver);
			imprintField.Visit(visitor);
			return visitor.result;
		}

		#region Private

		void IImprintVisitor.Visit(SimpleTypeImprint imprint)
		{
			result = imprint.Value;
		}

		void IImprintVisitor.Visit(AggregateImprint imprint)
		{
			var obj = FormatterServices.GetUninitializedObject(imprint.Type);

			foreach (var field in imprint.Fields)
			{
				field.Info.SetValue(obj, Create(field, resolver));
			}

			result = obj;
		}

		void IImprintVisitor.Visit(ReferenceImprint imprint)
		{
			result = resolver.Get(new ImprintIdentifier(imprint.ReferenceId, imprint.AbsoluteReferencePath));
		}

		void IImprintFieldVisitor.Visit(SimpleTypeImprintField field)
		{
			result = field.Value;
		}

		void IImprintFieldVisitor.Visit(NestedImprintField field)
		{
			result = resolver.Get(field.NestedItem);
		}

		void IImprintFieldVisitor.Visit(ArrayImprintField field)
		{
			var array = (Array)Activator.CreateInstance(field.Info.Type, field.Items.Count);
			for (var i = 0; i < array.Length; i++)
			{
				array.SetValue(resolver.Get(field.Items[i]), i);
			}
			result = array;
		}

		#endregion
	}
}

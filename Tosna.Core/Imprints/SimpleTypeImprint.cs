using System;

namespace Tosna.Core.Imprints
{
	public sealed class SimpleTypeImprint : Imprint
	{
		public object Value { get; }

		public SimpleTypeImprint(Type type, string publicName, string id, ImprintInfo info, object value) : base(type, publicName, id, info)
		{
			Value = value;
		}

		public override void Visit(IImprintVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
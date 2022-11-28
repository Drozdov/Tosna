using Tosna.Core.Common.Imprints;

namespace Tosna.Editor.IDE.Vm
{
	public class ImprintInfoVm
	{
		internal Imprint Imprint { get; }

		public string TypeName { get; }

		public bool IsRootImprint { get; }

		public string Id { get; }

		public ImprintInfoVm(Imprint imprint, bool isRootImprint)
		{
			Imprint = imprint;
			TypeName = imprint.TryGetInfo(out var info) ? info.TypeName : "<unknown>";
			IsRootImprint = isRootImprint;

			Id = Imprint.TryGetId(out var id) ? id : null;
		}

		public string Description
		{
			get
			{
				if (!Imprint.TryGetPublicName(out var publicName))
				{
					publicName = "-";
				}

				if (!Imprint.TryGetPublicName(out var id))
				{
					id = "-";
				}

				return $"{TypeName} ({Imprint.Type.FullName}); ID={id}; PublicName={publicName}";
			}
		}

		protected bool Equals(ImprintInfoVm other)
		{
			return Equals(Imprint, other.Imprint) && string.Equals(TypeName, other.TypeName);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((ImprintInfoVm) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Imprint != null ? Imprint.GetHashCode() : 0) * 397) ^ (TypeName != null ? TypeName.GetHashCode() : 0);
			}
		}
	}
}

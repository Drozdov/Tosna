using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tosna.Core.Helpers;
using Tosna.Core.Imprints;
using Tosna.Core.Imprints.Fields;
using Tosna.Core.IO;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Stamps
{
	public class StampImprintsFactory
	{
		private readonly IDictionary<Stamp, StampImprint> map = new Dictionary<Stamp, StampImprint>();

		public StampImprintsFactory(StampsEnvironment stampsEnvironment)
		{
			foreach (var stamp in stampsEnvironment.Stamps)
			{
				GetOrAddStamp(stamp, stampsEnvironment.DefaultFilePath);
			}
		}

		public Imprint Get(Stamp stamp, Stamp owner)
		{
			var stampImprint = map[stamp];

			if (stamp.SimpleTypeValue != null)
			{
				return new SimpleTypeImprint(stamp.Type, null, stampImprint.Id, null, stamp.SimpleTypeValue);
			}

			if (owner == null || stampImprint.Inlined || stampImprint.Parent == map[owner])
			{
				return new AggregateImprint(stamp.Type, stampImprint.PublicName, stampImprint.Id, null,
					stamp.Properties.Select(p => ImprintFieldsFactory.GetField(stamp, p, p.SerializingElement, this)).ToArray());
			}

			string referenceRelativePath = null;
			if (stampImprint.FilePath != map[owner].FilePath)
			{
				referenceRelativePath = PathUtils.GetRelativePath(stampImprint.FilePath, map[owner].FilePath.Directory);
			}

			return new ReferenceImprint(stamp.Type, null, null, null, stampImprint.Id,
				referenceRelativePath);
		}

		public IEnumerable<Tuple<Imprint, FileInfo>> GetRootImprints()
		{
			return GetRootStampImprints().Select(rootStampImprint => new Tuple<Imprint, FileInfo>(Get(rootStampImprint.Stamp, null), rootStampImprint.FilePath));
		}

		private IEnumerable<StampImprint> GetRootStampImprints()
		{
			return map.Values.Where(item => item.Parent == null);
		}

		private StampImprint GetOrAddStamp(Stamp stamp, FileInfo defaultFilePath)
		{
			if (map.TryGetValue(stamp, out var stampImprint))
			{
				return stampImprint;
			}

			stampImprint = map[stamp] = new StampImprint(stamp, stamp.UserFilePath ?? defaultFilePath);

			foreach (var stampProperty in stamp.Properties)
			{
				foreach (var dependency in StampsDependenciesGetter.GetDependencies(stampProperty))
				{
					var dependencyImprint = GetOrAddStamp(dependency, defaultFilePath);
					BindChildToOwner(stampImprint, dependencyImprint);
				}
			}

			return stampImprint;
		}
		
		private static void BindChildToOwner(StampImprint owner, StampImprint child)
		{
			if (owner == null)
			{
				return;
			}

			switch (child.Stamp.InlinePolicy)
			{
				case StampInlinePolicy.Always:

					child.Parent = owner;
					child.Inlined = true;

					break;

				case StampInlinePolicy.Dynamic:

					var isSameFile = owner.FilePath.FullName.Equals(child.FilePath.FullName);
					var userFileSet = child.Stamp.UserFilePath != null;
					var canInline = isSameFile || !userFileSet;

					bool idRequired;

					if (canInline && (child.Parent == null || child.Stamp.UserParent == owner.Stamp.UserParent))
					{
						idRequired = child.Parent != null;
						child.Parent = owner;
					}
					else
					{
						idRequired = true;
					}

					if (idRequired && child.Id == null)
					{
						GenerateAutoId(child);
					}

					break;

				case StampInlinePolicy.Never:

					child.Parent = null;
					if (child.Id == null)
					{
						GenerateAutoId(child);
					}

					break;
			}
		}

		private static void GenerateAutoId(StampImprint stamp)
		{
			stamp.Id = Guid.NewGuid().ToString();
		}


		private class ImprintFieldsFactory : IStampFieldVisitor
		{
			private ImprintField field;

			private readonly Stamp owner;
			private readonly SerializingElement serializingElement;
			private readonly StampImprintsFactory factory;

			private ImprintFieldsFactory(Stamp owner, SerializingElement serializingElement, StampImprintsFactory factory)
			{
				this.owner = owner;
				this.serializingElement = serializingElement;
				this.factory = factory;
			}

			public static ImprintField GetField(Stamp owner, IStampField field, SerializingElement serializingElement, StampImprintsFactory factory)
			{
				var visitor = new ImprintFieldsFactory(owner, serializingElement, factory);
				field.Accept(visitor);
				return visitor.field;
			}

			void IStampFieldVisitor.Visit(SimpleTypeField simpleTypeField)
			{
				field = new SimpleTypeImprintField(serializingElement, simpleTypeField.Value);
			}

			void IStampFieldVisitor.Visit(StampField stampField)
			{
				field = new NestedImprintField(serializingElement, factory.Get(stampField.Value, owner));
			}

			void IStampFieldVisitor.Visit(ArrayStampField arrayStampField)
			{
				field = new ArrayImprintField(serializingElement, arrayStampField.Values.Select(val => factory.Get(val, owner)).ToArray());
			}
		}
	}
}

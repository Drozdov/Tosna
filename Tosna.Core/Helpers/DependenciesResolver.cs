using System;
using System.Collections.Generic;
using System.Linq;

namespace Tosna.Core.Helpers
{
	public sealed class DependenciesResolver<TOutput, TIdentifier> : IResolver<TOutput, TIdentifier>
	{
		#region Fields

		private readonly IDictionary<TIdentifier, DependencyNode> nodes = new Dictionary<TIdentifier, DependencyNode>();

		#endregion

		#region Ctor

		public DependenciesResolver()
		{
		}

		public DependenciesResolver(IEnumerable<IResolverInput<TOutput, TIdentifier>> inputs)
		{
			AddRange(inputs);
		}

		#endregion

		#region Public

		public void Add(IResolverInput<TOutput, TIdentifier> input)
		{
			nodes[input.Id] = new DependencyNode(input);
		}

		public void AddRange(IEnumerable<IResolverInput<TOutput, TIdentifier>> inputs)
		{
			foreach (var input in inputs)
			{
				Add(input);
			}
		}

		public TOutput Resolve(TIdentifier id)
		{
			var node = GetNode(id);
			Dfs(node);
			return node.Output;
		}

		public IEnumerable<TOutput> ResolveAll()
		{
			return nodes.Values.Select(node =>
			{
				Dfs(node);
				return node.Output;
			});
		}

		public void Clear()
		{
			nodes.Clear();
		}

		#endregion

		#region Private

		private void Dfs(DependencyNode node)
		{
			switch (node.Color)
			{
				case Color.Black:
					return;

				case Color.Gray:
					throw new InvalidOperationException("Cycle found in dependencies list");

				default:
					node.Color = Color.Gray;

					var dependencies = node.Input.Dependencies.Select(GetNode);

					foreach (var dependency in dependencies)
					{
						Dfs(dependency);
					}

					node.Output = node.Input.Resolve(this);

					node.Color = Color.Black;

					break;
			}
		}

		private DependencyNode GetNode(TIdentifier id)
		{
			if (nodes.TryGetValue(id, out var node))
			{
				return node;
			}

			throw new InvalidOperationException($"No dependency with id {id}");
		}

		#endregion

		#region IResolver

		TOutput IResolver<TOutput, TIdentifier>.Get(TIdentifier id)
		{
			return GetNode(id).Output;
		}

		#endregion

		#region Nested

		private enum Color
		{
			White,
			Gray,
			Black
		}

		private class DependencyNode
		{
			public DependencyNode(IResolverInput<TOutput, TIdentifier> input)
			{
				Input = input;
			}

			public IResolverInput<TOutput, TIdentifier> Input { get; }

			public TOutput Output { get; set; }

			public Color Color { get; set; } = Color.White;
		}

		#endregion
	}

	public interface IResolverInput<TOutput, out TIdentifier>
	{
		TIdentifier Id { get; }

		TIdentifier[] Dependencies { get; }

		TOutput Resolve(IResolver<TOutput, TIdentifier> resolver);
	}

	public interface IResolver<out TOutput, in TIdentifier>
	{
		TOutput Get(TIdentifier id);
	}
}

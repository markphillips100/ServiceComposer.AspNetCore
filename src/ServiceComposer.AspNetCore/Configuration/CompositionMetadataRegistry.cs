using System;
using System.Collections.Generic;

namespace ServiceComposer.AspNetCore.Configuration
{
    internal class CompositionMetadataRegistry
    {
        internal HashSet<Type> Components { get; } = new();

        public void AddComponent(Type type)
        {
            Components.Add(type);
        }
    }
}
using System;
using System.Collections.Generic;

namespace ServiceComposer.AspNetCore.Configuration
{
    public class CompositionMetadataRegistry
    {
        internal HashSet<Type> Components { get; } = new();

        internal void AddComponent(Type type)
        {
            Components.Add(type);
        }
    }
}
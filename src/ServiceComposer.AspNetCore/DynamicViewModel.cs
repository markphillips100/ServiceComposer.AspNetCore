﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;

namespace ServiceComposer.AspNetCore
{
    public sealed class DynamicViewModel : DynamicObject
    {
        readonly ConcurrentDictionary<string, object> _properties = new();


        public override bool TryGetMember(GetMemberBinder binder, out object result) => _properties.TryGetValue(binder.Name, out result);

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _properties.AddOrUpdate(binder.Name, value, (_, _) => value);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;

            switch (binder.Name)
            {
                case "Merge":
                    result = MergeImpl((IDictionary<string, object>) args[0]);
                    return true;
                default:
                    return false;
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            foreach (var item in _properties.Keys)
            {
                yield return item;
            }

            yield return "Merge";
        }

        DynamicViewModel MergeImpl(IDictionary<string, object> source)
        {
            foreach (var item in source)
            {
                _properties[item.Key] = item.Value;
            }

            return this;
        }
    }
}
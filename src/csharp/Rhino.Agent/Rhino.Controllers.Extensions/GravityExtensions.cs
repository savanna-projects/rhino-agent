/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.Comet.Engine.Plugins;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rhino.Controllers.Extensions
{
    public static class GravityExtensions
    {
        public static IEnumerable<PluginAttribute> GetActionAttributes(this IEnumerable<Type> types)
        {
            // setup
            var actionTypes = types.Where(i => IsPlugin<ActionPlugin>(i) && IsAttribute<ActionAttribute>(i));
            var attributes = new List<ActionAttribute>();

            // build
            foreach (var type in actionTypes)
            {
                var attributeData = type.CustomAttributes.FirstOrDefault(i => IsAttribute<ActionAttribute>(i));
                var actionAttribute = Build(type, attributeData);

                if (actionAttribute == null || attributes.Select(i => i.Name).Contains(actionAttribute.Name))
                {
                    continue;
                }

                attributes.Add(actionAttribute);
            }

            // get
            return attributes;
        }

        private static bool IsPlugin<T>(Type type) => type.BaseType == typeof(T);

        private static bool IsAttribute<T>(Type type)
        {
            // setup
            var typeName = typeof(T).FullName;

            // get
            return type.CustomAttributes.Any(i => i.AttributeType.FullName == typeName);
        }

        private static bool IsAttribute<T>(CustomAttributeData attributeData)
        {
            return attributeData.AttributeType.FullName == typeof(T).FullName;
        }

        private static ActionAttribute Build(Type type, CustomAttributeData attributeData)
        {
            // setup
            var arguments = attributeData.ConstructorArguments;

            // exit conditions
            if (arguments.Count == 0)
            {
                return null;
            }

            // setup
            var assembly = arguments.Count > 1 ? arguments[0].Value : type.Assembly.Location;
            var resource = arguments.Count > 1 ? arguments[1].Value : arguments[0].Value;

            // get
            return new ActionAttribute($"{assembly}", $"{resource}");
        }
    }
}
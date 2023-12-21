/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.Comet.Engine.Plugins;

using System.Diagnostics;
using System.Reflection;

namespace Rhino.Controllers.Extensions
{
    public static class GravityExtensions
    {
        public static IEnumerable<PluginAttribute> GetActionAttributes(this IEnumerable<Type> types)
        {
            // setup
            var actionTypes = types.Where(i => typeof(ActionPlugin).IsAssignableFrom(i) && IsAttribute<ActionAttribute>(i));
            var attributes = new List<ActionAttribute>();

            // build
            foreach (var type in actionTypes)
            {
                var attributeData = type.CustomAttributes.FirstOrDefault(i => IsAttribute<ActionAttribute>(i));
                var actionAttribute = BuildActionAttribute(type, attributeData);

                if (actionAttribute == null || attributes.Select(i => i.Name).Contains(actionAttribute.Name))
                {
                    continue;
                }

                attributes.Add(actionAttribute);
            }

            // get
            return attributes;
        }

        private static ActionAttribute BuildActionAttribute(Type type, CustomAttributeData attributeData)
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
            try
            {
                return new ActionAttribute($"{assembly}", $"{resource}");
            }
            catch (Exception e) when(e is FileNotFoundException)
            {
                try
                {
                    var location = type.Assembly.Location;
                    return new ActionAttribute($"{location}", $"{resource}");
                }
                catch (Exception ie) when (ie != null)
                {
                    Trace.TraceError($"Load-Assembly -Name {assembly} = (InternalServerError | ie.Message)");
                }
            }

            // get default
            return default;
        }

        public static IEnumerable<PluginAttribute> GetMacroAttributes(this IEnumerable<Type> types)
        {
            // setup
            var actionTypes = types.Where(i => typeof(MacroPlugin).IsAssignableFrom(i) && IsAttribute<MacroAttribute>(i));
            var attributes = new List<MacroAttribute>();

            // build
            foreach (var type in actionTypes)
            {
                var attributeData = type.CustomAttributes.FirstOrDefault(i => IsAttribute<MacroAttribute>(i));
                var macroAttribute = BuildMacroAttribute(type, attributeData);

                if (macroAttribute == null || attributes.Select(i => i.Name).Contains(macroAttribute.Name))
                {
                    continue;
                }

                attributes.Add(macroAttribute);
            }

            // get
            return attributes;
        }

        private static MacroAttribute BuildMacroAttribute(Type type, CustomAttributeData attributeData)
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
            return new MacroAttribute($"{assembly}", $"{resource}");
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
    }
}

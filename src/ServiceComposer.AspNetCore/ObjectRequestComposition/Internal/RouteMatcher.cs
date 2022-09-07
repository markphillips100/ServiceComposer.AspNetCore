using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.Routing;

namespace ServiceComposer.AspNetCore.ObjectRequestComposition.Internal
{
    internal static class RouteMatcher
    {
        public static bool Match(string routeTemplate, string requestPath, RouteValueDictionary values)
        {
            var template = TemplateParser.Parse(routeTemplate);

            var matcher = new TemplateMatcher(template, GetDefaults(template));

            return matcher.TryMatch(requestPath, values);
        }

        // This method extracts the default argument values from the template.
        private static RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate)
        {
            var result = new RouteValueDictionary();

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.DefaultValue != null)
                {
                    result.Add(parameter.Name, parameter.DefaultValue);
                }
            }

            return result;
        }
    }
}

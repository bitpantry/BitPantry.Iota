namespace BitPantry.Tabs.Web
{
    public class EnumRouteConstraint<TEnum> : IRouteConstraint where TEnum : struct, Enum
    {
        public bool Match(HttpContext httpContext,
                          IRouter route,
                          string routeKey,
                          RouteValueDictionary values,
                          RouteDirection routeDirection)
        {
            if (!values.TryGetValue(routeKey, out var value) || value == null)
                return false;

            return Enum.TryParse(typeof(TEnum), value.ToString(), true, out _);
        }
    }
}

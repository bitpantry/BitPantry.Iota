using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Linq.Expressions;

namespace BitPantry.Iota.Web
{
    public static class Route
    {
        public static IActionResult RedirectTo<TController>(
                Expression<Func<TController, Task<IActionResult>>> actionExpression)
                where TController : Controller
        {
            return RedirectToInternal<TController>(actionExpression);
        }

        public static IActionResult RedirectTo<TController>(
            Expression<Func<TController, IActionResult>> actionExpression)
            where TController : Controller
        {
            return RedirectToInternal<TController>(actionExpression);
        }

        private static IActionResult RedirectToInternal<TController>(
            LambdaExpression actionExpression)
            where TController : Controller
        {
            var data = ExtractRouteData<TController>(actionExpression);
            return new RedirectToActionResult(data.Item1, data.Item2, data.Item3);
        }

        public static string Action<TController>(this IUrlHelper urlHelper,
        Expression<Func<TController, Task<IActionResult>>> actionExpression)
        where TController : Controller
        {
            return ActionInternal<TController>(urlHelper, actionExpression);
        }

        public static string Action<TController>(this IUrlHelper urlHelper,
            Expression<Func<TController, IActionResult>> actionExpression)
            where TController : Controller
        {
            return ActionInternal<TController>(urlHelper, actionExpression);
        }

        private static string ActionInternal<TController>(IUrlHelper urlHelper,
            LambdaExpression actionExpression)
            where TController : Controller
        {
            var data = ExtractRouteData<TController>(actionExpression);
            return urlHelper.Action(data.Item1, data.Item2, data.Item3);
        }

        private static Tuple<string, string, object> ExtractRouteData<TController>(LambdaExpression actionExpression)
        {
            if (actionExpression.Body is MethodCallExpression methodCall)
            {
                return new Tuple<string, string, object>(
                    methodCall.Method.Name,
                    typeof(TController).Name.Replace("Controller", ""),
                    GetRouteValues(methodCall));
            }

            throw new ArgumentException("Invalid expression for action method", nameof(actionExpression));

        }

        private static RouteValueDictionary GetRouteValues(MethodCallExpression methodCall)
        {
            var routeValues = new RouteValueDictionary();

            // Get method parameters
            var parameters = methodCall.Method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameterName = parameters[i].Name;

                // Get the argument value by evaluating the expression
                var argumentExpression = methodCall.Arguments[i];
                var argumentValue = Expression.Lambda(argumentExpression).Compile().DynamicInvoke();

                routeValues[parameterName] = argumentValue;
            }

            return routeValues;
        }
    }
}

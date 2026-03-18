using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Auth.Wiedersehen.Exceptions;

internal class HttpResponseExceptionFilter : IOrderedFilter, IActionFilter
{
	public void OnActionExecuting(ActionExecutingContext context)
	{
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
		if (context.Exception is not HttpResponseException exception)
		{
			return;
		}

		context.Result = new ObjectResult(null)
		{
			Value = new ErrorDetails(exception.StatusCode, exception.Errors), StatusCode = exception.StatusCode,
		};

		context.ExceptionHandled = true;
	}

	public int Order => int.MaxValue - 10;
}

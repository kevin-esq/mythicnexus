using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using MythicNexus.Application.Errors;

namespace MythicNexus.Api.Http;

public static class ApiResults
{
    public static IResult OkData<T>(T data) => Results.Ok(new { data });

    public static IResult ErrorJson(string code, string message, int statusCode) =>
        Results.Json(new { error = new { code, message } }, statusCode: statusCode);

    public static IResult ProblemWithCode(
        string code,
        string title,
        int statusCode,
        string? detail = null,
        IDictionary<string, string[]>? validationErrors = null)
    {
        var problem = new ProblemDetails
        {
            Type = $"https://api.mythicnexus.com/errors/{code}",
            Title = title,
            Detail = detail,
            Status = statusCode,
            Extensions = new Dictionary<string, object?> { ["code"] = code },
        };

        if (validationErrors is { Count: > 0 })
        {
            problem.Extensions["errors"] = validationErrors;
        }

        return Results.Problem(problem);
    }

    public static IResult ValidationFailed(ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return ProblemWithCode(
            ErrorCodes.ValidationFailed,
            "Validation failed",
            StatusCodes.Status400BadRequest,
            validationErrors: errors);
    }
}

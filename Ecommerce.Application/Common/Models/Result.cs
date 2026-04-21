using System;
using System.Collections.Generic;

namespace Ecommerce.Application.Common.Models;

public record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}

public class Result
{
    public bool Success { get; protected set; }
    public string? Message { get; protected set; }
    public Error Error { get; protected set; } = Error.None;

    public bool IsSuccess => Success;
    public bool IsFailure => !Success;

    protected Result(bool success, string? message, Error error)
    {
        Success = success;
        Message = message;
        Error = error;
    }

    public static Result SuccessResult(string? message = null)
        => new(true, message, Error.None);

    public static Result Failure(Error error)
        => new(false, error.Description, error);
    public static Result Failure(string message)
        => new(false, message, new Error("General.Error", message));

    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Error);
}

public class Result<T> : Result
{
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    private Result(bool success, T? data, string? message, Error error)
        : base(success, message, error)
    {
        Data = data;
    }

    public static Result<T> SuccessResult(T data, string? message = null)
        => new(true, data, message, Error.None);

    public static new Result<T> Failure(Error error)
        => new(false, default, error.Description, error);

    public static new Result<T> Failure(string message)
        => new(false, default, message, new Error("General.Error", message));

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
        => IsSuccess ? onSuccess(Data!) : onFailure(Error);
}
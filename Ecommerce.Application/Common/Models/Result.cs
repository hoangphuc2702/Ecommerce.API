using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Common.Models
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }

        public static Result<T> SuccessResult(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

        public static Result<T> Failure(string message, List<string>? errors = null)
            => new() { Success = false, Message = message, Errors = errors };
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization.Json;

namespace GnomeStack.Functional;

public class Error : IError
{
    public Error(string? message = null, IInnerError? innerError = null)
    {
        this.Message = message ?? "Unknown error";
        this.InnerError = innerError;
    }

    public static Func<Exception, Error> Convert { get; set; } = (ex) =>
    {
        if (ex is AggregateException aggEx)
            return new AggregateError(aggEx);

        if (ex is ArgumentNullException argNullEx)
            return new ArgumentNullError(argNullEx);

        if (ex is ArgumentOutOfRangeException argOutOfRangeEx)
            return new ArgumentOutOfRangeError(argOutOfRangeEx);

        if (ex is ArgumentException argEx)
            return new ArgumentError(argEx);

        return new ExceptionError(ex);
    };

    public string Message { get; set; }

    public string? Code { get; set; }

    public virtual string? Target
    {
        [RequiresUnreferencedCode("The TargetSite property for exceptions may be used in derived classes.")]
        get;
        protected set;
    }

    public virtual string? StackTrace { get; protected set; }

    public IInnerError? InnerError { get; set; }

    public static implicit operator Error(Exception ex)
        => Convert(ex);

    public virtual Exception ToException()
    {
        return new ResultException(this.Message);
    }

    public override string ToString()
    {
        return this.Message;
    }
}

public class ArgumentOutOfRangeError : ArgumentError
{
    public ArgumentOutOfRangeError(string paramName)
        : base($"Argument {paramName} is out of range.")
    {
        this.ParamName = paramName;
    }

    public ArgumentOutOfRangeError(string paramName, object? value)
        : base(paramName)
    {
        this.ActualValue = value;
    }

    public ArgumentOutOfRangeError(string paramName, string message)
        : base(paramName, message)
    {
    }

    public ArgumentOutOfRangeError(string paramName, object? value, string message)
        : base(paramName, message)
    {
        this.ActualValue = value;
    }

    public ArgumentOutOfRangeError(string message, string paramName, IInnerError inner)
        : base(message, paramName, inner)
    {
    }

    public ArgumentOutOfRangeError(ArgumentOutOfRangeException ex)
        : base(ex)
    {
        this.ActualValue = ex.ActualValue;
    }

    public object? ActualValue { get; set; }

    public static implicit operator ArgumentOutOfRangeError(ArgumentOutOfRangeException ex)
        => new(ex);

    public override Exception ToException()
    {
        return this.Exception ?? new ArgumentOutOfRangeException(this.ParamName, this.ActualValue, this.Message);
    }
}

public class ArgumentNullError : ArgumentError
{
    public ArgumentNullError(string paramName)
        : base(paramName, $"Argument {paramName} is null.")
    {
        this.ParamName = paramName;
    }

    public ArgumentNullError(string paramName, string message)
        : base(paramName, message)
    {
    }

    public ArgumentNullError(string paramName, string message, IInnerError inner)
        : base(paramName, message, inner)
    {
    }

    public ArgumentNullError(ArgumentNullException ex)
        : base(ex)
    {
    }

    public static implicit operator ArgumentNullError(ArgumentNullException ex)
        => new(ex);

    public override Exception ToException()
    {
        if (this.Exception is not null)
            return this.Exception;

        return new ArgumentNullException(this.ParamName, this.Message);
    }
}

public class ArgumentError : ExceptionError
{
    public ArgumentError(string paramName)
        : base($"Argument {paramName} is invalid.")
    {
        this.ParamName = paramName;
    }

    public ArgumentError(string paramName, string message)
        : base(message)
    {
        this.ParamName = paramName;
    }

    public ArgumentError(string paramName, string message, IInnerError inner)
        : base(message, inner)
    {
        this.ParamName = paramName;
    }

    public ArgumentError(ArgumentException ex)
        : base(ex)
    {
        this.ParamName = ex.ParamName ?? string.Empty;
    }

    public string ParamName { get; set; }

    public override Exception ToException()
    {
        if (this.Exception is not null)
            return this.Exception;

        return new ArgumentException(this.Message, this.ParamName);
    }
}

public class AggregateError : ExceptionError
{
    public AggregateError(string message, params Error[] errors)
        : base(message)
    {
        this.Code = "AggregateError";
        this.InnerErrors = errors;
    }

    public AggregateError(params Error[] errors)
        : base("Aggregate error")
    {
        this.Code = "AggregateError";
        this.InnerErrors = errors;
    }

    public AggregateError(params Exception[] exceptions)
        : base(new AggregateException(exceptions))
    {
        this.Code = "AggregateError";
        this.InnerErrors = exceptions.Select(Convert).ToArray();
    }

    public AggregateError(AggregateException ex)
        : base(ex)
    {
        this.Code = "AggregateError";
        this.InnerErrors = ex.InnerExceptions.Select(Convert).ToArray();
    }

    public Error[] InnerErrors { get; set; }

    public static implicit operator AggregateError(AggregateException ex)
        => new(ex);

    public override Exception ToException()
    {
        if (this.Exception is not null)
            return this.Exception;

        return new AggregateException(this.InnerErrors.Select(e => e.ToException()));
    }
}

public class ExceptionError : Error
{
    private readonly Exception? ex;

    public ExceptionError(string message)
        : base(message)
    {
        this.ex = null;
    }

    public ExceptionError(string message, IInnerError? inner)
        : base(message, inner)
    {
        this.ex = null;
    }

    public ExceptionError(Exception ex)
        : base(ex.Message, ex.InnerException is null ? null : new ExceptionError(ex.InnerException))
    {
        this.ex = ex;
        var e = ex.GetType().Name;

        // Remove the "Exception" suffix
        this.Code = e.Substring(0, e.Length - 9);
    }

    public override string? Target
    {
        [RequiresUnreferencedCode("The TargetSite for the Exception may be removed.")]
        get
        {
            if (base.Target is not null)
                return base.Target;

            try
            {
                var target = string.Empty;
                if (this.ex?.TargetSite is null)
                {
                    base.Target = string.Empty;
                    return base.Target;
                }

                if (this.ex.TargetSite?.DeclaringType?.FullName is not null)
                    target = this.ex.TargetSite.DeclaringType.FullName + ".";

                if (this.ex.TargetSite?.Name is not null)
                    target += this.ex.TargetSite.Name;

                base.Target = target;
            }
            catch
            {
                base.Target = string.Empty;
            }

            return base.Target;
        }

        protected set => base.Target = value;
    }

    public override string? StackTrace
    {
        get
        {
            if (base.StackTrace is not null)
                return base.StackTrace;

            if (this.ex is null)
                return null;

            base.StackTrace = this.ex.StackTrace;
            return this.ex.StackTrace;
        }

        protected set => base.StackTrace = value;
    }

    protected Exception? Exception => this.ex;

    public override Exception ToException()
    {
        return this.ex ?? new ResultException(this.Message);
    }

    public override string ToString()
    {
        return this.ex?.ToString() ?? base.ToString();
    }
}
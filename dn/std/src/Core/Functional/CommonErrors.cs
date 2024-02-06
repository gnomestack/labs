namespace GnomeStack.Functional;

public class InvalidCastError : ExceptionError
{
    public InvalidCastError(string message)
        : base(message)
    {
    }

    public InvalidCastError(string message, IInnerError? inner)
        : base(message, inner)
    {
    }

    public InvalidCastError(InvalidCastException ex)
        : base(ex)
    {
    }

    public override Exception ToException()
    {
        if (this.Exception is not null)
            return this.Exception;

        return new InvalidCastException(this.Message, this.Exception);
    }
}

public class InvalidOperationError : ExceptionError
{
    public InvalidOperationError(string message)
        : base(message)
    {
    }

    public InvalidOperationError(string message, IInnerError? inner)
        : base(message, inner)
    {
    }

    public InvalidOperationError(InvalidOperationException ex)
        : base(ex)
    {
    }

    public override Exception ToException()
    {
        if (this.Exception is not null)
            return this.Exception;

        return new InvalidOperationException(this.Message, this.Exception);
    }
}

public class NullReferenceError : ExceptionError
{
    public NullReferenceError(string message)
        : base(message)
    {
    }

    public NullReferenceError(string message, IInnerError? inner)
        : base(message, inner)
    {
    }

    public NullReferenceError(NullReferenceException ex)
        : base(ex)
    {
    }

    public override Exception ToException()
    {
        if (this.Exception is not null)
            return this.Exception;

        return new NullReferenceException(this.Message, this.Exception);
    }
}

public class TimeoutError : ExceptionError
{
    public TimeoutError(string message)
        : base(message)
    {
    }

    public TimeoutError(string message, IInnerError? inner)
        : base(message, inner)
    {
    }

    public TimeoutError(TimeoutException ex)
        : base(ex)
    {
    }

    public override Exception ToException()
    {
        if (this.Exception is not null)
            return this.Exception;

        return new TimeoutException(this.Message, this.Exception);
    }
}

public class FileNotFoundError : ExceptionError
{
    public FileNotFoundError(string message)
        : base(message)
    {
    }

    public FileNotFoundError(string message, IInnerError? inner)
        : base(message, inner)
    {
    }

    public FileNotFoundError(FileNotFoundException ex)
        : base(ex)
    {
        this.FileName = ex.FileName;
    }

    public string? FileName { get; set; }

    public override Exception ToException()
    {
        if (this.Exception is not null)
            return this.Exception;

        return new FileNotFoundException(this.Message, this.FileName, this.Exception);
    }
}

public class DirectoryNotFoundError : ExceptionError
{
    public DirectoryNotFoundError(string message)
        : base(message)
    {
    }

    public DirectoryNotFoundError(string message, IInnerError? inner)
        : base(message, inner)
    {
    }

    public DirectoryNotFoundError(DirectoryNotFoundException ex)
        : base(ex)
    {
    }

    public string? DirectoryName { get; set; }

    public override Exception ToException()
    {
        if (this.Exception is not null)
            return this.Exception;

        return new DirectoryNotFoundException(this.Message, this.Exception);
    }
}
namespace GnomeStack.Diagnostics;

public class NotFoundOnPathException : System.Exception
{
    public NotFoundOnPathException()
    {
    }

    public NotFoundOnPathException(string message)
        : base(message)
    {
    }

    public NotFoundOnPathException(string message, System.Exception inner)
        : base(message, inner)
    {
    }
}
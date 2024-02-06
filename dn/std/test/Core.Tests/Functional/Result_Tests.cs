using GnomeStack.Functional;

using static GnomeStack.Functional.Result;

namespace Tests;

public class Result_Tests
{
    [UnitTest]
    public void DeconstructValue()
    {
        var result = Ok(1);
        var (ok, value) = result;
        Assert.True(ok);
        Assert.Equal(1, value);
    }

    [UnitTest]
    public void DeconstructError()
    {
        var result = Error<int>(new Exception());
        var (ok, value, error) = result;
        Assert.False(ok);
        Assert.Equal(0, value);
        Assert.NotNull(error);
    }

    [UnitTest]
    public void Match()
    {
        var result = Ok(1);
        Assert.True(result.Match(x => x == 1));
    }

    [UnitTest]
    public void MatchError()
    {
        var result = Error<int>(new Exception());
        Assert.True(result.MatchError(x => x is ExceptionError));
    }

    [UnitTest]
    public void Unwrap()
    {
        var result = Ok(1);
        Assert.Equal(1, result.Unwrap());
    }

    [UnitTest]
    public void UnwrapError()
    {
        var result = Error<int>(new Exception("error"));
        var error = result.UnwrapError();
        Assert.NotNull(error);
        Assert.Equal("error", error.Message);
        Assert.IsType<ExceptionError>(error);
    }

    [UnitTest]
    public void StaticTry()
    {
        var result = Try(() => Console.WriteLine("test"));
        Assert.True(result.IsOk);
        Assert.False(result.IsError);

        var result2 = Try(() => throw new Exception());
        Assert.False(result2.IsOk);
        Assert.True(result2.IsError);
        Assert.IsType<ExceptionError>(result2.UnwrapError());
    }

    [UnitTest]
    public void StaticTryWithValue()
    {
        var result = Try(() => 1);
        Assert.True(result.IsOk);
        Assert.False(result.IsError);
        Assert.Equal(1, result.Unwrap());

        var result2 = Try(() =>
        {
            const int x = 1;
            return x == 1 ? throw new Exception() : x;
        });

        Assert.False(result2.IsOk);
        Assert.True(result2.IsError);
        Assert.IsType<ExceptionError>(result2.UnwrapError());
    }

    [UnitTest]
    public async Task StaticTryAsync()
    {
        var result = await TryAsync(() => Task.CompletedTask);
        Assert.True(result.IsOk);
        Assert.False(result.IsError);

        var result2 = await TryAsync(() => throw new Exception());
        Assert.False(result2.IsOk);
        Assert.True(result2.IsError);
        Assert.IsType<ExceptionError>(result2.UnwrapError());
    }

    [UnitTest]
    public async Task TryAsyncWithValue()
    {
        var result = await TryAsync(() => Task.FromResult(1));
        Assert.True(result.IsOk);
        Assert.False(result.IsError);
        Assert.Equal(1, result.Unwrap());

        var result2 = await TryAsync(() =>
        {
            const int x = 1;
            return x == 1 ? throw new Exception() : Task.FromResult(x);
        });

        Assert.False(result2.IsOk);
        Assert.True(result2.IsError);
        Assert.IsType<ExceptionError>(result2.UnwrapError());
    }

    [UnitTest]
    public void Expect()
    {
        var result = Ok(1);
        Assert.Equal(1, result.Expect("error"));

        var result2 = Error<int>(new Exception("Custom error"));
        var error = Assert.Throws<ResultException>(() => result2.Expect("error"));
        Assert.Equal("error: Custom error", error.Message);
    }

    [UnitTest]
    public void ExpectError()
    {
        var result = Error<int>(new Exception("Custom error"));
        var error = result.ExpectError("error");
        Assert.Equal("Custom error", error.Message);

        var result2 = Ok(1);
        var ex = Assert.Throws<ResultException>(() => result2.ExpectError("error"));
        Assert.Equal("error", ex.Message);
    }

    [UnitTest]
    public void Map()
    {
        var result = Ok(1);
        var result2 = result.Map(x => x.ToString());
        Assert.True(result2.IsOk);
        Assert.Equal("1", result2.Unwrap());

#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
        var result3 = Error<int>(new ArgumentException("value is missing", "value"));
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
        var result4 = result3.Map(x => x.ToString());
        Assert.True(result4.IsError);
        Assert.IsType<ArgumentError>(result4.UnwrapError());
    }

    [UnitTest]
    public void MapOrDefault()
    {
        var result = Ok(1);
        var result2 = result.MapOrDefault(o => o.ToString(), "3");
        Assert.True(result2.IsOk);
        Assert.Equal("1", result2.Unwrap());

        var result3 = Error<int>(new Exception());
        var result4 = result3.MapOrDefault(x => x.ToString(), "3");
        Assert.False(result4.IsError);
        Assert.Equal("3", result4.Unwrap());

        var result5 = Error<int>(new Exception());
        var result6 = result5.MapOrDefault(x => x.ToString(), () => "3");
        Assert.False(result6.IsError);
        Assert.Equal("3", result6.Unwrap());
    }

    [UnitTest]
    public void MapError()
    {
        var result = Ok(1);
        var result2 = result.MapError(x => new Exception(x.Message));
        Assert.True(result2.IsOk);
        Assert.Equal(1, result2.Unwrap());

        var result3 = Error<int>(new Exception());
        var result4 = result3.MapError(x => new ArgumentError(x.Message));
        Assert.True(result4.IsError);
        Assert.IsType<ArgumentError>(result4.UnwrapError());
    }

    [UnitTest]
    public void And()
    {
        var result = Ok(1);
        var result2 = Ok(2);
        var result3 = result.And(result2);
        Assert.True(result3.IsOk);
        Assert.Equal(2, result3.Unwrap());

        var branch = result.And(2);
        Assert.True(branch.IsOk);
        Assert.Equal(2, branch.Unwrap());

        var result4 = Error<int>(new Exception());
        var result5 = result4.And(result2);
        Assert.True(result5.IsError);
        Assert.IsType<ExceptionError>(result5.UnwrapError());

        var result6 = Error<int>(new Exception());
        var result7 = result6.And(Error<int>(new Exception()));
        Assert.True(result7.IsError);
        Assert.IsType<ExceptionError>(result7.UnwrapError());
    }

    [UnitTest]
    public void Or()
    {
        var result = Ok(1);
        var result2 = Ok(2);
        var result3 = result.Or(result2);
        Assert.True(result3.IsOk);
        Assert.Equal(1, result3.Unwrap());

        var branch = result.Or(2);
        Assert.True(branch.IsOk);
        Assert.Equal(1, branch.Unwrap());

        var result4 = Error<int>(new Exception());
        var result5 = result4.Or(result2);
        Assert.True(result5.IsOk);
        Assert.Equal(2, result5.Unwrap());

        var branch2 = result4.Or(2);
        Assert.True(branch2.IsOk);
        Assert.Equal(2, branch2.Unwrap());

        var result6 = Error<int>(new Exception());
        var result7 = result6.Or(Error<int>(new Exception()));
        Assert.True(result7.IsError);
        Assert.IsType<ExceptionError>(result7.UnwrapError());
    }

    [UnitTest]
    public void Inspect()
    {
        var result = Ok(1);
        var inspected = false;
        result.Inspect(_ => inspected = true);
        Assert.True(inspected);

        inspected = false;
        result = Error<int>(new Exception());
        result.Inspect(_ => inspected = true);
        Assert.False(inspected);
    }

    [UnitTest]
    public void InspectError()
    {
        var result = Ok(1);
        var inspected = false;
        result.InspectError(_ => inspected = true);
        Assert.False(inspected);

        inspected = false;
        result = Error<int>(new Exception());
        result.InspectError(_ => inspected = true);
        Assert.True(inspected);
    }

    [UnitTest]
    public void Replace()
    {
        var result = Ok(1);
        var result2 = result.Replace(2);
        Assert.True(result2.IsOk);
        Assert.Equal(2, result2.Unwrap());

        var result3 = Error<int>(new Exception());
        var result4 = result3.Replace(2);
        Assert.False(result4.IsError);
        Assert.Equal(2, result4.Unwrap());
    }

    [UnitTest]
    public void ReplaceError()
    {
        var result = Ok(1);
        result.ReplaceError(new Exception());
        Assert.True(result.IsError);
        Assert.IsType<ExceptionError>(result.UnwrapError());

        var result3 = Error<int>(new Exception("bad"));
        var result4 = result3.ReplaceError(new ArgumentNullError("error"));
        Assert.True(result4.IsError);
        var error = result4.UnwrapError();
        Assert.IsType<ArgumentNullError>(error);
        Assert.Equal("Argument error is null.", error.Message);
    }
}
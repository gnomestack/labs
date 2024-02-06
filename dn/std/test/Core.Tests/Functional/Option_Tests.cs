using GnomeStack;
using GnomeStack.Extras.Functional;
using GnomeStack.Functional;

using static GnomeStack.Functional.Option;

namespace Tests;

public class Option_Tests
{
    [UnitTest]
    public void SomeAndDeconstruct()
    {
        var optional = Some(1);
        var (value, some) = optional;
        Assert.True(some);
        Assert.Equal(1, value);
    }

    [UnitTest]
    public void NoneAndDeconstruct()
    {
        var optional = None<int>();
        var (value, some) = optional;
        Assert.False(some);
        Assert.Equal(0, value);
    }

    [UnitTest]
    public void SomeAndDeconstructWithOut()
    {
        var optional = Some(1);
        optional.Deconstruct(out var value, out var some);
        Assert.True(some);
        Assert.Equal(1, value);
    }

    [UnitTest]
    public void NoneAndDeconstructWithOut()
    {
        var optional = None<int>();
        optional.Deconstruct(out var value, out var some);
        Assert.False(some);
        Assert.Equal(0, value);
    }

    [UnitTest]
    public void Unwrap()
    {
        var optional = Some(1);
        Assert.Equal(1, optional.Unwrap());
    }

    [UnitTest]
    public void UnwrapWithNoneMustThrow()
    {
        var optional = None<int>();
        Assert.Throws<OptionException>(() => optional.Unwrap());
    }

    [UnitTest]
    public void UnwrapOrWithSome()
    {
        var optional = Some(1);
        Assert.Equal(1, optional.Unwrap(2));
    }

    [UnitTest]
    public void UnwrapOrWithNone()
    {
        var optional = None<int>();
        Assert.Equal(2, optional.Unwrap(2));
    }

    [UnitTest]
    public void UnwrapOrWithSomeAndFactory()
    {
        var optional = Some(1);
        Assert.Equal(1, optional.Unwrap(() => 2));
    }

    [UnitTest]
    public void UnwrapOrWithNoneAndFactory()
    {
        var optional = None<int>();
        Assert.Equal(2, optional.Unwrap(() => 2));
    }

    [UnitTest]
    public void OrWithSome()
    {
        var optional = Some(1);
        Assert.Equal(1, optional.Or(2));
    }

    [UnitTest]
    public void IsNoneCheck()
    {
        var none = None<int>();
        Assert.True(IsNone(none));
        Assert.False(IsNone(Some(1)));
        Assert.False(IsNone(1));
        Assert.False(IsNone("1"));
        Assert.False(IsNone(new object()));
        Assert.False(IsNone(new object[] { }));
        Assert.False(IsNone(new List<int>()));
        Assert.True(IsNone(null));
        Assert.True(IsNone(GnomeStack.Nil.Value));
        Assert.True(IsNone(DBNull.Value));
        Assert.True(IsNone(ValueTuple.Create()));
        Assert.True(IsNone(None()));
        Assert.True(none.Equals(None()));
        Assert.True(none.Equals(GnomeStack.Nil.Value));
    }

    [UnitTest]
    public void ExtensionMethods()
    {
        var optional = 1.ToOption();
        Assert.Equal(1, optional.Unwrap());

        int? i3 = null;
        var optional3 = i3.ToOption();
        Assert.False(optional3.IsSome);
    }

    [UnitTest]
    public void Map()
    {
        var optional = Some(1);
        Assert.Equal(2, optional.Map(x => x + 1).Unwrap());
    }

    [UnitTest]
    public void MapWithNone()
    {
        var optional = None<int>();
        Assert.False(optional.Map(x => x + 1).IsSome);
    }

    [UnitTest]
    public void MapOrDefault()
    {
        var optional = Some(1);
        Assert.Equal(2, optional.MapOrDefault(x => x + 1, 3).Unwrap());

        var optional2 = None<int>();
        Assert.Equal(3, optional2.MapOrDefault(x => x + 1, 3).Unwrap());
    }

    [UnitTest]
    public async Task MapAsync()
    {
        var optional = Some(1);
        var next = await optional.MapAsync(async x =>
        {
            await Task.Delay(100);
            return x + 1;
        });

        Assert.True(next.IsSome);
        Assert.Equal(2, next.Unwrap());
    }

    [UnitTest]
    public async Task MapAsyncWithNone()
    {
        var optional = None<int>();
        var next = await optional.MapAsync(async x =>
        {
            await Task.Delay(100);
            return x + 1;
        });

        Assert.False(next.IsSome);
    }

    [UnitTest]
    public async Task MapAsyncWithCancellation()
    {
        var optional = Some(1);
        var next = await optional.MapAsync(
            async (x, c) =>
            {
                await Task.Delay(100, c);
                return x + 1;
            },
            CancellationToken.None);

        Assert.True(next.IsSome);
        Assert.Equal(2, next.Unwrap());
    }

    [UnitTest]
    public void And()
    {
        var optional = Some(1);
        var optional2 = optional.And(Some(2));
        Assert.Equal(2, optional2.Unwrap());
        Assert.True(optional2.IsSome);
    }

    [UnitTest]
    public void AndWithNone()
    {
        var optional = None<int>();
        var optional2 = optional.And(2);
        Assert.False(optional2.IsSome);
    }

    [UnitTest]
    public void Take()
    {
        var optional = Some(1);
        var value = optional.Take();
        Assert.Equal(1, value);
        Assert.True(optional.IsNone);
    }

    [UnitTest]
    public void TakeWithNone()
    {
        var optional = None<int>();
        Assert.Throws<OptionException>(() => optional.Take());
    }

    [UnitTest]
    public void Replace()
    {
        var optional = Some(1);
        var optional2 = optional.Replace(2);
        Assert.Equal(2, optional2.Unwrap());
        Assert.True(optional2.IsSome);
    }

    [UnitTest]
    public void ReplaceWithNone()
    {
        var optional = None<int>();
        var optional2 = optional.Replace(2);
        Assert.Equal(2, optional2.Unwrap());
        Assert.True(optional2.IsSome);
    }

    [UnitTest]
    public void Filter()
    {
        var optional = Some(1);
        var optional2 = optional.Filter(x => x == 1);
        Assert.Equal(1, optional2.Unwrap());
        Assert.True(optional2.IsSome);

        var optional3 = optional.Filter(x => x == 2);
        Assert.False(optional3.IsSome);
    }

    [UnitTest]
    public void Match()
    {
        var optional = Some(1);
        Assert.True(optional.Match(x => x == 1));
    }

    [UnitTest]
    public void MatchWithNone()
    {
        var optional = None<int>();
        Assert.False(optional.Match(x => x == 1));
    }

    [UnitTest]
    public void Expect()
    {
        var optional = Some(1);
        Assert.Equal(1, optional.Expect("Expected a value"));
    }

    [UnitTest]
    public void ExpectWithNone()
    {
        var optional = None<int>();
        var ex = Assert.Throws<OptionException>(() => optional.Expect("Expected a value"));
        Assert.Equal("Expected a value", ex.Message);
    }

    [UnitTest]
    public void Zip()
    {
        var optional = Some(1);
        var optional2 = Some(2);
        var optional3 = optional.Zip(optional2);
        Assert.Equal((1, 2), optional3.Unwrap());
    }
}
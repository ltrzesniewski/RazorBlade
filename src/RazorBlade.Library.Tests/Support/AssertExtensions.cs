using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using NUnit.Framework;

namespace RazorBlade.Tests.Support;

#if NET
[StackTraceHidden]
#endif
internal static class AssertExtensions
{
    public static void ShouldEqual<T>(this T? actual, T? expected)
        => Assert.That(actual, Is.EqualTo(expected));

    public static void ShouldNotEqual<T>(this T? actual, T? expected)
        => Assert.That(actual, Is.Not.EqualTo(expected));

    public static void ShouldBeTrue(this bool actual)
        => Assert.That(actual, Is.True);

    public static void ShouldBeFalse(this bool actual)
        => Assert.That(actual, Is.False);

    public static TExpected ShouldBe<TExpected>(this object? actual)
        where TExpected : class
    {
        Assert.That(actual, Is.InstanceOf<TExpected>());
        return actual as TExpected ?? throw new AssertionException($"Expected instance of {typeof(TExpected).Name}");
    }

    [ContractAnnotation("notnull => halt")]
    public static void ShouldBeNull(this object? actual)
        => Assert.That(actual, Is.Null);

    [ContractAnnotation("null => halt")]
    public static T ShouldNotBeNull<T>(this T? actual)
        where T : class
    {
        Assert.That(actual, Is.Not.Null);
        return actual ?? throw new AssertionException("Expected non-null");
    }

    public static void ShouldBeTheSameAs<T>(this T? actual, T? expected)
        where T : class
        => Assert.That(actual, Is.SameAs(expected));

    public static void ShouldNotBeTheSameAs<T>(this T? actual, T? expected)
        where T : class
        => Assert.That(actual, Is.Not.SameAs(expected));

    public static void ShouldBeEmpty<T>(this T actual)
        => Assert.That(actual, Is.Empty);

    public static void ShouldNotBeEmpty<T>(this T actual)
        => Assert.That(actual, Is.Not.Empty);

    public static void ShouldContain(this string actual, string expected)
        => Assert.That(actual, Contains.Substring(expected));

    public static void ShouldContain<T>(this IEnumerable<T> actual, T expected)
        => Assert.That(actual, Contains.Item(expected));

    public static void ShouldContain<T>(this IEnumerable<T> actual, Predicate<T> predicate)
        => Assert.That(actual, Has.Some.Matches(predicate));

    public static void ShouldBeEquivalentTo<T>(this IEnumerable<T>? actual, IEnumerable<T> expected)
        => Assert.That(actual, Is.EquivalentTo(expected));
}

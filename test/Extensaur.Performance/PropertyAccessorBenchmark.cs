using System;
using System.Reflection;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using Extensaur.Performance.Models;

namespace Extensaur.Performance;


[BenchmarkCategory("Accessor")]
public class PropertyAccessorBenchmark
{
    private readonly Post _post;
    private readonly TestClass _testObject;

    public PropertyAccessorBenchmark()
    {
        _post = new Post("Tester", new DateTime(2025, 1, 1), "Test Title");
        _testObject = new TestClass { Title = "Test Title" };
    }

    // Test class for mutable operations
    private class TestClass
    {
        public string Title { get; set; } = string.Empty;
    }

    private PropertyInfo _propertyInfo;
    private IMemberAccessor _propertyAccessor;
    private Func<object, object> _functionAccessor;

    // SetValue test infrastructure
    private PropertyInfo _setPropertyInfo;
    private IMemberAccessor _setPropertyAccessor;
    private Action<object, object> _setFunctionAccessor;

    [GlobalSetup]
    public void Setup()
    {
        var type = _post.GetType();
        _propertyInfo = type.GetProperty(nameof(Post.Title));

        var typeAccessor = TypeAccessor.GetAccessor<Post>();
        _propertyAccessor = typeAccessor.FindProperty(nameof(Post.Title));
        _functionAccessor = (p) => (p as Post)?.Title;

        // Setup for SetValue benchmarks
        var testType = typeof(TestClass);
        _setPropertyInfo = testType.GetProperty(nameof(TestClass.Title));

        var testTypeAccessor = TypeAccessor.GetAccessor<TestClass>();
        _setPropertyAccessor = testTypeAccessor.FindProperty(nameof(TestClass.Title));
        _setFunctionAccessor = (obj, value) => (obj as TestClass)!.Title = (string)value;
    }

    [Benchmark(Description = "ProperyRead", Baseline = true)]
    public string ProperyRead()
    {
        var displayName = _post.Title;

        return displayName;
    }

    [Benchmark(Description = "ProperyReflection")]
    public string ProperyReflection()
    {
        return _propertyInfo.GetValue(_post) as string;
    }

    [Benchmark(Description = "ProperyFunction")]
    public string ProperyFunction()
    {
        return _functionAccessor(_post) as string;
    }

    [Benchmark(Description = "ProperyAccessor")]
    public string ProperyAccessor()
    {
        return _propertyAccessor.GetValue(_post) as string;
    }

    [Benchmark(Description = "PropertyWrite")]
    public void PropertyWrite()
    {
        _testObject.Title = "Benchmark Value";
    }

    [Benchmark(Description = "PropertyReflectionWrite")]
    public void PropertyReflectionWrite()
    {
        _setPropertyInfo.SetValue(_testObject, "Benchmark Value");
    }

    [Benchmark(Description = "PropertyFunctionWrite")]
    public void PropertyFunctionWrite()
    {
        _setFunctionAccessor(_testObject, "Benchmark Value");
    }

    [Benchmark(Description = "PropertyAccessorWrite")]
    public void PropertyAccessorWrite()
    {
        _setPropertyAccessor.SetValue(_testObject, "Benchmark Value");
    }
}

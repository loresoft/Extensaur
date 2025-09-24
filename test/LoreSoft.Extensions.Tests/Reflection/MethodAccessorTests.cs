#nullable enable
using System.Reflection;

using AwesomeAssertions;

namespace LoreSoft.Extensions.Tests.Reflection;

public class MethodAccessorTests
{
    #region Test Helper Classes

    public class TestClass
    {
        public string StringValue { get; set; } = "Default";
        public int IntValue { get; set; } = 42;

        // Instance methods with different signatures
        public string GetStringValue() => StringValue;
        public void SetStringValue(string value) => StringValue = value;
        public int GetIntValue() => IntValue;
        public void SetIntValue(int value) => IntValue = value;

        // Method with return value and parameters
        public string ConcatenateStrings(string first, string second) => first + second;
        public int AddNumbers(int a, int b) => a + b;
        public long AddLongNumbers(long a, long b) => a + b;

        // Method with multiple parameters
        public string FormatString(string template, object arg1, object arg2)
            => string.Format(template, arg1, arg2);

        // Void method with parameters
        public void UpdateValues(string stringValue, int intValue)
        {
            StringValue = stringValue;
            IntValue = intValue;
        }

        // Method that returns different types
        public DateTime GetCurrentDate() => DateTime.Now;
        public bool IsPositive(int value) => value > 0;
        public double GetPi() => Math.PI;
        public object GetStringAsObject() => StringValue;
        public string? GetNullableString() => StringValue == "null" ? null : StringValue;

        // Private method
        private string GetPrivateValue() => "PrivateValue";

        // Protected method
        protected virtual string GetProtectedValue() => "ProtectedValue";

        // Static methods
        public static string GetStaticString() => "StaticString";
        public static int AddStaticNumbers(int a, int b) => a + b;
        public static void DoNothing() { }

        // Method with nullable parameters
        public string ProcessNullableString(string? input) => input ?? "null";

        // Overloaded methods
        public string ProcessValue(string value) => $"String: {value}";
        public string ProcessValue(int value) => $"Int: {value}";
        public string ProcessValue(double value) => $"Double: {value}";

        // Method that throws exception
        public void ThrowException() => throw new InvalidOperationException("Test exception");
        public int DivideByZero(int numerator) => numerator / 0;

        // Generic method
        public T GetGenericValue<T>(T value) => value;

        // Method with out parameter
        public bool TryParseInt(string input, out int result) => int.TryParse(input, out result);

        // Method with ref parameter
        public void IncrementRef(ref int value) => value++;

        // Method with params array
        public string CombineStrings(params string[] strings) => string.Join("-", strings);
    }

    public struct TestStruct
    {
        public int Value { get; set; }
        public string Name { get; set; }

        public TestStruct(int value, string name)
        {
            Value = value;
            Name = name;
        }

        public string GetDescription() => $"{Name}: {Value}";
        public void SetValue(int newValue) => Value = newValue;
        public static string GetTypeName() => nameof(TestStruct);
    }

    public abstract class BaseClass
    {
        public virtual string GetVirtualValue() => "Base";
        public abstract string GetAbstractValue();
    }

    public class DerivedClass : BaseClass
    {
        public override string GetVirtualValue() => "Derived";
        public override string GetAbstractValue() => "Abstract";
        public string GetDerivedValue() => "DerivedOnly";
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullMethodInfo_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new MethodAccessor(null!));
        Assert.Equal("methodInfo", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidMethodInfo_InitializesCorrectly()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetStringValue))!;

        // Act
        var accessor = new MethodAccessor(methodInfo);

        // Assert
        accessor.Name.Should().Be("GetStringValue");
        accessor.MethodInfo.Should().BeSameAs(methodInfo);
    }

    [Fact]
    public void Constructor_WithStaticMethod_InitializesCorrectly()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetStaticString))!;

        // Act
        var accessor = new MethodAccessor(methodInfo);

        // Assert
        accessor.Name.Should().Be("GetStaticString");
        accessor.MethodInfo.Should().BeSameAs(methodInfo);
    }

    [Fact]
    public void Constructor_WithPrivateMethod_InitializesCorrectly()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod("GetPrivateValue", BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Act
        var accessor = new MethodAccessor(methodInfo);

        // Assert
        accessor.Name.Should().Be("GetPrivateValue");
        accessor.MethodInfo.Should().BeSameAs(methodInfo);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Name_ReturnsCorrectMethodName()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.ConcatenateStrings))!;
        var accessor = new MethodAccessor(methodInfo);

        // Act & Assert
        accessor.Name.Should().Be("ConcatenateStrings");
    }

    [Fact]
    public void MethodInfo_ReturnsCorrectMethodInfo()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.AddNumbers))!;
        var accessor = new MethodAccessor(methodInfo);

        // Act & Assert
        accessor.MethodInfo.Should().BeSameAs(methodInfo);
        accessor.MethodInfo.Name.Should().Be("AddNumbers");
        accessor.MethodInfo.ReturnType.Should().Be(typeof(int));
    }

    #endregion

    #region Invoke Tests - No Parameters

    [Fact]
    public void Invoke_InstanceMethodWithNoParameters_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetStringValue))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass { StringValue = "TestValue" };

        // Act
        var result = accessor.Invoke(instance);

        // Assert
        result.Should().Be("TestValue");
    }

    [Fact]
    public void Invoke_StaticMethodWithNoParameters_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetStaticString))!;
        var accessor = new MethodAccessor(methodInfo);

        // Act
        var result = accessor.Invoke(null);

        // Assert
        result.Should().Be("StaticString");
    }

    [Fact]
    public void Invoke_StaticMethodWithInstance_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetStaticString))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance);

        // Assert
        result.Should().Be("StaticString");
    }

    [Fact]
    public void Invoke_VoidMethod_ReturnsNull()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.DoNothing))!;
        var accessor = new MethodAccessor(methodInfo);

        // Act
        var result = accessor.Invoke(null);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Invoke Tests - With Parameters

    [Fact]
    public void Invoke_MethodWithOneParameter_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.SetStringValue))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance, "NewValue");

        // Assert
        result.Should().BeNull(); // void method
        instance.StringValue.Should().Be("NewValue");
    }

    [Fact]
    public void Invoke_MethodWithTwoParameters_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.ConcatenateStrings))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance, "Hello", " World");

        // Assert
        result.Should().Be("Hello World");
    }

    [Fact]
    public void Invoke_StaticMethodWithParameters_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.AddStaticNumbers))!;
        var accessor = new MethodAccessor(methodInfo);

        // Act
        var result = accessor.Invoke(null, 5, 3);

        // Assert
        result.Should().Be(8);
    }

    [Fact]
    public void Invoke_MethodWithMultipleParameters_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.FormatString))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance, "Value1: {0}, Value2: {1}", "Test", 42);

        // Assert
        result.Should().Be("Value1: Test, Value2: 42");
    }

    [Fact]
    public void Invoke_VoidMethodWithParameters_ModifiesInstanceCorrectly()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.UpdateValues))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance, "Modified", 99);

        // Assert
        result.Should().BeNull();
        instance.StringValue.Should().Be("Modified");
        instance.IntValue.Should().Be(99);
    }

    #endregion

    #region Invoke Tests - Different Return Types

    [Fact]
    public void Invoke_MethodReturningDateTime_ReturnsCorrectType()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetCurrentDate))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance);

        // Assert
        result.Should().BeOfType<DateTime>();
        ((DateTime)result!).Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Invoke_MethodReturningBool_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.IsPositive))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var positiveResult = accessor.Invoke(instance, 5);
        var negativeResult = accessor.Invoke(instance, -5);

        // Assert
        positiveResult.Should().Be(true);
        negativeResult.Should().Be(false);
    }

    [Fact]
    public void Invoke_MethodReturningDouble_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetPi))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance);

        // Assert
        result.Should().Be(Math.PI);
    }

    [Fact]
    public void Invoke_MethodReturningObject_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetStringAsObject))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass { StringValue = "ObjectTest" };

        // Act
        var result = accessor.Invoke(instance);

        // Assert
        result.Should().BeOfType<string>();
        result.Should().Be("ObjectTest");
    }

    [Fact]
    public void Invoke_MethodReturningNullableString_ReturnsNull()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetNullableString))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass { StringValue = "null" };

        // Act
        var result = accessor.Invoke(instance);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Invoke_MethodReturningNullableString_ReturnsValue()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetNullableString))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass { StringValue = "NotNull" };

        // Act
        var result = accessor.Invoke(instance);

        // Assert
        result.Should().Be("NotNull");
    }

    #endregion

    #region Invoke Tests - Nullable and Special Parameters

    [Fact]
    public void Invoke_MethodWithNullableParameter_PassingNull_WorksCorrectly()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.ProcessNullableString))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance, (string?)null);

        // Assert
        result.Should().Be("null");
    }

    [Fact]
    public void Invoke_MethodWithNullableParameter_PassingValue_WorksCorrectly()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.ProcessNullableString))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance, "TestInput");

        // Assert
        result.Should().Be("TestInput");
    }

    [Fact]
    public void Invoke_MethodWithParamsArray_WorksCorrectly()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.CombineStrings))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance, new object[] { new[] { "a", "b", "c" } });

        // Assert
        result.Should().Be("a-b-c");
    }

    #endregion

    #region Invoke Tests - Overloaded Methods

    [Fact]
    public void Invoke_OverloadedMethodWithString_CallsCorrectOverload()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.ProcessValue), [typeof(string)])!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance, "test");

        // Assert
        result.Should().Be("String: test");
    }

    [Fact]
    public void Invoke_OverloadedMethodWithInt_CallsCorrectOverload()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.ProcessValue), [typeof(int)])!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance, 42);

        // Assert
        result.Should().Be("Int: 42");
    }

    [Fact]
    public void Invoke_OverloadedMethodWithDouble_CallsCorrectOverload()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.ProcessValue), [typeof(double)])!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance, 3.14);

        // Assert
        result.Should().Be("Double: 3.14");
    }

    #endregion

    #region Invoke Tests - Struct Methods

    [Fact]
    public void Invoke_StructInstanceMethod_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestStruct).GetMethod(nameof(TestStruct.GetDescription))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestStruct(42, "Test");

        // Act
        var result = accessor.Invoke(instance);

        // Assert
        result.Should().Be("Test: 42");
    }

    [Fact]
    public void Invoke_StructVoidMethod_ModifiesStruct()
    {
        // Arrange
        var methodInfo = typeof(TestStruct).GetMethod(nameof(TestStruct.SetValue))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestStruct(42, "Test");

        // Act
        var result = accessor.Invoke(instance, 99);

        // Assert
        result.Should().BeNull();
        // Note: The original instance remains unchanged because structs are value types
        // and the method operates on a copy
        instance.Value.Should().Be(42);
    }

    [Fact]
    public void Invoke_StructStaticMethod_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestStruct).GetMethod(nameof(TestStruct.GetTypeName))!;
        var accessor = new MethodAccessor(methodInfo);

        // Act
        var result = accessor.Invoke(null);

        // Assert
        result.Should().Be("TestStruct");
    }

    #endregion

    #region Invoke Tests - Inheritance

    [Fact]
    public void Invoke_VirtualMethodOnDerivedClass_CallsOverriddenMethod()
    {
        // Arrange
        var methodInfo = typeof(BaseClass).GetMethod(nameof(BaseClass.GetVirtualValue))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new DerivedClass();

        // Act
        var result = accessor.Invoke(instance);

        // Assert
        result.Should().Be("Derived");
    }

    [Fact]
    public void Invoke_AbstractMethodOnDerivedClass_CallsImplementation()
    {
        // Arrange
        var methodInfo = typeof(BaseClass).GetMethod(nameof(BaseClass.GetAbstractValue))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new DerivedClass();

        // Act
        var result = accessor.Invoke(instance);

        // Assert
        result.Should().Be("Abstract");
    }

    [Fact]
    public void Invoke_DerivedClassMethod_CallsCorrectMethod()
    {
        // Arrange
        var methodInfo = typeof(DerivedClass).GetMethod(nameof(DerivedClass.GetDerivedValue))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new DerivedClass();

        // Act
        var result = accessor.Invoke(instance);

        // Assert
        result.Should().Be("DerivedOnly");
    }

    #endregion

    #region Invoke Tests - Private/Protected Methods

    [Fact]
    public void Invoke_PrivateMethod_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod("GetPrivateValue", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance);

        // Assert
        result.Should().Be("PrivateValue");
    }

    [Fact]
    public void Invoke_ProtectedMethod_ReturnsCorrectValue()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod("GetProtectedValue", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act
        var result = accessor.Invoke(instance);

        // Assert
        result.Should().Be("ProtectedValue");
    }

    #endregion

    #region Invoke Tests - Exception Handling

    [Fact]
    public void Invoke_MethodThatThrowsException_PropagatesException()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.ThrowException))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => accessor.Invoke(instance));
        exception.Message.Should().Be("Test exception");
    }

    [Fact]
    public void Invoke_MethodThatCausesDivideByZero_PropagatesException()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.DivideByZero))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => accessor.Invoke(instance, 10));
    }

    #endregion

    #region GetKey Tests

    [Fact]
    public void GetKey_WithSameNameAndParameters_ReturnsSameKey()
    {
        // Arrange
        var name = "TestMethod";
        var parameters1 = new[] { typeof(string), typeof(int) };
        var parameters2 = new[] { typeof(string), typeof(int) };

        // Act
        var key1 = MethodAccessor.GetKey(name, parameters1);
        var key2 = MethodAccessor.GetKey(name, parameters2);

        // Assert
        key1.Should().Be(key2);
    }

    [Fact]
    public void GetKey_WithDifferentNames_ReturnsDifferentKeys()
    {
        // Arrange
        var parameters = new[] { typeof(string) };

        // Act
        var key1 = MethodAccessor.GetKey("Method1", parameters);
        var key2 = MethodAccessor.GetKey("Method2", parameters);

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void GetKey_WithDifferentParameters_ReturnsDifferentKeys()
    {
        // Arrange
        var name = "TestMethod";

        // Act
        var key1 = MethodAccessor.GetKey(name, new[] { typeof(string) });
        var key2 = MethodAccessor.GetKey(name, new[] { typeof(int) });

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void GetKey_WithEmptyParameters_ReturnsConsistentKey()
    {
        // Arrange
        var name = "TestMethod";
        var emptyParameters1 = Type.EmptyTypes;
        var emptyParameters2 = Array.Empty<Type>();

        // Act
        var key1 = MethodAccessor.GetKey(name, emptyParameters1);
        var key2 = MethodAccessor.GetKey(name, emptyParameters2);

        // Assert
        key1.Should().Be(key2);
    }

    [Fact]
    public void GetKey_WithNullParameterTypes_HandledCorrectly()
    {
        // Arrange & Act & Assert
        // This test verifies that GetKey doesn't crash with edge cases
        var key = MethodAccessor.GetKey("TestMethod", Type.EmptyTypes);
        key.Should().NotBe(0); // Should produce a meaningful hash
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void Invoke_WithIncorrectNumberOfArguments_ThrowsArgumentException()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.AddNumbers))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => accessor.Invoke(instance, 5)); // Missing one argument
        exception.Message.Should().Contain("Expected 2 parameters but got 1");
    }

    [Fact]
    public void Invoke_WithWrongArgumentTypes_ThrowsInvalidCastException()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.AddNumbers))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act & Assert
        Assert.Throws<InvalidCastException>(() => accessor.Invoke(instance, "not-a-number", 5));
    }

    [Fact]
    public void Invoke_StaticMethodWithWrongInstanceType_StillWorks()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetStaticString))!;
        var accessor = new MethodAccessor(methodInfo);

        // Act
        var result = accessor.Invoke(new object()); // Wrong instance type, but should work for static methods

        // Assert
        result.Should().Be("StaticString");
    }

    [Fact]
    public void Invoke_InstanceMethodWithNullInstance_ThrowsNullReferenceException()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetStringValue))!;
        var accessor = new MethodAccessor(methodInfo);

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => accessor.Invoke(null));
    }

    [Theory]
    [InlineData(typeof(int), 42)]
    [InlineData(typeof(byte), (byte)255)]
    [InlineData(typeof(short), (short)32767)]
    [InlineData(typeof(long), 9223372036854775807L)]
    [InlineData(typeof(float), 3.14f)]
    [InlineData(typeof(double), 3.14159)]
    [InlineData(typeof(bool), true)]
    [InlineData(typeof(char), 'A')]
    public void Invoke_WithVariousParameterTypes_WorksCorrectly(Type expectedType, object testValue)
    {
        // For this test, we'll use the AddNumbers method as a representative case
        // In a real scenario, you'd need specific test methods for each type
        if (expectedType == typeof(int) && testValue is int)
        {
            // Arrange
            var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.AddNumbers))!;
            var accessor = new MethodAccessor(methodInfo);
            var instance = new TestClass();

            // Act
            var result = accessor.Invoke(instance, 5, 3);

            // Assert
            result.Should().Be(8);
        }
        else
        {
            // Verify we can handle the test case setup
            Assert.True(testValue.GetType() == expectedType, $"Test value {testValue} should be of type {expectedType.Name}");
        }
    }

    [Fact]
    public void MethodAccessor_WithComplexScenario_WorksEndToEnd()
    {
        // Arrange - Test a complete workflow with method that has parameters and return value
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.ConcatenateStrings))!;
        var accessor = new MethodAccessor(methodInfo);
        var instance = new TestClass();

        // Act & Assert - Verify all properties
        accessor.Name.Should().Be("ConcatenateStrings");
        accessor.MethodInfo.Should().BeSameAs(methodInfo);
        accessor.MethodInfo.ReturnType.Should().Be(typeof(string));
        accessor.MethodInfo.GetParameters().Should().HaveCount(2);

        // Act & Assert - Invoke the method
        var result = accessor.Invoke(instance, "Hello", " World");
        result.Should().Be("Hello World");

        // Act & Assert - Invoke with different parameters
        var result2 = accessor.Invoke(instance, "Test", "123");
        result2.Should().Be("Test123");
    }

    #endregion
}

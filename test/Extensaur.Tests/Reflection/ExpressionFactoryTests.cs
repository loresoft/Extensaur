#nullable enable
using System.Reflection;

using AwesomeAssertions;

namespace LoreSoft.Extensions.Tests.Reflection;

public class ExpressionFactoryTests
{
    #region Test Helper Classes

    public class TestClass
    {
        public string StringProperty { get; set; } = "Default";
        public int IntProperty { get; set; } = 42;
        public DateTime DateTimeProperty { get; set; } = DateTime.Now;
        public string? NullableStringProperty { get; set; }
        public int? NullableIntProperty { get; set; }

        public string StringField = "FieldValue";
        public readonly string ReadonlyField = "ReadonlyValue";
        public static string StaticProperty { get; set; } = "StaticValue";
        public static string StaticField = "StaticFieldValue";

        public string GetString() => "TestMethod";
        public string GetStringWithParam(string input) => $"Param: {input}";
        public int Add(int a, int b) => a + b;
        public void VoidMethod() { }
        public void VoidMethodWithParam(string input) { }

        public static string StaticMethod() => "StaticMethodResult";
        public static string StaticMethodWithParam(string input) => $"Static: {input}";

        private string PrivateProperty { get; set; } = "Private";
        private string PrivateMethod() => "PrivateResult";
    }

    public class TestClassWithConstructors
    {
        public string Value { get; }
        public int Number { get; }

        public TestClassWithConstructors()
        {
            Value = "Default";
            Number = 0;
        }

        public TestClassWithConstructors(string value)
        {
            Value = value;
            Number = 1;
        }

        public TestClassWithConstructors(string value, int number)
        {
            Value = value;
            Number = number;
        }
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
    }

    #endregion

    #region CreateMethod Tests

    [Fact]
    public void CreateMethod_WithNullMethodInfo_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => ExpressionFactory.CreateMethod(null!));
        Assert.Equal("methodInfo", exception.ParamName);
    }

    [Fact]
    public void CreateMethod_WithInstanceMethodNoParameters_ReturnsCorrectDelegate()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetString))!;
        var testInstance = new TestClass();

        // Act
        var methodDelegate = ExpressionFactory.CreateMethod(methodInfo);
        var result = methodDelegate(testInstance, []);

        // Assert
        result.Should().Be("TestMethod");
    }

    [Fact]
    public void CreateMethod_WithInstanceMethodWithParameters_ReturnsCorrectDelegate()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetStringWithParam))!;
        var testInstance = new TestClass();

        // Act
        var methodDelegate = ExpressionFactory.CreateMethod(methodInfo);
        var result = methodDelegate(testInstance, ["Hello"]);

        // Assert
        result.Should().Be("Param: Hello");
    }

    [Fact]
    public void CreateMethod_WithStaticMethod_ReturnsCorrectDelegate()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.StaticMethod))!;

        // Act
        var methodDelegate = ExpressionFactory.CreateMethod(methodInfo);
        var result = methodDelegate(null, []);

        // Assert
        result.Should().Be("StaticMethodResult");
    }

    [Fact]
    public void CreateMethod_WithStaticMethodWithParameters_ReturnsCorrectDelegate()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.StaticMethodWithParam))!;

        // Act
        var methodDelegate = ExpressionFactory.CreateMethod(methodInfo);
        var result = methodDelegate(null, ["Test"]);

        // Assert
        result.Should().Be("Static: Test");
    }

    [Fact]
    public void CreateMethod_WithVoidMethod_ReturnsNullResult()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.VoidMethod))!;
        var testInstance = new TestClass();

        // Act
        var methodDelegate = ExpressionFactory.CreateMethod(methodInfo);
        var result = methodDelegate(testInstance, []);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CreateMethod_WithVoidMethodWithParameters_ReturnsNullResult()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.VoidMethodWithParam))!;
        var testInstance = new TestClass();

        // Act
        var methodDelegate = ExpressionFactory.CreateMethod(methodInfo);
        var result = methodDelegate(testInstance, ["test"]);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CreateMethod_WithValueTypeParameters_HandlesCorrectly()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.Add))!;
        var testInstance = new TestClass();

        // Act
        var methodDelegate = ExpressionFactory.CreateMethod(methodInfo);
        var result = methodDelegate(testInstance, [5, 3]);

        // Assert
        result.Should().Be(8);
    }

    [Fact]
    public void CreateMethod_WithWrongParameterCount_ThrowsArgumentException()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.Add))!;
        var methodDelegate = ExpressionFactory.CreateMethod(methodInfo);
        var testInstance = new TestClass();

        // Act & Assert
        Action act = () => methodDelegate(testInstance, [5]);
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Expected 2 parameters but got 1*");
    }

    [Fact]
    public void CreateMethod_WithNullParameters_HandlesValueTypesCorrectly()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.Add))!;
        var methodDelegate = ExpressionFactory.CreateMethod(methodInfo);
        var testInstance = new TestClass();

        // Act
        var result = methodDelegate(testInstance, [null, null]);

        // Assert
        result.Should().Be(0); // Default values for int
    }

    #endregion

    #region CreateConstructor (Parameterless) Tests

    [Fact]
    public void CreateConstructor_WithNullType_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => ExpressionFactory.CreateConstructor((Type)null!));
        Assert.Equal("type", exception.ParamName);
    }

    [Fact]
    public void CreateConstructor_WithDefaultConstructor_ReturnsCorrectDelegate()
    {
        // Act
        var constructorDelegate = ExpressionFactory.CreateConstructor(typeof(TestClassWithConstructors));
        var result = constructorDelegate!();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestClassWithConstructors>();
        var typedResult = (TestClassWithConstructors)result!;
        typedResult.Value.Should().Be("Default");
        typedResult.Number.Should().Be(0);
    }

    [Fact]
    public void CreateConstructor_WithNoDefaultConstructor_ReturnsNull()
    {
        // Arrange - string has no parameterless constructor

        // Act
        var constructorDelegate = ExpressionFactory.CreateConstructor(typeof(string));

        // Assert
        constructorDelegate.Should().BeNull();
    }

    #endregion

    #region CreateConstructor (With Parameters) Tests

    [Fact]
    public void CreateConstructor_WithNullConstructorInfo_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => ExpressionFactory.CreateConstructor((ConstructorInfo)null!));
        Assert.Equal("constructorInfo", exception.ParamName);
    }

    [Fact]
    public void CreateConstructor_WithParameterizedConstructor_ReturnsCorrectDelegate()
    {
        // Arrange
        var constructorInfo = typeof(TestClassWithConstructors).GetConstructor([typeof(string)])!;

        // Act
        var constructorDelegate = ExpressionFactory.CreateConstructor(constructorInfo);
        var result = constructorDelegate(["TestValue"]);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestClassWithConstructors>();
        var typedResult = (TestClassWithConstructors)result!;
        typedResult.Value.Should().Be("TestValue");
        typedResult.Number.Should().Be(1);
    }

    [Fact]
    public void CreateConstructor_WithMultipleParameters_ReturnsCorrectDelegate()
    {
        // Arrange
        var constructorInfo = typeof(TestClassWithConstructors).GetConstructor([typeof(string), typeof(int)])!;

        // Act
        var constructorDelegate = ExpressionFactory.CreateConstructor(constructorInfo);
        var result = constructorDelegate(["TestValue", 42]);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestClassWithConstructors>();
        var typedResult = (TestClassWithConstructors)result!;
        typedResult.Value.Should().Be("TestValue");
        typedResult.Number.Should().Be(42);
    }

    [Fact]
    public void CreateConstructor_WithWrongParameterCount_ThrowsArgumentException()
    {
        // Arrange
        var constructorInfo = typeof(TestClassWithConstructors).GetConstructor([typeof(string), typeof(int)])!;
        var constructorDelegate = ExpressionFactory.CreateConstructor(constructorInfo);

        // Act & Assert
        Action act = () => constructorDelegate(["TestValue"]);
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Constructor expected 2 parameters but got 1*");
    }

    [Fact]
    public void CreateConstructor_WithStructType_ReturnsCorrectDelegate()
    {
        // Arrange
        var constructorInfo = typeof(TestStruct).GetConstructor([typeof(int), typeof(string)])!;

        // Act
        var constructorDelegate = ExpressionFactory.CreateConstructor(constructorInfo);
        var result = constructorDelegate([42, "TestStruct"]);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestStruct>();
        var typedResult = (TestStruct)result!;
        typedResult.Value.Should().Be(42);
        typedResult.Name.Should().Be("TestStruct");
    }

    #endregion

    #region CreateGet (Property) Tests

    [Fact]
    public void CreateGet_PropertyWithNullPropertyInfo_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => ExpressionFactory.CreateGet((PropertyInfo)null!));
        Assert.Equal("propertyInfo", exception.ParamName);
    }

    [Fact]
    public void CreateGet_WithReadableProperty_ReturnsCorrectDelegate()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty))!;
        var testInstance = new TestClass { StringProperty = "TestValue" };

        // Act
        var getDelegate = ExpressionFactory.CreateGet(propertyInfo);
        var result = getDelegate!(testInstance);

        // Assert
        result.Should().Be("TestValue");
    }

    [Fact]
    public void CreateGet_WithStaticProperty_ReturnsCorrectDelegate()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StaticProperty))!;
        TestClass.StaticProperty = "StaticTestValue";

        // Act
        var getDelegate = ExpressionFactory.CreateGet(propertyInfo);
        var result = getDelegate!(null);

        // Assert
        result.Should().Be("StaticTestValue");
    }

    [Fact]
    public void CreateGet_WithValueTypeProperty_ReturnsCorrectDelegate()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.IntProperty))!;
        var testInstance = new TestClass { IntProperty = 123 };

        // Act
        var getDelegate = ExpressionFactory.CreateGet(propertyInfo);
        var result = getDelegate!(testInstance);

        // Assert
        result.Should().Be(123);
    }

    [Fact]
    public void CreateGet_WithPrivateProperty_ReturnsCorrectDelegate()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty("PrivateProperty", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var testInstance = new TestClass();

        // Act
        var getDelegate = ExpressionFactory.CreateGet(propertyInfo);
        var result = getDelegate!(testInstance);

        // Assert
        result.Should().Be("Private");
    }

    #endregion

    #region CreateGet (Field) Tests

    [Fact]
    public void CreateGet_FieldWithNullFieldInfo_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => ExpressionFactory.CreateGet((FieldInfo)null!));
        Assert.Equal("fieldInfo", exception.ParamName);
    }

    [Fact]
    public void CreateGet_WithField_ReturnsCorrectDelegate()
    {
        // Arrange
        var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.StringField))!;
        var testInstance = new TestClass();
        testInstance.StringField = "ModifiedFieldValue";

        // Act
        var getDelegate = ExpressionFactory.CreateGet(fieldInfo);
        var result = getDelegate!(testInstance);

        // Assert
        result.Should().Be("ModifiedFieldValue");
    }

    [Fact]
    public void CreateGet_WithReadonlyField_ReturnsCorrectDelegate()
    {
        // Arrange
        var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.ReadonlyField))!;
        var testInstance = new TestClass();

        // Act
        var getDelegate = ExpressionFactory.CreateGet(fieldInfo);
        var result = getDelegate!(testInstance);

        // Assert
        result.Should().Be("ReadonlyValue");
    }

    [Fact]
    public void CreateGet_WithStaticField_ReturnsCorrectDelegate()
    {
        // Arrange
        var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.StaticField))!;
        TestClass.StaticField = "ModifiedStaticField";

        // Act
        var getDelegate = ExpressionFactory.CreateGet(fieldInfo);
        var result = getDelegate!(null);

        // Assert
        result.Should().Be("ModifiedStaticField");
    }

    #endregion

    #region CreateSet (Property) Tests

    [Fact]
    public void CreateSet_PropertyWithNullPropertyInfo_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => ExpressionFactory.CreateSet((PropertyInfo)null!));
        Assert.Equal("propertyInfo", exception.ParamName);
    }

    [Fact]
    public void CreateSet_WithWritableProperty_ReturnsCorrectDelegate()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty))!;
        var testInstance = new TestClass();

        // Act
        var setDelegate = ExpressionFactory.CreateSet(propertyInfo);
        setDelegate!(testInstance, "NewValue");

        // Assert
        testInstance.StringProperty.Should().Be("NewValue");
    }

    [Fact]
    public void CreateSet_WithStaticProperty_ReturnsCorrectDelegate()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StaticProperty))!;

        // Act
        var setDelegate = ExpressionFactory.CreateSet(propertyInfo);
        setDelegate!(null, "NewStaticValue");

        // Assert
        TestClass.StaticProperty.Should().Be("NewStaticValue");
    }

    [Fact]
    public void CreateSet_WithValueTypeProperty_ReturnsCorrectDelegate()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.IntProperty))!;
        var testInstance = new TestClass();

        // Act
        var setDelegate = ExpressionFactory.CreateSet(propertyInfo);
        setDelegate!(testInstance, 999);

        // Assert
        testInstance.IntProperty.Should().Be(999);
    }

    [Fact]
    public void CreateSet_WithNullableProperty_HandlesNullCorrectly()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.NullableStringProperty))!;
        var testInstance = new TestClass();

        // Act
        var setDelegate = ExpressionFactory.CreateSet(propertyInfo);
        setDelegate!(testInstance, null);

        // Assert
        testInstance.NullableStringProperty.Should().BeNull();
    }

    [Fact]
    public void CreateSet_WithReadOnlyProperty_ReturnsNull()
    {
        // Arrange
        var propertyInfo = typeof(TestClassWithConstructors).GetProperty(nameof(TestClassWithConstructors.Value))!; // Use a proper read-only property

        // Act
        var setDelegate = ExpressionFactory.CreateSet(propertyInfo);

        // Assert
        setDelegate.Should().BeNull();
    }

    #endregion

    #region CreateSet (Field) Tests

    [Fact]
    public void CreateSet_FieldWithNullFieldInfo_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => ExpressionFactory.CreateSet((FieldInfo)null!));
        Assert.Equal("fieldInfo", exception.ParamName);
    }

    [Fact]
    public void CreateSet_WithField_ReturnsCorrectDelegate()
    {
        // Arrange
        var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.StringField))!;
        var testInstance = new TestClass();

        // Act
        var setDelegate = ExpressionFactory.CreateSet(fieldInfo);
        setDelegate!(testInstance, "NewFieldValue");

        // Assert
        testInstance.StringField.Should().Be("NewFieldValue");
    }

    [Fact]
    public void CreateSet_WithStaticField_ReturnsCorrectDelegate()
    {
        // Arrange
        var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.StaticField))!;

        // Act
        var setDelegate = ExpressionFactory.CreateSet(fieldInfo);
        setDelegate!(null, "NewStaticFieldValue");

        // Assert
        TestClass.StaticField.Should().Be("NewStaticFieldValue");
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void CreateMethod_WithNullInstance_ForInstanceMethod_HandlesGracefully()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.GetString))!;
        var methodDelegate = ExpressionFactory.CreateMethod(methodInfo);

        // Act & Assert
        Action act = () => methodDelegate(null, []);
        act.Should().Throw<Exception>(); // Should handle null instance appropriately
    }

    [Fact]
    public void CreateGet_WithNullInstance_ForInstanceProperty_HandlesGracefully()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty))!;
        var getDelegate = ExpressionFactory.CreateGet(propertyInfo);

        // Act & Assert
        Action act = () => getDelegate!(null);
        act.Should().Throw<Exception>(); // Should handle null instance appropriately
    }

    [Fact]
    public void CreateSet_WithNullInstance_ForInstanceProperty_HandlesGracefully()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty))!;
        var setDelegate = ExpressionFactory.CreateSet(propertyInfo);

        // Act & Assert
        Action act = () => setDelegate!(null, "test");
        act.Should().Throw<Exception>(); // Should handle null instance appropriately
    }

    [Fact]
    public void CreateMethod_WithParameterTypeMismatch_HandlesCorrectly()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.Add))!;
        var methodDelegate = ExpressionFactory.CreateMethod(methodInfo);
        var testInstance = new TestClass();

        // Act & Assert
        Action act = () => methodDelegate(testInstance, ["5", "3"]); // String instead of int
        act.Should().Throw<InvalidCastException>()
           .WithMessage("*Unable to cast object of type 'System.String' to type 'System.Int32'*");
    }

    #endregion
}

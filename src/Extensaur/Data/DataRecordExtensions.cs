using System.Data.Common;

#nullable enable

namespace System.Data;

/// <summary>
/// Extension methods for <see cref="IDataRecord"/>.
/// </summary>
public static class DataRecordExtensions
{
    /// <summary>Gets the value of the specified column.</summary>
    /// <param name="dataRecord">The data record.</param>
    /// <param name="name">The name of the column to find.</param>
    /// <returns>The value of the specified column.</returns>
    public static object? GetValue(this IDataRecord dataRecord, string name)
    {
        if (dataRecord is null)
            throw new ArgumentNullException(nameof(dataRecord));

        int ordinal = dataRecord.GetOrdinal(name);
        return dataRecord.IsDBNull(ordinal)
            ? null
            : dataRecord.GetValue(ordinal);
    }

    /// <summary>
    /// Gets the value of the specified column as the requested type.
    /// </summary>
    /// <typeparam name="T">The record value type</typeparam>
    /// <param name="dataRecord">The data record.</param>
    /// <param name="name">The name of the column to find.</param>
    /// <returns>The value of the specified column.</returns>
    public static T? GetValue<T>(this IDataRecord dataRecord, string name)
    {
        if (dataRecord is null)
            throw new ArgumentNullException(nameof(dataRecord));

        int ordinal = dataRecord.GetOrdinal(name);

        return GetValue<T>(dataRecord, ordinal);
    }

    /// <summary>
    /// Gets the value of the specified column as the requested type.
    /// </summary>
    /// <typeparam name="T">The record value type</typeparam>
    /// <param name="dataRecord">The data record.</param>
    /// <param name="ordinal">The zero-based column ordinal.</param>
    /// <returns>The value of the specified column.</returns>
    public static T? GetValue<T>(this IDataRecord dataRecord, int ordinal)
    {
        if (dataRecord is null)
            throw new ArgumentNullException(nameof(dataRecord));

        if (dataRecord.IsDBNull(ordinal))
            return default;

        if (dataRecord is DbDataReader dataReader)
            return dataReader.GetFieldValue<T>(ordinal);

        return (T)dataRecord.GetValue(ordinal);
    }

    /// <summary>Determines whether the specified field is set to <see langword="null"/>.</summary>
    /// <param name="dataRecord">The data record.</param>
    /// <param name="name">The <paramref name="name"/> of the field to find.</param>
    /// <returns><c>true</c> if the specified field is set to <see langword="null"/>; otherwise, <c>false</c>.</returns>
    public static bool IsDBNull(this IDataRecord dataRecord, string name)
    {
        int ordinal = dataRecord.GetOrdinal(name);
        return dataRecord.IsDBNull(ordinal);
    }

}

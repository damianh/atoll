using System.ComponentModel.DataAnnotations;

namespace Atoll.Build.Content.Frontmatter;

/// <summary>
/// Validates frontmatter data objects using <see cref="System.ComponentModel.DataAnnotations"/>
/// attributes defined on the schema type's properties.
/// </summary>
/// <remarks>
/// <para>
/// Schema types can use standard DataAnnotation attributes like <see cref="RequiredAttribute"/>,
/// <see cref="StringLengthAttribute"/>, <see cref="RangeAttribute"/>, etc. The validator collects
/// all validation errors and returns them as <see cref="FrontmatterValidationResult"/>.
/// </para>
/// </remarks>
public static class FrontmatterValidator
{
    /// <summary>
    /// Validates the specified data object using DataAnnotation attributes.
    /// </summary>
    /// <param name="data">The frontmatter data object to validate.</param>
    /// <returns>A <see cref="FrontmatterValidationResult"/> containing any validation errors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
    public static FrontmatterValidationResult Validate(object data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(data);
        var isValid = Validator.TryValidateObject(data, context, validationResults, validateAllProperties: true);

        return new FrontmatterValidationResult(isValid, validationResults);
    }
}

/// <summary>
/// The result of validating a frontmatter data object against its DataAnnotation schema.
/// </summary>
public sealed class FrontmatterValidationResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="FrontmatterValidationResult"/>.
    /// </summary>
    /// <param name="isValid">Whether the data passed all validations.</param>
    /// <param name="errors">The collection of validation errors, if any.</param>
    public FrontmatterValidationResult(bool isValid, IReadOnlyList<ValidationResult> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        IsValid = isValid;
        Errors = errors;
    }

    /// <summary>
    /// Gets a value indicating whether the data passed all validations.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the collection of validation errors. Empty when <see cref="IsValid"/> is <c>true</c>.
    /// </summary>
    public IReadOnlyList<ValidationResult> Errors { get; }

    /// <summary>
    /// Throws a <see cref="FrontmatterValidationException"/> if validation failed.
    /// </summary>
    /// <param name="entryId">The content entry identifier, used in the error message.</param>
    /// <exception cref="FrontmatterValidationException">Validation failed.</exception>
    public void ThrowIfInvalid(string entryId)
    {
        if (!IsValid)
        {
            var errorMessages = string.Join("; ", Errors.Select(e => e.ErrorMessage));
            throw new FrontmatterValidationException(
                $"Frontmatter validation failed for '{entryId}': {errorMessages}",
                Errors);
        }
    }
}

/// <summary>
/// Exception thrown when frontmatter data fails DataAnnotation validation.
/// </summary>
public sealed class FrontmatterValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="FrontmatterValidationException"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errors">The validation errors.</param>
    public FrontmatterValidationException(string message, IReadOnlyList<ValidationResult> errors) : base(message)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors;
    }

    /// <summary>
    /// Gets the validation errors that caused this exception.
    /// </summary>
    public IReadOnlyList<ValidationResult> Errors { get; }
}

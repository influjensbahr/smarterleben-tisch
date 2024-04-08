//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
using System;
using System.IO;
#if UNITY_EDITOR
using System.Reflection;
#endif
using System.Text.RegularExpressions;

namespace Sparrow.Verification
{
    [AttributeUsage(AttributeTargets.Field)]
    public class VStringCheckAttribute : Attribute, IVerifyAttribute
    {
        private StringCheck checks;
        public string DisallowedPattern { get; set; } = "";
        private int minLength;
        private int maxLength;

        public VStringCheckAttribute(StringCheck checks, string disallowPattern = "", int minLength = 0, int maxLength = int.MaxValue)
        {
            this.checks = checks;
            this.DisallowedPattern = disallowPattern;
            this.minLength = minLength;
            this.maxLength = maxLength;
        }

    #if UNITY_EDITOR
        public void PerformCheck(VerifyCheckBase checker, object value, FieldInfo field, UnityEngine.Object parentObject, VerifyResult.Severity severity)
        {
            if (field.FieldType == typeof(string))
            {
                // Get the custom attribute
                VStringCheckAttribute attribute = (VStringCheckAttribute)Attribute.GetCustomAttribute(field, typeof(VStringCheckAttribute));
                if (attribute != null)
                {
                    // Get the CheckType from the custom attribute
                    StringCheck checks = attribute.checks;
                    string stringValue = value as string;

                    if (checks.HasFlag(StringCheck.NotEmpty))
                        checker.Check(!string.IsNullOrEmpty(stringValue), $"StringCheck: {field.Name} should not be empty", parentObject, category: "VStringCheck attribute");

                    if (checks.HasFlag(StringCheck.URL))
                        checker.Check(Uri.IsWellFormedUriString(stringValue, UriKind.RelativeOrAbsolute), $"StringCheck: {field.Name} is not a valid URL", parentObject, category: "VStringCheck attribute");
                    if (checks.HasFlag(StringCheck.URLAbsolute))
                        checker.Check(Uri.IsWellFormedUriString(stringValue, UriKind.Absolute), $"StringCheck: {field.Name} is not a valid absolute URL", parentObject, category: "VStringCheck attribute");
                    if (checks.HasFlag(StringCheck.URLRelative))
                        checker.Check(Uri.IsWellFormedUriString(stringValue, UriKind.Relative), $"StringCheck: {field.Name} is not a valid relative URL", parentObject, category: "VStringCheck attribute");

                    if (checks.HasFlag(StringCheck.NoWhiteSpace))
                        checker.Check(!stringValue.Contains(" "), $"StringCheck: {field.Name} should not contain whitespace", parentObject, category: "VStringCheck attribute");

                    if (checks.HasFlag(StringCheck.Numeric))
                        checker.Check(Regex.IsMatch(stringValue, @"^\d+$"), $"StringCheck: {field.Name} should be numeric", parentObject, category: "VStringCheck attribute");

                    if (checks.HasFlag(StringCheck.Email))
                        checker.Check(Regex.IsMatch(stringValue, @"^[\w\.-]+@[a-zA-Z\d\.-]+\.[a-zA-Z]+$"), $"StringCheck: {field.Name} should be a valid email address", parentObject, category: "VStringCheck attribute");

                    if (checks.HasFlag(StringCheck.Alphabetic))
                        checker.Check(Regex.IsMatch(stringValue, @"^[a-zA-Z]+$"), $"StringCheck: {field.Name} should be alphabetic", parentObject, category: "VStringCheck attribute");

                    if (checks.HasFlag(StringCheck.Alphanumeric))
                        checker.Check(Regex.IsMatch(stringValue, @"^[a-zA-Z0-9]+$"), $"StringCheck: {field.Name} should be alphanumeric", parentObject, category: "VStringCheck attribute");

                    var pattern = attribute.DisallowedPattern;
                    if (pattern != null)
                    {
                        if (!pattern.Equals(""))
                        {
                            bool matchesPattern = Regex.IsMatch(stringValue, pattern);
                            checker.Check(!matchesPattern, $"StringCheck: {field.Name} matches a disallowed pattern", parentObject, category: "VStringCheck attribute");
                        }
                    }

                    if (checks.HasFlag(StringCheck.MinLength))
                        checker.Check(stringValue.Length >= attribute.minLength, $"StringCheck: {field.Name} should be at least {attribute.minLength} characters long", parentObject, category: "VStringCheck attribute");

                    if (checks.HasFlag(StringCheck.MaxLength))
                        checker.Check(stringValue.Length <= attribute.maxLength, $"StringCheck: {field.Name} should be at most {attribute.maxLength} characters long", parentObject, category: "VStringCheck attribute");

                    if (checks.HasFlag(StringCheck.FilePath))
                        checker.Check(File.Exists(stringValue), $"StringCheck: {field.Name} should be a valid file path", parentObject, category: "VStringCheck attribute");

                    if (checks.HasFlag(StringCheck.DirectoryPath))
                        checker.Check(Directory.Exists(stringValue), $"StringCheck: {field.Name} should be a valid directory path", parentObject, category: "VStringCheck attribute");


                }
            }
        }
    #endif
    }

}
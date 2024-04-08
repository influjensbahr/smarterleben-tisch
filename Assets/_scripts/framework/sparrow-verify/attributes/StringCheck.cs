//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System;

namespace Sparrow.Verification
{
    [Flags]
    public enum StringCheck
    {
        NotEmpty = 1 << 0,
        URL = 1 << 1,
        NoWhiteSpace = 1 << 2,
        Numeric = 1 << 3,
        Email = 1 << 4,
        Alphabetic = 1 << 5,
        Alphanumeric = 1 << 6,
        URLAbsolute = 1 << 7,
        URLRelative = 1 << 8,
        MinLength = 1 << 9,
        MaxLength = 1 << 10,
        FilePath = 1 << 11,
        DirectoryPath = 1 << 12
    }
}
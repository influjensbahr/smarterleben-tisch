//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

namespace Sparrow.Verification
{
    /// <summary>
    /// Any class implementing this can be checked for being valid using our project check window
    /// </summary>
    public interface IVerify
    {
        void Verify(CheckVerifyInterface checker);
    }
}
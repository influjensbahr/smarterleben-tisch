//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// An interface you can use to do some processing while the build runs - i.e. for collecting all objects of a specific type. In editor the function is called on “Play”, in builds during the build process. Make sure that the data you’re storing is put into serialized fields to make sure they are saved during the build process.
    /// </summary>
    public interface IPrepareOnBuild
    {
        /// <summary>
        /// This is executed on scene start in editor and during a build for stuff that does not change
        /// </summary>
        public void PrepareOnBuildOrAwake();
    }
}

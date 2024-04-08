using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OTBT.Framework.Core
{
    /// <summary>
    /// An interface classes can implement to follow a consistent structure for saving and loading across our project, and to easily identify which classes use saving and loading mechanics. Has automatically triggered methods for saving and loading when the savegame manager triggers it
    /// </summary
    public interface ISaveData
    {
        /// <summary>
        /// General method for saving data. Is not called automatically
        /// </summary>
        public void Save();
        /// <summary>
        /// General method for loading data. Is not called automatically
        /// </summary>
        public void Load();
        /// <summary>
        /// Executed before the SaveGame manager saves to disk - use this to save your data, if needed
        /// </summary>
        public void PreSaveAction();
        /// <summary>
        /// Executed on when the SaveGame manager loaded a save - use to load the new data, if needed
        /// </summary>
        public void WhenLoadReady();
    }
}

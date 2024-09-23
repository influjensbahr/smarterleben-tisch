using OTBT.Framework.Utils;
using System;
using System.Collections.Generic;
using TuioNet.Common;
using TuioNet.Tuio11;
using TuioUnity.Common;
using TuioUnity.Tuio11;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class CustomTuio11Visualizer : Singleton<CustomTuio11Visualizer>
{
    [SerializeField] CustomTuioSessionBehaviour _tuioSessionBehaviour;
    [SerializeField] CustomTuio11CursorTransform _cursorPrefab;
    [SerializeField] CustomTuio11ObjectTransform _objectPrefab;
    [SerializeField] CustomTuio11BlobTransform _blobPrefab;

    readonly Dictionary<uint, CustomTuio11Behaviour> _customTuioBehaviours = new();

    Tuio11Dispatcher _dispatcher;
    Tuio11Dispatcher Dispatcher => (Tuio11Dispatcher)_tuioSessionBehaviour.TuioDispatcher;
    
    public static event Action<Tuio11Cursor> onCursorAdd = delegate { };
    public static event Action<Tuio11Cursor> onCursorRemove = delegate { };
    public static event Action<Tuio11Cursor> onCursorUpdate = delegate { }; 
    public static event Action<Tuio11Object> onObjectAdd = delegate { };
    public static event Action<Tuio11Object> onObjectRemove = delegate { };
    public static event Action<Tuio11Object> onObjectUpdate = delegate { };

    [SerializeField] List<CategoryIcons> m_CategoryIcons = new List<CategoryIcons>();
    public List<ObjectVisualisationManager> objectVisManagers = new List<ObjectVisualisationManager>();

    private void Update()
    {
        foreach(var icon in m_CategoryIcons)
        {
            bool hasOneObject = false;
            foreach(var objViz in objectVisManagers)
                if(objViz.category == icon.category)
                {
                    hasOneObject = true;
                    break;
                }
            if(hasOneObject)
            {
                icon.Show();
            } else
            {
                icon.Hide();
            }
        }

        foreach(var viz in objectVisManagers)
        {
            viz.SetMode(objectVisManagers.Count > 1 ? ObjectVisualisationManager.ArrangementMode.List : ObjectVisualisationManager.ArrangementMode.Circle);
        }
    }

    void OnEnable()
    {
        try
        {
            Dispatcher.OnCursorAdd += AddTuioCursor;
            Dispatcher.OnCursorRemove += RemoveTuioCursor;
            Dispatcher.OnCursorUpdate += UpdateTuioCursor;

            Dispatcher.OnObjectAdd += AddTuioObject;
            Dispatcher.OnObjectRemove += RemoveTuioObject;
            Dispatcher.OnObjectUpdate += UpdateTuioObject;

            Dispatcher.OnBlobAdd += AddTuioBlob;
            Dispatcher.OnBlobRemove += RemoveTuioBlob;
        }
        catch (InvalidCastException exception)
        {
            Debug.LogError($"[Tuio Client] Check the TUIO-Version on the TuioSession object. {exception.Message}");
        }
    }

    void OnDisable()
    {
        try
        {
            Dispatcher.OnCursorAdd -= AddTuioCursor;
            Dispatcher.OnCursorRemove -= RemoveTuioCursor;
            Dispatcher.OnCursorUpdate -= UpdateTuioCursor;

            Dispatcher.OnObjectAdd -= AddTuioObject;
            Dispatcher.OnObjectRemove -= RemoveTuioObject;
            Dispatcher.OnObjectUpdate -= UpdateTuioObject;

            Dispatcher.OnBlobAdd -= AddTuioBlob;
            Dispatcher.OnBlobRemove -= RemoveTuioBlob;
        }
        catch (InvalidCastException exception)
        {
            Debug.LogError($"[Tuio Client] Check the TUIO-Version on the TuioSession object. {exception.Message}");
        }
    }

    void AddTuioCursor(object sender, Tuio11Cursor tuioCursor)
    {
        var tuio11CursorBehaviour = Instantiate(_cursorPrefab, transform);
        tuio11CursorBehaviour.Initialize(tuioCursor);
        _customTuioBehaviours.Add(tuioCursor.SessionId, tuio11CursorBehaviour);
        onCursorAdd.Invoke(tuioCursor);
    }

    void RemoveTuioCursor(object sender, Tuio11Cursor tuioCursor)
    {
        if (_customTuioBehaviours.Remove(tuioCursor.SessionId, out var cursorBehaviour))
        {
            cursorBehaviour.Destroy();
            onCursorRemove.Invoke(tuioCursor);
        }
    }
    
    void UpdateTuioCursor(object sender, Tuio11Cursor tuioCursor)
    {
        if (_customTuioBehaviours.TryGetValue(tuioCursor.SessionId, out var cursorBehaviour))
        {
            onCursorUpdate.Invoke(tuioCursor);
        }
    }

    void AddTuioObject(object sender, Tuio11Object tuioObject)
    {
        var objectBehaviour = Instantiate(_objectPrefab, transform);
        objectBehaviour.Initialize(tuioObject);
        var objVizManager = objectBehaviour.GetComponent<ObjectVisualisationManager>();
        if (objVizManager)
            objVizManager.canvasRectTransform = this.transform as RectTransform;
        _customTuioBehaviours.Add(tuioObject.SessionId, objectBehaviour);
        onObjectAdd.Invoke(tuioObject);
    }

    void RemoveTuioObject(object sender, Tuio11Object tuioObject)
    {
        if (_customTuioBehaviours.Remove(tuioObject.SessionId, out var objectBehaviour))
        {
            onObjectRemove.Invoke(tuioObject);
            //objectBehaviour.Destroy();
        }
    }
    
    void UpdateTuioObject(object sender, Tuio11Object tuioObject)
    {
        if (_customTuioBehaviours.TryGetValue(tuioObject.SessionId, out var objectBehaviour))
        {
            onObjectUpdate.Invoke(tuioObject);
        }
    }

    void AddTuioBlob(object sender, Tuio11Blob tuioBlob)
    {
        var blobBehaviour = Instantiate(_blobPrefab, transform);
        blobBehaviour.Initialize(tuioBlob);
        _customTuioBehaviours.Add(tuioBlob.SessionId, blobBehaviour);
    }

    void RemoveTuioBlob(object sender, Tuio11Blob tuioBlob)
    {
        if (_customTuioBehaviours.Remove(tuioBlob.SessionId, out var blobBehaviour))
        {
            blobBehaviour.Destroy();
        }
    }
}
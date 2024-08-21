using System;
using System.Collections.Generic;
using TuioNet.Common;
using TuioNet.Tuio11;
using TuioUnity.Common;
using TuioUnity.Tuio11;
using UnityEngine;

public class CustomTuio11Visualizer : MonoBehaviour
{
    [SerializeField] private TuioSessionBehaviour _tuioSessionBehaviour;
    [SerializeField] private CustomTuio11CursorTransform _cursorPrefab;
    [SerializeField] private CustomTuio11ObjectTransform _objectPrefab;
    [SerializeField] private CustomTuio11BlobTransform _blobPrefab;

    private readonly Dictionary<uint, CustomTuio11Behaviour> _customTuioBehaviours = new();

    private Tuio11Dispatcher _dispatcher;
    private Tuio11Dispatcher Dispatcher => (Tuio11Dispatcher)_tuioSessionBehaviour.TuioDispatcher;
    
    public static event Action onCursorAdd = delegate { };
    public static event Action onCursorRemove = delegate { };
    public static event Action<Tuio11Object> onObjectAdd = delegate { };
    public static event Action<Tuio11Object> onObjectRemove = delegate { };

    private void OnEnable()
    {
        try
        {
            Dispatcher.OnCursorAdd += AddTuioCursor;
            Dispatcher.OnCursorRemove += RemoveTuioCursor;

            Dispatcher.OnObjectAdd += AddTuioObject;
            Dispatcher.OnObjectRemove += RemoveTuioObject;

            Dispatcher.OnBlobAdd += AddTuioBlob;
            Dispatcher.OnBlobRemove += RemoveTuioBlob;
        }
        catch (InvalidCastException exception)
        {
            Debug.LogError($"[Tuio Client] Check the TUIO-Version on the TuioSession object. {exception.Message}");
        }
    }

    private void OnDisable()
    {
        try
        {
            Dispatcher.OnCursorAdd -= AddTuioCursor;
            Dispatcher.OnCursorRemove -= RemoveTuioCursor;

            Dispatcher.OnObjectAdd -= AddTuioObject;
            Dispatcher.OnObjectRemove -= RemoveTuioObject;

            Dispatcher.OnBlobAdd -= AddTuioBlob;
            Dispatcher.OnBlobRemove -= RemoveTuioBlob;
        }
        catch (InvalidCastException exception)
        {
            Debug.LogError($"[Tuio Client] Check the TUIO-Version on the TuioSession object. {exception.Message}");
        }
    }

    private void AddTuioCursor(object sender, Tuio11Cursor tuioCursor)
    {
        var tuio11CursorBehaviour = Instantiate(_cursorPrefab, transform);
        tuio11CursorBehaviour.Initialize(tuioCursor);
        _customTuioBehaviours.Add(tuioCursor.SessionId, tuio11CursorBehaviour);
        onCursorAdd.Invoke();
    }

    private void RemoveTuioCursor(object sender, Tuio11Cursor tuioCursor)
    {
        if (_customTuioBehaviours.Remove(tuioCursor.SessionId, out var cursorBehaviour))
        {
            cursorBehaviour.Destroy();
            onCursorRemove.Invoke();
        }
    }

    private void AddTuioObject(object sender, Tuio11Object tuioObject)
    {
        var objectBehaviour = Instantiate(_objectPrefab, transform);
        objectBehaviour.Initialize(tuioObject);
        _customTuioBehaviours.Add(tuioObject.SessionId, objectBehaviour);
        onObjectAdd.Invoke(tuioObject);
    }

    private void RemoveTuioObject(object sender, Tuio11Object tuioObject)
    {
        if (_customTuioBehaviours.Remove(tuioObject.SessionId, out var objectBehaviour))
        {
            onObjectRemove.Invoke(tuioObject);
            objectBehaviour.Destroy();
        }
    }

    private void AddTuioBlob(object sender, Tuio11Blob tuioBlob)
    {
        var blobBehaviour = Instantiate(_blobPrefab, transform);
        blobBehaviour.Initialize(tuioBlob);
        _customTuioBehaviours.Add(tuioBlob.SessionId, blobBehaviour);
    }

    private void RemoveTuioBlob(object sender, Tuio11Blob tuioBlob)
    {
        if (_customTuioBehaviours.Remove(tuioBlob.SessionId, out var blobBehaviour))
        {
            blobBehaviour.Destroy();
        }
    }
}
using TuioNet.Tuio11;
using UnityEngine;

public class CustomTuio11CursorTransform : CustomTuio11Behaviour
    {
        public Tuio11Cursor _tuioCursor;

        public override void Initialize(Tuio11Container container)
        {
            _tuioCursor = (Tuio11Cursor)container;
            base.Initialize(container);
            Debug.Log("TuioCursor initialized");
        }

        public override string DebugText()
        {
            return $"ID: {_tuioCursor.SessionId}";
        }
    }


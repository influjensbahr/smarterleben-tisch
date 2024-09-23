using TuioNet.Tuio11;
using TuioUnity.Utils;
using UnityEngine;

    public class CustomTuio11BlobTransform : CustomTuio11Behaviour
    {
        public Tuio11Blob _blob;

        public override void Initialize(Tuio11Container container)
        {
            _blob = (Tuio11Blob)container;
            base.Initialize(container);
            Debug.Log("TuioBlob initialized");
        }
        
        protected override void UpdateContainer()
        {
            base.UpdateContainer();
            UpdateRotation();
            UpdateSize();
        }


        private void UpdateRotation()
        {
            var angle = -Mathf.Rad2Deg * _blob.Angle;
            RectTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        private void UpdateSize()
        {
            RectTransform.sizeDelta = _blob.Size.ToUnity();
        }

        public override string DebugText()
        {
            return
                $"ID: {_blob.BlobId} \nAngle: {(Mathf.Rad2Deg * _blob.Angle):f2}\u00b0 \nPosition: {RectTransform.position} \nSize: {_blob.Size}";
        }
    }

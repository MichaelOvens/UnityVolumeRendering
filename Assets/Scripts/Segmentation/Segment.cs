using UnityVolumeRendering;

namespace UWA.VolumeViewer.Volumetrics.Segmentations
{
    [System.Serializable]
    public class Segment
    {
        public int labelValue;
        public VolumeDataset dataset;
    }
}
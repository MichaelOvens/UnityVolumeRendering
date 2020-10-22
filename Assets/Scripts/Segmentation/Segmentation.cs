using System.Collections.Generic;

namespace UnityVolumeRendering
{
    [System.Serializable]
    public class Segmentation
    {
        public Dictionary<int, VolumeDataset> segments;
    }
}
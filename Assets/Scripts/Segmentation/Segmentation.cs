using System.Collections.Generic;

namespace UnityVolumeRendering
{
    [System.Serializable]
    public class Segmentation
    {
        public Dictionary<int, Segment> segments;
    }
}
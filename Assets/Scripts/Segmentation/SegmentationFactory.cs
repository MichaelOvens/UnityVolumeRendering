using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Segments a base volume according to the mapping provided by the label volume.
    /// </summary>
    public class SegmentationFactory
    {
        private VolumeDataset baseVolume;
        private VolumeDataset labelVolume;

        public SegmentationFactory(VolumeDataset baseVolume, VolumeDataset labelVolume)
        {
            this.baseVolume = baseVolume;
            this.labelVolume = labelVolume;
        }

        public Segmentation SplitIntoSegments ()
        {
            if (baseVolume.data.Length != labelVolume.data.Length)
            {
                Debug.LogError(string.Format("Base dimensions: {0},{1},{2}", baseVolume.dimX.ToString(), baseVolume.dimY.ToString(), baseVolume.dimZ.ToString()));
                Debug.LogError(string.Format("Label dimensions: {0},{1},{2}", labelVolume.dimX.ToString(), labelVolume.dimY.ToString(), labelVolume.dimZ.ToString()));
                throw new DataMisalignedException("Base and label volumes must have the same dimensions!");
            }

            var labelToBaseMap = CreateLabelToBaseMapDictionary(baseVolume.data, labelVolume.data);
            var segments = ConvertDensityMapsToSegments(labelToBaseMap);

            var segmentation = new Segmentation()
            {
                segments = segments
            };

            return segmentation;
        }

        private Dictionary<int, int[]> CreateLabelToBaseMapDictionary(int[] baseValues, int[] labelValues)
        {
            var labelValueToSegmentMap = new Dictionary<int, int[]>();

            // Create a blank array for copying later
            var blankSegment = new int[labelValues.Length];
            for (int i = 0; i < labelValues.Length; i++)
                blankSegment[i] = 0;

            int baseValue; int labelValue;
            for (int i = 0; i < labelValues.Length; i++)
            {
                // Read the values of the cell
                baseValue = baseValues[i];
                labelValue = labelValues[i];

                // If there's no corresponding label-segment pair, create a new blank segment map
                if (!labelValueToSegmentMap.ContainsKey(labelValues[i]))
                {
                    int[] segmentMap = new int[labelValues.Length];
                    Array.Copy(blankSegment, 0, segmentMap, 0, labelValues.Length);
                    labelValueToSegmentMap.Add(labelValues[i], segmentMap);
                }

                // Overwrite the color in the cell of the corresponding segment
                labelValueToSegmentMap[labelValue][i] = baseValue;
            }

            return labelValueToSegmentMap;
        }

        /// <summary>
        /// Converts the raw sequential data into VolumeDatasets
        /// </summary>
        /// <param name="labelValueToSegmentMap"></param>
        /// <param name="discardBlackData"></param>
        /// <returns></returns>
        private Dictionary<int, Segment> ConvertDensityMapsToSegments (Dictionary<int, int[]> labelValueToSegmentMap)
        {
            Dictionary<int, Segment> segments = new Dictionary<int, Segment>();

            foreach (var pair in labelValueToSegmentMap)
            {
                VolumeDataset dataset = ScriptableObject.CreateInstance<VolumeDataset>();

                dataset.datasetName = "Untitled";
                dataset.data = pair.Value;

                dataset.dimX = baseVolume.dimX;
                dataset.dimY = baseVolume.dimY;
                dataset.dimZ = baseVolume.dimZ;

                dataset.scaleX = 1f; // Scale arbitrarily normalised around the x-axis 
                dataset.scaleY = (float)baseVolume.dimY / (float)baseVolume.dimX;
                dataset.scaleZ = (float)baseVolume.dimZ / (float)baseVolume.dimX;

                Segment segment = new Segment()
                {
                    labelValue = pair.Key,
                    dataset = dataset
                };

                segments.Add(pair.Key, segment);
            }

            return segments;
        }
    }
}
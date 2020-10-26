using System.IO;
using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{
    public class CreateSegmentationWindow : EditorWindow
    {
        private string basePath;
        private string labelPath;
        private bool discardBlack = true;

        [MenuItem("Volume Rendering/Segmentation/Create segmentation from image sequence")]
        public static void ShowWindow()
        {
            GetWindow(typeof(CreateSegmentationWindow));
        }

        private void OnGUI()
        {
            GetDirectoryControl("Select Base Volume Directory", ref basePath);
            GetDirectoryControl("Select Label Volume Directory", ref labelPath);

            bool readyToImport = Directory.Exists(basePath) && Directory.Exists(labelPath);

            using (new EditorGUI.DisabledScope(!readyToImport))
            {
                if (GUILayout.Button("Create Segmentation"))
                {
                    VolumeDataset baseVolume = new ImageSequenceImporter(basePath).Import();
                    VolumeDataset labelVolume = new ImageSequenceImporter(labelPath).Import();

                    SegmentationFactory factory = new SegmentationFactory(baseVolume, labelVolume);
                    Segmentation segmentation = factory.SplitIntoSegments();

                    GameObject segmentManager = new GameObject(baseVolume.datasetName + " Segment Manager");
                    foreach (var segment in segmentation.segments)
                    {
                        VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(segment.Value);
                        obj.transform.parent = segmentManager.transform;
                    }
                }
            }
        }

        private void GetDirectoryControl (string instructions, ref string path)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(instructions))
            {
                string directory = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
                if (Directory.Exists(directory))
                {
                    path = directory;
                }
            }

            path = EditorGUILayout.TextField(path);

            EditorGUILayout.EndHorizontal();
        }
    }
}
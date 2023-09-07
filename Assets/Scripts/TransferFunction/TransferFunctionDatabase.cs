using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace UnityVolumeRendering
{
    public class TransferFunctionDatabase
    {
        [System.Serializable]
        private struct TF1DSerialisationData
        {
            public int version;
            public List<TFColourControlPoint> colourPoints;
            public List<TFAlphaControlPoint> alphaPoints;
            public float minDataValue;
            public float maxDataValue;

            public const int VERSION_ID = 2;
        }

        [System.Serializable]
        private struct TF2DSerialisationData
        {
            public int version;
            public List<TransferFunction2D.TF2DBox> boxes;

            public const int VERSION_ID = 1;
        }

        public static TransferFunction CreateTransferFunction()
        {
            TransferFunction tf = ScriptableObject.CreateInstance<TransferFunction>();
            tf.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.11f, 0.14f, 0.13f, 1.0f)));
            tf.AddControlPoint(new TFColourControlPoint(0.2415f, new Color(0.469f, 0.354f, 0.223f, 1.0f)));
            tf.AddControlPoint(new TFColourControlPoint(0.3253f, new Color(1.0f, 1.0f, 1.0f, 1.0f)));

            tf.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf.AddControlPoint(new TFAlphaControlPoint(0.1787f, 0.0f));
            tf.AddControlPoint(new TFAlphaControlPoint(0.2f, 0.024f));
            tf.AddControlPoint(new TFAlphaControlPoint(0.28f, 0.03f));
            tf.AddControlPoint(new TFAlphaControlPoint(0.4f, 0.546f));
            tf.AddControlPoint(new TFAlphaControlPoint(0.547f, 0.5266f));

            tf.GenerateTexture();
            return tf;
        }

        public static TransferFunction2D CreateTransferFunction2D()
        {
            TransferFunction2D tf2D = ScriptableObject.CreateInstance<TransferFunction2D>();
            tf2D.AddBox(0.05f, 0.1f, 0.8f, 0.7f, Color.white, 0.4f);
            return tf2D;
        }

        public static TransferFunction LoadTransferFunction(string filepath)
        {
            if(!File.Exists(filepath))
            {
                Debug.LogError(string.Format("File does not exist: {0}", filepath));
                return null;
            }
            string jsonstring = File.ReadAllText(filepath);
            TF1DSerialisationData data = JsonUtility.FromJson<TF1DSerialisationData>(jsonstring);
            TransferFunction tf = ScriptableObject.CreateInstance<TransferFunction>();
            tf.colourControlPoints = data.colourPoints;
            tf.alphaControlPoints = data.alphaPoints;
            tf.minDataValue = data.minDataValue;
            tf.maxDataValue = data.maxDataValue;
            return tf;
        }

        public static TransferFunction2D LoadTransferFunction2D(string filepath)
        {
            if(!File.Exists(filepath))
            {
                Debug.LogError(string.Format("File does not exist: {0}", filepath));
                return null;
            }
            string jsonstring = File.ReadAllText(filepath);
            TF2DSerialisationData data = JsonUtility.FromJson<TF2DSerialisationData>(jsonstring);
            TransferFunction2D tf = ScriptableObject.CreateInstance<TransferFunction2D>();
            tf.boxes = data.boxes;
            return tf;
        }

        public static void SaveTransferFunction(TransferFunction tf, string filepath)
        {
            TF1DSerialisationData data = new TF1DSerialisationData();
            data.version = TF1DSerialisationData.VERSION_ID;
            data.colourPoints = new List<TFColourControlPoint>(tf.colourControlPoints);
            data.alphaPoints =　new List<TFAlphaControlPoint>(tf.alphaControlPoints);
            data.minDataValue = tf.minDataValue;
            data.maxDataValue = tf.maxDataValue;
            string jsonstring = JsonUtility.ToJson(data);
            File.WriteAllText(filepath, jsonstring);
        }

        public static void SaveTransferFunction2D(TransferFunction2D tf2d, string filepath)
        {
            TF2DSerialisationData data = new TF2DSerialisationData();
            data.version = TF2DSerialisationData.VERSION_ID;
            data.boxes = new List<TransferFunction2D.TF2DBox>(tf2d.boxes);
            string jsonstring = JsonUtility.ToJson(data);
            File.WriteAllText(filepath, jsonstring);
        }

        public static void ConvertTransferFunctionToScale(TransferFunction tf, float newMinValue, float newMaxValue)
        {
            if (tf.minDataValue == tf.maxDataValue)
            {
                Debug.Log("Transfer function does not have a defined data value scale.");
                return;
            }

            for (int i = 0; i < tf.alphaControlPoints.Count; i++)
            {
                TFAlphaControlPoint point = tf.alphaControlPoints[i];
                float t = Mathf.InverseLerp(tf.minDataValue, tf.maxDataValue, point.dataValue);
                point.dataValue = Mathf.Lerp(newMinValue, newMaxValue, t);
                tf.alphaControlPoints[i] = point;
            }

            for (int i = 0; i < tf.colourControlPoints.Count; i++)
            {
                TFColourControlPoint point = tf.colourControlPoints[i];
                float t = Mathf.InverseLerp(tf.minDataValue, tf.maxDataValue, point.dataValue);
                point.dataValue = Mathf.Lerp(newMinValue, newMaxValue, t);
                tf.colourControlPoints[i] = point;
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using SceneUnderstanding = Microsoft.MixedReality.SceneUnderstanding;

    /// <summary>
    /// Provides helper methods that allow one to save Scene Understanding data.
    /// </summary>
    public static class SceneUnderstandingSaveDataUtils
    {
        /// <summary>
        /// Get the current date and time, formatted a particular way.
        /// </summary>
        /// <returns>Current datetime string.</returns>
        private static string GetCurrentDateTimeString()
        {
            DateTime currDateTime = DateTime.Now;
            string currDateTimeString = string.Format("{0}-{1}-{2}_{3}-{4}-{5}", 
                                                       currDateTime.Date.Year, currDateTime.Date.Month, currDateTime.Date.Day,
                                                       currDateTime.TimeOfDay.Hours, currDateTime.TimeOfDay.Minutes, currDateTime.TimeOfDay.Seconds);

            return currDateTimeString;
        }

        /// <summary>
        /// Generates a file name.
        /// </summary>
        /// <returns>Generated file name.</returns>
        private static string GetDefaultFileName()
        {
            return string.Format("SU_{0}", GetCurrentDateTimeString());
        }
        
        /// <summary>
        /// Saves a serialized scene from Scene Understanding to disk.
        /// </summary>
        /// <param name="serializedScene">Serialized scene.</param>
        /// <param name="fileName">Name for the file that will be saved to disk.</param>
        /// <returns>Task.</returns>
        public static async Task SaveBytesToDiskAsync(byte[] serializedScene, string fileName = null)
        {
            if (serializedScene == null)
            {
                Logger.LogWarning("SceneUnderstandingSaveDataUtils.SaveBytesToDiskAsync: Nothing to save.");
                return;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = GetDefaultFileName() + ".bytes";
            }

#if WINDOWS_UWP
            var folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            await Windows.Storage.FileIO.WriteBytesAsync(file, serializedScene);
#else
            var folder = Path.GetTempPath();
            var file = Path.Combine(folder, fileName);
            File.WriteAllBytes(file, serializedScene);
            Logger.Log(string.Format("SceneUnderstandingSaveDataUtils.SaveBytesToDiskAsync: Bytes saved to: {0}", file));
#endif
        }

        /// <summary>
        /// Saves a serialized scene from Scene Understanding to disk as obj files. One obj file is saved per class, i.e. one file for walls, one file for ceilings, etc.
        /// </summary>
        /// <param name="serializedScene">Serialized scene.</param>
        /// <returns>Task.</returns>
        public static async Task SaveObjsToDiskAsync(byte[] serializedScene)
        {   
            if (serializedScene == null)
            {
                Logger.LogWarning("SceneUnderstandingSaveDataUtils.SaveObjsToDisk: Nothing to save.");
                return;
            }

            // Deserialize the scene.
            SceneUnderstanding.Scene scene = SceneUnderstanding.Scene.Deserialize(serializedScene);
            
            // List of all SceneObjectKind enum values.
            List<SceneUnderstanding.SceneObjectKind> sceneObjectKinds = Enum.GetValues(typeof(SceneUnderstanding.SceneObjectKind)).Cast<SceneUnderstanding.SceneObjectKind>().ToList();
        
            List<Task> tasks = new List<Task>();
            foreach (SceneUnderstanding.SceneObjectKind soKind in sceneObjectKinds)
            {
                tasks.Add(SaveSceneObjectsAsObjAsync(
                    scene.SceneObjects.Where<SceneUnderstanding.SceneObject>(so => so.Kind == soKind),
                    SceneUnderstandingUtils.GetColorForLabel(soKind) == null ? Color.black : SceneUnderstandingUtils.GetColorForLabel(soKind).Value,
                    string.Format("{0}_{1}", GetDefaultFileName(), soKind.ToString())));
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Saves a set of scene objects to one .obj file.
        /// </summary>
        /// <param name="sceneObjects">Set of scene objects to save.</param>
        /// <param name="color">Vertex color to use.</param>
        /// <param name="filename">Name for the file that will be saved to disk.</param>
        /// <returns>Task.</returns>
        private static async Task SaveSceneObjectsAsObjAsync(IEnumerable<SceneUnderstanding.SceneObject> sceneObjects, Color color, string filename)
        {
            if (sceneObjects == null)
            {
                Logger.LogWarning("SceneUnderstandingSaveDataUtils.SaveSceneObjectsAsObj: No scene objects to save.");
                return;
            }
            
            List<System.Numerics.Vector3> combinedMeshVertices = new List<System.Numerics.Vector3>();
            List<uint> combinedMeshIndices = new List<uint>();
            
            // Go through each scene object, retrieve its meshes and add them to the combined lists, defined above.
            foreach (SceneUnderstanding.SceneObject so in sceneObjects)
            {
                if (so == null)
                {
                    continue;
                }

                IEnumerable<SceneUnderstanding.SceneMesh> meshes = so.Meshes;
                if (meshes == null)
                {
                    continue;
                }
                
                foreach (SceneUnderstanding.SceneMesh mesh in meshes)
                {
                    // Get the mesh vertices.
                    var mvList = new System.Numerics.Vector3[mesh.VertexCount];
                    mesh.GetVertexPositions(mvList);

                    // Transform the vertices using the transformation matrix.
                    TransformUtils.TransformVertices(so.GetLocationAsMatrix(), mvList);
                    
                    // Store the current set of vertices in the combined list. As we add indices, we'll offset it by this value.
                    uint indexOffset = (uint)combinedMeshVertices.Count;
                    
                    // Add the new set of mesh vertices to the existing set.
                    combinedMeshVertices.AddRange(mvList);

                    // Get the mesh indices.
                    uint[] mi = new uint[mesh.TriangleIndexCount];
                    mesh.GetTriangleIndices(mi);

                    // Add the new set of mesh indices to the existing set.
                    for (int i = 0; i < mi.Length; ++i)
                    {
                        combinedMeshIndices.Add((uint)(mi[i] + indexOffset));
                    }
                }
            }

            // Write as string to file.
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < combinedMeshVertices.Count; ++i)
            {
                sb.Append(string.Format("v {0} {1} {2} {3} {4} {5}\n", combinedMeshVertices[i].X, combinedMeshVertices[i].Y, combinedMeshVertices[i].Z, color.r, color.g, color.b));
            }

            for (int i = 0; i < combinedMeshIndices.Count; i += 3)
            {
                // Indices start at index 1 (as opposed to 0) in objs.
                sb.Append(string.Format("f {0} {1} {2}\n", combinedMeshIndices[i] + 1, combinedMeshIndices[i + 1] + 1, combinedMeshIndices[i + 2] + 1));
            }

            await SaveStringToDiskAsync(sb.ToString(), string.Format("{0}.obj", filename));
        }

        /// <summary>
        /// Saves a string to a file on disk.
        /// </summary>
        /// <param name="str">String to save.</param>
        /// <param name="fileName">Name for the file that will be saved to disk.</param>
        /// <returns>Task.</returns>
        private static async Task SaveStringToDiskAsync(string str, string fileName = null)
        {
            if (string.IsNullOrEmpty(str))
            {
                Logger.LogWarning("SceneUnderstandingSaveDataUtils.SaveStringToDisk: Nothing to save.");
                return;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = GetDefaultFileName();
            }

#if WINDOWS_UWP
            var folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            await Windows.Storage.FileIO.AppendTextAsync(file, str);
#else
            var folder = Path.GetTempPath();
            var file = Path.Combine(folder, fileName);
            File.WriteAllText(file, str);
            Logger.Log(string.Format("SceneUnderstandingSaveDataUtils.SaveStringToDiskAsync: String saved to: {0}", file));
#endif
        }
    }
}
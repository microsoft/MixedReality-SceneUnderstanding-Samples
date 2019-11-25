// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System.Collections.Generic;
    using UnityEngine;
    using SceneUnderstanding = Microsoft.MixedReality.SceneUnderstanding;

    /// <summary>
    /// Helper methods for Unity and Scene Understanding related operations.
    /// </summary>
    public class SceneUnderstandingUtils : MonoBehaviour
    {
        /// <summary>
        /// Creates a game object with the specified name and parents it to the parent Transform.
        /// </summary>
        /// <param name="name">Name for the game object.</param>
        /// <param name="parent">Parent transform.</param>
        /// <returns>A newly created game object.</returns>
        public GameObject CreateGameObject(string name, Transform parent)
        {
            GameObject go = new GameObject();
            go.name = name;
            go.transform.parent = parent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            return go;
        }

        /// <summary>
        /// Creates a game object with the specified name, parents it, adds rendering components and assigns the mesh, material and color.
        /// </summary>
        /// <param name="name">Name for the game object.</param>
        /// <param name="parent">Parent transform.</param>
        /// <param name="mesh">Mesh for the game object.</param>
        /// <param name="material">Material to use for the game object.</param>
        /// <param name="color">Color to use on the material.</param>
        /// <returns>A newly created game object.</returns>
        public GameObject CreateGameObjectWithMeshComponents(string name, Transform parent, Mesh mesh, Material material, Color? color)
        {
            if (mesh == null || material == null)
            {
                Logger.LogWarning("SceneUnderstandingUtils.CreateGameObjectWithMeshAndLabel: One or more arguments are null.");
                return null;
            }

            GameObject go = CreateGameObject(name, parent);
            AddMeshRenderingComponents(go, mesh, material, color);
            return go;
        }

        /// <summary>
        /// Adds mesh rendering related components to the game object.
        /// </summary>
        /// <param name="go">Game object on which to add the mesh rendering components.</param>
        /// <param name="mesh">Mesh to assign.</param>
        /// <param name="material">Material to use for the game object.</param>
        /// <param name="color">Color to use on the material.</param>
        public void AddMeshRenderingComponents(GameObject go, Mesh mesh, Material material, Color? color)
        {
            if (go == null || mesh == null || material == null)
            {
                Logger.LogWarning("SceneUnderstandingUtils.AddMeshRenderingComponents: One or more arguments are null.");
                return;
            }

            MeshFilter filter = go.GetComponent<MeshFilter>();
            if (filter == null)
            {
                filter = go.AddComponent<MeshFilter>();
            }
            filter.sharedMesh = mesh;

            Material clonedMaterial = Instantiate(material);
            if (color != null)
            {
                clonedMaterial.color = color.Value;
            }

            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = go.AddComponent<MeshRenderer>();
            }
            renderer.material = clonedMaterial;
        }

        /// <summary>
        /// Adds a game object with a text mesh under the passed in game object.
        /// </summary>
        /// <param name="go">Game object under which to add the Text game object.</param>
        /// <param name="label">Label to set on the text mesh.</param>
        /// <param name="labelFont">Font for the text labels.</param>
        public void AddTextLabel(GameObject go, string label, Font labelFont)
        {
            if (go == null || label == null)
            {
                Logger.LogWarning("SceneUnderstandingUtils.AddTextLabel: One or more arguments are null.");
                return;
            }

            GameObject textGO = CreateGameObject("Label", go.transform);
            textGO.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);

            // Add a text mesh component.
            TextMesh textMesh = textGO.AddComponent<TextMesh>() as TextMesh;
            textMesh.characterSize = 0.4f;
            textMesh.text = label;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.font = labelFont;

            // Add a mesh renderer, if one doesn't exist and configure it.
            MeshRenderer renderer = textGO.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = textGO.AddComponent<MeshRenderer>();
            }
            renderer.material = labelFont.material;
            renderer.material.color = new Color(0, 1.0f, 1.0f);
            // Render on top of everything else.
            renderer.material.renderQueue = 3000;

            // Add the billboard script.
            textGO.AddComponent<Billboard>();
        }

        /// <summary>
        /// Destroys all children of the passed in parent.
        /// </summary>
        /// <param name="parent">Parent whose children will be destroyed.</param>
        public void DestroyAllGameObjectsUnderParent(Transform parent)
        {
            if (parent == null)
            {
                Logger.LogWarning("SceneUnderstandingUtils.DestroyAllGameObjectsUnderParent: Parent is null.");
                return;
            }

            //Find all child objects and store them.
            List<GameObject> allChildren = new List<GameObject>(parent.transform.childCount);
            foreach (Transform child in parent.transform)
            {
                allChildren.Add(child.gameObject);
            }
            Logger.Log("SceneUnderstandingUtils.DestroyAllGameObjectsUnderParent: Found " + allChildren.Count + " game objects to destroy.");

            // Destroy runs at the end of the render loop. It is not immediate.
            foreach (GameObject child in allChildren)
            {
                Destroy(child);
            }
        }

        /// <summary>
        /// Provides a Unity color for a scene object label.
        /// </summary>
        /// <param name="label">Label for the Scene Understanding scene object.</param>
        /// <returns>Unity color.</returns>
        public static Color? GetColorForLabel(SceneUnderstanding.SceneObjectKind label)
        {
            Color? color = null;

            switch(label)
            {
                case SceneUnderstanding.SceneObjectKind.Background:
                    color = new Color(0.953f, 0.475f, 0.875f, 1.0f);    // Pink'ish
                    break;
                case SceneUnderstanding.SceneObjectKind.Wall:
                    color = new Color(0.953f, 0.494f, 0.475f, 1.0f);    // Orange'ish
                    break;
                case SceneUnderstanding.SceneObjectKind.Floor:
                    color = new Color(0.733f, 0.953f, 0.475f, 1.0f);    // Green'ish
                    break;
                case SceneUnderstanding.SceneObjectKind.Ceiling:
                    color = new Color(0.475f, 0.596f, 0.953f, 1.0f);    // Purple'ish
                    break;
                case SceneUnderstanding.SceneObjectKind.Platform:
                    color = new Color(0.204f, 0.792f, 0.714f, 1.0f);    // Blue'ish
                    break;
                case SceneUnderstanding.SceneObjectKind.Unknown:
                    color = new Color(1.0f, 1.0f, 1.0f, 1.0f);          // White
                    break;       
                case SceneUnderstanding.SceneObjectKind.CompletelyInferred:
                    color = new Color(0.5f, 0.5f, 0.5f, 1.0f);          // Gray
                    break;
                case SceneUnderstanding.SceneObjectKind.World:
                    color = Color.blue;
                    break;
            }

            return color;
        }

        /// <summary>
        /// Generates and returns a Unity mesh for a Scene Understanding quad.
        /// </summary>
        /// <param name="quad">Scene Understanding quad.</param>
        /// <returns>Unity mesh.</returns>
        public Mesh GenerateUnityMeshForSceneObjectQuad(SceneUnderstanding.SceneQuad quad)
        {
            if (quad == null)
            {
                Logger.LogWarning("SceneUnderstandingUtils.GenerateUnityMeshForSceneObjectQuad: Quad is null.");
                return null;
            }
        
            float widthInMeters = quad.Extents.X;
            float heightInMeters = quad.Extents.Y;

            // Bounds of the quad.
            List<Vector3> vertices = new List<Vector3>()
            {
                new Vector3(-widthInMeters / 2, -heightInMeters / 2, 0),
                new Vector3( widthInMeters / 2, -heightInMeters / 2, 0),
                new Vector3(-widthInMeters / 2,  heightInMeters / 2, 0),
                new Vector3( widthInMeters / 2,  heightInMeters / 2, 0)
            };

            List<int> triangles = new List<int>()
            {
                1, 3, 0,
                3, 2, 0
            };

            List<Vector2> uvs = new List<Vector2>()
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            Mesh unityMesh = new Mesh();
            unityMesh.SetVertices(vertices);
            unityMesh.SetIndices(triangles.ToArray(), MeshTopology.Triangles, 0);
            unityMesh.SetUVs(0, uvs);
        
            return unityMesh;
        }
        
        /// <summary>
        /// Generates and returns a Unity mesh for a Scene Understanding mesh.
        /// </summary>
        /// <param name="mesh">Scene Understanding mesh.</param>
        /// <returns>Unity mesh.</returns>
        public Mesh GenerateUnityMeshForSceneObjectMesh(SceneUnderstanding.SceneMesh mesh)
        {
            if (mesh == null)
            {
                Logger.LogWarning("SceneUnderstandingUtils.GenerateUnityMeshForSceneObjectMesh: Mesh is null.");
                return null;
            }

            return GenerateUnityMeshForSceneObjectMeshes(new SceneUnderstanding.SceneMesh[] { mesh });
        }

        /// <summary>
        /// Generates and returns a combined Unity mesh for a list of Scene Understanding meshes.
        /// </summary>
        /// <param name="meshes">Set of Scene Understanding meshes.</param>
        /// <returns>Combined unity mesh.</returns>
        public Mesh GenerateUnityMeshForSceneObjectMeshes(IEnumerable<SceneUnderstanding.SceneMesh> meshes)
        {
            if (meshes == null)
            {
                Logger.LogWarning("SceneUnderstandingUtils.GenerateUnityMeshForSceneObjectMeshes: Meshes is null.");
                return null;
            }

            List<int> combinedMeshIndices = new List<int>();
            List<Vector3> combinedMeshVertices = new List<Vector3>();

            foreach (SceneUnderstanding.SceneMesh suMesh in meshes)
            {
                if (suMesh == null)
                {
                    Logger.LogWarning("SceneUnderstandingUtils.GenerateUnityMeshForSceneObjectMeshes: Mesh is null.");
                    continue;
                }

                var meshIndices = new uint[suMesh.TriangleIndexCount];
                suMesh.GetTriangleIndices(meshIndices);

                var meshVertices = new System.Numerics.Vector3[suMesh.VertexCount];
                suMesh.GetVertexPositions(meshVertices);
            
                uint indexOffset = (uint)combinedMeshVertices.Count;

                for (int j = 0; j < meshVertices.Length; j++)
                {
                    combinedMeshVertices.Add(new Vector3(meshVertices[j].X, meshVertices[j].Y, -meshVertices[j].Z));
                }
            
                for (int j = 0; j < meshIndices.Length; ++j)
                {
                    combinedMeshIndices.Add((int)(meshIndices[j] + indexOffset));
                }
            }

            Mesh unityMesh = new Mesh();
        
            // Unity has a limit of 65,535 vertices in a mesh.
            // This limit exists because by default Unity uses 16-bit index buffers.
            // Starting with 2018.1, Unity allows one to use 32-bit index buffers.
            if (combinedMeshVertices.Count > 65535)
            {
                Logger.Log("SceneUnderstandingUtils.GenerateUnityMeshForSceneObjectMeshes: CombinedMeshVertices count is " + combinedMeshVertices.Count + ". Will be using a 32-bit index buffer.");
                unityMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            unityMesh.SetVertices(combinedMeshVertices);
            unityMesh.SetIndices(combinedMeshIndices.ToArray(), MeshTopology.Triangles, 0);
            unityMesh.RecalculateNormals();

            return unityMesh;
        }

        /// <summary>
        /// Applies the quad region mask on the passed in game object.
        /// </summary>
        /// <param name="quad">Scene Understanding quad.</param>
        /// <param name="go">Game object associated with the quad.</param>
        /// <param name="color">Color to use for the valid regions.</param>
        public void ApplyQuadRegionMask(SceneUnderstanding.SceneQuad quad, GameObject go, Color? color)
        {
            if (quad == null || go == null)
            {
                Logger.LogWarning("SceneUnderstandingUtils.ApplyQuadRegionMask: One or more arguments are null.");
                return;
            }

            // If no color has been provided, paint it red.
            color = color == null ? Color.red : color.Value;

            // Resolution of the mask.
            ushort width = 256;
            ushort height = 256;

            byte[] mask = new byte[width * height];
            quad.GetSurfaceMask(width, height, mask);

            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            if (meshRenderer == null || meshRenderer.material == null || meshRenderer.material.HasProperty("_MainTex") == false)
            {
                Logger.LogWarning("SceneUnderstandingUtils.ApplyQuadRegionMask: Mesh renderer component is null or does not have a valid material.");
                return;
            }

            // Create a new texture.
            Texture2D texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            // Transfer the invalidation mask onto the texture.
            Color[] pixels = texture.GetPixels();
            for (int i = 0; i < pixels.Length; ++i)
            {
                var value = mask[i];

                if (value == (byte)SceneUnderstanding.SceneRegionSurfaceKind.NotSurface)
                {
                    pixels[i] = Color.clear;
                }
                else
                {
                    pixels[i] = color.Value;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(true);

            // Set the texture on the material.
            meshRenderer.material.mainTexture = texture;
        }

        /// <summary>
        /// Orients the root game object, such that the Scene Understanding floor lies on the Unity world's X-Z plane.
        /// </summary>
        /// <param name="sceneRoot">Root game object.</param>
        /// <param name="scene">Scene Understanding scene.</param>
        public void OrientSceneRootForPC(GameObject sceneRoot, SceneUnderstanding.Scene scene)
        {
            if (scene == null)
            {
                Logger.LogWarning("SceneUnderstandingUtils.OrientSceneRootForPC: Scene is null.");
                return;
            }

            IEnumerable<SceneUnderstanding.SceneObject> sceneObjects = scene.SceneObjects;

            float areaForlargestFloorSoFar = 0;
            SceneUnderstanding.SceneObject floorSceneObject = null;
            SceneUnderstanding.SceneQuad floorQuad = null;
    
            // Find the largest floor quad.
            foreach(SceneUnderstanding.SceneObject so in sceneObjects)
            {
                if (so.Kind == SceneUnderstanding.SceneObjectKind.Floor)
                {
                    IEnumerable<SceneUnderstanding.SceneQuad> quads = so.Quads;
                
                    if (quads != null)
                    {
                        foreach (SceneUnderstanding.SceneQuad quad in quads)
                        {
                            float quadArea = quad.Extents.X * quad.Extents.Y;
                    
                            if (quadArea > areaForlargestFloorSoFar)
                            {
                                areaForlargestFloorSoFar = quadArea;
                                floorSceneObject = so;
                                floorQuad = quad;
                            }
                        }
                    }
                }
            }

            if (floorQuad != null)
            {
                // Compute the floor quad's normal.
                float widthInMeters = floorQuad.Extents.X;
                float heightInMeters = floorQuad.Extents.Y;

                System.Numerics.Vector3 point1 = new System.Numerics.Vector3(-widthInMeters / 2, -heightInMeters / 2, 0);
                System.Numerics.Vector3 point2 = new System.Numerics.Vector3( widthInMeters / 2, -heightInMeters / 2, 0);
                System.Numerics.Vector3 point3 = new System.Numerics.Vector3(-widthInMeters / 2,  heightInMeters / 2, 0);

                System.Numerics.Matrix4x4 floorTransform = floorSceneObject.GetLocationAsMatrix();
                floorTransform = TransformUtils.ConvertRightHandedMatrix4x4ToLeftHanded(floorTransform);
            
                System.Numerics.Vector3 tPoint1 = System.Numerics.Vector3.Transform(point1, floorTransform);
                System.Numerics.Vector3 tPoint2 = System.Numerics.Vector3.Transform(point2, floorTransform);
                System.Numerics.Vector3 tPoint3 = System.Numerics.Vector3.Transform(point3, floorTransform);
           
                System.Numerics.Vector3 p21 = tPoint2 - tPoint1;
                System.Numerics.Vector3 p31 = tPoint3 - tPoint1;

                System.Numerics.Vector3 floorNormal = System.Numerics.Vector3.Cross(p31, p21);
        
                // Numerics to Unity conversion.
                Vector3 floorNormalUnity = new Vector3(floorNormal.X, floorNormal.Y, floorNormal.Z);

                // Get the rotation between the floor normal and Unity world's up vector.
                Quaternion rotation = Quaternion.FromToRotation(floorNormalUnity, Vector3.up);

                // Apply the rotation to the root, so that the floor is on the Camera's x-z plane.
                sceneRoot.transform.rotation = rotation;
            }
        }
    }
}

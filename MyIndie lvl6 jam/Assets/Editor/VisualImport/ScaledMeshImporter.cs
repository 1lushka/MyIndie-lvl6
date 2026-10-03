using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

// Import-time variants for meshes that need different proportions or mirrored geometry.
// Scene transforms stay at one; normals and tangent handedness follow the baked geometry.
[ScriptedImporter(1, "scaledmesh")]
public sealed class ScaledMeshImporter : ScriptedImporter
{
    [Serializable]
    public sealed class Definition
    {
        public string sourceGuid;
        public string meshName;
        public string builtinMesh;
        public Matrix4x4 transform = Matrix4x4.identity;
    }

    public override void OnImportAsset(AssetImportContext context)
    {
        var definition = JsonUtility.FromJson<Definition>(File.ReadAllText(context.assetPath));
        Mesh source;
        if (!string.IsNullOrEmpty(definition.builtinMesh))
        {
            source = Resources.GetBuiltinResource<Mesh>(definition.builtinMesh + ".fbx");
        }
        else
        {
            string path = AssetDatabase.GUIDToAssetPath(definition.sourceGuid);
            if (string.IsNullOrEmpty(path))
                throw new InvalidDataException("The source model GUID does not resolve: " + definition.sourceGuid);
            context.DependsOnArtifact(path);
            source = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>()
                .Single(mesh => mesh.name == definition.meshName);
        }
        if (!source) throw new InvalidDataException("Source mesh is missing: " + definition.meshName);
        if (source.bindposes.Length != 0 || source.blendShapeCount != 0)
            throw new InvalidDataException("Scaled mesh variants support static meshes only.");
        Matrix4x4 matrix = definition.transform;
        if (Mathf.Abs(matrix.determinant) < 0.000000001f)
            throw new InvalidDataException("Import transform must be invertible.");

        var mesh = Instantiate(source);
        mesh.name = Path.GetFileNameWithoutExtension(context.assetPath);
        mesh.vertices = source.vertices.Select(matrix.MultiplyPoint3x4).ToArray();
        Matrix4x4 normalMatrix = matrix.inverse.transpose;
        Vector3[] normals = source.normals.Select(n => normalMatrix.MultiplyVector(n).normalized).ToArray();
        mesh.normals = normals;
        float handedness = Mathf.Sign(matrix.determinant);
        var tangents = source.tangents;
        for (int i = 0; i < tangents.Length; i++)
        {
            Vector3 tangent = matrix.MultiplyVector(tangents[i]);
            if (normals.Length == tangents.Length) tangent -= normals[i] * Vector3.Dot(normals[i], tangent);
            tangent.Normalize();
            tangents[i] = new Vector4(tangent.x, tangent.y, tangent.z, tangents[i].w * handedness);
        }
        mesh.tangents = tangents;
        if (handedness < 0)
        {
            for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                if (mesh.GetTopology(submesh) != UnityEngine.MeshTopology.Triangles) continue;
                int[] triangles = mesh.GetIndices(submesh);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int first = triangles[i]; triangles[i] = triangles[i + 1]; triangles[i + 1] = first;
                }
                mesh.SetIndices(triangles, MeshTopology.Triangles, submesh);
            }
        }
        mesh.RecalculateBounds();
        context.AddObjectToAsset("mesh", mesh);
        context.SetMainObject(mesh);
    }
}

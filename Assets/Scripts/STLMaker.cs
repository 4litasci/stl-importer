using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class STLMaker : MonoBehaviour
{
    public Text logText;
    private List<Facet> facets;

    private Stopwatch stopwatch = new Stopwatch();
    
    private void OnGUI()
    {
        
        if (GUI.Button(new Rect(100,100,100,100),"create ast ascii"))
        {
            StartStopwatch();
            var filePath = Path.Combine(Application.dataPath, "stls", "russianascii.stl");
            var readFromFile = File.ReadAllText(filePath);
            StopStopwatchWithMessage(filePath.Substring(filePath.Length-15,15)+" read");
            CreateMeshFromAscii(readFromFile);
        }
        
        if (GUI.Button(new Rect(0,100,100,100),"create ast ascii"))
        {
            StartStopwatch();
            var filePath = Path.Combine(Application.dataPath, "stls", "russianbinary.stl");
            var readFromFile = File.ReadAllBytes(filePath);
            StopStopwatchWithMessage(filePath.Substring(filePath.Length-15,15)+" read");
            CreateMeshFromBinary(readFromFile);
        }
        
        if (GUI.Button(new Rect(0,200,100,100),"create1"))
        {
            StartStopwatch();
            var filePath = Path.Combine(Application.dataPath, "stls", "1.stl");
            var readFromFile = File.ReadAllBytes(filePath);
            StopStopwatchWithMessage(filePath.Substring(filePath.Length-15,15)+" read");
            CreateMeshFromBinary(readFromFile);
        }
          
        if (GUI.Button(new Rect(0,300,100,100),"create2"))
        {
            StartStopwatch();
            var filePath = Path.Combine(Application.dataPath, "stls", "2.stl");
            var readFromFile = File.ReadAllBytes(filePath);
            StopStopwatchWithMessage(filePath.Substring(filePath.Length-15,15)+" read");
            CreateMeshFromBinary(readFromFile);
        }
        if (GUI.Button(new Rect(0,400,100,100),"create3"))
        {
            StartStopwatch();
            var filePath = Path.Combine(Application.dataPath, "stls", "3.stl");
            var readFromFile = File.ReadAllBytes(filePath);
            StopStopwatchWithMessage(filePath.Substring(filePath.Length-15,15)+" read");
            CreateMeshFromBinary(readFromFile);
        }
        
    }

    public void CreateMesh()
    {
        StartStopwatch();
        var meshFilter = GetComponent<MeshFilter>();
        var clonedMesh = new Mesh(); 

        clonedMesh.name = "newMesh";
        
        List<Vector3> normals = new List<Vector3>();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        
        for (int i = 0; i < facets.Count; i++)
        {
            normals.Add(facets[i].normal);
            normals.Add(facets[i].normal);
            normals.Add(facets[i].normal);
            vertices.Add(facets[i].v1);
            vertices.Add(facets[i].v2);
            vertices.Add(facets[i].v3);
            triangles.Add(i*3+0);
            triangles.Add(i*3+1);
            triangles.Add(i*3+2);
        }

        clonedMesh.indexFormat = IndexFormat.UInt32; // For 65k+ Meshes
        clonedMesh.vertices = vertices.ToArray();
        clonedMesh.normals = normals.ToArray();
        clonedMesh.triangles = triangles.ToArray();
        meshFilter.mesh = clonedMesh;  
        
        StopStopwatchWithMessage("created mesh");
    }

    public void CreateMeshFromAscii(string stlData)
    {
        StartStopwatch();
        facets = new List<Facet>();
        
        var facetSplits = stlData.Split(new []{"facet normal"},StringSplitOptions.None);
        
        foreach (var split in facetSplits)
        {
            var facetValues = split.Replace("outer loop", "")
                .Replace("vertex", "")
                .Replace("endloop", "")
                .Replace("endfacet", "")
                .Replace("\n", "").Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            if (facetValues.Length == 12)
            {
                facets.Add(new Facet()
                {
                    normal = new Vector3(Convert.ToSingle(facetValues[0]),Convert.ToSingle(facetValues[1]),Convert.ToSingle(facetValues[2])),
                    v1 = new Vector3(Convert.ToSingle(facetValues[3]),Convert.ToSingle(facetValues[4]),Convert.ToSingle(facetValues[5])),
                    v2 = new Vector3(Convert.ToSingle(facetValues[6]),Convert.ToSingle(facetValues[7]),Convert.ToSingle(facetValues[8])),
                    v3 = new Vector3(Convert.ToSingle(facetValues[9]),Convert.ToSingle(facetValues[10]),Convert.ToSingle(facetValues[11])),
                });
            }
        }

        StopStopwatchWithMessage("ascii prepared");
        CreateMesh();
    }

    public void CreateMeshFromBinary(byte[] stlData)
    {
        StartStopwatch();
        facets = new List<Facet>();
        Stream s = new MemoryStream(stlData);
        BinaryReader br = new BinaryReader(s);
        
        var header = br.ReadBytes(80);
        
        var triCount = br.ReadUInt32();

        for (int i = 0; i < triCount; i++)
        {
            var normalx = br.ReadSingle();
            var normaly = br.ReadSingle();
            var normalz = br.ReadSingle();
            var v1x = br.ReadSingle();
            var v1y = br.ReadSingle();
            var v1z = br.ReadSingle();
            var v2x = br.ReadSingle();
            var v2y = br.ReadSingle();
            var v2z = br.ReadSingle();
            var v3x = br.ReadSingle();
            var v3y = br.ReadSingle();
            var v3z = br.ReadSingle();
            var kekd = br.ReadUInt16();
            facets.Add(new Facet()
            {
                normal = new Vector3(normalx,normaly,normalz),
                v1 = new Vector3(v1x,v1y,v1z),
                v2 = new Vector3(v2x,v2y,v2z),
                v3 = new Vector3(v3x,v3y,v3z),
            });
        }
        
        br.Close();
        br.Dispose();
        s.Close();
        s.Dispose();
        
        StopStopwatchWithMessage("binary prepared");
        
        CreateMesh();
    }

    void StartStopwatch()
    {
        stopwatch.Start();
    }

    private int count = 0;
    void StopStopwatchWithMessage(string task)
    {
        stopwatch.Stop();
        var log = $"{++count} - {task} finished in {stopwatch.ElapsedMilliseconds} ms";
        logText.text =(count%3==0?"\n":"")+ log + "\n" + logText.text;
        print(log);
        stopwatch.Reset();
    }
}

public class Facet
{
    public Vector3 normal { get; set; }
    public Vector3 v1 { get; set; }
    public Vector3 v2 { get; set; }
    public Vector3 v3 { get; set; }
}
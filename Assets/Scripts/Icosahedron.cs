﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Icosahedron : MonoBehaviour {
  
  private List<Vector3> vertices;
  private List<int> triangles;
  private Dictionary<long, int> midpointCache;
  private Mesh mesh;

  public int scale = 1;
  [Range(0, 4)]
  public int subdivisions = 0;

  public bool autoUpdate;

  private void Awake() {
    Generate();
  }

  public void Generate() {

    vertices = new List<Vector3>();
    triangles = new List<int>();
    midpointCache = new Dictionary<long, int>();
    mesh = new Mesh();
    mesh.name = "Procedural Icosahedron";
    this.GetComponent<MeshFilter>().mesh = mesh;

    float t = 1 + Mathf.Sqrt(5) / 2;
    CreateVertices(t);
    CreateTriangles();
  }

  // create the 12 vertices
  private void CreateVertices(float t) {

    vertices.Add(new Vector3(-1,  t, 0) * scale);
    vertices.Add(new Vector3( 1,  t, 0) * scale);
    vertices.Add(new Vector3(-1, -t, 0) * scale);
    vertices.Add(new Vector3( 1, -t, 0) * scale);

    vertices.Add(new Vector3(0, -1, -t) * scale);
    vertices.Add(new Vector3(0,  1, -t) * scale);
    vertices.Add(new Vector3(0, -1,  t) * scale);
    vertices.Add(new Vector3(0,  1,  t) * scale);

    vertices.Add(new Vector3( t, 0,  1) * scale);
    vertices.Add(new Vector3( t, 0, -1) * scale);
    vertices.Add(new Vector3(-t, 0,  1) * scale);
    vertices.Add(new Vector3(-t, 0, -1) * scale);

    mesh.vertices = vertices.ToArray();
  }

  private void CreateTriangles() {

    // order vertices clockwise so that the face faces the right direction

    // create the 20 faces
    // five faces around point 0
    AddTriangle(0, 5, 11);
    AddTriangle(0, 1, 5);
    AddTriangle(0, 7, 1);
    AddTriangle(0, 10, 7);
    AddTriangle(0, 11, 10);

    // five adjacent faces
    AddTriangle(1, 9, 5);
    AddTriangle(5, 4, 11);
    AddTriangle(11, 2, 10);
    AddTriangle(10, 6, 7);
    AddTriangle(7, 8, 1);

    // five face around point 3 (polar opposite of point 0)
    AddTriangle(3, 4, 9);
    AddTriangle(3, 2, 4);
    AddTriangle(3, 6, 2);
    AddTriangle(3, 8, 6);
    AddTriangle(3, 9, 8);

    // five adjacent faces
    AddTriangle(4, 5, 9);
    AddTriangle(2, 11, 4);
    AddTriangle(6, 10, 2);
    AddTriangle(8, 7, 6);
    AddTriangle(9, 1, 8);

    // subdivisions
    List<int> trianglesSubdivisions = new List<int>();

    // refine triangles
    for(int i = 0; i < subdivisions; i++) {
      for(int j = 0; j < triangles.Count; j += 3) {
        // find midpoints for each triangle
        int mp1 = GetMidpoint(triangles[j], triangles[j + 1]);
        int mp2 = GetMidpoint(triangles[j + 1], triangles[j + 2]);
        int mp3 = GetMidpoint(triangles[j], triangles[j + 2]);

        // first subdivision
        trianglesSubdivisions.Add(triangles[j]);
        trianglesSubdivisions.Add(mp1);
        trianglesSubdivisions.Add(mp3);
        
        // second subdivision
        trianglesSubdivisions.Add(mp1);
        trianglesSubdivisions.Add(triangles[j + 1]);
        trianglesSubdivisions.Add(mp2);

        // third subdivision
        trianglesSubdivisions.Add(mp3);
        trianglesSubdivisions.Add(mp2);
        trianglesSubdivisions.Add(triangles[j + 2]);

        // middle subdivision
        trianglesSubdivisions.Add(mp1);
        trianglesSubdivisions.Add(mp2);
        trianglesSubdivisions.Add(mp3);
      }

      // update triangles List
      triangles.AddRange(trianglesSubdivisions);
    }

    // update mesh
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
  }

  // creates a single triangle face
  private void AddTriangle(int v1, int v2, int v3) {

    triangles.Add(v1);
    triangles.Add(v2);
    triangles.Add(v3);
  }

  // gets the midpoint between two points of a triangle's edge
  private int GetMidpoint(int v1, int v2) {

    long smallerIndex;
    long largerIndex;
    if(v1 >= v2) {
      smallerIndex = v2;
      largerIndex = v1;
    } else {
      smallerIndex = v1;
      largerIndex = v2;
    }

    // the key is unique to any two pair of points
    long key = (smallerIndex << 32) + largerIndex;

    int ret;
    if(midpointCache.TryGetValue(key, out ret)) {
      return ret; 
    }

    /* -- continue past this point if the key does not exist -- */

    // key does not exist, calculate midpoint
    // first point
    Vector3 p1 = vertices[v1];
    // second point
    Vector3 p2 = vertices[v2];
    // midpoint between first and second points
    Vector3 mp = new Vector3((p1.x + p2.x) / 2f,
                             (p1.y + p2.y) / 2f,
                             (p1.z + p2.z) / 2f);
    vertices.Add(mp); 

    // add key and midpoint vertex index (value) to cache
    midpointCache.Add(key, vertices.Count - 1);
    return vertices.Count - 1;
  }
  
  // draw gizmo at each vertex
  private void OnDrawGizmos() {

    if(vertices != null) {
      Gizmos.color = Color.black;
      for (int i = 0; i < vertices.Count; i++) {
        Gizmos.DrawSphere(vertices[i], 0.1f);
      }
    }
  }
}
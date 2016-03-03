using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Test : MonoBehaviour {

    Vector3 start;
    Vector3 end;

    List<Vector3> leafNodes;

    Vector3 rnd;
    Vector3 leafNode;
    Vector3 test;
    Vector3 originDir;
    Vector3 offsetPoint, offsetLeaf;

    float distance = 2f;
    float leafSize = 2f;

	// Use this for initialization
	void Start () {
        leafNodes = new List<Vector3>();
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        
        transform.Translate(Vector3.forward * distance);
        start = transform.position;

        transform.position = startPos;
        transform.rotation = startRot;

        transform.Rotate(Vector3.right * -90f);
        transform.Translate(Vector3.forward * distance * 5);
        end = transform.position;

        rnd = Vector3.Lerp(start, end, Random.value);

        originDir = start - startPos;
        originDir.Normalize(); 


        Vector3 dir = end - start;
        dir.Normalize();
        Vector3 perp = Vector3.Cross(dir, originDir);

        leafNodes.Add(rnd + (perp * (leafSize / 5)));
        leafNodes.Add(rnd + (perp * -(leafSize / 5)));

        offsetPoint = rnd + (perp * leafSize);
        Vector3 perpDir = (rnd - offsetPoint).normalized;
        perp = Vector3.Cross(dir, perpDir);
        offsetPoint = rnd + (perp * leafSize);
        offsetLeaf = rnd + (perp * leafSize * (leafSize));
        offsetLeaf.y = offsetPoint.y;
        offsetLeaf += Vector3.down;
        
        dir = startPos - offsetPoint;
        perpDir = offsetLeaf - offsetPoint;

        float leafNodeOffset = Vector3.Distance(rnd, offsetPoint);
        Vector3 leafNodeMid = (offsetLeaf + offsetPoint) / 2;
        perp = Vector3.Cross(dir.normalized, perpDir.normalized);

        leafNode = leafNodeMid + (perp * (leafNodeOffset/2));
        leafNodes.Add(leafNode);
        leafNode = leafNodeMid + (perp * -(leafNodeOffset/2));
        leafNodes.Add(leafNode);
        leafNode = offsetPoint + (perp * (leafNodeOffset/2));
        leafNodes.Add(leafNode);
        leafNode = offsetPoint + (perp * -(leafNodeOffset/2));
        leafNodes.Add(leafNode);
        leafNodeOffset = Vector3.Distance(leafNode, offsetPoint);

        leafNode = (offsetPoint + ((offsetPoint - offsetLeaf).normalized) * (leafNodeOffset / 2)) + (perp * (leafNodeOffset / 2));
        leafNodes.Add(leafNode);
        leafNode = (offsetPoint + ((offsetPoint - offsetLeaf).normalized) * (leafNodeOffset / 2)) + (perp * -(leafNodeOffset / 2));
        leafNodes.Add(leafNode);

    }



    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, end);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(rnd, 0.5f);
        Gizmos.DrawWireSphere(offsetLeaf, 0.5f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(rnd, offsetPoint);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(offsetPoint, 0.5f);

        if (leafNodes != null)
        {
            foreach (Vector3 node in leafNodes)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(node, 0.2f);
            }
        }
    }
}

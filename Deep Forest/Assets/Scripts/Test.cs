using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Test : MonoBehaviour {

    Vector3 start;
    Vector3 end;

    List<Vector3> leafNodes;

    Vector3 leafPosition;
    Vector3 leafNode;
    Vector3 test;
    Vector3 toStart;
    Vector3 leafStart, leafEnd;

    float distance = 2f;
    float leafSize = 2f;

	// Use this for initialization
	void Start () {
        leafNodes = new List<Vector3>();
        Vector3 centre = transform.position;
        Quaternion startRot = transform.rotation;
        
        transform.Translate(Vector3.forward * distance);
        start = transform.position;

        transform.position = centre;
        transform.rotation = startRot;

        transform.Rotate(Vector3.right * -90f);
        transform.Translate(Vector3.forward * distance * 5);
        end = transform.position;

        leafPosition = Vector3.Lerp(start, end, Random.value);

        toStart = start - centre;
        toStart.Normalize(); 


        Vector3 toEnd = end - start;
        toEnd.Normalize();
        Vector3 leafPerp = Vector3.Cross(toEnd, toStart);

        leafNodes.Add(leafPosition + (leafPerp * (leafSize / 5)));
        leafNodes.Add(leafPosition + (leafPerp * -(leafSize / 5)));

        leafStart = leafPosition + (leafPerp * leafSize);
        Vector3 toLeafPos = (leafPosition - leafStart).normalized;
        leafPerp = Vector3.Cross(toEnd, toLeafPos);
        leafStart = leafPosition + (leafPerp * leafSize);

        leafEnd = leafPosition + (leafPerp * leafSize * (leafSize));
        leafEnd.y = leafStart.y;
        leafEnd += Vector3.down * leafSize;
        
        toEnd = centre - leafStart;
        toLeafPos = leafEnd - leafStart;

        float leafNodeOffset = Vector3.Distance(leafPosition, leafStart);
        Vector3 leafMid = (leafEnd + leafStart) / 2;
        leafPerp = Vector3.Cross(toEnd.normalized, toLeafPos.normalized);

        leafNode = leafMid + (leafPerp * (leafNodeOffset/2));
        leafNodes.Add(leafNode);
        leafNode = leafMid + (leafPerp * -(leafNodeOffset/2));
        leafNodes.Add(leafNode);
        leafNode = leafStart + (leafPerp * (leafNodeOffset/2));
        leafNodes.Add(leafNode);
        leafNode = leafStart + (leafPerp * -(leafNodeOffset/2));
        leafNodes.Add(leafNode);
        leafNodeOffset = Vector3.Distance(leafNode, leafStart);

        leafNode = (leafStart + ((leafStart - leafEnd).normalized) * (leafNodeOffset / 2)) + (leafPerp * (leafNodeOffset / 2));
        leafNodes.Add(leafNode);
        leafNode = (leafStart + ((leafStart - leafEnd).normalized) * (leafNodeOffset / 2)) + (leafPerp * -(leafNodeOffset / 2));
        leafNodes.Add(leafNode);

    }



    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, end);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(leafPosition, 0.5f);
        Gizmos.DrawWireSphere(leafEnd, 0.5f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(leafPosition, leafStart);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(leafStart, 0.5f);

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

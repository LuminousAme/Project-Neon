using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//use this class to link the proceduaral rigs with other gameobjects
public class Alligner : MonoBehaviour
{
    [SerializeField] List<AlignObject> Alignments = new List<AlignObject>();

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < Alignments.Count; i++)
        {
            if(Alignments[i].target != null && Alignments[i].copyFrom != null)
            {
                Alignments[i].target.position = Alignments[i].copyFrom.position;
                Alignments[i].target.rotation = Alignments[i].copyFrom.rotation * Quaternion.Euler(Alignments[i].eulerOffSet);
            }
        }
    }
}

[System.Serializable]
public class AlignObject
{
    public Transform target;
    public Transform copyFrom;
    public Vector3 eulerOffSet = Vector3.zero;
}

using UnityEngine;

public class CloudPointMeshView : MonoBehaviour
{
     public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public Mesh GetMesh()
        {
            return GetComponent<MeshFilter>().mesh;
        }
}

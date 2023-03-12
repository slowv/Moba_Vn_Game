using UnityEngine;
using UnityStandardAssets.Water;

namespace MOBA.Standard_Assets.Environment.Water.Water.Scripts
{
    [ExecuteInEditMode]
    public class WaterTile : MonoBehaviour
    {
        public PlanarReflection reflection;
        public WaterBase waterBase;


        public void Start()
        {
            AcquireComponents();
        }


        // ReSharper disable Unity.PerformanceAnalysis
        void AcquireComponents()
        {
            if (!reflection)
            {
                reflection = transform.parent ? transform.parent.GetComponent<PlanarReflection>() : transform.GetComponent<PlanarReflection>();
            }

            if (!waterBase)
            {
                waterBase = transform.parent ? transform.parent.GetComponent<WaterBase>() : transform.GetComponent<WaterBase>();
            }
        }


#if UNITY_EDITOR
        public void Update()
        {
            AcquireComponents();
        }
#endif


        public void OnWillRenderObject()
        {
            if (reflection)
            {
                reflection.WaterTileBeingRendered(transform, Camera.current);
            }
            if (waterBase)
            {
                waterBase.WaterTileBeingRendered(transform, Camera.current);
            }
        }
    }
}
﻿using UnityEngine;
using GEAR.Serialize;
using System.Collections.Generic;

namespace Maroon.Physics.HuygensPrinciple
{
    //[ExecuteInEditMode]
    public class SlitPlate : MonoBehaviour, IResetObject
    {
        [Header("Properties")]

        [SerializeField]
        [Range(1, 5, order = 0)]
        [SerializeProperty("NumberOfSlits", order = 1)]
        private int numberOfSlits = 1;

        [SerializeField]
        [Range(0,5, order = 0)]
        [SerializeProperty("SlitWidth", order = 1)]
        private float slitWidth = 0.1f;

        [Header("Plate Objects")]
        
        [SerializeField]
        private GameObject top;

        [SerializeField]
        private GameObject bottom;

        [SerializeField]
        private GameObject right;

        [SerializeField]
        private GameObject left;

        [SerializeField]
        private Material plateMaterial;

        private Vector3 TopSize => top.GetComponentInChildren<MeshRenderer>().bounds.size;
        private Vector3 BottomSize => bottom.GetComponentInChildren<MeshRenderer>().bounds.size;
        private Vector3 RightSize => right.GetComponentInChildren<MeshRenderer>().bounds.size;
        private Vector3 LeftSize => left.GetComponentInChildren<MeshRenderer>().bounds.size;

        public float PlateWidth => top.transform.localScale.x;    
        public float PlateHeight => TopSize.y + RightSize.y + BottomSize.y;

        private int generatorCountPerSlit;

        private Vector3 previousPlateScale;
        private Vector3 previousPlatePosition;

        private List<GameObject> midSections = new List<GameObject>();
        private List<WaveGenerator> waveGeneratorList = new List<WaveGenerator>();

        private static WaveGeneratorPoolHandler _wgphInstance;
        public static WaveGeneratorPoolHandler Instance
        {
            get
            {
                if (_wgphInstance == null)
                    _wgphInstance = FindObjectOfType<WaveGeneratorPoolHandler>();
                return _wgphInstance;
            }
        }

        public int NumberOfSlits
        {
            get => numberOfSlits;
            set
            {
                numberOfSlits = value;
                generatorCountPerSlit = CalculateGeneratorsPerSlit();
                ResetCubes();
                SetupPlateSlits(true);
            }
        }

        public float SlitWidth
        {
            get => slitWidth;
            set
            {
                slitWidth = value;
                generatorCountPerSlit = CalculateGeneratorsPerSlit();
                ResetWaveGenerators();
                SetupPlateSlits(false);
            }
        }

        public void SetNumberOfSlits(float value)
        {
            this.numberOfSlits = (int)value;
            generatorCountPerSlit = CalculateGeneratorsPerSlit();
            ResetCubes();
            SetupPlateSlits(true);
        }

        public void SetSlitWidth(float value)
        {
            this.slitWidth = value;
            generatorCountPerSlit = CalculateGeneratorsPerSlit();
            ResetWaveGenerators();
            SetupPlateSlits(false);
        }
        
      
        private void Start()
        {
            if(top == null)
                Debug.LogError("SlitPlate::Start: Top object cannot be null.");
            if (bottom == null)
                Debug.LogError("SlitPlate::Start: Bottom object cannot be null.");
            if (right == null)
                Debug.LogError("SlitPlate::Start: Right object cannot be null.");
            if (left == null)
                Debug.LogError("SlitPlate::Start: Left object cannot be null.");
            if (plateMaterial == null)
                plateMaterial = top.GetComponent<MeshRenderer>().sharedMaterial;
            if (!Mathf.Approximately(TopSize.z, BottomSize.z))
                Debug.LogError("SlitPlate::Start: Top and Bottom object width must be equal.");

            if (!Mathf.Approximately(RightSize.y, LeftSize.y))
                Debug.LogError("SlitPlate::Start: Right and Left object height must be equal.");

            generatorCountPerSlit = CalculateGeneratorsPerSlit();
            SetupPlateSlits(false);
            StorePreviousState();
        }

        private void SetupPlateSlits(bool numberOfSlitsChanged)
        {
            var cubeCount = numberOfSlits + 1;
            var scale = right.transform.localScale;
            bool scaleInBounds = PlateWidth - SlitWidth * numberOfSlits >= 0 ; 

            scale.x = scaleInBounds ? (PlateWidth - (slitWidth * numberOfSlits)) / cubeCount : 0.0f ; 
            right.transform.localScale = left.transform.localScale = scale;

            if (numberOfSlitsChanged)
            {               
                for (var count = 0; count < cubeCount - 2; count++)
                {
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.parent = gameObject.transform;
                    cube.transform.rotation = gameObject.transform.rotation;
                    cube.transform.localPosition = left.transform.localPosition;
                    cube.GetComponent<Renderer>().material = plateMaterial;
                    midSections.Add(cube);                 
                }
            }

            AddAllWaveGenerators();
            ScaleAndPositionPlates(scale, scaleInBounds);
            ScaleAndPositionWaveGenerators(scale, scaleInBounds); 
        }

        public void ScaleAndPositionPlates(Vector3 scale, bool scaleInBounds)
        {
            if (midSections.Count > 0)
                for (var index = 0; index < midSections.Count; index++)
                {
                    midSections[index].transform.localScale = scale;
                    if(scaleInBounds)
                    {
                        var transformCubeSection = left.transform.localPosition.x + ((scale.x + slitWidth) * (float)(index + 1)) + scale.x / 2.0f;                                
                        midSections[index].transform.localPosition = new Vector3(transformCubeSection, left.transform.localPosition.y, left.transform.localPosition.z);
                    }
                }
        }

        public void ScaleAndPositionWaveGenerators(Vector3 scale, bool scaleInBounds)
        {
            var transition = scale.x + slitWidth;
            var initialPositionLeft = left.transform.localPosition.x + scale.x;
            var generatorPlacementTransistion = (slitWidth / (float)(generatorCountPerSlit + 1));

            if (slitWidth > 0.0f)
            {
                ActivatePlateWaveGenerators();
                for (var slitIndex = 0; slitIndex < numberOfSlits; slitIndex++)
                {
                    if (scaleInBounds)
                    {
                        var generatorGroupTransition = initialPositionLeft + (transition * (float)(slitIndex));
                        for (var count = 0; count < generatorCountPerSlit; count++)
                        {
                            waveGeneratorList[count + (slitIndex * (generatorCountPerSlit))].transform.localPosition = new Vector3(generatorGroupTransition + (generatorPlacementTransistion * (count + 1)), left.transform.localPosition.y, left.transform.localPosition.z + 0.02f);                 
                        }
                    }                             
                }
            }
            else
            {
                DeactivatePlateWaveGenerators();
            }
        }

        private int CalculateGeneratorsPerSlit()
        {        
            int numberOfGeneratorsPerSlit = 1 + (int)((slitWidth * 10.0f)/2.0f);
            return numberOfGeneratorsPerSlit; 
        }

        public void AddWaveGenerator()
        {
            var waveGenerator = Instance.CreateWaveGenerator();
            waveGenerator.transform.parent = gameObject.transform;
            waveGeneratorList.Add(waveGenerator);
        }

        public void AddAllWaveGenerators() 
        {
            var totalNumberOfGenerators = generatorCountPerSlit * numberOfSlits; 
            for (int count = 0; count < totalNumberOfGenerators; count++)
            {
                AddWaveGenerator();
            }

        }

        private void ActivatePlateWaveGenerators()
        {
            foreach (var generator in waveGeneratorList)
                generator.SetGeneratorActive(true);
        }

        private void DeactivatePlateWaveGenerators()
        {
            foreach (var generator in waveGeneratorList)
                generator.SetGeneratorActive(false);
        }

        public void ResetCubes()
        {
            foreach (var section in midSections)
            {
                DestroyImmediate(section);
            }

            ResetWaveGenerators();
            midSections.Clear();
        }

        public void ResetWaveGenerators()
        {
            foreach (var generator in waveGeneratorList)
            {
                Instance.RemoveWaveGenerator(generator);
                DestroyImmediate(generator.gameObject);
            }
            waveGeneratorList.Clear();
        }

        private void StorePreviousState()
        {
            previousPlateScale = gameObject.transform.localScale;
            previousPlatePosition = gameObject.transform.position;
        }

        private void LoadPreviousState()
        {
            gameObject.transform.localScale = previousPlateScale;
            gameObject.transform.position = previousPlatePosition;
        }

        public void ResetObject()
        {
            ResetCubes();
            SetupPlateSlits(false);
            LoadPreviousState();
        }
    }
}

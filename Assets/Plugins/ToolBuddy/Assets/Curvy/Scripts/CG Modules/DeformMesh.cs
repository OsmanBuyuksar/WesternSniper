// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine.Assertions;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    /// <summary>
    /// Deforms a mesh following a path
    /// </summary>
    [ModuleInfo("Modifier/Deform Mesh", ModuleName = "Deform Mesh", Description = "Deform a mesh following a path")]
    [HelpURL(CurvySpline.DOCLINK + "cgdeformmesh")]
    public class DeformMesh : CGModule
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGVMesh), Array = true, Name = "VMesh")]
        public CGModuleInputSlot InVMeshes = new CGModuleInputSlot();

        [HideInInspector]
        [InputSlotInfo(typeof(CGPath), Name = "Path", DisplayName = "Volume/Rasterized Path")]
        public CGModuleInputSlot InPath = new CGModuleInputSlot();

        [HideInInspector]
        [InputSlotInfo(typeof(CGSpots), Array = true, Name = "Spots", Optional = true)]
        public CGModuleInputSlot InSpots = new CGModuleInputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGVMesh), Name = "VMesh", Array = true)]
        public CGModuleOutputSlot OutVMeshes = new CGModuleOutputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGSpots), Array = true, Name = "Spots")]
        public CGModuleOutputSlot OutSpots = new CGModuleOutputSlot();

        [SerializeField]
        [Tooltip("Stretch the meshes to make them fit the end of the path")]
        private bool stretchToEnd;

        private ThreadPoolWorker<CGSpot> threadWorker = new ThreadPoolWorker<CGSpot>();

        /// <summary>
        /// Stretch the meshes to make them fit the end of the path
        /// </summary>
        public bool StretchToEnd
        {
            get => stretchToEnd;
            set
            {
                if (stretchToEnd != value)
                {
                    stretchToEnd = value;
                    Dirty = true;
                }
            }
        }

        #region ### Module Overrides ###

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Dirty = true;
        }
#endif

        public override void Reset()
        {
            base.Reset();
            StretchToEnd = false;
        }

        public override void Refresh()
        {
            base.Refresh();

            CGPath path = InPath.GetData<CGPath>(out bool isPathDisposable);
            List<CGVMesh> inputMeshes = InVMeshes.GetAllData<CGVMesh>(out bool isMeshesDisposable);

            CGData[] outVMeshesData;
            CGSpots outSpotsData;
            if (inputMeshes.Count != 0)
            {
                List<CGSpots> inputSpots = InSpots.GetAllData<CGSpots>(out bool isSpotsDisposable);
                SubArray<CGSpot>? inputSpotsArray = ToOneDimensionalArray(inputSpots, out bool isCopy);
                bool thereAreSpots = inputSpotsArray.HasValue && inputSpotsArray.Value.Count != 0;

                if (thereAreSpots)
                {
                    int spotsCount = inputSpotsArray.Value.Count;
                    CGSpot[] spotsArray = inputSpotsArray.Value.Array;

                    //inputs verification
                    bool areSpotsValid = true;
                    for (int i = 0; i < spotsCount; i++)
                    {
                        int meshIndex = spotsArray[i].Index;
                        if (meshIndex < 0 || meshIndex >= inputMeshes.Count)
                        {
                            UIMessages.Add($"Spot #{i} has an invalid Index value of '{meshIndex}'. An index can't be greater or equal to the number of input Meshes, which is '{inputMeshes.Count}'");
                            areSpotsValid = false;
                            break;
                        }
                    }

                    //the actual processing
                    if (areSpotsValid)
                    {
                        CGVMesh[] outputMeshes = new CGVMesh[spotsCount];
                        SubArray<CGSpot> outputSpots = ArrayPools.CGSpot.Allocate(spotsCount);

                        DeformMeshes(inputMeshes, inputSpotsArray.Value, outputSpots, outputMeshes, path, StretchToEnd, threadWorker);

                        outVMeshesData = outputMeshes;
                        outSpotsData = new CGSpots(outputSpots);
                    }
                    else
                    {
                        outVMeshesData = Array.Empty<CGVMesh>();
                        outSpotsData = new CGSpots();
                    }
                }
                else
                {
                    outVMeshesData = Array.Empty<CGVMesh>();
                    outSpotsData = new CGSpots();
                }

                if (isCopy)
                    ArrayPools.CGSpot.Free(inputSpotsArray.Value);

                if (isSpotsDisposable)
                    inputSpots.ForEach(s => s.Dispose());
            }
            else
            {
                outVMeshesData = Array.Empty<CGVMesh>();
                outSpotsData = new CGSpots();
            }

            OutVMeshes.SetData(outVMeshesData);
            OutSpots.SetData(outSpotsData);

            if (isPathDisposable)
                path.Dispose();

            if (isMeshesDisposable)
                inputMeshes.ForEach(m => m.Dispose());
        }
        #endregion

        /// <summary>
        /// Deforms multiple <see cref="CGVMesh"/>s following a path 
        /// </summary>
        /// <param name="inputMeshes">The list of meshes the <see cref="CGSpot.Index"/> of <paramref name="inputSpots"/> refer to</param>
        /// <param name="inputSpots">The <see cref="CGSpot"/>s defining the transform and mesh to use as inputs</param>
        /// <param name="outputSpots">Should have the same <see cref="ArraySegment{T}.Count"/> as <paramref name="inputMeshes"/></param>
        /// <param name="outputMeshes">Should have the same <see cref="ArraySegment{T}.Count"/> as <paramref name="inputMeshes"/></param>
        /// <param name="path">A path defining how the meshes should be deformed</param>
        /// <param name="stretchToEnd">see <see cref="StretchToEnd"/></param>
        /// <param name="threadPoolWorker">An instance of <see cref="ThreadPoolWorker{CGSpot}"/> to run the mesh deformation on</param>
        public static void DeformMeshes([NotNull] List<CGVMesh> inputMeshes, SubArray<CGSpot> inputSpots, SubArray<CGSpot> outputSpots, [NotNull] CGVMesh[] outputMeshes, [NotNull] CGPath path, bool stretchToEnd, ThreadPoolWorker<CGSpot> threadPoolWorker)
        {
            if (inputMeshes == null) throw new ArgumentNullException(nameof(inputMeshes));
            if (outputMeshes == null) throw new ArgumentNullException(nameof(outputMeshes));
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (inputSpots.Count == 0) throw new ArgumentException("input spots should have at least one element", nameof(inputSpots));
            if (inputMeshes.Count == 0) throw new ArgumentException("input meshes should have at least one element", nameof(inputMeshes));

            CGSpot[] spotsArray = inputSpots.Array;
            int spotsCount = inputSpots.Count;

#if CURVY_SANITY_CHECKS
            for (int i = 0; i < spotsCount; i++)
            {
                int meshIndex = spotsArray[i].Index;

                Assert.IsTrue(meshIndex >= 0);
                Assert.IsTrue(meshIndex < inputMeshes.Count);
            }
#endif

            //prepare output meshes
            for (int i = 0; i < spotsCount; i++)
            {
                CGVMesh inputMesh = inputMeshes[spotsArray[i].Index];

                CGVMesh outputMesh = new CGVMesh(inputMesh.Count, inputMesh.HasUV, inputMesh.HasUV2, inputMesh.HasNormals, inputMesh.HasTangents);

                if (inputMesh.HasUV)
                    Array.Copy(inputMesh.UVs.Array, 0,
                        outputMesh.UVs.Array, 0, inputMesh.UVs.Count);

                if (inputMesh.HasUV2)
                    Array.Copy(inputMesh.UV2s.Array, 0,
                    outputMesh.UV2s.Array, 0, inputMesh.UV2s.Count);

                outputMesh.SubMeshes = new CGVSubMesh[inputMesh.SubMeshes.Length];
                for (int j = 0; j < inputMesh.SubMeshes.Length; j++)
                    outputMesh.SubMeshes[j] = new CGVSubMesh(inputMesh.SubMeshes[j]);

                outputMeshes[i] = outputMesh;
            }

            //if StretchToEnd, prepare stretching data
            float smallestVertexDistance;
            float stretchingAdditionalDistanceRatio; //How much additional distance should be applied per unit of distance
            if (stretchToEnd)
            {
                //smallestVertexDistance
                {
                    CGSpot firstSpot = spotsArray[0];
                    float firstSpotDistance = GetSpotDistance(path, firstSpot.Position, path.Positions.Array, path.Count - 1, path.RelativeDistances.Array, path.Length);

                    CGVMesh firstMesh = inputMeshes[firstSpot.Index];
                    if (firstMesh.Count == 0)
                    {
                        smallestVertexDistance = 0;
                    }
                    else
                    {
                        SubArray<int> firstMeshSortedVertexIndices = firstMesh.GetCachedSortedVertexIndices();
                        smallestVertexDistance = firstSpotDistance +
                                                 firstMesh.Vertices.Array[firstMeshSortedVertexIndices.Array[0]].z;
                    }

                }

                float largestVertexDistance;
                {
                    CGSpot lastSpot = spotsArray[spotsCount - 1];
                    float lastSpotDistance = GetSpotDistance(path, lastSpot.Position, path.Positions.Array, path.Count - 1, path.RelativeDistances.Array, path.Length);

                    CGVMesh lastMesh = inputMeshes[lastSpot.Index];
                    if (lastMesh.Count == 0)
                    {
                        largestVertexDistance = 0;
                    }
                    else
                    {
                        SubArray<int> lastMeshSortedVertexIndices = lastMesh.GetCachedSortedVertexIndices();
                        largestVertexDistance = lastSpotDistance +
                                                lastMesh.Vertices.Array[lastMeshSortedVertexIndices.Array[lastMesh.Vertices.Count - 1]].z;
                    }
                }

                float deltaDistances = (largestVertexDistance - smallestVertexDistance);
#if CURVY_SANITY_CHECKS
                Assert.IsTrue(deltaDistances >= 0);
#endif
                stretchingAdditionalDistanceRatio = deltaDistances > 0 ? (path.Length - largestVertexDistance) / deltaDistances : 0;
            }
            else
            {
                smallestVertexDistance = stretchingAdditionalDistanceRatio = Single.NaN;
            }

            //OPTIM Replace this action with one that is created at instance creation. This action had to take all the parameters needed, this way you avoid the compiler automatically creating a class for the captured parameters, which will be automatically instantiated at each call

            //the action that does the mesh deformation for a specific spot
            Action<CGSpot, int, int> action = (spot, spotIndex, elementsCount) =>
             {
                 //path
                 int pathPointsCount = path.Count;
                 Vector3[] pathDirections = path.Directions.Array;
                 Vector3[] pathNormals = path.Normals.Array;
                 float[] pathRelativeDistances = path.RelativeDistances.Array;
                 Vector3[] pathPoints = path.Positions.Array;

                 //input mesh
                 CGVMesh inputMesh = inputMeshes[spot.Index];
                 int verticesCount = inputMesh.Vertices.Count;
                 Vector3[] inputMeshVertices = inputMesh.Vertices.Array;
                 Vector3[] inputMeshNormals = inputMesh.NormalsList.Array;
                 Vector4[] inputMeshTangents = inputMesh.TangentsList.Array;
                 int[] sortedInputVertexIndices = inputMesh.GetCachedSortedVertexIndices().Array;

                 //output mesh
                 CGVMesh outputMesh = outputMeshes[spotIndex];
                 Vector3[] outputMeshVertices = outputMesh.Vertices.Array;
                 Vector3[] outputMeshNormals = outputMesh.NormalsList.Array;
                 Vector4[] outputMeshTangents = outputMesh.TangentsList.Array;

                 outputMesh.Name = inputMesh.Name;

                 int maxIndex = pathPointsCount - 1;
                 float pathLength = path.Length;
                 float inversePathLength = 1 / pathLength;

                 Vector3 spotPosition = spot.Position;
                 Vector3 spotScale = spot.Scale;
                 float spotZScale = spot.Scale.z;
                 float inverseSpotXScale = 1 / spot.Scale.x;
                 float inverseSpotYScale = 1 / spot.Scale.y;
                 float inverseSpotZScale = 1 / spot.Scale.z;
                 float spotPositionX = spotPosition.x;
                 float spotPositionY = spotPosition.y;
                 float spotPositionZ = spotPosition.z;

                 float spotDistance = GetSpotDistance(path, spotPosition, pathPoints, maxIndex, pathRelativeDistances, pathLength);

                 //update index to make the create mesh module use the right mesh from the meshes array
                 outputSpots.Array[spotIndex] = new CGSpot(spotIndex, spotPosition, Quaternion.identity, spotScale);

                 float previousZ = float.NaN;
                 Vector3 previousPositionOnPathMeshSpace = Vector3.zero;
                 //The following numbers are computed at each rotation multiplication. I compute them only once and store them
                 float previousDeltaRotationMultiplication_precomputedNumber_4 = float.NaN;
                 float previousDeltaRotationMultiplication_precomputedNumber_5 = float.NaN;
                 float previousDeltaRotationMultiplication_precomputedNumber_6 = float.NaN;
                 float previousDeltaRotationMultiplication_precomputedNumber_7 = float.NaN;
                 float previousDeltaRotationMultiplication_precomputedNumber_8 = float.NaN;
                 float previousDeltaRotationMultiplication_precomputedNumber_9 = float.NaN;
                 float previousDeltaRotationMultiplication_precomputedNumber_10 = float.NaN;
                 float previousDeltaRotationMultiplication_precomputedNumber_11 = float.NaN;
                 float previousDeltaRotationMultiplication_precomputedNumber_12 = float.NaN;

                 for (int coupleIndex = 0; coupleIndex < verticesCount; coupleIndex++)
                 {
                     int vertexShitIndex = sortedInputVertexIndices[coupleIndex];
                     Vector3 vertexPosition = inputMeshVertices[vertexShitIndex];

                     Vector3 vertexPositionDelta;
                     {
                         vertexPositionDelta.x = vertexPosition.x;
                         vertexPositionDelta.y = vertexPosition.y;
                         vertexPositionDelta.z = 0;
                     }

                     Vector3 positionOnPathMeshSpace;
                     //The following numbers are computed at each rotation multiplication. I compute them only once and store them
                     float formulaNumber_4;
                     float formulaNumber_5;
                     float formulaNumber_6;
                     float formulaNumber_7;
                     float formulaNumber_8;
                     float formulaNumber_9;
                     float formulaNumber_10;
                     float formulaNumber_11;
                     float formulaNumber_12;

                     float currentZ = vertexPosition.z;
                     if (coupleIndex > 0 && previousZ == currentZ)
                     {
                         positionOnPathMeshSpace = previousPositionOnPathMeshSpace;

                         formulaNumber_4 = previousDeltaRotationMultiplication_precomputedNumber_4;
                         formulaNumber_5 = previousDeltaRotationMultiplication_precomputedNumber_5;
                         formulaNumber_6 = previousDeltaRotationMultiplication_precomputedNumber_6;
                         formulaNumber_7 = previousDeltaRotationMultiplication_precomputedNumber_7;
                         formulaNumber_8 = previousDeltaRotationMultiplication_precomputedNumber_8;
                         formulaNumber_9 = previousDeltaRotationMultiplication_precomputedNumber_9;
                         formulaNumber_10 = previousDeltaRotationMultiplication_precomputedNumber_10;
                         formulaNumber_11 = previousDeltaRotationMultiplication_precomputedNumber_11;
                         formulaNumber_12 = previousDeltaRotationMultiplication_precomputedNumber_12;
                     }
                     else
                     {
                         int vertexFIndex;
                         float vertexFFragment;
                         {
                             float vertexF;
                             {
                                 float vertexDistance = spotDistance + vertexPosition.z * spotZScale;
                                 if (stretchToEnd)
                                 {
                                     vertexDistance += (vertexDistance - smallestVertexDistance) * stretchingAdditionalDistanceRatio;
                                 }

                                 //inlined version of path.DistanceToF()
                                 vertexF = vertexDistance * inversePathLength;

                                 if (path.Seamless)
                                 {
                                     while (vertexF < 0)
                                         vertexF++;
                                     while (vertexF > 1)
                                         vertexF--;
                                 }
                                 else
                                 {
                                     if (vertexF < 0)
                                         vertexF = 0;
                                     else if (vertexF > 1)
                                         vertexF = 1;
                                 }
                             }

                             //Inlined version of path.GetFIndex
                             {
                                 vertexFIndex = CurvyUtility.InterpolationSearch(pathRelativeDistances, pathPointsCount, vertexF);

                                 if (vertexFIndex == maxIndex)
                                 {
                                     vertexFIndex -= 1;
                                     vertexFFragment = 1;
                                 }
                                 else
                                     vertexFFragment = (vertexF - pathRelativeDistances[vertexFIndex]) / (pathRelativeDistances[vertexFIndex + 1] - pathRelativeDistances[vertexFIndex]);
                             }
                         }

                         {
                             int nextIndex = Math.Min(vertexFIndex + 1, maxIndex);

                             Vector3 positionOnPath = Vector3.LerpUnclamped(pathPoints[vertexFIndex], pathPoints[nextIndex], vertexFFragment);

                             positionOnPathMeshSpace.x = (positionOnPath.x - spotPositionX) * inverseSpotXScale;
                             positionOnPathMeshSpace.y = (positionOnPath.y - spotPositionY) * inverseSpotYScale;
                             positionOnPathMeshSpace.z = (positionOnPath.z - spotPositionZ) * inverseSpotZScale;

                             Quaternion indexRotation = Quaternion.LookRotation(pathDirections[vertexFIndex], pathNormals[vertexFIndex]);
                             Quaternion nextIndexRotation = Quaternion.LookRotation(pathDirections[nextIndex], pathNormals[nextIndex]);
                             Quaternion deltaRotation = (Quaternion.LerpUnclamped(indexRotation, nextIndexRotation, vertexFFragment));

                             float formulaNumber_1 = deltaRotation.x * 2f;
                             float formulaNumber_2 = deltaRotation.y * 2f;
                             float formulaNumber_3 = deltaRotation.z * 2f;
                             formulaNumber_4 = deltaRotation.x * formulaNumber_1;
                             formulaNumber_5 = deltaRotation.y * formulaNumber_2;
                             formulaNumber_6 = deltaRotation.z * formulaNumber_3;
                             formulaNumber_7 = deltaRotation.x * formulaNumber_2;
                             formulaNumber_8 = deltaRotation.x * formulaNumber_3;
                             formulaNumber_9 = deltaRotation.y * formulaNumber_3;
                             formulaNumber_10 = deltaRotation.w * formulaNumber_1;
                             formulaNumber_11 = deltaRotation.w * formulaNumber_2;
                             formulaNumber_12 = deltaRotation.w * formulaNumber_3;

                         }

                         previousZ = currentZ;
                         previousPositionOnPathMeshSpace = positionOnPathMeshSpace;
                         previousDeltaRotationMultiplication_precomputedNumber_4 = formulaNumber_4;
                         previousDeltaRotationMultiplication_precomputedNumber_5 = formulaNumber_5;
                         previousDeltaRotationMultiplication_precomputedNumber_6 = formulaNumber_6;
                         previousDeltaRotationMultiplication_precomputedNumber_7 = formulaNumber_7;
                         previousDeltaRotationMultiplication_precomputedNumber_8 = formulaNumber_8;
                         previousDeltaRotationMultiplication_precomputedNumber_9 = formulaNumber_9;
                         previousDeltaRotationMultiplication_precomputedNumber_10 = formulaNumber_10;
                         previousDeltaRotationMultiplication_precomputedNumber_11 = formulaNumber_11;
                         previousDeltaRotationMultiplication_precomputedNumber_12 = formulaNumber_12;

                     }

                     //Optimized version of outputMeshVertex[vertexShitIndex] = (deltaRotation * vertexPositionDelta) + (positionOnPathMeshSpace);
                     {
                         Vector3 outputVertex;
                         {
                             outputVertex.x = positionOnPathMeshSpace.x +
                                              (1.0f - (formulaNumber_5 + formulaNumber_6)) * vertexPositionDelta.x + (formulaNumber_7 - formulaNumber_12) * vertexPositionDelta.y + (formulaNumber_8 + formulaNumber_11) * vertexPositionDelta.z;
                             outputVertex.y = positionOnPathMeshSpace.y +
                                              (formulaNumber_7 + formulaNumber_12) * vertexPositionDelta.x + (1.0f - (formulaNumber_4 + formulaNumber_6)) * vertexPositionDelta.y + (formulaNumber_9 - formulaNumber_10) * vertexPositionDelta.z;
                             outputVertex.z = positionOnPathMeshSpace.z +
                                              (formulaNumber_8 - formulaNumber_11) * vertexPositionDelta.x + (formulaNumber_9 + formulaNumber_10) * vertexPositionDelta.y + (1.0f - (formulaNumber_4 + formulaNumber_5)) * vertexPositionDelta.z;
                         }

                         outputMeshVertices[vertexShitIndex] = outputVertex;
                     }


                     //Optimized version of outputMeshNormal[vertexShitIndex] = deltaRotation * inputMeshNormal[vertexShitIndex];
                     {
                         Vector3 outputNormal;
                         {
                             Vector3 inputNormal = inputMeshNormals[vertexShitIndex];

                             outputNormal.x = (1.0f - (formulaNumber_5 + formulaNumber_6)) * inputNormal.x + (formulaNumber_7 - formulaNumber_12) * inputNormal.y + (formulaNumber_8 + formulaNumber_11) * inputNormal.z;
                             outputNormal.y = (formulaNumber_7 + formulaNumber_12) * inputNormal.x + (1.0f - (formulaNumber_4 + formulaNumber_6)) * inputNormal.y + (formulaNumber_9 - formulaNumber_10) * inputNormal.z;
                             outputNormal.z = (formulaNumber_8 - formulaNumber_11) * inputNormal.x + (formulaNumber_9 + formulaNumber_10) * inputNormal.y + (1.0f - (formulaNumber_4 + formulaNumber_5)) * inputNormal.z;
                         }

                         outputMeshNormals[vertexShitIndex] = outputNormal;
                     }


                     //Optimized version of outputMeshTangents[vertexShitIndex] = deltaRotation * inputMeshTangents[vertexShitIndex];
                     {
                         Vector4 outputMeshTangent;
                         {

                             Vector4 inputTangent = inputMeshTangents[vertexShitIndex];

                             outputMeshTangent.x = (1.0f - (formulaNumber_5 + formulaNumber_6)) * inputTangent.x + (formulaNumber_7 - formulaNumber_12) * inputTangent.y + (formulaNumber_8 + formulaNumber_11) * inputTangent.z;
                             outputMeshTangent.y = (formulaNumber_7 + formulaNumber_12) * inputTangent.x + (1.0f - (formulaNumber_4 + formulaNumber_6)) * inputTangent.y + (formulaNumber_9 - formulaNumber_10) * inputTangent.z;
                             outputMeshTangent.z = (formulaNumber_8 - formulaNumber_11) * inputTangent.x + (formulaNumber_9 + formulaNumber_10) * inputTangent.y + (1.0f - (formulaNumber_4 + formulaNumber_5)) * inputTangent.z;
                             outputMeshTangent.w = inputTangent.w;
                         }
                         outputMeshTangents[vertexShitIndex] = outputMeshTangent;
                     }
                 }
             };

            threadPoolWorker.ParallelFor(action, spotsArray, spotsCount);
        }

        private static float GetSpotDistance(CGPath path, Vector3 spotPosition, Vector3[] pathPoints, int maxIndex, float[] pathRelativeDistances, float pathLength)
        {
            float spotDistance;
            {
                int spotRelativeDistanceIndex;
                CurvyUtility.GetNearestPointIndex(spotPosition, pathPoints, path.Positions.Count, out spotRelativeDistanceIndex, out float fragment);
                int nextIndex = Math.Min(spotRelativeDistanceIndex + 1, maxIndex);
                spotDistance = Mathf.LerpUnclamped(pathRelativeDistances[spotRelativeDistanceIndex], pathRelativeDistances[nextIndex], fragment) * pathLength;
            }
            return spotDistance;
        }

        private static SubArray<CGSpot>? ToOneDimensionalArray(List<CGSpots> spotsList, out bool arrayIsCopy)
        {
            SubArray<CGSpot>? output;
            switch (spotsList.Count)
            {
                case 1:
                    if (spotsList[0] != null)
                    {
                        output = new SubArray<CGSpot>(spotsList[0].Spots.Array, spotsList[0].Spots.Count);
                        arrayIsCopy = false;
                    }
                    else
                    {
                        output = null;
                        arrayIsCopy = false;
                    }
                    break;
                case 0:
                    output = null;
                    arrayIsCopy = false;
                    break;
                default:
                    {
                        output = ArrayPools.CGSpot.Allocate(spotsList.Where(s => s != null).Sum(s => s.Count));
                        arrayIsCopy = true;

                        CGSpot[] array = output.Value.Array;
                        int destinationIndex = 0;
                        foreach (CGSpots cgSpots in spotsList)
                        {
                            if (cgSpots == null)
                                continue;
                            Array.Copy(cgSpots.Spots.Array, 0, array, destinationIndex, cgSpots.Spots.Count);
                            destinationIndex += cgSpots.Spots.Count;
                        }
                    }

                    break;
            }

#if CURVY_SANITY_CHECKS
            Assert.IsTrue(arrayIsCopy == false || output.HasValue);
#endif

            return output;
        }
    }
}
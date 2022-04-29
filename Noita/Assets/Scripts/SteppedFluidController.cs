using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System;

public class SteppedFluidController : MonoBehaviour
{
    [SerializeField] int3 gridSize;
    float[] datas;
    float minquantity = 0.05f;
    float dividePower = 8f;
    private void Start()
    {
        datas = new float[gridSize.x * gridSize.y * gridSize.z];
        //datas[25] = 1;
        datas[3] = 1;
        //datas[103] = 1;
        //datas[203] = 1;
        //datas[202] = 1;
        //datas[303] = 1;
        //datas[304] = 1;
        //datas[400] = 1;
        //datas[500] = 1;

        int randomFluidcount = Mathf.RoundToInt(datas.Length / 3f);
        for (int i = 0; i < randomFluidcount; i++)
            datas[UnityEngine.Random.Range(0, datas.Length)] = UnityEngine.Random.Range(minquantity, 1f);


        //CreateArray();
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            //for (int i = 0; i < 10; i++)
            //   datas[UnityEngine.Random.Range(900, 1000)] = UnityEngine.Random.value;

            ApplyFluidMove();
        }
    }

    [ContextMenu("TestCoord")]
    void TestCoord()
    {
        int3 coord = new int3(UnityEngine.Random.Range(0, gridSize.x), UnityEngine.Random.Range(0, gridSize.y), UnityEngine.Random.Range(0, gridSize.z));
        Debug.Log("coord : " + coord);
        int index = CalculateIndex(coord, gridSize.x);
        Debug.Log("index : " + index);
        Debug.Log("newCoord : " + CalculateCoord(index));
        coord += new int3(0, -1, 0);
        Debug.Log("-1 coord : " + coord);
        Debug.Log("-1 index : " + CalculateIndex(coord, gridSize.x));
    }
    void CreateArray()
    {
        NativeArray<FluidNode> fluidArray = new NativeArray<FluidNode>(gridSize.x * gridSize.y * gridSize.z, Allocator.Persistent);

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.x; z++)
            {
                for (int y = 0; y < gridSize.x; y++)
                {
                    FluidNode fNode = new FluidNode();
                    fNode.x = x;
                    fNode.y = y;
                    fNode.z = z;
                    fNode.index = CalculateIndex(x, y, z, gridSize.x);
                }
            }
        }
    }

    int CalculateIndex(int x, int y, int z, int size)
    {
        return x + z * size + size * size * y;
    }

    int CalculateIndex(int3 coord, int size)
    {
        return coord.x + coord.z * size + size * size * coord.y;
    }

    Vector3 CalculatePosition(int rank)
    {
        return new Vector3(rank % gridSize.x, rank / (gridSize.x * gridSize.y), rank / gridSize.x % gridSize.y);
    }

    int3 CalculateCoord(int rank)
    {
        return new int3(rank % gridSize.x, rank / (gridSize.x * gridSize.y), rank / gridSize.x % gridSize.y);
    }

    struct FluidNode
    {
        public int x;
        public int y;
        public int z;

        public int index;

        public float quantity;
    }

    [ContextMenu("ApplyfluidMove")]
    void ApplyFluidMove()
    {
        float startTime = Time.time;
        float adjcentPower = 0.6f;

        FluidPower under = new FluidPower(new int3(0, -1, 0), 1f); //Down

        List<FluidPower> adjacentUnder = new List<FluidPower>();
        adjacentUnder.Add(new FluidPower(new int3(1, -1, 0), adjcentPower)); // right
        adjacentUnder.Add(new FluidPower(new int3(-1, -1, 0), adjcentPower)); // Left
        adjacentUnder.Add(new FluidPower(new int3(1, -1, 1), adjcentPower)); // right forward
        adjacentUnder.Add(new FluidPower(new int3(-1, -1, 1), adjcentPower)); // Left forward
        adjacentUnder.Add(new FluidPower(new int3(1, -1, -1), adjcentPower)); // right back
        adjacentUnder.Add(new FluidPower(new int3(-1, -1, -1), adjcentPower)); // left back
        adjacentUnder.Add(new FluidPower(new int3(0, -1, 1), adjcentPower)); // forward
        adjacentUnder.Add(new FluidPower(new int3(0, -1, -1), adjcentPower)); // back

        List<FluidQuantity> adjacentUnderFluidValues = new List<FluidQuantity>(adjacentUnder.Count);


        List<FluidPower> adjacent = new List<FluidPower>();
        adjacent.Add(new FluidPower(new int3(1, 0, 0), adjcentPower)); // right
        adjacent.Add(new FluidPower(new int3(-1, 0, 0), adjcentPower)); // Left
        adjacent.Add(new FluidPower(new int3(1, 0, 1), adjcentPower)); // right forward
        adjacent.Add(new FluidPower(new int3(-1, 0, 1), adjcentPower)); // Left forward
        adjacent.Add(new FluidPower(new int3(1, 0, -1), adjcentPower)); // right back
        adjacent.Add(new FluidPower(new int3(-1, 0, -1), adjcentPower)); // left back
        adjacent.Add(new FluidPower(new int3(0, 0, 1), adjcentPower)); // forward
        adjacent.Add(new FluidPower(new int3(0, 0, -1), adjcentPower)); // back
        adjacent.Add(new FluidPower(new int3(0, 0, 0), adjcentPower)); // Center

        List<FluidQuantity> adjacentFluidValues = new List<FluidQuantity>(adjacent.Count);

        List<int> toModify = new List<int>(datas.Length / 3);

        for (int i = 0; i < gridSize.x * gridSize.y * gridSize.z; i++)
        {
            if (datas[i] > 0)
                toModify.Add(i);
        }

        for (int p = 0; p < toModify.Count; p++)
        {
            int i = toModify[p];
            float fluidLeft = datas[i];
            if (fluidLeft <= 0)
                continue;

            int3 coord = CalculateCoord(i);
            // Apply Going Under
            if (coord.y > 0)
            {
                int index = CalculateIndex(coord + under.coord, gridSize.x);
                float delta = Mathf.Min(1f - datas[index], fluidLeft);
                datas[index] += delta;
                fluidLeft -= delta;
                datas[i] = fluidLeft;
            }
        }

        // Around Under
        for (int p = 0; p < toModify.Count; p++)
        {
            int i = toModify[p];
            float fluidLeft = datas[i];
            if (fluidLeft <= 0)
                continue;

            int3 coord = CalculateCoord(i);

            if (coord.y > 0)
            {
                adjacentUnderFluidValues.Clear();
                for (int o = 0; o < adjacentUnder.Count; o++)
                {
                    int3 temp = coord + adjacentUnder[o].coord;
                    if (temp.x >= 0 && temp.x < gridSize.x &&
                        temp.y >= 0 && temp.y < gridSize.y &&
                        temp.z >= 0 && temp.z < gridSize.z)
                    {
                        int rank = CalculateIndex(coord + adjacentUnder[o].coord, gridSize.x);

                        if (datas[rank] < 1f)
                            adjacentUnderFluidValues.Add(new FluidQuantity(rank, datas[rank]));
                    }
                }

                adjacentUnderFluidValues.Sort((x, y) => x.quantity.CompareTo(y.quantity));
                adjacentUnderFluidValues.Add(new FluidQuantity(0, 1f));

                for (int o = 0; o < adjacentUnderFluidValues.Count - 1; o++)
                {
                    if (adjacentUnderFluidValues[o + 1].quantity > adjacentUnderFluidValues[o].quantity)
                    {
                        float count = o + 1;
                        float fluidDelta = Mathf.Min(fluidLeft, ((adjacentUnderFluidValues[o + 1].quantity - adjacentUnderFluidValues[o].quantity) * count));

                        for (int u = 0; u <= o; u++)
                        {
                            FluidQuantity temp = adjacentUnderFluidValues[u];
                            adjacentUnderFluidValues[u] = new FluidQuantity(temp.index, temp.quantity + fluidDelta / count);
                        }
                        fluidLeft -= fluidDelta;
                    }


                    if (fluidLeft <= minquantity)
                    {
                        adjacentUnderFluidValues.RemoveAt(adjacentUnderFluidValues.Count - 1);
                        //adjacentUnderFluidValues.RemoveRange(o + 1, adjacentUnderFluidValues.Count - (o-1));
                        break;
                    }
                }

                for (int o = 0; o < adjacentUnderFluidValues.Count; o++)
                {
                    datas[adjacentUnderFluidValues[o].index] = adjacentUnderFluidValues[o].quantity;
                }
            }
        }


        // Adjacent
        for (int p = 0; p < toModify.Count; p++)
        {
            int i = toModify[p];
            float fluidLeft = datas[i];
            if (fluidLeft <= 0)
                continue;

            int3 coord = CalculateCoord(i);

            // Around
            adjacentFluidValues.Clear();
            float adjacentQuantity = fluidLeft / dividePower;
            for (int o = 0; o < adjacent.Count; o++)
            {
                int3 temp = coord + adjacent[o].coord;
                if (temp.x >= 0 && temp.x < gridSize.x &&
                    temp.y >= 0 && temp.y < gridSize.y &&
                    temp.z >= 0 && temp.z < gridSize.z)
                {
                    int rank = CalculateIndex(coord + adjacent[o].coord, gridSize.x);

                    fluidLeft -= adjacentQuantity;
                    datas[rank] += adjacentQuantity;
                }
            }

            datas[i] = fluidLeft;
        }

        Debug.Log("Time spent : " + (Time.time - startTime));
    }

    struct FluidQuantity
    {
        public int index;
        public float quantity;

        public FluidQuantity(int i, float p)
        {
            index = i;
            quantity = p;
        }
    }

    struct FluidPower
    {
        public int3 coord;
        public float power;

        public FluidPower(int3 c, float p)
        {
            coord = c;
            power = p;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (datas != null)
        {
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] != 0f)
                {
                    Gizmos.DrawWireCube(CalculatePosition(i), (datas[i] / 2f + 0.5f) * Vector3.one);
                }
            }
        }
    }
}
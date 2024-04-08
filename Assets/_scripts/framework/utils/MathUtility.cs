//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System;
using UnityEngine;

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// Shorthands for various often repeated mathematical checks.
    /// </summary>
    public static class MathUtility
    {
        public static float SinLerp(float t)
        {
            return (-Mathf.Cos(t * Mathf.PI) + 1f) / 2f;
        }

        public static bool Approximately(Vector3 a, Vector3 b)
        {
            return Vector3.SqrMagnitude(a - b) < 0.5;
        }

        public static bool Approximately(Quaternion a, Quaternion b)
        {
            return Mathf.Abs(a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w) > 0.99f;
        }

        public static bool InBetween(float x, float a, float b)
        {
            return x > a && x < b;
        }

        public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
        {
            return Mathf.Lerp(outMin, outMax, Mathf.InverseLerp(inMin, inMax, value));
        }

        public static bool Chance(float percent)
        {
            return UnityEngine.Random.Range(0f, 1f) < percent;
        }

        public static bool Chance(this System.Random random, float percent)
        {
            return random.NextDouble() < percent;
        }

        public static int WrapIndex(int index, int max)
        {
            while (index > max) index -= max;
            while (index < 0) index += max;

            return index;
        }

        public static float Frac(float value)
        {
            return value - Mathf.Floor(value);
        }

        public static T WeightedRandomDraw<T>(ValueTuple<float, T>[] input) 
        {
            if (input.Length == 0) return default(T);
            if (input.Length == 1) return input[0].Item2;

            float sum = 0f;
            foreach (ValueTuple<float, T> v in input) sum += v.Item1;
            float random = UnityEngine.Random.Range(0f, sum);
            for(int i = 0; i < input.Length; i++)
            {
                if (random < input[i].Item1) return input[i].Item2;
                random -= input[i].Item1;
            }
            return input[UnityEngine.Random.Range(0, input.Length)].Item2;
        }
    }
}

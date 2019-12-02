// <copyright file="CoroutineWithData.cs" company="Zebedee Inc.">
//
//  Copyright (c) Zebedee, Inc. and its affiliates.
//
//  ZEBEDEE Unity SDK can not be copied and/or distributed without the express
//  permission of Zebedee, Inc. and its affiliates.
//
// </copyright>

namespace ZbdUnitySDK.Unity
{
    using System.Collections;
    using UnityEngine;

    public class CoroutineWithData
    {
        public Coroutine Coroutine { get; private set; }

        public object Result;
        private IEnumerator target;

        public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
        {
            this.target = target;
            this.Coroutine = owner.StartCoroutine(this.Run());
        }

        private IEnumerator Run()
        {
            while (this.target.MoveNext())
            {
                this.Result = this.target.Current;
                yield return this.Result;

            }
        }
    }

}

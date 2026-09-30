#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEngine;
namespace Nemequene.UI
{
    public sealed class UIValidationDriver : MonoBehaviour
    {
        public void Run(IEnumerator steps, Action<int> complete) { StartCoroutine(Execute(steps,complete)); }
        private IEnumerator Execute(IEnumerator steps, Action<int> complete)
        {
            while(true)
            {
                bool next; object current=null;
                try { next=steps.MoveNext(); if(next) current=steps.Current; }
                catch(Exception e) { Debug.LogException(e); complete(1); yield break; }
                if(!next) { complete(0); yield break; }
                yield return current;
            }
        }
    }
}
#endif

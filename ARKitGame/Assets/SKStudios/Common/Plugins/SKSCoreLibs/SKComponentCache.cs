using System;
using System.Collections;
using System.Collections.Generic;
using Eppy;
using UnityEngine;

namespace SKStudios.Common.Extensions {
    public static class SKComponentCache {
        private static readonly Dictionary<Type, Dictionary<GameObject, Component>> CompDictionary;
        private static readonly Dictionary<Type, Component> Components;
        private static readonly HashSet<Tuple<GameObject, Type>> ComponentsChecked;

        static SKComponentCache() {
            CompDictionary = new Dictionary<Type, Dictionary<GameObject, Component>>();
            Components = new Dictionary<Type, Component>();
            ComponentsChecked = new HashSet<Tuple<GameObject, Type>>();
        }

        private static Dictionary<GameObject, Component> GetCompDictionary<T>() {
            Dictionary<GameObject, Component> compDict;
            //Grab the right Dictionary
            if (CompDictionary.ContainsKey(typeof(T))) {
                compDict = CompDictionary[typeof(T)];
            }
            else {
                compDict = new Dictionary<GameObject, Component>();
                CompDictionary.Add(typeof(T), compDict);
            }
            return compDict;
        }

        public static T SKGetComponentOnce<T>(this GameObject gameObject) where T : Component {
            var compDict = GetCompDictionary<T>();
            var tup = new Tuple<GameObject, Type>(gameObject, typeof(T));
            if (ComponentsChecked.Contains(tup)) {
                if (compDict.ContainsKey(gameObject))
                    return (T)compDict[gameObject];
                else
                    return null;

            }
            ComponentsChecked.Add(tup);
            return gameObject.SKGetComponent<T>();
        }

        public static T SKGetComponent<T>(this GameObject gameObject) where T : Component {

            var compDict = GetCompDictionary<T>();
            Component component;
            //Grab the component
            if (compDict.ContainsKey(gameObject)) {
                component = compDict[gameObject];
            }
            else {
                component = gameObject.GetComponent<T>();
                compDict.Add(gameObject, component);
            }
            return (T)component;
        }


    }
}
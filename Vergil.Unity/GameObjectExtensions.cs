using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vergil.Unity {
    public static class GameObjectExtensions {
        public static IEnumerable<GameObject> GetChildren(this GameObject gobj) {
            for (int i = 0; i < gobj.transform.childCount; i++) {
                yield return gobj.transform.GetChild(i).gameObject;
            }
        }

        public static IEnumerable<GameObject> GetChildren(this MonoBehaviour mbeh) {
            return mbeh.gameObject.GetChildren();
        }

        public static GameObject? FindChild(this GameObject gobj, Func<GameObject, bool> predicate) {
            try {
                return gobj.GetChildren().ToList().First(predicate);
            } catch {
                return null;
            }
        }

        public static GameObject FindChild(this MonoBehaviour mbeh, Func<GameObject, bool> predicate) {
            return mbeh.gameObject.FindChild(predicate);
        }

        public static GameObject Find(this GameObject gobj, string name) {
            return gobj.transform.Find(name).gameObject;
        }

        public static GameObject Find(this MonoBehaviour mbeh, string name) {
            return mbeh.gameObject.Find(name);
        }

        public static GameObject? FindRecursive(this GameObject gobj, string name) {
            GameObject? found = gobj.Find(name);
            if (found != null) return found;
            foreach (GameObject child in gobj.GetChildren()) {
                found = child.FindRecursive(name);
                if (found != null) return found;
            }

            return null;
        }

        public static GameObject? FindRecursive(this MonoBehaviour mbeh, string name) {
            return mbeh.gameObject.FindRecursive(name);
        }

        public static void DestroyChildren(this GameObject gobj) {
            foreach (GameObject child in gobj.GetChildren()) UnityEngine.Object.Destroy(child);
        }
        public static void DestroyChildren(this MonoBehaviour mbeh) {
            mbeh.gameObject.DestroyChildren();
        }
    }
}
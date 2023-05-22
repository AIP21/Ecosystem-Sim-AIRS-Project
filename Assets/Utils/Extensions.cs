#if UNITY_EDITOR

// This class adds functions to built-in types.
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ProjectWindowCallback;
using UnityEditor;

public static class Extensions
{

    #region Physics
    public const int MAX_ALLOC_COUNT = 256;

    /**
    * <summary>Set the layer of every child gameObject</summary>
    */
    public static void SetLayerRecursively(this GameObject target, LayerMask layer)
    {
        if (target == null)
            return;

        target.layer = ToLayer(layer.value);

        for (int i = 0; i < target.transform.childCount; i++)
        {
            if (target.transform.GetChild(i) == null)
                continue;

            SetLayerRecursively(target.transform.GetChild(i).gameObject, layer);
        }
    }

    /**
    * <summary>Convert a bitmask to a layer</summary>
    */
    public static int ToLayer(int bitmask)
    {
        int result = bitmask > 0 ? 0 : 31;

        while (bitmask > 1)
        {
            int v = bitmask >> 1;
            bitmask = v;

            result++;
        }

        return result;
    }

    private static Collider[] sphereColliders = new Collider[MAX_ALLOC_COUNT];

    /**
    * <summary>Get all objects of a type within a specified sphere</summary>
    */
    public static T[] GetNeighborsTypeBySphere<T>(Vector3 position, float size, LayerMask layer, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
    {
        sphereColliders = new Collider[MAX_ALLOC_COUNT];
        int ColliderCount = Physics.OverlapSphereNonAlloc(position, size, sphereColliders, layer, query);

        List<T> types = new List<T>();

        for (int i = 0; i < ColliderCount; i++)
        {
            T type = sphereColliders[i].GetComponentInParent<T>();

            if (type != null)
            {
                if (type is T)
                {
                    if (!types.Contains(type))
                    {
                        types.Add(type);
                    }
                }
            }
        }

        return types.ToArray();
    }

    private static Collider[] boxColliders = new Collider[MAX_ALLOC_COUNT];

    /**
    * <summary>Get all objects of a type within a specified box</summary>
    */
    public static T[] GetNeighborsTypeByBox<T>(Vector3 position, Vector3 size, Quaternion rotation, LayerMask layer, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
    {
        bool initQueries = Physics.queriesHitTriggers;

        Physics.queriesHitTriggers = true;

        boxColliders = new Collider[MAX_ALLOC_COUNT];

        int colliderCount = Physics.OverlapBoxNonAlloc(position, size, boxColliders, rotation, layer, query);

        Physics.queriesHitTriggers = initQueries;

        List<T> types = new List<T>();

        for (int i = 0; i < colliderCount; i++)
        {
            T type = boxColliders[i].GetComponentInParent<T>();

            if (type != null)
            {
                if (type is T)
                {
                    if (!types.Contains(type))
                        types.Add(type);
                }
            }
        }

        return types.ToArray();
    }
    #endregion

    #region Math
    /**
    * <summary>Get the bounds of a child gameObject</summary>
    */
    public static Bounds GetChildsBounds(this GameObject target)
    {
        MeshRenderer[] Renders = target.GetComponentsInChildren<MeshRenderer>();

        Quaternion CurrentRotation = target.transform.rotation;

        Vector3 CurrentScale = target.transform.localScale;

        target.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        target.transform.localScale = Vector3.one;

        Bounds ResultBounds = new Bounds(target.transform.position, Vector3.zero);

        foreach (Renderer Render in Renders)
        {
            ResultBounds.Encapsulate(Render.bounds);
        }

        Vector3 RelativeCenter = ResultBounds.center - target.transform.position;

        ResultBounds.center = RelativeCenter;

        ResultBounds.size = ResultBounds.size;

        target.transform.rotation = CurrentRotation;

        target.transform.localScale = CurrentScale;

        return ResultBounds;
    }

    /**
    * <summary>Get the bounds of a parent gameObject</summary>
    */
    public static Bounds GetParentBounds(this GameObject target)
    {
        MeshRenderer[] Renders = target.GetComponents<MeshRenderer>();

        Quaternion CurrentRotation = target.transform.rotation;

        Vector3 CurrentScale = target.transform.localScale;

        target.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        target.transform.localScale = Vector3.one;

        Bounds ResultBounds = new Bounds(target.transform.position, Vector3.zero);

        foreach (Renderer Render in Renders)
        {
            ResultBounds.Encapsulate(Render.bounds);
        }

        Vector3 RelativeCenter = ResultBounds.center - target.transform.position;

        ResultBounds.center = PositionToGridPosition(0.1f, 0f, RelativeCenter);

        ResultBounds.size = PositionToGridPosition(0.1f, 0f, ResultBounds.size);

        target.transform.rotation = CurrentRotation;

        target.transform.localScale = CurrentScale;

        return ResultBounds;
    }

    /**
    * <summary>Convert bounds from local space to world space</summary>
    */
    public static Bounds ConvertBoundsToWorld(this Transform transform, Bounds localBounds)
    {
        if (transform != null)
        {
            return new Bounds(transform.TransformPoint(localBounds.center), new Vector3(localBounds.size.x * transform.localScale.x,
                localBounds.size.y * transform.localScale.y,
                localBounds.size.z * transform.localScale.z));
        }
        else
        {
            return new Bounds(localBounds.center, new Vector3(localBounds.size.x * transform.localScale.x,
                localBounds.size.y * transform.localScale.y,
                localBounds.size.z * transform.localScale.z));
        }
    }

    /**
    * <summary>Convert a float to one on a grid</summary>
    */
    public static float ConvertToGrid(float gridSize, float gridOffset, float axis)
    {
        return Mathf.Round(axis) * gridSize + gridOffset;
    }

    /**
    * <summary>Round a position to one on a grid</summary>
    */
    public static Vector3 PositionToGridPosition(float gridSize, float gridOffset, Vector3 position)
    {
        position -= Vector3.one * gridOffset;
        position /= gridSize;
        position = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z));
        position *= gridSize;
        position += Vector3.one * gridOffset;
        return position;
    }

    /**
    * <summary>Clamp a Vector3 between two other Vector3 values</summary>
    */
    public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
    {
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.y = Mathf.Clamp(value.y, min.y, max.y);
        value.z = Mathf.Clamp(value.z, min.z, max.z);
        return value;
    }
    #endregion

    #region Material
    /**
    * <summary>Change every material color in every child renderer of an object</summary>
    */
    public static void ChangeAllMaterialsColorInChildren(this GameObject go, Renderer[] renderers, Color color, float lerpTime = 0f)
    {
        Renderer[] Renderers = go.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < Renderers.Length; i++)
        {
            if (Renderers[i] != null)
            {
                for (int x = 0; x < Renderers[i].materials.Length; x++)
                {
                    if (lerpTime == 0)
                    {
                        Renderers[i].materials[x].SetColor("_BaseColor", color);
                    }
                    else
                    {
                        Renderers[i].materials[x].SetColor("_BaseColor",
                            Color.Lerp(Renderers[i].materials[x].GetColor("_BaseColor"), color, lerpTime * Time.deltaTime));
                    }
                }
            }
        }
    }

    /**
    * <summary>Change every material in every child renderer of an object</summary>
    */
    public static void ChangeAllMaterialsInChildren(this GameObject go, Renderer[] renderers, Material material)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                Material[] materials = new Material[renderers[i].sharedMaterials.Length];

                for (int x = 0; x < renderers[i].sharedMaterials.Length; x++)
                {
                    materials[x] = material;
                }

                renderers[i].sharedMaterials = materials;
            }
        }
    }

    /**
    * <summary>Change every material in every child renderer of an object using a dictionary of a material for every child</summary>
    */
    public static void ChangeAllMaterialsInChildren(this GameObject go, Renderer[] renderers, Dictionary<Renderer, Material[]> materials)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] CacheMaterials = renderers[i].sharedMaterials;

            for (int c = 0; c < CacheMaterials.Length; c++)
            {
                CacheMaterials[c] = materials[renderers[i]][c];
            }

            renderers[i].materials = CacheMaterials;
        }
    }
    #endregion

#if UNITY_EDITOR
    #region ScriptableObject 
    private class EndNameEdit : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
        }
    }

    /**
    * <summary>Create a scriptableObject asset with a specified name</summary>
    */
    public static T CreateAsset<T>(string name, bool select = true) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        string path = "Assets/" + name + ".asset";

        AssetDatabase.CreateAsset(asset, path);

        AssetDatabase.SaveAssets();

        if (select)
        {
            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;

            EditorGUIUtility.PingObject(asset);
        }

        return asset;
    }

    /**
    * <summary>Get all occurances of a scriptableObject in the project</summary>
    */
    public static T[] GetAllInstances<T>() where T : ScriptableObject
    {
        string[] Guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        T[] Result = new T[Guids.Length];

        for (int i = 0; i < Guids.Length; i++)
        {
            string Path = AssetDatabase.GUIDToAssetPath(Guids[i]);
            Result[i] = AssetDatabase.LoadAssetAtPath<T>(Path);
        }

        return Result;
    }
    #endregion
#endif

    #region List
    public enum MoveDirection
    {
        Increase,
        Decrease
    }

    /**
    * <summary>Move an item in a list by a specified amount of units in a direction</summary>
    */
    public static void Move<T>(this IList<T> list, int iIndexToMove, MoveDirection direction)
    {
        if (direction == MoveDirection.Increase)
        {
            T old = list[iIndexToMove - 1];
            list[iIndexToMove - 1] = list[iIndexToMove];
            list[iIndexToMove] = old;
        }
        else
        {
            T old = list[iIndexToMove + 1];
            list[iIndexToMove + 1] = list[iIndexToMove];
            list[iIndexToMove] = old;
        }
    }

#if UNITY_EDITOR
    /**
    * <summary>Get a list of all assets (not gameObjects!) by a type</summary>
    */
    public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
    {
        List<T> assets = new List<T>();
        string[] guids = UnityEditor.AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }
#endif
    #endregion

    #region GameObject
    /**
    * <summary>Destroy every child gameObject in a transform</summary>
    */
    public static void DeleteChildren(this Transform t)
    {
        foreach (Transform child in t) UnityEngine.Object.Destroy(child.gameObject);
    }

    /**
    * <summary>Adds a rigidbody component to a gameObject with the given attributes</summary>
    */
    public static Rigidbody AddRigibody(this GameObject target, bool useGravity, bool isKinematic, float maxDepenetrationVelocity = 15f, HideFlags flag = HideFlags.HideAndDontSave)
    {
        if (target == null)
            return null;

        if (target.GetComponent<Rigidbody>() != null)
            return target.GetComponent<Rigidbody>();

        Rigidbody Component = target.AddComponent<Rigidbody>();
        Component.maxDepenetrationVelocity = maxDepenetrationVelocity;
        Component.useGravity = useGravity;
        Component.isKinematic = isKinematic;
        Component.hideFlags = flag;

        return Component;
    }

    /**
    * <summary>Adds a sphere collider component to a gameObject with the given attributes</summary>
    */
    public static void AddSphereCollider(this GameObject target, float radius, bool isTrigger = true, HideFlags flag = HideFlags.HideAndDontSave)
    {
        if (target == null)
            return;

        if (target.GetComponent<Rigidbody>() != null)
            return;

        SphereCollider Component = target.AddComponent<SphereCollider>();
        Component.radius = radius;
        Component.isTrigger = isTrigger;
        Component.hideFlags = flag;
    }

    /**
    * <summary>Adds a boc collider component to a gameObject with the given attributes</summary>
    */
    public static void AddBoxCollider(this GameObject target, Vector3 size, Vector3 center, bool isTrigger = true, HideFlags flag = HideFlags.HideAndDontSave)
    {
        if (target == null)
            return;

        if (target.GetComponent<Rigidbody>() != null)
            return;

        BoxCollider Component = target.AddComponent<BoxCollider>();
        Component.size = size;
        Component.center = center;
        Component.isTrigger = isTrigger;
        Component.hideFlags = flag;
    }
    #endregion

    /**
    * <summary>String to int (returns errVal if failed)</summary>
    */
    public static int ToInt(this string value, int errVal = 0)
    {
        Int32.TryParse(value, out errVal);
        return errVal;
    }

    /**
    * <summary>UI SetListener extension that removes previous and then adds new listener (this version is for onClick etc.)</summary>
    */
    public static void SetListener(this UnityEvent uEvent, UnityAction call)
    {
        uEvent.RemoveAllListeners();
        uEvent.AddListener(call);
    }

    /**
    * <summary>UI SetListener extension that removes previous and then adds new listener (this version is for onEndEdit, onValueChanged etc.)</summary>
    */
    public static void SetListener<T>(this UnityEvent<T> uEvent, UnityAction<T> call)
    {
        uEvent.RemoveAllListeners();
        uEvent.AddListener(call);
    }

    /**
    * <summary>Check if a list has duplicates</summary>
    */
    public static bool HasDuplicates<T>(this List<T> list)
    {
        return list.Count != list.Distinct().Count();
    }

    /**
    * <summary>Find all duplicates in a list. Uses linq!</summary>
    */
    public static List<U> FindDuplicates<T, U>(this List<T> list, Func<T, U> keySelector)
    {
        return list.GroupBy(keySelector)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key).ToList();
    }

    /**
    * <summary>String.GetHashCode is not guaranteed to be the same on all machines, but we need one that is the same on all machines</summary>
    */
    public static int GetStableHashCode(this string text)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in text)
                hash = hash * 31 + c;
            return hash;
        }
    }

    /**
    * <summary>NavMeshAgent's ResetPath() function clears the path, but doesn't clear the velocity immediately. This is a nightmare for finite state machines because we often reset a path, then switch to casting, which would then receive a movement event because velocity still isn't 0 until a few frames later. This function truly stops all movement.</summary>
    */
    public static void ResetMovement(this NavMeshAgent agent)
    {
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }
}
#endif
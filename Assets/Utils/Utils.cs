using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

public class Utils
{

    private static readonly Dictionary<float, WaitForSeconds> WaitDictionary = new Dictionary<float, WaitForSeconds>();
    /**
    * <summary>Returns a cached WaitForSeconds given a time. Avoids unnecessary allocations for multiple new WaitForSeconds</summary>
    */
    public static WaitForSeconds GetWait(float time)
    {
        if (WaitDictionary.TryGetValue(time, out WaitForSeconds wait)) return wait;

        WaitDictionary[time] = new WaitForSeconds(time);
        return WaitDictionary[time];
    }

    private static Camera _camera;
    /**
    * <summary>Returns the cached main camera from the scene. Using Camera.main is EXPENSIVE</summary>
    */
    public static Camera PrimaryCamera
    {
        get
        {
            if (_camera == null) _camera = Camera.main;
            return _camera;
        }
    }

    /**
    * <summary>Smoothly interpolate between two curves using a time t. Make sure that both curves have the same number of keys</summary>
    */
    public static AnimationCurve CurveLerp(AnimationCurve a, AnimationCurve b, float t)
    {
        AnimationCurve tempCurve = AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f);
        List<float> times = new List<float>();
        List<float> values = new List<float>();
        Keyframe[] keys = new Keyframe[a.length];

        if (a.length == b.length)
        {
            for (int i = 0; i < a.length; i++)
            {
                times.Add(Mathf.Lerp(a.keys[i].time, b.keys[i].time, t));
                values.Add(Mathf.Lerp(a.keys[i].value, b.keys[i].value, t));
                keys[i] = new Keyframe(times[i], values[i]);
            }
            tempCurve.keys = keys;
        }

        return tempCurve;
    }

    /**
    * <summary>Get a biased random point inside a sphere whose arc is clamped at a set angle</summary>
    */
    public static Vector3 GetBiasedPointOnUnitCone(Quaternion targetDirection, float angle, float bias, float size)
    {
        float angleAsRads = BiasedRandom(0.0f, angle, bias) * Mathf.Deg2Rad;
        Vector3 randPoint = UnityEngine.Random.insideUnitCircle.normalized * Mathf.Sin(angleAsRads);
        Vector3 newVector = new Vector3(randPoint.x, randPoint.y, Mathf.Cos(angleAsRads)) * size;
        return targetDirection * newVector;
    }

    /**
    * <summary>Get a random point inside a sphere whose arc is clamped at a set angle</summary>
    */
    public static Vector3 GetPointOnUnitCone(Quaternion targetDirection, float angle, float size)
    {
        float angleAsRads = UnityEngine.Random.Range(0.0f, angle) * Mathf.Deg2Rad;
        Vector3 randPoint = UnityEngine.Random.insideUnitCircle.normalized * Mathf.Sin(angleAsRads);
        Vector3 newVector = new Vector3(randPoint.x, randPoint.y, Mathf.Cos(angleAsRads)) * size;
        return targetDirection * newVector;
    }

    /**
    * <summary>Smoothly interpolate between two gradients using a time t</summary>
    */
    public static Gradient GradientLerp(Gradient a, Gradient b, float t, bool uniformSampling = false, int keys = 64)
    {
        var myBlend = new GradientBlend(a, b, t, uniformSampling, keys); // you can omit the last two arguments to fall back to defaults

        return myBlend.blend;
    }

    private class GradientBlend
    {
        private const int MAX_KEYS = 8;
        Gradient _g1;
        public Gradient gradient1 => _g1;
        Gradient _g2;
        public Gradient gradient2 => _g2;
        Gradient _blend;
        public Gradient blend => _blend;
        public int Resolution { get; private set; }
        bool _uniform;
        public bool UniformSampling
        {
            get => _uniform;
            set => Rebuild(_g1, _g2, _blendValue, value, Resolution);
        }
        float _blendValue;
        public float CurrentBlend
        {
            get => _blendValue;
            set => UpdateBlend(value);
        }
        public Color Evaluate(float time) => _blend.Evaluate(time);
        public Color Evaluate(float time, float blend)
        {
            UpdateBlend(blend);
            return Evaluate(time);
        }
        List<float> _timeStamps;

        public GradientBlend(Gradient g1, Gradient g2, float defaultBlend = .5f, bool uniformSampling = false, int resolution = 64)
        {
            Rebuild(g1, g2, defaultBlend, uniformSampling, resolution);
        }

        public void Rebuild(Gradient g1, Gradient g2, float defaultBlend = .5f, bool uniformSampling = false, int resolution = 64)
        {
            _g1 = g1;
            _g2 = g2;
            _uniform = uniformSampling;
            Resolution = uniformSampling ? (MAX_KEYS - 1) : resolution;
            _blend = buildCombinedGradient();
            UpdateBlend(defaultBlend);
        }

        private void scanForKeysInColors(GradientColorKey[] keys, ref HashSet<int> keySet)
        {
            for (int i = 0; i < keys.Length; i++)
                keySet.Add(Mathf.FloorToInt(keys[i].time * Resolution));
        }

        private void scanForKeysInAlphas(GradientAlphaKey[] keys, ref HashSet<int> keySet)
        {
            for (int i = 0; i < keys.Length; i++)
                keySet.Add(Mathf.FloorToInt(keys[i].time * Resolution));
        }

        private List<float> buildTimeStamps(HashSet<int> keySet)
        {
            float reciprocal = 1f / Resolution;
            var list = new List<float>(keySet.Count);
            foreach (var key in keySet) { list.Add((float)key * reciprocal); }
            list.Sort();
            while (list.Count >= MAX_KEYS) list.RemoveAt(list.Count - 1);
            return list;
        }

        private Gradient buildCombinedGradient()
        {
            var keys = new HashSet<int>();

            if (!_uniform)
            {
                keys.Add(0);
                keys.Add(Resolution);

                scanForKeysInColors(_g1.colorKeys, ref keys);
                scanForKeysInColors(_g2.colorKeys, ref keys);
                scanForKeysInAlphas(_g1.alphaKeys, ref keys);
                scanForKeysInAlphas(_g2.alphaKeys, ref keys);
            }
            else
                for (int i = 0; i < MAX_KEYS; i++) keys.Add(i);

            _timeStamps = buildTimeStamps(keys);

            var gradient = new Gradient();
            gradient.SetKeys(
              new GradientColorKey[Mathf.Min(MAX_KEYS, _timeStamps.Count)],
              new GradientAlphaKey[Mathf.Min(MAX_KEYS, _timeStamps.Count)]
            );

            return gradient;
        }

        public void UpdateBlend(float blend)
        {
            var colorKeys = _blend.colorKeys;
            var alphaKeys = _blend.alphaKeys;

            for (int i = 0; i < _timeStamps.Count; i++)
            {
                var time = _timeStamps[i];
                var color = Color.Lerp(_g1.Evaluate(time), _g2.Evaluate(time), blend);
                colorKeys[i].color = new Color(color.r, color.g, color.b);
                alphaKeys[i].alpha = color.a;
                colorKeys[i].time = alphaKeys[i].time = time;
            }

            _blend.SetKeys(colorKeys, alphaKeys);
            _blendValue = blend;
        }
    }

    /**
    * <summary>Get a list of randomly offset points on a line</summary>
    */
    public static List<Vector3> GetPointsOnLineRandomized(Vector3 from, Vector3 to, int numOfPoints, float randomness)
    {
        // Divider must be between 0 and 1
        float divider = 1f / numOfPoints;
        float linear = 0f;
        List<Vector3> result = new List<Vector3>();

        if (numOfPoints == 0)
        {
            Debug.LogError("Number of points must be > 0 instead of " + numOfPoints);
            return null;
        }

        if (numOfPoints == 1)
        {
            result[0] = Vector3.Lerp(from, to, 0.5f); // Return half/middle point
            return result;
        }

        for (int i = 0; i < numOfPoints; i++)
        {
            if (i == 0)
                linear = (divider / 2);
            else
                linear += divider + (UnityEngine.Random.Range(-1f, 1f) * (randomness / 10)); // Add the divider to it to get the next distance

            // print("Loop " + i + ", is " + linear);
            result.Add(Vector3.Lerp(from, to, linear));
        }

        return result;
    }

    /**
    * <summary>Get a list of points on a line</summary>
    */
    public static List<Vector3> GetPointsOnLine(Vector3 from, Vector3 to, int numOfPoints)
    {
        // Divider must be between 0 and 1
        float divider = 1f / numOfPoints;
        float linear = 0f;
        List<Vector3> result = new List<Vector3>();

        if (numOfPoints == 0)
        {
            Debug.LogError("Number of points must be > 0 instead of " + numOfPoints);
            return null;
        }

        if (numOfPoints == 1)
        {
            result[0] = Vector3.Lerp(from, to, 0.5f); // Return half/middle point
            return result;
        }

        for (int i = 0; i < numOfPoints; i++)
        {
            if (i == 0)
                linear = (divider / 2);
            else
                linear += divider; // Add the divider to it to get the next distance

            // print("Loop " + i + ", is " + linear);
            result.Add(Vector3.Lerp(from, to, linear));
        }

        return result;
    }

    /**
    * <summary>Get a point that is a distance along a line</summary>
    */
    public static Vector3 GetPointOnLine(Vector3 x, Vector3 y, float normalizedDistance)
    {
        return x + (y - x) * normalizedDistance;
    }

    /**
    * <summary>Get a random number within a range using a bias to make certain numbers more likely</summary>
    */
    public static float BiasedRandom(float low, float high, float bias)
    {
        float r = UnityEngine.Random.Range(0f, 1f);
        r = Mathf.Pow(r, bias);
        return Mathf.Floor(low + (high - low) * r);
    }

    /**
    * <summary>Try to parse a boolean from a string, return false if it can't</summary>
    */
    public static bool TryParseBool(string stringToParse)
    {
        bool val;
        if (bool.TryParse(stringToParse, out val))
            return val;
        else
            return false;
    }

    /**
    * <summary>Try to parse a float from a string, return 0f if it can't</summary>
    */
    public static float TryParseFloat(string stringToParse)
    {
        float val;
        if (float.TryParse(stringToParse, out val))
            return val;
        else
            return 0f;
    }

    /**
    * <summary>Try to parse an integer from a string, return 0 if it can't</summary>
    */
    public static int TryParseInt(string stringToParse)
    {
        int val;
        if (int.TryParse(stringToParse, out val))
            return val;
        else
            return 0;
    }

    /**
    * <summary>Remove any numbers from a string</summary>
    */
    public static string RemoveNumbers(string text)
    {
        string newText = "";
        string number = "";
        for (int i = 0; i < text.Length; i++)
        {
            if ((text[i] < 48) || (text[i] > 57))
            { //is a char
                newText += text[i];
            }
            else
            { //is number
                number += text[i];
            }
        }
        return newText;
    }

    /**
    * <summary>Scale a number between its current min and max to a new min and max</summary>
    */
    public static float ScaleNumber(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue)
    {
        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

        return (NewValue);
    }

    /**
    * <summary>Convert a string name to a Type</summary>
    */
    public static Type StringToType(string typeAsString)
    {
        Type typeAsType = Type.GetType(typeAsString);
        return typeAsType;
    }

    /**
    * <summary>Convert a string name to an array of Objects</summary>
    */
    public static UnityEngine.Object[] FindObjectsOfTypeByName(string aClassName)
    {
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            var types = assemblies[i].GetTypes();
            for (int n = 0; n < types.Length; n++)
            {
                if (typeof(UnityEngine.Object).IsAssignableFrom(types[n]) && aClassName == types[n].Name)
                    return UnityEngine.Object.FindObjectsOfType(types[n]);
            }
        }
        return new UnityEngine.Object[0];
    }

    /**
    * <summary>Convert a quaternion from one local space to another</summary>
    */
    public static Quaternion TransformRotToOtherLocalSpace(Transform spaceTo, Transform spaceFrom, Quaternion rotationToConvert)
    {
        Quaternion converted = Utils.InverseTransformRotation(spaceTo, Utils.TransformRotation(spaceFrom, rotationToConvert));
        return converted;
    }

    /**
    * <summary>Convert a position from one local space to another</summary>
    */
    public static Vector3 TransformPosToOtherLocalSpace(Transform spaceTo, Transform spaceFrom, Vector3 positionToConvert)
    {
        Vector3 converted = spaceTo.InverseTransformPoint(spaceFrom.TransformPoint(positionToConvert));
        return converted;
    }

    /**
    * <summary>Convert a world-space quaternion to local-space using a target Transform</summary>
    */
    public static Quaternion InverseTransformRotation(Transform localTarget, Quaternion WorldRotation)
    {
        Quaternion LocalRotation = Quaternion.Inverse(localTarget.rotation) * WorldRotation;

        return LocalRotation;
    }

    /**
    * <summary>Convert a local-space quaternion to world-space using a target Transform</summary>
    */
    public static Quaternion TransformRotation(Transform worldTarget, Quaternion LocalRotation)
    {
        Quaternion WorldRotation = worldTarget.rotation * LocalRotation;
        return WorldRotation;
    }

    // Mathf.Clamp only works for float and int. we need some more versions:
    public static double Clamp(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    // is any of the keys UP?
    public static bool AnyKeyUp(KeyCode[] keys)
    {
        // avoid Linq.Any because it is HEAVY(!) on GC and performance
        foreach (KeyCode key in keys)
            if (Input.GetKeyUp(key))
                return true;
        return false;
    }

    // is any of the keys DOWN?
    public static bool AnyKeyDown(KeyCode[] keys)
    {
        // avoid Linq.Any because it is HEAVY(!) on GC and performance
        foreach (KeyCode key in keys)
            if (Input.GetKeyDown(key))
                return true;
        return false;
    }

    // is any of the keys PRESSED?
    public static bool AnyKeyPressed(KeyCode[] keys)
    {
        // avoid Linq.Any because it is HEAVY(!) on GC and performance
        foreach (KeyCode key in keys)
            if (Input.GetKey(key))
                return true;
        return false;
    }

    // is a 2D point in screen?
    // (if width = 1024, then indices from 0..1023 are valid (=1024 indices)
    public static bool IsPointInScreen(Vector2 point) =>
        0 <= point.x && point.x < Screen.width &&
        0 <= point.y && point.y < Screen.height;

    // Distance between two ClosestPoints
    // this is needed in cases where entites are really big. in those cases,
    // we can't just move to entity.transform.position, because it will be
    // unreachable. instead we have to go the closest point on the boundary.
    //
    // Vector3.Distance(a.transform.position, b.transform.position):
    //    _____        _____
    //   |     |      |     |
    //   |  x==|======|==x  |
    //   |_____|      |_____|
    //
    //
    // Utils.ClosestDistance(a.collider, b.collider):
    //    _____        _____
    //   |     |      |     |
    //   |     |x====x|     |
    //   |_____|      |_____|
    //
    // IMPORTANT: unlike uMMORPG. we use collider positions instead of animation
    //            independent transform.positions. this matters because here,
    //            the vertical distance matters too. e.g. if a player is
    //            standing higher than a monster.
    public static float ClosestDistance(Collider a, Collider b)
    {
        // return 0 if both intersect or if one is inside another.
        // ClosestPoint distance wouldn't be > 0 in those cases otherwise.
        if (a.bounds.Intersects(b.bounds))
            return 0;

        // Unity offers ClosestPointOnBounds and ClosestPoint.
        // ClosestPoint is more accurate. OnBounds often doesn't get <1 because
        // it uses a point at the top of the player collider, not in the center.
        // (use Debug.DrawLine here to see the difference)
        return Vector3.Distance(a.ClosestPoint(b.transform.position),
                                b.ClosestPoint(a.transform.position));
    }

    // CastWithout functions all need a backups dictionary. this is in hot path
    // and creating a Dictionary for every single call would be insanity.
    static Dictionary<Transform, int> castBackups = new Dictionary<Transform, int>();

    // Raycast while ignoring self (by setting layer to "Ignore Raycasts" first)
    // => setting layer to IgnoreRaycasts before casting is the easiest way to do it
    // => raycast + !=this check would still cause hit.point to be on player
    // => raycastall is not sorted and child objects might have different layers etc.
    public static bool RaycastWithout(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance, GameObject ignore, int layerMask = Physics.DefaultRaycastLayers)
    {
        // remember layers
        castBackups.Clear();

        // set all to ignore raycast
        foreach (Transform tf in ignore.GetComponentsInChildren<Transform>(true))
        {
            castBackups[tf] = tf.gameObject.layer;
            tf.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        // raycast
        bool result = Physics.Raycast(origin, direction, out hit, maxDistance, layerMask);

        // restore layers
        foreach (KeyValuePair<Transform, int> kvp in castBackups)
            kvp.Key.gameObject.layer = kvp.Value;

        return result;
    }

    public static bool SphereCastWithout(Vector3 origin, float sphereRadius, Vector3 direction, out RaycastHit hit, float maxDistance, GameObject ignore, int layerMask = Physics.DefaultRaycastLayers)
    {
        // remember layers
        castBackups.Clear();

        // set all to ignore raycast
        foreach (Transform tf in ignore.GetComponentsInChildren<Transform>(true))
        {
            castBackups[tf] = tf.gameObject.layer;
            tf.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        // raycast
        bool result = Physics.SphereCast(origin, sphereRadius, direction, out hit, maxDistance, layerMask);

        // restore layers
        foreach (KeyValuePair<Transform, int> kvp in castBackups)
            kvp.Key.gameObject.layer = kvp.Value;

        return result;
    }

    // Hard mouse scrolling that is consistent between all platforms
    //   Input.GetAxis("Mouse ScrollWheel") and
    //   Input.GetAxisRaw("Mouse ScrollWheel")
    //   both return values like 0.01 on standalone and 0.5 on WebGL, which
    //   causes too fast zooming on WebGL etc.
    // Normally GetAxisRaw should return -1,0,1, but it doesn't for scrolling
    public static float GetAxisRawScrollUniversal()
    {
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll < 0) return -1;
        if (scroll > 0) return 1;
        return 0;
    }

    // Two finger pinch detection
    // source: https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
    public static float GetPinch()
    {
        if (Input.touchCount == 2)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            return touchDeltaMag - prevTouchDeltaMag;
        }
        return 0;
    }

    // Universal zoom: mouse scroll if mouse, two finger pinching otherwise
    public static float GetZoomUniversal()
    {
        if (Input.mousePresent)
            return GetAxisRawScrollUniversal();
        else if (Input.touchSupported)
            return GetPinch();
        return 0;
    }

    // parse last upper cased noun from a string, e.g.
    //   EquipmentWeaponBow => Bow
    //   EquipmentShield => Shield
    static Regex lastNountRegEx = new Regex(@"([A-Z][a-z]*)"); // cache to avoid allocations. this is used a lot.
    public static string ParseLastNoun(string text)
    {
        MatchCollection matches = lastNountRegEx.Matches(text);
        return matches.Count > 0 ? matches[matches.Count - 1].Value : "";
    }

    // check if the cursor is over a UI or OnGUI element right now
    // note: for UI, this only works if the UI's CanvasGroup blocks Raycasts
    // note: for OnGUI: hotControl is only set while clicking, not while zooming
    public static bool IsCursorOverUserInterface()
    {
        // IsPointerOverGameObject check for left mouse (default)
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // IsPointerOverGameObject check for touches
        for (int i = 0; i < Input.touchCount; ++i)
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;

        // OnGUI check
        return GUIUtility.hotControl != 0;
    }

    // PBKDF2 hashing recommended by NIST:
    // http://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-132.pdf
    // salt should be at least 128 bits = 16 bytes
    public static string PBKDF2Hash(string text, string salt)
    {
        byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(text, saltBytes, 10000);
        byte[] hash = pbkdf2.GetBytes(20);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }

    // random point on NavMesh for item drops, etc.
    public static Vector3 RandomUnitCircleOnNavMesh(Vector3 position, float radiusMultiplier)
    {
        // random circle point
        Vector2 r = UnityEngine.Random.insideUnitCircle * radiusMultiplier;

        // convert to 3d
        Vector3 randomPosition = new Vector3(position.x + r.x, position.y, position.z + r.y);

        // raycast to find valid point on NavMesh. otherwise return original one
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, radiusMultiplier * 2, NavMesh.AllAreas))
            return hit.position;
        return position;
    }

    // random point on NavMesh that has no obstacles (walls) between point and center
    // -> useful because items shouldn't be dropped behind walls, etc.
    public static Vector3 ReachableRandomUnitCircleOnNavMesh(Vector3 position, float radiusMultiplier, int solverAttempts)
    {
        for (int i = 0; i < solverAttempts; ++i)
        {
            // get random point on navmesh around position
            Vector3 candidate = RandomUnitCircleOnNavMesh(position, radiusMultiplier);

            // check if anything obstructs the way (walls etc.)
            if (!NavMesh.Raycast(position, candidate, out NavMeshHit hit, NavMesh.AllAreas))
                return candidate;
        }

        // otherwise return original position if we can't find any good point.
        // in that case it's best to just drop it where the entity stands.
        return position;
    }

    // can Collider A 'reach' Collider B?
    // e.g. can monster reach player to attack?
    //      can player reach item to pick up?
    // => NOTE: we only try to reach the center vertical line of the collider.
    //    this is not a perfect 'is collider reachable' function that checks
    //    any point on the collider. it is perfect for monsters and players
    //    though, because they are rather vertical
    public static bool IsReachableVertically(Collider origin, Collider other, float maxDistance)
    {
        // we need to find the closest collider points first, because using
        // maxDistance for checks between collider.center points is meaningless
        // for monsters with huge colliders.
        // (we use ClosestPointOnBounds for all other attack range checks too)
        Vector3 originClosest = origin.ClosestPoint(other.transform.position);
        Vector3 otherClosest = other.ClosestPoint(origin.transform.position);

        // linecast from origin to other to decide if reachable
        // -> we cast from origin center/top to all center/top/bottom of other
        //    aka 'can origin attack any part of other with head or hands?'
        Vector3 otherCenter = new Vector3(otherClosest.x, other.bounds.center.y, otherClosest.z); // closest centered at y
        Vector3 otherTop = otherCenter + Vector3.up * other.bounds.extents.y;
        Vector3 otherBottom = otherCenter + Vector3.down * other.bounds.extents.y;

        Vector3 originCenter = new Vector3(originClosest.x, origin.bounds.center.y, originClosest.z); // origin centered at y
        Vector3 originTop = originCenter + Vector3.up * origin.bounds.extents.y;

        // maxDistance is from origin center to any other point.
        // -> it's not meant from origin head to other feet, in which case we
        //    could reach objects that are too far above us, e.g. a monster
        //    could reach a player standing on the battle bus.
        // -> in other words, the origin head checks should be reduced by size/2
        //    since they start further away from the hips
        float originHalf = origin.bounds.size.y / 2;

        // reachable if there is nothing between us and the other collider
        // -> check distance too, e.g. monsters attacking upwards
        //
        // NOTE: checking 'if nothing is between' is the way to go, because
        //       monster and player main colliders have IgnoreRaycast layers, so
        //       checking 'if linecast reaches other collider' wouldn't work.
        //       (this is also faster, since we only Linecast if dist <= ...)
        //
        // NOTE: this can be done shorter with just Linecast || Linecast || ...
        //       but color coded DrawLines are significantly(!) easier to debug!
        //
        // IMPORTANT: we do NOT have to ignore any colliders manually because
        //            the monster/player main colliders are on IgnoreRaycast
        //            layers, and all the body part colliders are triggers!
        if (Vector3.Distance(originCenter, otherCenter) <= maxDistance &&
            !Physics.Linecast(originCenter, otherCenter, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(originCenter, otherCenter, Color.white);
            return true;
        }
        else Debug.DrawLine(originCenter, otherCenter, Color.gray);

        if (Vector3.Distance(originCenter, otherTop) <= maxDistance &&
            !Physics.Linecast(originCenter, otherTop, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(originCenter, otherTop, Color.white);
            return true;
        }
        else Debug.DrawLine(originCenter, otherTop, Color.gray);

        if (Vector3.Distance(originCenter, otherBottom) <= maxDistance &&
            !Physics.Linecast(originCenter, otherBottom, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(originCenter, otherBottom, Color.white);
            return true;
        }
        else Debug.DrawLine(originCenter, otherBottom, Color.gray);

        if (Vector3.Distance(originTop, otherCenter) <= maxDistance - originHalf &&
            !Physics.Linecast(originTop, otherCenter, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(originTop, otherCenter, Color.white);
            return true;
        }
        else Debug.DrawLine(originTop, otherCenter, Color.gray);

        if (Vector3.Distance(originTop, otherTop) <= maxDistance - originHalf &&
            !Physics.Linecast(originTop, otherTop, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(originTop, otherTop, Color.white);
            return true;
        }
        else Debug.DrawLine(originTop, otherTop, Color.gray);

        if (Vector3.Distance(originTop, otherBottom) <= maxDistance - originHalf &&
            !Physics.Linecast(originTop, otherBottom, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(originTop, otherBottom, Color.white);
            return true;
        }
        else Debug.DrawLine(originTop, otherBottom, Color.gray);

        // no point was reachable
        return false;
    }

    // clamp a rotation around x axis
    // (e.g. camera up/down rotation so we can't look below character's pants etc.)
    // original source: Unity's standard assets MouseLook.cs
    public static Quaternion ClampRotationAroundXAxis(Quaternion q, float min, float max)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, min, max);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
}
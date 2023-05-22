using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spektr;
using Utilities;

[ExecuteInEditMode]
public class LightningGenerator : MonoBehaviour
{
    #region Bolt generation
    [Header("Bolt generation")]

    #region Main stuff
    [Space(5)]
    [Range(0, 100)]
    [Tooltip("Resolution of the main bolt")]
    public int MainBoltIterations = 50;
    [Range(0, 100)]
    [Tooltip("Resolution of the branch bolts")]
    public int BranchIterations = 5;
    [Range(0, 75)]
    [Tooltip("The maximum number of branches")]
    public int MaxBranches = 5;
    #endregion

    #region Main bolt
    [Header("Main bolt")]
    [Range(0, 180)]
    [Tooltip("The maximum angle for the node to be randomly offset")]
    public float NodeOffsetAngleLimit = 45;

    [Space]
    [Range(1, 5)]
    [Tooltip("The number of hints to use when offsetting the nodes")]
    public int NumberOfHints = 3;
    [Range(0, 1)]
    [Tooltip("The weight the hint will have on the curve of the bolt")]
    public float HintWeight = 0.5f;
    [Range(0, 1)]
    [Tooltip("How much the distance a node is from the hint will affect the its hint weight")]
    public float HintDistanceFalloff = 0.5f;
    [Range(0, 180)]
    [Tooltip("The maximum angle for the hint to be randomly set to")]
    public float HintRandomAngleLimit = 45;
    [Range(0, 2)]
    [Tooltip("The chance that the hint will be closer to the center of the bolt")]
    public float HintRandomBias = 0.1f;
    [Range(0.15f, 1)]
    [Tooltip("How far out the hint positions should be distributed")]
    public float HintDistributionSize = 0.1f;
    #endregion

    #region Branches
    [Header("Branches")]
    [Range(0, 180)]
    [Tooltip("The maximum angle for the branch root node to be randomly offset")]
    public float BranchRootOffsetAngleLimit = 45;
    [Range(0, 180)]
    [Tooltip("The maximum angle for the branch nodes to be randomly offset")]
    public float BranchNodeOffsetAngleLimit = 45;
    [Range(0, 5)]
    [Tooltip("How far apart each branch should be spaced")]
    public int BranchSpacing = 3;
    #endregion

    [Space(10)]
    public bool DisableAfter = true;
    #endregion

    #region End goal finding
    [Header("End goal finding")]
    public float MinXMove;
    public float MaxXMove;
    public float MinZMove;
    public float MaxZMove;
    public LayerMask LightningStrikeMask;
    public float PlayerStrikeRadius = 5;
    #endregion

    #region Sound and Light
    [Header("Sound and light")]
    public AudioClip[] ThunderNoises;
    public Light FlashLightSource;
    public int LightIntensity;
    public AnimationCurve LightFrequency;
    // public ShakePreset StrikeShake;
    public int ShakeSize = 250;
    #endregion

    #region Bolt rendering
    [Header("Bolt rendering")]
    public float FadeDuration = 0.5f;
    [Range(0, 1)]
    public float Throttle = 0.1f;
    public float PulseInterval = 0.2f;
    [Range(0, 1)]
    public float BoltLength = 0.85f;
    [Range(0, 1)]
    public float LengthRandomness = 0.8f;
    public float NoiseAmplitude = 1.2f;
    public float NoiseFrequency = 0.1f;
    public float NoiseMotion = 0.1f;
    [ColorUsageAttribute(true, true)]
    public Color BoltColor = Color.white;
    public Shader BoltShader;
    public LightningMesh BoltMesh;
    #endregion

    #region Strike explosion
    [Header("Strike explosion")]
    public float ExplosionRadius;
    public float ExplosionForce;
    public float ExplosionUpwardsForce;
    public ParticleSystem ExplosionFX;
    #endregion

    #region Debug
    [Header("Debug")]
    public bool DebugShowPaths = false;
    public bool DebugShowNodes = false;
    public bool DebugCheckGoal = true;
    public bool DoPhysicsExplosion = true;
    public int DebugGenerationSeed;
    public bool DebugConstantUpdating = false;
    public int DebugUpdateInterval = 10;
    #endregion

    #region Private
    int i = 0;
    private bool isDrawing = false;
    private Transform cam;
    private Material _material;
    private MaterialPropertyBlock _materialProps;
    private Dictionary<Vector3, Vector3> LinesToRender = new Dictionary<Vector3, Vector3>();
    private AudioClip curClip;
    private AudioSource explosionSound;
    private Vector3 goalPosition;
    private Random.State oldState;
    [SerializeField]
    private Color col;
    // private bool canPlayAudioClip = true;
    #endregion

    private void Awake()
    {
        explosionSound = transform.Find("ExplosionSound").GetComponent<AudioSource>();
        cam = Utils.PrimaryCamera.transform;
    }

    // Color oldc;
    private void Update()
    {
        if (DebugConstantUpdating)
        {
            i++;
            if (i == DebugUpdateInterval)
            {
                Generate(Random.Range(1, 3), false);
                i = 0;
            }
            else if (i > 100)
                i = 0;

            foreach (KeyValuePair<Vector3, Vector3> keyPair in LinesToRender)
                CreateBolt(keyPair.Key + transform.position, keyPair.Value + transform.position);
        }
        else if (isDrawing)
            foreach (KeyValuePair<Vector3, Vector3> keyPair in LinesToRender)
                CreateBolt(keyPair.Key + transform.position, keyPair.Value + transform.position);

        // if (BoltColor != oldc)
        // {
        //     oldc = BoltColor;
        //     _materialProps.SetColor("_Color", BoltColor);
        // }
    }

    public void Generate(int count, bool doSound = true, bool firstTime = true)
    {
        #region Values initialization
        float totalLength = Vector3.Distance(Vector3.zero, goalPosition);
        List<Vector3> nodePoints = Utils.GetPointsOnLine(Vector3.zero, goalPosition, MainBoltIterations);
        List<Vector3> hintPositions = new List<Vector3>();
        Vector3 oldPos = Vector3.zero;
        int numOfBranches = 0;
        int nodesSinceLastBranch = 0;
        LinesToRender.Clear();
        if (DebugGenerationSeed != 0)
            Random.InitState(DebugGenerationSeed);
        #endregion

        #region (Only on the first time) Find goal point
        if (firstTime)
        {
            if (DebugCheckGoal)
                goalPosition = FindGoalPoint() - transform.position;

            if (Vector3.Distance(goalPosition + transform.position, cam.position) < PlayerStrikeRadius)
            {
                goalPosition = cam.position - transform.position;

                // Kill the player
                print("Player will die because of lightning strike (placeholder)");
            }

            DrawCube(goalPosition, 1, Color.yellow);
        }
        #endregion

        #region Finding starting hint points
        for (int h = 0; h < NumberOfHints; h++)
        {
            Vector3 hintPos = nodePoints[0] + (Utils.GetBiasedPointOnUnitCone(Quaternion.LookRotation(goalPosition - nodePoints[0]), HintRandomAngleLimit, HintRandomBias, HintDistributionSize) * (totalLength / 2));
            DrawCube(hintPos, 0.5f, Color.white);
            hintPositions.Add(hintPos);
        }
        #endregion

        #region Get points from start to finish with a random offset, then randomize the positional offset of the points
        for (int e = 0; e < nodePoints.Count; e++)
        {
            // Hints
            Vector3 hintInfluence = Vector3.zero;
            foreach (Vector3 pos in hintPositions)
            {
                float dist = Vector3.Distance(pos.normalized, nodePoints[e].normalized);
                hintInfluence += pos * Mathf.Lerp((HintWeight / 5), 0, dist / HintDistanceFalloff);
            }
            nodePoints[e] += hintInfluence;

            // Randomize
            Vector3 randomOffset = Utils.GetPointOnUnitCone(Quaternion.LookRotation(goalPosition - oldPos), NodeOffsetAngleLimit, 1);
            // DrawLine(nodePoints[e], nodePoints[e] + randomOffset, Color.gray);

            nodePoints[e] += randomOffset;

            DrawCube(oldPos, 0.1f, Color.red);
            DrawLine(oldPos, nodePoints[e], Color.blue);
            LinesToRender[oldPos] = nodePoints[e];
            // CreateBolt(oldPos, nodePoints[e]);
            oldPos = nodePoints[e];

            // Branch
            int branchChance = (int)(60 / Utils.ScaleNumber(0, 100, 0, 5, MainBoltIterations)) - numOfBranches;
            if (nodesSinceLastBranch == BranchSpacing && numOfBranches < MaxBranches)
            {
                nodesSinceLastBranch = 0;
                if (branchChance >= Random.Range(0, 100))
                {
                    numOfBranches++;
                    Vector3 branchOldPos = oldPos;
                    Vector3 branchPos;
                    Vector3 branchGoal = oldPos + Utils.GetPointOnUnitCone(Quaternion.LookRotation((oldPos + Vector3.down) - oldPos), BranchRootOffsetAngleLimit, BranchIterations);
                    DrawCube(branchGoal, 0.15f, Color.green);

                    // Create a new branch
                    for (int b = 0; b < (BranchIterations - (numOfBranches / 1.5f)); b++)
                    {
                        Vector3 direction = (branchGoal - branchOldPos).normalized;
                        branchPos = branchOldPos + direction;

                        // Randomize
                        Vector3 branchRandomOffset = Utils.GetPointOnUnitCone(Quaternion.LookRotation(branchGoal - branchOldPos), BranchNodeOffsetAngleLimit, 1);
                        // DrawLine(branchPos, branchPos + branchRandomOffset, Color.black);

                        branchPos += branchRandomOffset;

                        DrawCube(branchPos, 0.1f, Color.white);
                        DrawLine(branchOldPos, branchPos, Color.magenta);
                        if (branchOldPos == oldPos)
                            LinesToRender.Add(branchOldPos + new Vector3(0.01f, 0.01f, 0.01f), branchPos);
                        else
                            LinesToRender.Add(branchOldPos, branchPos);

                        // CreateBolt(branchOldPos, branchPos);
                        branchOldPos = branchPos;
                    }
                }
            }
            else if (Random.Range(0, BranchSpacing) == 0)
                nodesSinceLastBranch++;
        }
        #endregion

        // Call the draw bolt coroutine
        StartCoroutine(Draw(count));

        #region Thunder, flash, strike explosion, and camera shake
        // if (doSound)
        // {
        //     explosionSound.transform.position = goalPosition;
        //     explosionSound.PlayOneShot(explosionSound.clip);

        //     int randAudio = Random.Range(0, ThunderNoises.Length);
        //     GetComponent<AudioSource>().clip = ThunderNoises[randAudio];
        //     curClip = ThunderNoises[randAudio];
        //     StartCoroutine(PlaySound());
        // }

        if (DoPhysicsExplosion)
        {
            Collider[] collsInRange = Physics.OverlapSphere(goalPosition, ExplosionRadius);
            foreach (Collider coll in collsInRange)
            {
                Rigidbody rb = coll.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddExplosionForce(ExplosionForce, goalPosition, ExplosionRadius, ExplosionUpwardsForce);
            }
        }

        ExplosionFX.transform.position = goalPosition;
        ExplosionFX.Play();

        StartCoroutine(FadeLight(1));

        // Camera shake
        // Shaker.ShakeAllFromPoint(goalPosition, ShakeSize, StrikeShake);
        #endregion

        #region Resetting the old seed
        if (DebugGenerationSeed != 0)
            Random.state = oldState;
        #endregion
    }

    public void CreateBolt(Vector3 p0, Vector3 p1)
    {
        if (_material == null)
        {
            _material = new Material(BoltShader);
            _material.hideFlags = HideFlags.DontSave;
            _material.enableInstancing = true;
        }

        if (_materialProps == null)
            _materialProps = new MaterialPropertyBlock();

        p0 = transform.InverseTransformPoint(p0);
        p1 = transform.InverseTransformPoint(p1);

        _materialProps.SetVector("_Point0", p0);
        _materialProps.SetVector("_Point1", p1);
        _materialProps.SetFloat("_Distance", (p1 - p0).magnitude);

        // Make orthogonal bolt axes
        var v0 = (p1 - p0).normalized;
        var v0s = Mathf.Abs(v0.y) > 0.707f ? Vector3.right : Vector3.up;
        var v1 = Vector3.Cross(v0, v0s).normalized;
        var v2 = Vector3.Cross(v0, v1);

        _materialProps.SetVector("_Axis0", v0);
        _materialProps.SetVector("_Axis1", v1);
        _materialProps.SetVector("_Axis2", v2);

        // Other bolt params
        _materialProps.SetFloat("_Throttle", Throttle);
        _materialProps.SetVector("_Interval", new Vector2(0.01f, PulseInterval - 0.01f));
        _materialProps.SetVector("_Length", new Vector2(1 - LengthRandomness, 1) * BoltLength);

        _materialProps.SetVector("_NoiseAmplitude", new Vector2(1, 0.1f) * NoiseAmplitude);
        _materialProps.SetVector("_NoiseFrequency", new Vector2(1, 10) * NoiseFrequency);
        _materialProps.SetVector("_NoiseMotion", new Vector2(1, 10) * NoiseMotion);

        _materialProps.SetColor("_Color", col);
        _materialProps.SetFloat("_Seed", BoltMesh.lineCount);

        // Draw bolt lines
        Matrix4x4[] matrix = new Matrix4x4[1];
        matrix[0] = transform.localToWorldMatrix;
        Graphics.DrawMeshInstanced(BoltMesh.sharedMesh, 0, _material, matrix, 1, _materialProps, UnityEngine.Rendering.ShadowCastingMode.Off);
    }

    private IEnumerator Draw(int numsLeft)
    {
        isDrawing = true;

        if (numsLeft > 0)
        {
            col = BoltColor;

            // if (_materialProps != null)
            _materialProps.SetColor("_Color", BoltColor);

            yield return new WaitForSeconds(Random.Range(0.05f, 0.075f));
            Generate(numsLeft - 1, false, false);
        }
        else
        {
            yield return Utils.GetWait(.5f);

            float h = 0, s = 0, v = 0;
            Color.RGBToHSV(BoltColor, out h, out s, out v);

            // Fade the bolt out
            float elapsed = 0.0f;
            while (elapsed < FadeDuration)
            {
                v = Mathf.Lerp(0.75f, 0, elapsed / FadeDuration);
                col = Color.HSVToRGB(h, s, v);

                // if (_materialProps != null)
                _materialProps.SetColor("_Color", col);
                elapsed += Time.deltaTime;

                yield return null;
            }

            isDrawing = false;
            // print("Bolt faded out");
        }
    }

    private float SoundDelay()
    {
        float speed = 331.4f / 0.6f;
        return Vector3.Distance(cam.position, goalPosition + transform.position) / speed;
    }

    private IEnumerator PlaySound()
    {
        float delay = SoundDelay();
        // print("Lightning sound delayed by: " + delay);
        yield return new WaitForSeconds(delay);
        GetComponent<AudioSource>().Play();
        if (DisableAfter)
        {
            yield return Utils.GetWait(curClip.length + 0.1f);
            this.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeLight(float duration)
    {
        FlashLightSource.intensity = LightFrequency.Evaluate(0) * LightIntensity;

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            FlashLightSource.intensity = LightFrequency.Evaluate(Mathf.Lerp(0, 1, (elapsed / duration))) * LightIntensity;
            elapsed += Time.deltaTime;
            yield return null;
        }
        FlashLightSource.intensity = LightFrequency.Evaluate(1);
    }

    private Vector3 FindGoalPoint()
    {
        Vector3 raycastStart;
        raycastStart.x = transform.position.x + Random.Range(MinXMove, MaxXMove);
        raycastStart.y = 500;
        raycastStart.z = transform.position.z + Random.Range(MinZMove, MaxZMove);

        RaycastHit hit;
        if (DebugShowNodes)
            Debug.DrawRay(raycastStart, Vector3.down * 2000, Color.magenta, 1);

        if (Physics.Raycast(raycastStart, Vector3.down, out hit, 2000, LightningStrikeMask))
            return hit.point;
        else
            return new Vector3(raycastStart.x, Random.Range(transform.position.y - 50, transform.position.y - 30), raycastStart.z);
    }

    private void DrawCube(Vector3 position, float size, Color color, float duration = 0.1f)
    {
        if (!DebugShowNodes)
            return;

        // if (GenerationSeed != 0)
        // duration = 0.01f;

        position += transform.position;

        if (!Application.isPlaying)
            duration = 1;

        Vector3 a;
        Vector3 b;

        a = new Vector3(position.x - size, position.y - size, position.z - size);
        b = new Vector3(position.x + size, position.y - size, position.z - size);
        Debug.DrawLine(a, b, color, duration);
        a = new Vector3(position.x - size, position.y - size, position.z - size);
        b = new Vector3(position.x - size, position.y + size, position.z - size);
        Debug.DrawLine(a, b, color, duration);
        a = new Vector3(position.x - size, position.y - size, position.z - size);
        b = new Vector3(position.x - size, position.y - size, position.z + size);
        Debug.DrawLine(a, b, color, duration);

        a = new Vector3(position.x + size, position.y + size, position.z + size);
        b = new Vector3(position.x + size, position.y + size, position.z - size);
        Debug.DrawLine(a, b, color, duration);
        a = new Vector3(position.x + size, position.y + size, position.z + size);
        b = new Vector3(position.x - size, position.y + size, position.z + size);
        Debug.DrawLine(a, b, color, duration);
        a = new Vector3(position.x + size, position.y + size, position.z + size);
        b = new Vector3(position.x + size, position.y - size, position.z + size);
        Debug.DrawLine(a, b, color, duration);

        a = new Vector3(position.x + size, position.y - size, position.z + size);
        b = new Vector3(position.x + size, position.y - size, position.z - size);
        Debug.DrawLine(a, b, color, duration);
        a = new Vector3(position.x + size, position.y - size, position.z + size);
        b = new Vector3(position.x - size, position.y - size, position.z + size);
        Debug.DrawLine(a, b, color, duration);

        a = new Vector3(position.x - size, position.y + size, position.z + size);
        b = new Vector3(position.x - size, position.y - size, position.z + size);
        Debug.DrawLine(a, b, color, duration);
        a = new Vector3(position.x + size, position.y + size, position.z - size);
        b = new Vector3(position.x + size, position.y - size, position.z - size);
        Debug.DrawLine(a, b, color, duration);

        a = new Vector3(position.x - size, position.y + size, position.z - size);
        b = new Vector3(position.x - size, position.y + size, position.z + size);
        Debug.DrawLine(a, b, color, duration);
        a = new Vector3(position.x - size, position.y + size, position.z - size);
        b = new Vector3(position.x + size, position.y + size, position.z - size);
        Debug.DrawLine(a, b, color, duration);
    }

    private void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.1f)
    {
        // if (GenerationSeed != 0)
        // duration = 0.01f;

        if (!Application.isPlaying)
            duration = 1;

        if (DebugShowPaths)
            Debug.DrawLine(start + transform.position, end + transform.position, color, duration);
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.green;
    //     if (goalPosition != null)
    //         Gizmos.DrawSphere(goalPosition, PlayerStrikeRadius);
    // }
}
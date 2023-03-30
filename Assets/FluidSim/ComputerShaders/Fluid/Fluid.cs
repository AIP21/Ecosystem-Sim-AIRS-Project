/* Reference
My code was originally based on: https://github.com/Scrawk/GPU-GEMS-2D-Fluid-Simulation
Nice tutorial understanding basic fluid concept: https://www.youtube.com/watch?v=iKAVRgIrUOU
Very nice tutorial for artists to understand the maths: https://shahriyarshahrabi.medium.com/gentle-introduction-to-fluid-simulation-for-programmers-and-technical-artists-7c0045c40bac
*/

using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class Fluid : MonoBehaviour
{
    public ComputeShader shader;
    public int size = 1024;
    public int solverIterations = 50;
    public Texture2D obstacleTex;

    [Header("Force Settings")]
    public float forceIntensity = 200f;
    public float forceRange = 0.01f;
    public Vector2 lastMousePosition;

    public RenderTexture densityTex;
    public RenderTexture velocityTex;
    public RenderTexture pressureTex;
    public RenderTexture divergenceTex;
    public RenderTexture testX;
    public RenderTexture testY;
    public RenderTexture testH;
    [Range(0, 6)]
    public int textureToRender = 0;

    private int dispatchSize = 0;
    private int kernelCount = 0;
    private int kernel_Init = 0;
    // private int kernel_bruh = 0;
    private int kernel_Diffusion = 0;
    private int kernel_UserInput = 0;
    private int kernel_Jacobi = 0;
    private int kernel_Advection = 0;
    private int kernel_Divergence = 0;
    private int kernel_SubtractGradient = 0;

    private RenderTexture CreateTexture(GraphicsFormat format)
    {
        RenderTexture dataTex = new RenderTexture(size, size, 24, format);
        dataTex.filterMode = FilterMode.Bilinear;
        dataTex.wrapMode = TextureWrapMode.Clamp;
        dataTex.enableRandomWrite = true;
        dataTex.Create();

        return dataTex;
    }

    private void DispatchCompute(int kernel)
    {
        shader.Dispatch(kernel, dispatchSize, dispatchSize, 1);
    }

    void Start()
    {
        // Create textures
        velocityTex = CreateTexture(GraphicsFormat.R16G16_SFloat); // float2 velocity
        densityTex = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); // float3 color, float density
        pressureTex = CreateTexture(GraphicsFormat.R16_SFloat); // float pressure
        divergenceTex = CreateTexture(GraphicsFormat.R16_SFloat); // float divergence
        testX = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); // float4 test gradient
        testY = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); // float4 test gradient
        testH = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); // float4 test gradient

        // Set shared variables for compute shader
        shader.SetInt("size", size);
        shader.SetFloat("forceIntensity", forceIntensity);
        shader.SetFloat("forceRange", forceRange);

        // Set texture for compute shader
        kernel_Init = shader.FindKernel("Kernel_Init"); kernelCount++;
        // kernel_bruh = shader.FindKernel("Kernel_bruh"); kernelCount++;
        kernel_Diffusion = shader.FindKernel("Kernel_Diffusion"); kernelCount++;
        kernel_UserInput = shader.FindKernel("Kernel_UserInput"); kernelCount++;
        kernel_Divergence = shader.FindKernel("Kernel_Divergence"); kernelCount++;
        kernel_Jacobi = shader.FindKernel("Kernel_Jacobi"); kernelCount++;
        kernel_Advection = shader.FindKernel("Kernel_Advection"); kernelCount++;
        kernel_SubtractGradient = shader.FindKernel("Kernel_SubtractGradient"); kernelCount++;
        for (int kernel = 0; kernel < kernelCount; kernel++)
        {
            /* 
			This example is not optimized, not all kernels read/write into all textures,
			but I keep it like this for the sake of convenience
			*/
            shader.SetTexture(kernel, "VelocityTex", velocityTex);
            shader.SetTexture(kernel, "DensityTex", densityTex);
            shader.SetTexture(kernel, "PressureTex", pressureTex);
            shader.SetTexture(kernel, "DivergenceTex", divergenceTex);
            shader.SetTexture(kernel, "ObstacleTex", obstacleTex);
            shader.SetTexture(kernel, "testX", testX);
            shader.SetTexture(kernel, "testY", testY);
            shader.SetTexture(kernel, "testH", testH);
        }

        // Init data texture value
        dispatchSize = Mathf.CeilToInt(size / 16);
        DispatchCompute(kernel_Init);
    }

    public bool useMouseVelocity = false;
    public float mouseVelocityMultiplier = 10f;
    public Color dyeColor;
    public bool fixedSource = false;
    public Vector2 fixedSourcePos;
    public Vector2 fixedSourceVelocity;

    [Space(10)]
    public float advectStrength = 0.99f;


    void FixedUpdate()
    {
        Vector2 mousePosition = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);

        if (fixedSource)
        {
            // Send source position
            shader.SetVector("sourcePos", fixedSourcePos);

            // Set source velocity
            shader.SetVector("sourceVelocity", fixedSourceVelocity);
        }
        else
        {
            // Send mouse position
            shader.SetVector("sourcePos", mousePosition);

            if (useMouseVelocity)
            {
                // Send mouse velocity
                Vector2 mouseVelocity = mousePosition - lastMousePosition;
                shader.SetVector("sourceVelocity", mouseVelocity * mouseVelocityMultiplier);
            }
            else
            {
                // Send fixed velocity
                shader.SetVector("sourceVelocity", fixedSourceVelocity);
            }
        }

        shader.SetFloat("_deltaTime", Time.fixedDeltaTime);
        shader.SetVector("dyeColor", dyeColor);
        shader.SetFloat("advectStrength", advectStrength);

        // Run compute shader
        // DispatchCompute(kernel_bruh);
        DispatchCompute(kernel_Diffusion);
        // DispatchCompute(kernel_Advection);
        DispatchCompute(kernel_UserInput);
        // DispatchCompute(kernel_Divergence);
        // for (int i = 0; i < solverIterations; i++)
        // {
        //     DispatchCompute(kernel_Jacobi);
        // }
        // DispatchCompute(kernel_SubtractGradient);

        //Save the previous position for velocity
        lastMousePosition = mousePosition;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        switch (textureToRender)
        {
            case 0:
                Graphics.Blit(densityTex, dst);
                break;
            case 1:
                Graphics.Blit(velocityTex, dst);
                break;
            case 2:
                Graphics.Blit(pressureTex, dst);
                break;
            case 3:
                Graphics.Blit(divergenceTex, dst);
                break;
            case 4:
                Graphics.Blit(testX, dst);
                break;
            case 5:
                Graphics.Blit(testY, dst);
                break;
            case 6:
                Graphics.Blit(testH, dst);
                break;
            default:
                Graphics.Blit(densityTex, dst);
                break;
        }
    }

    public bool showDebugTextures = false;
    void OnGUI()
    {
        if (showDebugTextures)
        {
            GUI.DrawTexture(new Rect(0, 0, 128, 128), testX, ScaleMode.ScaleToFit, false, 1.0f);
            GUI.DrawTexture(new Rect(0, 128, 128, 128), testY, ScaleMode.ScaleToFit, false, 1.0f);
            GUI.DrawTexture(new Rect(0, 128 * 2, 128, 128), obstacleTex, ScaleMode.ScaleToFit, false, 1.0f);
            GUI.DrawTexture(new Rect(0, 128 * 3, 128, 128), densityTex, ScaleMode.ScaleToFit, false, 1.0f);
            GUI.DrawTexture(new Rect(0, 128 * 4, 128, 128), velocityTex, ScaleMode.ScaleToFit, false, 1.0f);
            GUI.DrawTexture(new Rect(0, 128 * 5, 128, 128), pressureTex, ScaleMode.ScaleToFit, false, 1.0f);
            GUI.DrawTexture(new Rect(0, 128 * 6, 128, 128), divergenceTex, ScaleMode.ScaleToFit, false, 1.0f);
        }

        if (GUI.Button(new Rect(350, 50, 100, 50), "Clear"))
        {
            DispatchCompute(kernel_Init);
        }
        if (GUI.Button(new Rect(450, 50, 100, 50), "Reset"))
        {
            Start();
        }
    }
}

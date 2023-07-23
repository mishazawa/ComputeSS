using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeUAVTexture : MonoBehaviour 
{
	public ComputeShader shader;
	public Material material;

	public int size = 256;
	
	[Range(0f, 1f)]
	public float simulationSpeed = 1.0f;

	private Camera cam;

	private float simSpeed = .025f;
	private float simSpeedMax = .01f;
	private float simSpeedMin = .03f;

	private int ComputeFluid;
	private RenderTexture texture;
	private float nextUpdate;
	private int groupSize;

	private Material mat;
	private ComputeShader cs;

	void Start () 
	{
        cam = Camera.main;
		cs = Instantiate(shader);

		ComputeFluid = cs.FindKernel ("ComputeFluid");
		
		// generate 1 pixel wide texture
		texture = new RenderTexture(size, 1, 16, RenderTextureFormat.ARGBHalf);
		texture.wrapMode = TextureWrapMode.Mirror;
		texture.enableRandomWrite = true;
		texture.Create();

		mat = Instantiate<Material>(material);

		if (mat != null) {
			mat.SetTexture("_MainTex", texture);
			GetComponent<Renderer>().material = mat;
		}
		
		cs.SetVector("resolution", new Vector2(size, 1));
		cs.SetTexture (ComputeFluid, "InputBuffer", texture);
		cs.SetTexture (ComputeFluid, "OutputBuffer", texture);

		nextUpdate = Time.time;
		groupSize = Mathf.CeilToInt(size / 8f);
		simSpeed = Mathf.Lerp(simSpeedMin, simSpeedMax, simulationSpeed);
	}

	void Update()
	{
		RunSimulationStep();
	}

	void OnDestroy() {
		texture.DiscardContents();
		texture.Release();
	}

	void RunSimulationStep() {
		if (nextUpdate <= Time.time) {
			nextUpdate += Mathf.Abs(simSpeed);
		
			cs.Dispatch(ComputeFluid, groupSize, 1, 1);
		}
	}


	void OnMouseUp() {
		cs.SetVector("mouse", new Vector4(-1000.0f, -1000.0f, -1.0f, -1.0f));
	}

	void OnMouseDown()
    {
		RaycastHit hit;
		if (Input.GetMouseButton(0))
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
				Debug.Log(gameObject.name);
				cs.SetVector("mouse", new Vector4(
					hit.textureCoord.x, hit.textureCoord.y, 1.0f, 1.0f));
		}
		else
		{
			cs.SetVector("mouse", new Vector4(-1000.0f, -1000.0f, -1.0f, -1.0f));
		}
    }
}

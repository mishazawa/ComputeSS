using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeUAVTexture : MonoBehaviour 
{
	public ComputeShader shader;
	public Transform cursor;

	public int size = 256;
	
	[Range(0f, 1f)]
	public float simulationSpeed = 1.0f;

	private float simSpeed = .025f;
	private float simSpeedMax = .01f;
	private float simSpeedMin = .03f;

	private int ComputeFluid;
	private RenderTexture texture;
	private Material mat;
	private float nextUpdate;
	private int groupSize;

	void Start () 
	{

		ComputeFluid = shader.FindKernel ("ComputeFluid");
		
		// generate 1 pixel wide texture
		texture = new RenderTexture(size, 1, 16, RenderTextureFormat.ARGBHalf);
		texture.wrapMode = TextureWrapMode.Mirror;
		texture.enableRandomWrite = true;
		texture.Create();

		mat = GetComponent<Renderer>().material;

		if (mat != null) {
			mat.SetTexture("_MainTex", texture);
		}
		
		shader.SetVector("resolution", new Vector2(size, 1));
		shader.SetTexture (ComputeFluid, "InputBuffer", texture);
		shader.SetTexture (ComputeFluid, "OutputBuffer", texture);

		nextUpdate = Time.time;
		groupSize = Mathf.CeilToInt(size / 8f);
		simSpeed = Mathf.Lerp(simSpeedMin, simSpeedMax, simulationSpeed);
	}

	void Update()
	{
		RunSimulationStep();
	}

	void FixedUpdate() 
	{
		Vector2 npos = new Vector2(
			cursor.position.x / transform.localScale.x,
			cursor.position.y / transform.localScale.z
		);

		shader.SetVector("mouse", npos);
	}

	void OnDestroy() {
		texture.DiscardContents();
		texture.Release();
	}

	void RunSimulationStep() {
		if (nextUpdate <= Time.time) {
			nextUpdate += Mathf.Abs(simSpeed);
			shader.Dispatch(ComputeFluid, groupSize, 1, 1);
		}
	}
}

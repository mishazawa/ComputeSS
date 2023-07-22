using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeUAVTexture : MonoBehaviour 
{
	public ComputeShader shader;
	public Transform cursor;

	public int size = 256;
	
	[Range(0.01f, .1f)]
	public float simSpeed = 1.0f;
	

	private int ComputeFluid;
	private RenderTexture texture;
	private Material mat;
	private float nextUpdate;
	private int groupSize;

	void Start () 
	{
		ComputeFluid = shader.FindKernel ("ComputeFluid");
		
		texture = new RenderTexture(size, size, 16, RenderTextureFormat.ARGB32);
		texture.enableRandomWrite = true;
		texture.Create();

		mat = GetComponent<Renderer>().material;

		if (mat != null) {
			mat.SetTexture("_MainTex", texture);
		}
		
		shader.SetVector("resolution", new Vector2(size, size));
		shader.SetTexture (ComputeFluid, "InputBuffer", texture);
		shader.SetTexture (ComputeFluid, "OutputBuffer", texture);

		nextUpdate = Time.time;
		groupSize = Mathf.CeilToInt(size / 8f);
	}

	void Update()
	{
		if (nextUpdate <= Time.time) {
			nextUpdate += Mathf.Abs(simSpeed);
			shader.Dispatch(ComputeFluid, groupSize, groupSize, 1);
		}
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
}

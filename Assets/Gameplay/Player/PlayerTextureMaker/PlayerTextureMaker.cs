using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTextureMaker : MonoBehaviour
{
	private MeshRenderer faceMesh;
	private MeshRenderer skinMesh;
	private MeshRenderer shirtMesh;
	private MeshRenderer pantsMesh;
	private MeshRenderer hairMesh;
	private MeshRenderer shoesMesh;
	private Camera cam;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		faceMesh = transform.Find("FaceQuad").GetComponent<MeshRenderer>();
		skinMesh = transform.Find("SkinQuad").GetComponent<MeshRenderer>();
		shirtMesh = transform.Find("ShirtQuad").GetComponent<MeshRenderer>();
		pantsMesh = transform.Find("PantsQuad").GetComponent<MeshRenderer>();
		hairMesh = transform.Find("HairQuad").GetComponent<MeshRenderer>();
		shoesMesh = transform.Find("ShoesQuad").GetComponent<MeshRenderer>();
		cam = GetComponent<Camera>();

		DontDestroyOnLoad(gameObject);
	}

	/**********************************************************/
	// Interface

	public void Build(Texture2D texture)
	{
		RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = cam.targetTexture;
		cam.Render();
		texture.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
		texture.Apply();
		RenderTexture.active = currentRT;
	}

	/**********************************************************/
	// Accessors/Mutators

	public Material FaceMaterial
	{
		get
		{
			return faceMesh.material;
		}
		set
		{
			faceMesh.material = value;
		}
	}

	public Material SkinMaterial
	{
		get
		{
			return skinMesh.material;
		}
		set
		{
			skinMesh.material = value;
		}
	}

	public Material ShirtMaterial
	{
		get
		{
			return shirtMesh.material;
		}
		set
		{
			shirtMesh.material = value;
		}
	}

	public Material PantsMaterial
	{
		get
		{
			return pantsMesh.material;
		}
		set
		{
			pantsMesh.material = value;
		}
	}

	public Material HairMaterial
	{
		get
		{
			return hairMesh.material;
		}
		set
		{
			hairMesh.material = value;
		}
	}

	public Material ShoesMaterial
	{
		get
		{
			return shoesMesh.material;
		}
		set
		{
			shoesMesh.material = value;
		}
	}
}

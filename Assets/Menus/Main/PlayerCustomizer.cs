using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomizer : MonoBehaviour
{
	[SerializeField]
	private GameObject playerTextureMakerPrefab;
	[SerializeField]
	private Material[] faceMaterials;
	[SerializeField]
	private Material[] skinMaterials;
	[SerializeField]
	private Material[] hairMaterials;
	[SerializeField]
	private Material[] shirtMaterials;
	[SerializeField]
	private Material[] pantsMaterials;
	[SerializeField]
	private Material[] shoesMaterials;

	private PlayerCustomizationOptions previewOptions;
	private Texture2D previewTexture;
	private int needsRandomize;

	private SkinnedMeshRenderer previewMesh;
	private PlayerTextureMaker maker;

	/**********************************************************/
	// MonoBehaviour Interface

	private void Awake()
	{
		if (GameObject.FindGameObjectsWithTag("PlayerCustomizer").Length > 1)
		{
			Destroy(gameObject);
		}
		else
		{
			previewOptions = new PlayerCustomizationOptions();

			maker = Instantiate(playerTextureMakerPrefab).GetComponent<PlayerTextureMaker>();

			previewMesh = transform.Find("Mesh").GetComponentInChildren<SkinnedMeshRenderer>();
			previewTexture = new Texture2D(1024, 1024);
			previewMesh.material.mainTexture = previewTexture;

			DontDestroyOnLoad(gameObject);
		}
	}

	private void Update()
	{
		if (needsRandomize == 1)
		{
			Randomize();
		}
		needsRandomize++;
	}

	/**********************************************************/
	// Interface

	public void Randomize()
	{
		previewOptions.face = Random.Range(0, faceMaterials.Length);
		previewOptions.skin = Random.Range(0, skinMaterials.Length);
		previewOptions.hair = Random.Range(0, hairMaterials.Length);
		previewOptions.shirt = Random.Range(0, shirtMaterials.Length);
		previewOptions.pants = Random.Range(0, pantsMaterials.Length);
		previewOptions.shoes = Random.Range(0, shoesMaterials.Length);

		BuildPreview();
	}

	public void CreateTexture(Texture2D texture, PlayerCustomizationOptions options)
	{
		maker.FaceMaterial = faceMaterials[options.face];
		maker.SkinMaterial = skinMaterials[options.skin];
		maker.HairMaterial = skinMaterials[options.hair];
		maker.ShirtMaterial = shirtMaterials[options.shirt];
		maker.PantsMaterial = pantsMaterials[options.pants];
		maker.ShoesMaterial = shoesMaterials[options.shoes];

		maker.Build(texture);
	}

	public void NextFace()
	{
		previewOptions.face++;
		if (previewOptions.face >= faceMaterials.Length)
		{
			previewOptions.face = 0;
		}
		BuildPreview();
	}

	public void PrevFace()
	{
		previewOptions.face--;
		if (previewOptions.face < 0)
		{
			previewOptions.face = faceMaterials.Length - 1;
		}
		BuildPreview();
	}

	public void NextSkin()
	{
		previewOptions.skin++;
		if (previewOptions.skin >= skinMaterials.Length)
		{
			previewOptions.skin = 0;
		}
		BuildPreview();
	}

	public void PrevSkin()
	{
		previewOptions.skin--;
		if (previewOptions.skin < 0)
		{
			previewOptions.skin = skinMaterials.Length - 1;
		}
		BuildPreview();
	}

	public void NextHair()
	{
		previewOptions.hair++;
		if (previewOptions.hair >= hairMaterials.Length)
		{
			previewOptions.hair = 0;
		}
		BuildPreview();
	}

	public void PrevHair()
	{
		previewOptions.hair--;
		if (previewOptions.hair < 0)
		{
			previewOptions.hair = hairMaterials.Length - 1;
		}
		BuildPreview();
	}

	public void NextShirt()
	{
		previewOptions.shirt++;
		if (previewOptions.shirt >= shirtMaterials.Length)
		{
			previewOptions.shirt = 0;
		}
		BuildPreview();
	}

	public void PrevShirt()
	{
		previewOptions.shirt--;
		if (previewOptions.shirt < 0)
		{
			previewOptions.shirt = shirtMaterials.Length - 1;
		}
		BuildPreview();
	}

	public void NextPants()
	{
		previewOptions.pants++;
		if (previewOptions.pants >= pantsMaterials.Length)
		{
			previewOptions.pants = 0;
		}
		BuildPreview();
	}

	public void PrevPants()
	{
		previewOptions.pants--;
		if (previewOptions.pants < 0)
		{
			previewOptions.pants = pantsMaterials.Length - 1;
		}
		BuildPreview();
	}

	public void NextShoes()
	{
		previewOptions.shoes++;
		if (previewOptions.shoes >= shoesMaterials.Length)
		{
			previewOptions.shoes = 0;
		}
		BuildPreview();
	}

	public void PrevShoes()
	{
		previewOptions.shoes--;
		if (previewOptions.shoes < 0)
		{
			previewOptions.shoes = shoesMaterials.Length - 1;
		}
		BuildPreview();
	}

	/**********************************************************/
	// Helper Functions

	private void BuildPreview()
	{
		CreateTexture(previewTexture, previewOptions);
	}

	/**********************************************************/
	// Accessors/Mutators

	public PlayerCustomizationOptions Options
	{
		get
		{
			return previewOptions;
		}
	}
}

public struct PlayerCustomizationOptions
{
	public int face;
	public int skin;
	public int hair;
	public int shirt;
	public int pants;
	public int shoes;
}
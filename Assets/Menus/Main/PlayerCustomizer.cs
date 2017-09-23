using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomizer : MonoBehaviour
{
	[SerializeField]
	private Transform headTransform;
	[SerializeField]
	private GameObject playerTextureMakerPrefab;
	[SerializeField]
	private Material[] skinMaterials;
	[SerializeField]
	private Material[] faceMaterials;
	[SerializeField]
	private Material[] hairMaterials;
	[SerializeField]
	private GameObject[] maskPrefabs;
	[SerializeField]
	private Material[] shirtMaterials;
	[SerializeField]
	private Material[] pantsMaterials;
	[SerializeField]
	private Material[] shoesMaterials;

	private PlayerCustomizationOptions previewOptions;
	private Texture2D previewTexture;
	private GameObject mask;
	private int needsRandomize;

	private SkinnedMeshRenderer previewMesh;
	private PlayerTextureMaker maker;

	private static PlayerCustomizer instance;

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
			instance = this;

			previewOptions = new PlayerCustomizationOptions();

			maker = Instantiate(playerTextureMakerPrefab).GetComponent<PlayerTextureMaker>();

			previewMesh = transform.Find("Mesh").GetComponentInChildren<SkinnedMeshRenderer>();
			previewTexture = new Texture2D(1024, 1024);
			previewMesh.material.mainTexture = previewTexture;

			JP.Event.Register(this, "OnNextSkinButtonClick");
			JP.Event.Register(this, "OnPrevSkinButtonClick");
			JP.Event.Register(this, "OnNextFaceButtonClick");
			JP.Event.Register(this, "OnPrevFaceButtonClick");
			JP.Event.Register(this, "OnNextHairButtonClick");
			JP.Event.Register(this, "OnPrevHairButtonClick");
			JP.Event.Register(this, "OnNextMaskButtonClick");
			JP.Event.Register(this, "OnPrevMaskButtonClick");
			JP.Event.Register(this, "OnNextShirtButtonClick");
			JP.Event.Register(this, "OnPrevShirtButtonClick");
			JP.Event.Register(this, "OnNextPantsButtonClick");
			JP.Event.Register(this, "OnPrevPantsButtonClick");
			JP.Event.Register(this, "OnNextShoesButtonClick");
			JP.Event.Register(this, "OnPrevShoesButtonClick");
			JP.Event.Register(this, "OnRandomizeButtonClick");

			DontDestroyOnLoad(gameObject);
		}
	}

	private void Update()
	{
		if (needsRandomize == 1)
		{
			OnRandomizeButtonClick();
		}
		needsRandomize++;
	}

	public void OnDestroy()
	{
		JP.Event.UnregisterAll(this);
	}

	/**********************************************************/
	// Interface

	public void OnRandomizeButtonClick()
	{
		previewOptions.Face = Random.Range(0, faceMaterials.Length);
		previewOptions.Skin = Random.Range(0, skinMaterials.Length);
		previewOptions.Hair = Random.Range(0, hairMaterials.Length);
		previewOptions.Mask = Random.Range(-1, maskPrefabs.Length);
		previewOptions.Shirt = Random.Range(0, shirtMaterials.Length);
		previewOptions.Pants = Random.Range(0, pantsMaterials.Length);
		previewOptions.Shoes = Random.Range(0, shoesMaterials.Length);

		BuildPreview();
	}

	public void CreateTexture(Texture2D texture, PlayerCustomizationOptions options)
	{
		maker.FaceMaterial = faceMaterials[options.Face];
		maker.SkinMaterial = skinMaterials[options.Skin];
		maker.HairMaterial = skinMaterials[options.Hair];
		maker.ShirtMaterial = shirtMaterials[options.Shirt];
		maker.PantsMaterial = pantsMaterials[options.Pants];
		maker.ShoesMaterial = shoesMaterials[options.Shoes];

		maker.Build(texture);
	}

	public void OnNextSkinButtonClick()
	{
		previewOptions.Skin++;
		if (previewOptions.Skin >= skinMaterials.Length)
		{
			previewOptions.Skin = 0;
		}
		BuildPreview();
	}

	public void OnPrevSkinButtonClick()
	{
		previewOptions.Skin--;
		if (previewOptions.Skin < 0)
		{
			previewOptions.Skin = skinMaterials.Length - 1;
		}
		BuildPreview();
	}

	public void OnNextFaceButtonClick()
	{
		previewOptions.Face++;
		if (previewOptions.Face >= faceMaterials.Length)
		{
			previewOptions.Face = 0;
		}
		BuildPreview();
	}

	public void OnPrevFaceButtonClick()
	{
		previewOptions.Face--;
		if (previewOptions.Face < 0)
		{
			previewOptions.Face = faceMaterials.Length - 1;
		}
		BuildPreview();
	}

	public void OnNextHairButtonClick()
	{
		previewOptions.Hair++;
		if (previewOptions.Hair >= hairMaterials.Length)
		{
			previewOptions.Hair = 0;
		}
		BuildPreview();
	}

	public void OnPrevHairButtonClick()
	{
		previewOptions.Hair--;
		if (previewOptions.Hair < 0)
		{
			previewOptions.Hair = hairMaterials.Length - 1;
		}
		BuildPreview();
	}

	public void OnNextMaskButtonClick()
	{
		previewOptions.Mask++;
		if (previewOptions.Mask >= maskPrefabs.Length)
		{
			previewOptions.Mask = -1;
		}
		BuildPreview();
	}

	public void OnPrevMaskButtonClick()
	{
		previewOptions.Mask--;
		if (previewOptions.Mask < -1)
		{
			previewOptions.Mask = maskPrefabs.Length - 1;
		}
		BuildPreview();
	}

	public void OnNextShirtButtonClick()
	{
		previewOptions.Shirt++;
		if (previewOptions.Shirt >= shirtMaterials.Length)
		{
			previewOptions.Shirt = 0;
		}
		BuildPreview();
	}

	public void OnPrevShirtButtonClick()
	{
		previewOptions.Shirt--;
		if (previewOptions.Shirt < 0)
		{
			previewOptions.Shirt = shirtMaterials.Length - 1;
		}
		BuildPreview();
	}

	public void OnNextPantsButtonClick()
	{
		previewOptions.Pants++;
		if (previewOptions.Pants >= pantsMaterials.Length)
		{
			previewOptions.Pants = 0;
		}
		BuildPreview();
	}

	public void OnPrevPantsButtonClick()
	{
		previewOptions.Pants--;
		if (previewOptions.Pants < 0)
		{
			previewOptions.Pants = pantsMaterials.Length - 1;
		}
		BuildPreview();
	}

	public void OnNextShoesButtonClick()
	{
		previewOptions.Shoes++;
		if (previewOptions.Shoes >= shoesMaterials.Length)
		{
			previewOptions.Shoes = 0;
		}
		BuildPreview();
	}

	public void OnPrevShoesButtonClick()
	{
		previewOptions.Shoes--;
		if (previewOptions.Shoes < 0)
		{
			previewOptions.Shoes = shoesMaterials.Length - 1;
		}
		BuildPreview();
	}

	/**********************************************************/
	// Helper Functions

	private void BuildPreview()
	{
		CreateTexture(previewTexture, previewOptions);

		if (mask)
		{
			Destroy(mask);
		}
		if (previewOptions.Mask > -1)
		{
			mask = Instantiate(maskPrefabs[previewOptions.Mask], headTransform);
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public static PlayerCustomizationOptions Options
	{
		get
		{
			return instance.previewOptions;
		}
	}

	public static GameObject[] MaskPrefabs
	{
		get
		{
			return instance.maskPrefabs;
		}
	}
}

public struct PlayerCustomizationOptions
{
	public int Skin;
	public int Face;
	public int Hair;
	public int Mask;
	public int Shirt;
	public int Pants;
	public int Shoes;
}
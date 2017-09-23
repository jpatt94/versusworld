using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public enum PlayerTraitsType
{
	Default,
	NumTypes,
}

public enum PlayerTraitModifiersType
{
	DamageResist,
	SpeedBoost,
	DamageBoost,
	NumTypes,
	None,
}

public class GameSettings : MonoBehaviour
{
	protected GameVariantMetaData metaData;
	protected GenericGameSettings genericGameSettings;
	protected PlayerTraits[] playerTraits;
	protected PlayerTraits[] playerTraitModifiers;
	protected WeaponSettings weaponSettings;
	protected GrenadeSettings grenadeSettings;
	protected PowerUpSettings powerUpSettings;

	protected XmlDocument doc;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		metaData = new GameVariantMetaData();
		genericGameSettings = new GenericGameSettings();
		playerTraits = new PlayerTraits[(int)PlayerTraitsType.NumTypes];
		for (int i = 0; i < (int)PlayerTraitsType.NumTypes; i++)
		{
			playerTraits[i] = new PlayerTraits();
		}
		playerTraitModifiers = new PlayerTraits[(int)PlayerTraitModifiersType.NumTypes];
		for (int i = 0; i < (int)PlayerTraitModifiersType.NumTypes; i++)
		{
			playerTraitModifiers[i] = new PlayerTraits();
			playerTraitModifiers[i].SetModifierDefaults();
		}
		weaponSettings = new WeaponSettings();
		grenadeSettings = new GrenadeSettings();
		powerUpSettings = new PowerUpSettings();
	}

	/**********************************************************/
	// Interface

	public void Save(string fileName)
	{
		doc = new XmlDocument();

		XmlNode head = doc.CreateElement("GameVariant");
		doc.AppendChild(head);

		SaveMetaData(head);
		SaveGenericGameSettings(head);
		SaveGameSpecificSettings(head);
		SavePlayerTraits(head, playerTraits[(int)PlayerTraitsType.Default], "DefaultPlayerTraits");
		SaveWeaponSettings(head);
		SaveGrenadeSettings(head);
		SavePowerUpSettings(head);

		doc.Save(fileName);
		doc = null;
	}

	public void Load(string fileName)
	{
		if (!File.Exists(fileName))
		{
			Debug.LogError("Unable to find game variant: " + fileName);
			return;
		}

		LoadFromFile("GameVariants/Default/DefaultSettings.xml", "//DefaultSettings", true);
		LoadFromFile(fileName, "//GameVariant", false);

		doc = null;
	}

	public static void LoadMetaData(string fileName, GameVariantMetaData metaData)
	{
		XmlDocument doc = new XmlDocument();
		doc.Load(fileName);
		XmlNode head = doc.SelectSingleNode("//GameVariant");
		XmlNode node = head.SelectSingleNode("MetaData");

		metaData.Name = node.SelectSingleNode("Name").InnerText;
		metaData.Description = node.SelectSingleNode("Description").InnerText;
		metaData.Type = (GameType)(System.Convert.ToInt32(node.SelectSingleNode("Type").InnerText));
		metaData.CreatorName = node.SelectSingleNode("CreatorName").InnerText;
		metaData.CompatibleVersion = System.Convert.ToInt32(node.SelectSingleNode("CompatibleVersion").InnerText);
		metaData.FileName = fileName;
	}

	public void Serialize(out byte[] bytes)
	{
		NetworkWriter writer = new NetworkWriter();

		metaData.Serialize(writer);
		genericGameSettings.Serialize(writer);
		SerializeGameSpecific(writer);

		for (int i = 0; i < (int)PlayerTraitsType.NumTypes; i++)
		{
			playerTraits[i].Serialize(writer);
		}
		for (int i = 0; i < (int)PlayerTraitModifiersType.NumTypes; i++)
		{
			playerTraitModifiers[i].Serialize(writer);
		}

		weaponSettings.Serialize(writer);
		grenadeSettings.Serialize(writer);
		powerUpSettings.Serialize(writer);

		bytes = writer.ToArray();
	}

	public void Deserialize(byte[] bytes)
	{
		NetworkReader reader = new NetworkReader(bytes);

		metaData.Deserialize(reader);
		genericGameSettings.Deserialize(reader);
		DeserializeGameSpecific(reader);

		for (int i = 0; i < (int)PlayerTraitsType.NumTypes; i++)
		{
			playerTraits[i].Deserialize(reader);
		}
		for (int i = 0; i < (int)PlayerTraitModifiersType.NumTypes; i++)
		{
			playerTraitModifiers[i].Deserialize(reader);
		}

		weaponSettings.Deserialize(reader);
		grenadeSettings.Deserialize(reader);
		powerUpSettings.Deserialize(reader);
	}

	protected virtual void SerializeGameSpecific(NetworkWriter writer)
	{
	}

	protected virtual void DeserializeGameSpecific(NetworkReader reader)
	{
	}

	/**********************************************************/
	// Saving

	protected void SaveValue(XmlNode parent, string name, string value)
	{
		XmlNode n = doc.CreateElement(name);

		XmlAttribute attr = doc.CreateAttribute("Value");
		attr.Value = value;
		n.Attributes.Append(attr);

		attr = doc.CreateAttribute("UseDefault");
		attr.Value = "TRUE";
		n.Attributes.Append(attr);

		parent.AppendChild(n);
	}

	protected void SaveValue(XmlNode parent, string name, int value)
	{
		SaveValue(parent, name, value.ToString());
	}

	protected void SaveValue(XmlNode parent, string name, float value)
	{
		SaveValue(parent, name, value.ToString("0.00"));
	}

	protected void SaveValue(XmlNode parent, string name, bool value)
	{
		SaveValue(parent, name, value ? "TRUE" : "FALSE");
	}

	protected void SaveMetaData(XmlNode parent)
	{
		XmlNode meta = doc.CreateElement("MetaData");

		XmlNode n = doc.CreateElement("Name");
		n.InnerText = metaData.Name;
		meta.AppendChild(n);

		n = doc.CreateElement("Description");
		n.InnerText = metaData.Description;
		meta.AppendChild(n);

		n = doc.CreateElement("Type");
		n.InnerText = ((int)metaData.Type).ToString();
		meta.AppendChild(n);

		n = doc.CreateElement("CreatorName");
		n.InnerText = metaData.CreatorName;
		meta.AppendChild(n);

		n = doc.CreateElement("CompatibleVersion");
		n.InnerText = metaData.CompatibleVersion.ToString();
		meta.AppendChild(n);

		parent.AppendChild(meta);
	}

	protected void SaveGenericGameSettings(XmlNode parent)
	{
		XmlNode node = doc.CreateElement("Generic");

		SaveValue(node, "ScoreToWin", genericGameSettings.ScoreToWin);
		SaveValue(node, "Time", genericGameSettings.Time);
		SaveValue(node, "Teams", genericGameSettings.Teams);
		SaveValue(node, "ScorePerKill", genericGameSettings.ScorePerKill);
		SaveValue(node, "ScorePerDeath", genericGameSettings.ScorePerDeath);
		SaveValue(node, "ScorePerSuicide", genericGameSettings.ScorePerSuicide);
		SaveValue(node, "ScorePerBetrayal", genericGameSettings.ScorePerBetrayal);

		parent.AppendChild(node);
	}

	protected virtual void SaveGameSpecificSettings(XmlNode parent)
	{
	}

	protected void SavePlayerTraits(XmlNode parent, PlayerTraits playerTraits, string name)
	{
		XmlNode node = doc.CreateElement(name);

		XmlNode n = doc.CreateElement("Health");
		SaveValue(n, "Modifer", playerTraits.Health.Modifier);
		SaveValue(n, "RegenDelay", playerTraits.Health.RegenDelay);
		SaveValue(n, "RegenRate", playerTraits.Health.RegenRate);
		SaveValue(n, "DamageResistance", playerTraits.Health.DamageResistance);
		SaveValue(n, "LifeSteal", playerTraits.Health.LifeSteal);
		node.AppendChild(n);

		n = doc.CreateElement("Respawn");
		SaveValue(n, "Time", playerTraits.Respawn.Time);
		node.AppendChild(n);

		n = doc.CreateElement("Movement");
		SaveValue(n, "Speed", playerTraits.Movement.Speed);
		SaveValue(n, "Acceleration", playerTraits.Movement.Acceleration);
		SaveValue(n, "JumpHeight", playerTraits.Movement.JumpHeight);
		SaveValue(n, "AirControl", playerTraits.Movement.AirControl);
		SaveValue(n, "SprintSpeedMultiplier", playerTraits.Movement.SprintSpeedMultiplier);
		SaveValue(n, "CrouchSpeedMultiplier", playerTraits.Movement.CrouchSpeedMultiplier);
		SaveValue(n, "GravityMultiplier", playerTraits.Movement.GravityMultiplier);
		SaveValue(n, "ThrustEnabled", playerTraits.Movement.ThrustEnabled);
		SaveValue(n, "ThrustHorizontalForce", playerTraits.Movement.ThrustHorizontalForce);
		SaveValue(n, "ThrustVerticalForce", playerTraits.Movement.ThrustVerticalForce);
		SaveValue(n, "ThrustDelay", playerTraits.Movement.ThrustDelay);
		node.AppendChild(n);

		n = doc.CreateElement("Melee");
		SaveValue(n, "Damage", playerTraits.Melee.Damage);
		SaveValue(n, "Rate", playerTraits.Melee.Rate);
		node.AppendChild(n);

		n = doc.CreateElement("Weapons");
		SaveValue(n, "StartingPrimaryWeapon", (int)playerTraits.Weapons.StartingPrimaryWeapon);
		SaveValue(n, "StartingSecondaryWeapon", (int)playerTraits.Weapons.StartingSecondaryWeapon);
		SaveValue(n, "InfiniteAmmo", (int)playerTraits.Weapons.InfiniteAmmo);
		SaveValue(n, "DamageMultiplier", playerTraits.Weapons.DamageMultiplier);

		parent.AppendChild(node);
	}

	protected void SaveWeaponSettings(XmlNode parent)
	{
		XmlNode node = doc.CreateElement("Weapons");

		SaveValue(node, "SpawnOnMap", weaponSettings.SpawnOnMap);
		SaveValue(node, "DropWhenKilled", weaponSettings.DropWhenKilled);

		XmlNode n = doc.CreateElement("AssaultRifle");
		SaveValue(n, "Damage", weaponSettings.AssaultRifle.Damage);
		SaveValue(n, "HeadShotDamageMultiplier", weaponSettings.AssaultRifle.HeadShotDamageMultiplier);
		SaveValue(n, "StartingAmmo", weaponSettings.AssaultRifle.Ammo);
		SaveValue(n, "MaxAmmo", weaponSettings.AssaultRifle.MaxAmmo);
		SaveValue(n, "ClipSize", weaponSettings.AssaultRifle.ClipSize);
		SaveValue(n, "FireRate", weaponSettings.AssaultRifle.FireRate);
		SaveValue(n, "ReloadTime", weaponSettings.AssaultRifle.ReloadTime);
		SaveValue(n, "BurstAmount", weaponSettings.AssaultRifle.BurstAmount);
		SaveValue(n, "BurstRate", weaponSettings.AssaultRifle.BurstRate);
		node.AppendChild(n);

		n = doc.CreateElement("SMG");
		SaveValue(n, "Damage", weaponSettings.Smg.Damage);
		SaveValue(n, "HeadShotDamageMultiplier", weaponSettings.Smg.HeadShotDamageMultiplier);
		SaveValue(n, "StartingAmmo", weaponSettings.Smg.Ammo);
		SaveValue(n, "MaxAmmo", weaponSettings.Smg.MaxAmmo);
		SaveValue(n, "ClipSize", weaponSettings.Smg.ClipSize);
		SaveValue(n, "FireRate", weaponSettings.Smg.FireRate);
		SaveValue(n, "ReloadTime", weaponSettings.Smg.ReloadTime);
		SaveValue(n, "Spread", weaponSettings.Smg.Spread);
		SaveValue(n, "ADSSpread", weaponSettings.Smg.AimDownSightsSpread);
		node.AppendChild(n);

		n = doc.CreateElement("Sniper");
		SaveValue(n, "Damage", weaponSettings.Sniper.Damage);
		SaveValue(n, "HeadShotDamageMultiplier", weaponSettings.Sniper.HeadShotDamageMultiplier);
		SaveValue(n, "StartingAmmo", weaponSettings.Sniper.Ammo);
		SaveValue(n, "MaxAmmo", weaponSettings.Sniper.MaxAmmo);
		SaveValue(n, "ClipSize", weaponSettings.Sniper.ClipSize);
		SaveValue(n, "FireRate", weaponSettings.Sniper.FireRate);
		SaveValue(n, "ReloadTime", weaponSettings.Sniper.ReloadTime);
		node.AppendChild(n);

		n = doc.CreateElement("RocketLauncher");
		SaveValue(n, "Damage", weaponSettings.RocketLauncher.Damage);
		SaveValue(n, "Radius", weaponSettings.RocketLauncher.Radius);
		SaveValue(n, "RocketInitialSpeed", weaponSettings.RocketLauncher.InitialSpeed);
		SaveValue(n, "RocketAcceleration", weaponSettings.RocketLauncher.Acceleration);
		SaveValue(n, "StartingAmmo", weaponSettings.RocketLauncher.Ammo);
		SaveValue(n, "MaxAmmo", weaponSettings.RocketLauncher.MaxAmmo);
		SaveValue(n, "ClipSize", weaponSettings.RocketLauncher.ClipSize);
		SaveValue(n, "FireRate", weaponSettings.RocketLauncher.FireRate);
		SaveValue(n, "ReloadTime", weaponSettings.RocketLauncher.ReloadTime);
		node.AppendChild(n);

		n = doc.CreateElement("Shotgun");
		SaveValue(n, "Damage", weaponSettings.Shotgun.Damage);
		SaveValue(n, "Range", weaponSettings.Shotgun.Range);
		SaveValue(n, "PelletsPerShot", weaponSettings.Shotgun.NumPellets);
		SaveValue(n, "Spread", weaponSettings.Shotgun.Spread);
		SaveValue(n, "StartingAmmo", weaponSettings.Shotgun.Ammo);
		SaveValue(n, "MaxAmmo", weaponSettings.Shotgun.MaxAmmo);
		SaveValue(n, "ClipSize", weaponSettings.Shotgun.ClipSize);
		SaveValue(n, "FireRate", weaponSettings.Shotgun.FireRate);
		SaveValue(n, "ReloadTime", weaponSettings.Shotgun.ReloadTime);
		node.AppendChild(n);

		n = doc.CreateElement("Pistol");
		SaveValue(n, "Damage", weaponSettings.Pistol.Damage);
		SaveValue(n, "HeadShotDamageMultiplier", weaponSettings.Pistol.HeadShotDamageMultiplier);
		SaveValue(n, "StartingAmmo", weaponSettings.Pistol.Ammo);
		SaveValue(n, "MaxAmmo", weaponSettings.Pistol.MaxAmmo);
		SaveValue(n, "ClipSize", weaponSettings.Pistol.ClipSize);
		SaveValue(n, "FireRate", weaponSettings.Pistol.FireRate);
		SaveValue(n, "ReloadTime", weaponSettings.Pistol.ReloadTime);
		node.AppendChild(n);

		n = doc.CreateElement("MachineGun");
		SaveValue(n, "Damage", weaponSettings.MachineGun.Damage);
		SaveValue(n, "HeadShotDamageMultiplier", weaponSettings.MachineGun.HeadShotDamageMultiplier);
		SaveValue(n, "StartingAmmo", weaponSettings.MachineGun.Ammo);
		SaveValue(n, "MaxAmmo", weaponSettings.MachineGun.MaxAmmo);
		SaveValue(n, "ClipSize", weaponSettings.MachineGun.ClipSize);
		SaveValue(n, "FireRate", weaponSettings.MachineGun.FireRate);
		SaveValue(n, "ReloadTime", weaponSettings.MachineGun.ReloadTime);
		SaveValue(n, "Spread", weaponSettings.MachineGun.Spread);
		SaveValue(n, "ADSSpread", weaponSettings.MachineGun.AimDownSightsSpread);
		node.AppendChild(n);

		parent.AppendChild(node);
	}

	protected void SaveGrenadeSettings(XmlNode parent)
	{
		XmlNode node = doc.CreateElement("Grenades");

		SaveValue(node, "ThrowRate", grenadeSettings.ThrowRate);
		SaveValue(node, "MinThrowForce", grenadeSettings.MinThrowForce);
		SaveValue(node, "MaxThrowForce", grenadeSettings.MaxThrowForce);
		SaveValue(node, "DropWhenKilled", grenadeSettings.DropWhenKilled);

		XmlNode n = doc.CreateElement("Frag");
		SaveValue(n, "StartingAmount", grenadeSettings.Quantities[(int)GrenadeType.Frag].StartingAmount);
		SaveValue(n, "MaxAmount", grenadeSettings.Quantities[(int)GrenadeType.Frag].MaxAmount);
		SaveValue(n, "Damage", grenadeSettings.Frag.Damage);
		SaveValue(n, "Range", grenadeSettings.Frag.Range);
		SaveValue(n, "FuseTime", grenadeSettings.Frag.FuseTime);
		SaveValue(n, "NoContactFuseTime", grenadeSettings.Frag.NoContactFuseTime);
		node.AppendChild(n);

		n = doc.CreateElement("Tesla");
		SaveValue(n, "StartingAmount", grenadeSettings.Quantities[(int)GrenadeType.Tesla].StartingAmount);
		SaveValue(n, "MaxAmount", grenadeSettings.Quantities[(int)GrenadeType.Tesla].MaxAmount);
		SaveValue(n, "DamagePerSecond", grenadeSettings.Tesla.DamagePerSecond);
		SaveValue(n, "Range", grenadeSettings.Tesla.Range);
		SaveValue(n, "FuseTime", grenadeSettings.Tesla.FuseTime);
		SaveValue(n, "SparkTime", grenadeSettings.Tesla.SparkTime);
		SaveValue(n, "ExplosiveDamage", grenadeSettings.Tesla.ExplosiveDamage);
		SaveValue(n, "ExplosiveRange", grenadeSettings.Tesla.ExplosiveRange);
		SaveValue(n, "IgnitedOnCollision", grenadeSettings.Tesla.IgnitedOnCollision);
		node.AppendChild(n);

		n = doc.CreateElement("Spike");
		SaveValue(n, "StartingAmount", grenadeSettings.Quantities[(int)GrenadeType.Spike].StartingAmount);
		SaveValue(n, "MaxAmount", grenadeSettings.Quantities[(int)GrenadeType.Spike].MaxAmount);
		SaveValue(n, "Damage", grenadeSettings.Spike.Damage);
		SaveValue(n, "Range", grenadeSettings.Spike.Range);
		SaveValue(n, "FuseTime", grenadeSettings.Spike.FuseTime);
		SaveValue(n, "StickDamage", grenadeSettings.Spike.StickDamage);
		node.AppendChild(n);

		parent.AppendChild(node);
	}

	protected void SavePowerUpSettings(XmlNode parent)
	{
		XmlNode node = doc.CreateElement("Items");

		SaveValue(node, "RespawnTime", powerUpSettings.RespawnTime);
		SaveValue(node, "SpinnerDuration", powerUpSettings.SpinnerDuration);
		SaveValue(node, "SpawnOnMap", powerUpSettings.SpawnOnMap);

		XmlNode n = doc.CreateElement("AmmoRefill");
		SaveValue(n, "Weight", powerUpSettings.PowerUpWeights[(int)PowerUpType.AmmoRefill]);
		node.AppendChild(n);

		n = doc.CreateElement("PowerPlay");
		SaveValue(n, "Weight", powerUpSettings.PowerUpWeights[(int)PowerUpType.PowerPlay]);
		node.AppendChild(n);

		n = doc.CreateElement("TeslaGrenade");
		SaveValue(n, "Weight", powerUpSettings.PowerUpWeights[(int)PowerUpType.TeslaGrenade]);
		node.AppendChild(n);

		n = doc.CreateElement("SpikeGrenade");
		SaveValue(n, "Weight", powerUpSettings.PowerUpWeights[(int)PowerUpType.SpikeGrenade]);
		node.AppendChild(n);

		n = doc.CreateElement("GrenadeCloud");
		SaveValue(n, "Weight", powerUpSettings.PowerUpWeights[(int)PowerUpType.GrenadeCloud]);
		SaveValue(n, "Size", powerUpSettings.GrenadeCloud.Size);
		SaveValue(n, "MinTimeBetweenSpawns", powerUpSettings.GrenadeCloud.MinimumSpawnTime);
		SaveValue(n, "MaxTimeBetweenSpawns", powerUpSettings.GrenadeCloud.MaximumSpawnTime);
		SaveValue(n, "GrenadeType", (int)powerUpSettings.GrenadeCloud.GrenadeType);
		SaveValue(n, "IntroDuration", powerUpSettings.GrenadeCloud.IntroDuration);
		SaveValue(n, "Speed", powerUpSettings.GrenadeCloud.Speed);
		SaveValue(n, "GrenadeFuseTime", powerUpSettings.GrenadeCloud.GrenadeFuseTime);
		node.AppendChild(n);

		n = doc.CreateElement("BigHeads");
		SaveValue(n, "Weight", powerUpSettings.PowerUpWeights[(int)PowerUpType.BigHeads]);
		SaveValue(n, "Scale", powerUpSettings.BigHeads.Scale);
		SaveValue(n, "Duration", powerUpSettings.BigHeads.Duration);
		node.AppendChild(n);

		n = doc.CreateElement("DamageResist");
		SaveValue(n, "Weight", powerUpSettings.PowerUpWeights[(int)PowerUpType.DamageResist]);
		SaveValue(n, "Duration", powerUpSettings.DamageResistDuration);
		node.AppendChild(n);

		n = doc.CreateElement("SpeedBoost");
		SaveValue(n, "Weight", powerUpSettings.PowerUpWeights[(int)PowerUpType.SpeedBoost]);
		SaveValue(n, "Duration", powerUpSettings.SpeedBoostDuration);
		node.AppendChild(n);

		n = doc.CreateElement("DamageBoost");
		SaveValue(n, "Weight", powerUpSettings.PowerUpWeights[(int)PowerUpType.DamageBoost]);
		SaveValue(n, "Duration", powerUpSettings.DamageBoostDuration);
		node.AppendChild(n);

		parent.AppendChild(node);
	}

	/**********************************************************/
	// Loading

	protected void LoadFromFile(string fileName, string headName, bool isDefault)
	{
		doc = new XmlDocument();
		doc.Load(fileName);
		XmlNode head = doc.SelectSingleNode(headName);

		LoadMetaData(head);
		LoadGenericGameSettings(head);
		LoadGameSpecificSettings(head);

		LoadPlayerTraits(head, playerTraits[(int)PlayerTraitsType.Default], "DefaultPlayerTraits");
		if (isDefault)
		{
			for (PlayerTraitsType t = PlayerTraitsType.Default + 1; t < PlayerTraitsType.NumTypes; t++)
			{
				playerTraits[(int)t].Clone(playerTraits[(int)PlayerTraitsType.Default]);
			}
		}

		LoadWeaponSettings(head);
		LoadGrenadeSettings(head);
		LoadPowerUpSettings(head);
	}

	protected void LoadValue(XmlNode parent, string name, ref string value)
	{
		XmlNode node = parent.SelectSingleNode(name);
		if (node != null && (node.Attributes["UseDefault"] == null || node.Attributes["UseDefault"].Value == "FALSE"))
		{
			value = parent.SelectSingleNode(name).Attributes["Value"].Value;
		}
	}

	protected void LoadValue(XmlNode parent, string name, ref int value)
	{
		string v = value.ToString();
		LoadValue(parent, name, ref v);
		value = System.Convert.ToInt32(v);
	}

	protected void LoadValue(XmlNode parent, string name, ref float value)
	{
		string v = value.ToString();
		LoadValue(parent, name, ref v);
		value = (float)System.Convert.ToDouble(v);
	}

	protected void LoadValue(XmlNode parent, string name, ref bool value)
	{
		string v = value ? "TRUE" : "FALSE";
		LoadValue(parent, name, ref v);
		value = (v == "TRUE");
	}

	protected void LoadValue(XmlNode parent, string name, ref WeaponType value)
	{
		int v = (int)value;
		LoadValue(parent, name, ref v);
		value = v > -1 ? (WeaponType)v : WeaponType.None;
	}

	protected void LoadValue(XmlNode parent, string name, ref GrenadeType value)
	{
		int v = (int)value;
		LoadValue(parent, name, ref v);
		value = (GrenadeType)v;
	}

	protected void LoadValue(XmlNode parent, string name, ref InfiniteAmmoType value)
	{
		int v = (int)value;
		LoadValue(parent, name, ref v);
		value = (InfiniteAmmoType)v;
	}

	protected void LoadMetaData(XmlNode parent)
	{
		XmlNode node = parent.SelectSingleNode("MetaData");
		if (node == null)
		{
			return;
		}

		metaData.Name = node.SelectSingleNode("Name").InnerText;
		metaData.Description = node.SelectSingleNode("Description").InnerText;
		metaData.Type = (GameType)(System.Convert.ToInt32(node.SelectSingleNode("Type").InnerText));
		metaData.CreatorName = node.SelectSingleNode("CreatorName").InnerText;
		metaData.CompatibleVersion = System.Convert.ToInt32(node.SelectSingleNode("CompatibleVersion").InnerText);
	}

	protected void LoadGenericGameSettings(XmlNode parent)
	{
		XmlNode node = parent.SelectSingleNode("Generic");
		if (node == null)
		{
			return;
		}

		LoadValue(node, "ScoreToWin", ref genericGameSettings.ScoreToWin);
		LoadValue(node, "Time", ref genericGameSettings.Time);
		LoadValue(node, "Teams", ref genericGameSettings.Teams);
		LoadValue(node, "ScorePerKill", ref genericGameSettings.ScorePerKill);
		LoadValue(node, "ScorePerDeath", ref genericGameSettings.ScorePerDeath);
		LoadValue(node, "ScorePerSuicide", ref genericGameSettings.ScorePerSuicide);
		LoadValue(node, "ScorePerBetrayal", ref genericGameSettings.ScorePerBetrayal);
	}

	protected virtual void LoadGameSpecificSettings(XmlNode parent)
	{
	}

	protected void LoadPlayerTraits(XmlNode parent, PlayerTraits playerTraits, string name)
	{
		XmlNode node = parent.SelectSingleNode(name);
		if (node == null)
		{
			return;
		}

		XmlNode n = node.SelectSingleNode("Health");
		if (n != null)
		{
			LoadValue(n, "Modifer", ref playerTraits.Health.Modifier);
			LoadValue(n, "RegenDelay", ref playerTraits.Health.RegenDelay);
			LoadValue(n, "RegenRate", ref playerTraits.Health.RegenRate);
			LoadValue(n, "DamageResistance", ref playerTraits.Health.DamageResistance);
			LoadValue(n, "LifeSteal", ref playerTraits.Health.LifeSteal);
		}

		n = node.SelectSingleNode("Respawn");
		if (n != null)
		{
			LoadValue(n, "Time", ref playerTraits.Respawn.Time);
		}

		n = node.SelectSingleNode("Movement");
		if (n != null)
		{
			LoadValue(n, "Speed", ref playerTraits.Movement.Speed);
			LoadValue(n, "Acceleration", ref playerTraits.Movement.Acceleration);
			LoadValue(n, "JumpHeight", ref playerTraits.Movement.JumpHeight);
			LoadValue(n, "AirControl", ref playerTraits.Movement.AirControl);
			LoadValue(n, "SprintSpeedMultiplier", ref playerTraits.Movement.SprintSpeedMultiplier);
			LoadValue(n, "CrouchSpeedMultiplier", ref playerTraits.Movement.CrouchSpeedMultiplier);
			LoadValue(n, "GravityMultiplier", ref playerTraits.Movement.GravityMultiplier);
			LoadValue(n, "ThrustEnabled", ref playerTraits.Movement.ThrustEnabled);
			LoadValue(n, "ThrustHorizontalForce", ref playerTraits.Movement.ThrustHorizontalForce);
			LoadValue(n, "ThrustVerticalForce", ref playerTraits.Movement.ThrustVerticalForce);
			LoadValue(n, "ThrustDelay", ref playerTraits.Movement.ThrustDelay);
		}

		n = node.SelectSingleNode("Melee");
		if (n != null)
		{
			LoadValue(n, "Damage", ref playerTraits.Melee.Damage);
			LoadValue(n, "Rate", ref playerTraits.Melee.Rate);
		}

		n = node.SelectSingleNode("Weapons");
		if (n != null)
		{
			LoadValue(n, "StartingPrimaryWeapon", ref playerTraits.Weapons.StartingPrimaryWeapon);
			LoadValue(n, "StartingSecondaryWeapon", ref playerTraits.Weapons.StartingSecondaryWeapon);
			LoadValue(n, "InfiniteAmmo", ref playerTraits.Weapons.InfiniteAmmo);
			LoadValue(n, "DamageMultiplier", ref playerTraits.Weapons.DamageMultiplier);
		}

		n = node.SelectSingleNode("FriendlyNameTags");
		if (n != null)
		{

		}
	}

	protected void LoadWeaponSettings(XmlNode parent)
	{
		XmlNode node = parent.SelectSingleNode("Weapons");
		if (node == null)
		{
			return;
		}

		LoadValue(node, "SpawnOnMap", ref weaponSettings.SpawnOnMap);
		LoadValue(node, "DropWhenKilled", ref weaponSettings.DropWhenKilled);

		XmlNode n = node.SelectSingleNode("AssaultRifle");
		if (n != null)
		{
			LoadValue(n, "Damage", ref weaponSettings.AssaultRifle.Damage);
			LoadValue(n, "HeadShotDamageMultiplier", ref weaponSettings.AssaultRifle.HeadShotDamageMultiplier);
			LoadValue(n, "StartingAmmo", ref weaponSettings.AssaultRifle.Ammo);
			LoadValue(n, "MaxAmmo", ref weaponSettings.AssaultRifle.MaxAmmo);
			LoadValue(n, "ClipSize", ref weaponSettings.AssaultRifle.ClipSize);
			LoadValue(n, "FireRate", ref weaponSettings.AssaultRifle.FireRate);
			LoadValue(n, "ReloadTime", ref weaponSettings.AssaultRifle.ReloadTime);
			LoadValue(n, "BurstAmount", ref weaponSettings.AssaultRifle.BurstAmount);
			LoadValue(n, "BurstRate", ref weaponSettings.AssaultRifle.BurstRate);
		}

		n = node.SelectSingleNode("SMG");
		if (n != null)
		{
			LoadValue(n, "Damage", ref weaponSettings.Smg.Damage);
			LoadValue(n, "HeadShotDamageMultiplier", ref weaponSettings.Smg.HeadShotDamageMultiplier);
			LoadValue(n, "StartingAmmo", ref weaponSettings.Smg.Ammo);
			LoadValue(n, "MaxAmmo", ref weaponSettings.Smg.MaxAmmo);
			LoadValue(n, "ClipSize", ref weaponSettings.Smg.ClipSize);
			LoadValue(n, "FireRate", ref weaponSettings.Smg.FireRate);
			LoadValue(n, "ReloadTime", ref weaponSettings.Smg.ReloadTime);
			LoadValue(n, "Spread", ref weaponSettings.Smg.Spread);
			LoadValue(n, "ADSSpread", ref weaponSettings.Smg.AimDownSightsSpread);
		}

		n = node.SelectSingleNode("Sniper");
		if (n != null)
		{
			LoadValue(n, "Damage", ref weaponSettings.Sniper.Damage);
			LoadValue(n, "HeadShotDamageMultiplier", ref weaponSettings.Sniper.HeadShotDamageMultiplier);
			LoadValue(n, "StartingAmmo", ref weaponSettings.Sniper.Ammo);
			LoadValue(n, "MaxAmmo", ref weaponSettings.Sniper.MaxAmmo);
			LoadValue(n, "ClipSize", ref weaponSettings.Sniper.ClipSize);
			LoadValue(n, "FireRate", ref weaponSettings.Sniper.FireRate);
			LoadValue(n, "ReloadTime", ref weaponSettings.Sniper.ReloadTime);
		}

		n = node.SelectSingleNode("RocketLauncher");
		if (n != null)
		{
			LoadValue(n, "Damage", ref weaponSettings.RocketLauncher.Damage);
			LoadValue(n, "Radius", ref weaponSettings.RocketLauncher.Radius);
			LoadValue(n, "RocketInitialSpeed", ref weaponSettings.RocketLauncher.InitialSpeed);
			LoadValue(n, "RocketAcceleration", ref weaponSettings.RocketLauncher.Acceleration);
			LoadValue(n, "StartingAmmo", ref weaponSettings.RocketLauncher.Ammo);
			LoadValue(n, "MaxAmmo", ref weaponSettings.RocketLauncher.MaxAmmo);
			LoadValue(n, "ClipSize", ref weaponSettings.RocketLauncher.ClipSize);
			LoadValue(n, "FireRate", ref weaponSettings.RocketLauncher.FireRate);
			LoadValue(n, "ReloadTime", ref weaponSettings.RocketLauncher.ReloadTime);
		}

		n = node.SelectSingleNode("Shotgun");
		if (n != null)
		{
			LoadValue(n, "Damage", ref weaponSettings.Shotgun.Damage);
			LoadValue(n, "Range", ref weaponSettings.Shotgun.Range);
			LoadValue(n, "PelletsPerShot", ref weaponSettings.Shotgun.NumPellets);
			LoadValue(n, "Spread", ref weaponSettings.Shotgun.Spread);
			LoadValue(n, "StartingAmmo", ref weaponSettings.Shotgun.Ammo);
			LoadValue(n, "MaxAmmo", ref weaponSettings.Shotgun.MaxAmmo);
			LoadValue(n, "ClipSize", ref weaponSettings.Shotgun.ClipSize);
			LoadValue(n, "FireRate", ref weaponSettings.Shotgun.FireRate);
			LoadValue(n, "ReloadTime", ref weaponSettings.Shotgun.ReloadTime);
		}

		n = node.SelectSingleNode("Pistol");
		if (n != null)
		{
			LoadValue(n, "Damage", ref weaponSettings.Pistol.Damage);
			LoadValue(n, "HeadShotDamageMultiplier", ref weaponSettings.Pistol.HeadShotDamageMultiplier);
			LoadValue(n, "StartingAmmo", ref weaponSettings.Pistol.Ammo);
			LoadValue(n, "MaxAmmo", ref weaponSettings.Pistol.MaxAmmo);
			LoadValue(n, "ClipSize", ref weaponSettings.Pistol.ClipSize);
			LoadValue(n, "FireRate", ref weaponSettings.Pistol.FireRate);
			LoadValue(n, "ReloadTime", ref weaponSettings.Pistol.ReloadTime);
		}

		n = node.SelectSingleNode("MachineGun");
		if (n != null)
		{
			LoadValue(n, "Damage", ref weaponSettings.MachineGun.Damage);
			LoadValue(n, "HeadShotDamageMultiplier", ref weaponSettings.MachineGun.HeadShotDamageMultiplier);
			LoadValue(n, "StartingAmmo", ref weaponSettings.MachineGun.Ammo);
			LoadValue(n, "MaxAmmo", ref weaponSettings.MachineGun.MaxAmmo);
			LoadValue(n, "ClipSize", ref weaponSettings.MachineGun.ClipSize);
			LoadValue(n, "FireRate", ref weaponSettings.MachineGun.FireRate);
			LoadValue(n, "ReloadTime", ref weaponSettings.MachineGun.ReloadTime);
			LoadValue(n, "Spread", ref weaponSettings.MachineGun.Spread);
			LoadValue(n, "ADSSpread", ref weaponSettings.MachineGun.AimDownSightsSpread);
		}
	}

	protected void LoadGrenadeSettings(XmlNode parent)
	{
		XmlNode node = parent.SelectSingleNode("Grenades");
		if (node == null)
		{
			return;
		}

		LoadValue(node, "ThrowRate", ref grenadeSettings.ThrowRate);
		LoadValue(node, "MinThrowForce", ref grenadeSettings.MinThrowForce);
		LoadValue(node, "MaxThrowForce", ref grenadeSettings.MaxThrowForce);
		LoadValue(node, "DropWhenKilled", ref grenadeSettings.DropWhenKilled);

		XmlNode n = node.SelectSingleNode("Frag");
		if (n != null)
		{
			LoadValue(n, "StartingAmount", ref grenadeSettings.Quantities[(int)GrenadeType.Frag].StartingAmount);
			LoadValue(n, "MaxAmount", ref grenadeSettings.Quantities[(int)GrenadeType.Frag].MaxAmount);
			LoadValue(n, "Damage", ref grenadeSettings.Frag.Damage);
			LoadValue(n, "Range", ref grenadeSettings.Frag.Range);
			LoadValue(n, "FuseTime", ref grenadeSettings.Frag.FuseTime);
			LoadValue(n, "NoContactFuseTime", ref grenadeSettings.Frag.NoContactFuseTime);
		}

		n = node.SelectSingleNode("Tesla");
		if (n != null)
		{
			LoadValue(n, "StartingAmount", ref grenadeSettings.Quantities[(int)GrenadeType.Tesla].StartingAmount);
			LoadValue(n, "MaxAmount", ref grenadeSettings.Quantities[(int)GrenadeType.Tesla].MaxAmount);
			LoadValue(n, "DamagePerSecond", ref grenadeSettings.Tesla.DamagePerSecond);
			LoadValue(n, "Range", ref grenadeSettings.Tesla.Range);
			LoadValue(n, "FuseTime", ref grenadeSettings.Tesla.FuseTime);
			LoadValue(n, "SparkTime", ref grenadeSettings.Tesla.SparkTime);
			LoadValue(n, "ExplosiveDamage", ref grenadeSettings.Tesla.ExplosiveDamage);
			LoadValue(n, "ExplosiveRange", ref grenadeSettings.Tesla.ExplosiveRange);
			LoadValue(n, "IgnitedOnCollision", ref grenadeSettings.Tesla.IgnitedOnCollision);
		}

		n = node.SelectSingleNode("Spike");
		if (n != null)
		{
			LoadValue(n, "StartingAmount", ref grenadeSettings.Quantities[(int)GrenadeType.Spike].StartingAmount);
			LoadValue(n, "MaxAmount", ref grenadeSettings.Quantities[(int)GrenadeType.Spike].MaxAmount);
			LoadValue(n, "Damage", ref grenadeSettings.Spike.Damage);
			LoadValue(n, "Range", ref grenadeSettings.Spike.Range);
			LoadValue(n, "FuseTime", ref grenadeSettings.Spike.FuseTime);
			LoadValue(n, "StickDamage", ref grenadeSettings.Spike.StickDamage);
		}

		n = node.SelectSingleNode("RogiBall");
		if (n != null)
		{
			LoadValue(n, "StartingAmount", ref grenadeSettings.Quantities[(int)GrenadeType.RogiBall].StartingAmount);
			LoadValue(n, "MaxAmount", ref grenadeSettings.Quantities[(int)GrenadeType.RogiBall].MaxAmount);
		}
	}

	protected void LoadPowerUpSettings(XmlNode parent)
	{
		XmlNode node = parent.SelectSingleNode("Items");
		if (node == null)
		{
			return;
		}

		LoadValue(node, "RespawnTime", ref powerUpSettings.RespawnTime);
		LoadValue(node, "SpinnerDuration", ref powerUpSettings.SpinnerDuration);
		LoadValue(node, "SpawnOnMap", ref powerUpSettings.SpawnOnMap);

		XmlNode n = node.SelectSingleNode("AmmoRefill");
		if (n != null)
		{
			LoadValue(n, "Weight", ref powerUpSettings.PowerUpWeights[(int)PowerUpType.AmmoRefill]);
		}

		n = node.SelectSingleNode("PowerPlay");
		if (n != null)
		{
			LoadValue(n, "Weight", ref powerUpSettings.PowerUpWeights[(int)PowerUpType.PowerPlay]);
		}

		n = node.SelectSingleNode("TeslaGrenade");
		if (n != null)
		{
			LoadValue(n, "Weight", ref powerUpSettings.PowerUpWeights[(int)PowerUpType.TeslaGrenade]);
		}

		n = node.SelectSingleNode("SpikeGrenade");
		if (n != null)
		{
			LoadValue(n, "Weight", ref powerUpSettings.PowerUpWeights[(int)PowerUpType.SpikeGrenade]);
		}

		n = node.SelectSingleNode("GrenadeCloud");
		if (n != null)
		{
			LoadValue(n, "Weight", ref powerUpSettings.PowerUpWeights[(int)PowerUpType.GrenadeCloud]);
			LoadValue(n, "Size", ref powerUpSettings.GrenadeCloud.Size);
			LoadValue(n, "MinTimeBetweenSpawns", ref powerUpSettings.GrenadeCloud.MinimumSpawnTime);
			LoadValue(n, "MaxTimeBetweenSpawns", ref powerUpSettings.GrenadeCloud.MaximumSpawnTime);
			LoadValue(n, "GrenadeType", ref powerUpSettings.GrenadeCloud.GrenadeType);
			LoadValue(n, "IntroDuration", ref powerUpSettings.GrenadeCloud.IntroDuration);
			LoadValue(n, "Speed", ref powerUpSettings.GrenadeCloud.Speed);
			LoadValue(n, "GrenadeFuseTime", ref powerUpSettings.GrenadeCloud.GrenadeFuseTime);
		}

		n = node.SelectSingleNode("BigHeads");
		if (n != null)
		{
			LoadValue(n, "Weight", ref powerUpSettings.PowerUpWeights[(int)PowerUpType.BigHeads]);
			LoadValue(n, "Scale", ref powerUpSettings.BigHeads.Scale);
			LoadValue(n, "Duration", ref powerUpSettings.BigHeads.Duration);
		}

		n = node.SelectSingleNode("DamageResist");
		if (n != null)
		{
			LoadValue(n, "Weight", ref powerUpSettings.PowerUpWeights[(int)PowerUpType.DamageResist]);
			LoadValue(n, "Duration", ref powerUpSettings.DamageResistDuration);
			LoadPlayerTraits(n, playerTraitModifiers[(int)PlayerTraitModifiersType.DamageResist], "TraitModifiers");
		}

		n = node.SelectSingleNode("SpeedBoost");
		if (n != null)
		{
			LoadValue(n, "Weight", ref powerUpSettings.PowerUpWeights[(int)PowerUpType.SpeedBoost]);
			LoadValue(n, "Duration", ref powerUpSettings.SpeedBoostDuration);
			LoadPlayerTraits(n, playerTraitModifiers[(int)PlayerTraitModifiersType.SpeedBoost], "TraitModifiers");
		}

		n = node.SelectSingleNode("DamageBoost");
		if (n != null)
		{
			LoadValue(n, "Weight", ref powerUpSettings.PowerUpWeights[(int)PowerUpType.DamageBoost]);
			LoadValue(n, "Duration", ref powerUpSettings.DamageBoostDuration);
			LoadPlayerTraits(n, playerTraitModifiers[(int)PlayerTraitModifiersType.DamageBoost], "TraitModifiers");
		}

		n = node.SelectSingleNode("SixPack");
		if (n != null)
		{
			LoadValue(n, "Weight", ref powerUpSettings.PowerUpWeights[(int)PowerUpType.SixPack]);
			LoadValue(n, "Amount", ref powerUpSettings.SixPack.Amount);
		}

		n = node.SelectSingleNode("RogiBall");
		if (n != null)
		{
			LoadValue(n, "Weight", ref powerUpSettings.PowerUpWeights[(int)PowerUpType.RogiBall]);
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public GameVariantMetaData MetaData
	{
		get
		{
			return metaData;
		}
	}

	public GenericGameSettings Generic
	{
		get
		{
			return genericGameSettings;
		}
	}

	public PlayerTraits GetPlayerTraits(PlayerTraitsType type)
	{
		return playerTraits[(int)type];
	}

	public PlayerTraits GetPlayerTraitModifiers(PlayerTraitModifiersType type)
	{
		return playerTraitModifiers[(int)type];
	}

	public WeaponSettings Weapons
	{
		get
		{
			return weaponSettings;
		}
	}

	public GrenadeSettings Grenades
	{
		get
		{
			return grenadeSettings;
		}
	}

	public PowerUpSettings PowerUps
	{
		get
		{
			return powerUpSettings;
		}
	}
}

public class GameVariantMetaData
{
	public string Name;
	public string Description;
	public GameType Type;
	public string CreatorName;
	public int CompatibleVersion;
	public string FileName;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Name);
		writer.Write(Description);
		writer.Write((byte)Type);
		writer.Write(CreatorName);
		writer.Write(CompatibleVersion);
	}

	public void Deserialize(NetworkReader reader)
	{
		Name = reader.ReadString();
		Description = reader.ReadString();
		Type = (GameType)reader.ReadByte();
		CreatorName = reader.ReadString();
		CompatibleVersion = reader.ReadInt32();
	}
}

public class GenericGameSettings
{
	public int ScoreToWin;
	public int Time;
	public bool Teams;
	public int ScorePerKill;
	public int ScorePerDeath;
	public int ScorePerSuicide;
	public int ScorePerBetrayal;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write((short)ScoreToWin);
		writer.Write((short)Time);
		writer.Write(Teams);
		writer.Write((short)ScorePerKill);
		writer.Write((short)ScorePerDeath);
		writer.Write((short)ScorePerSuicide);
		writer.Write((short)ScorePerBetrayal);
	}

	public void Deserialize(NetworkReader reader)
	{
		ScoreToWin = reader.ReadInt16();
		Time = reader.ReadInt16();
		Teams = reader.ReadBoolean();
		ScorePerKill = reader.ReadInt16();
		ScorePerDeath = reader.ReadInt16();
		ScorePerSuicide = reader.ReadInt16();
		ScorePerBetrayal = reader.ReadInt16();
	}
}

#region Player Traits

public class PlayerTraits
{
	public HealthSettings Health;
	public RespawnSettings Respawn;
	public MovementSettings Movement;
	public MeleeSettings Melee;
	public PlayerWeaponSettings Weapons;
	public NameTagSettings FriendlyNameTags;
	public NameTagSettings EnemyNameTags;
	public RadarSettings Radar;

	public PlayerTraits()
	{
		Health = new HealthSettings();
		Respawn = new RespawnSettings();
		Movement = new MovementSettings();
		Melee = new MeleeSettings();
		Weapons = new PlayerWeaponSettings();
		FriendlyNameTags = new NameTagSettings();
		EnemyNameTags = new NameTagSettings();
		Radar = new RadarSettings();
	}

	public void Serialize(NetworkWriter writer)
	{
		Health.Serialize(writer);
		Respawn.Serialize(writer);
		Movement.Serialize(writer);
		Melee.Serialize(writer);
		Weapons.Serialize(writer);
		FriendlyNameTags.Serialize(writer);
		EnemyNameTags.Serialize(writer);
		Radar.Serialize(writer);
	}

	public void Deserialize(NetworkReader reader)
	{
		Health.Deserialize(reader);
		Respawn.Deserialize(reader);
		Movement.Deserialize(reader);
		Melee.Deserialize(reader);
		Weapons.Deserialize(reader);
		FriendlyNameTags.Deserialize(reader);
		EnemyNameTags.Deserialize(reader);
		Radar.Deserialize(reader);
	}

	public void Clone(PlayerTraits other)
	{
		Health.Clone(other.Health);
		Respawn.Clone(other.Respawn);
		Movement.Clone(other.Movement);
		Melee.Clone(other.Melee);
		Weapons.Clone(other.Weapons);
		FriendlyNameTags.Clone(other.FriendlyNameTags);
		EnemyNameTags.Clone(other.EnemyNameTags);
		Radar.Clone(other.Radar);
	}

	public void SetModifierDefaults()
	{
		Health.SetModifierDefaults();
		Respawn.SetModifierDefaults();
		Movement.SetModifierDefaults();
		Melee.SetModifierDefaults();
		Weapons.SetModifierDefaults();
		FriendlyNameTags.SetModifierDefaults();
		EnemyNameTags.SetModifierDefaults();
		Radar.SetModifierDefaults();
	}

	public void Multiply(PlayerTraits other)
	{
		Health.Multiply(other.Health);
		Respawn.Multiply(other.Respawn);
		Movement.Multiply(other.Movement);
		Melee.Multiply(other.Melee);
		Weapons.Multiply(other.Weapons);
		FriendlyNameTags.Multiply(other.FriendlyNameTags);
		EnemyNameTags.Multiply(other.EnemyNameTags);
		Radar.Multiply(other.Radar);
	}
}

public class HealthSettings
{
	public float Modifier;
	public float RegenDelay;
	public float RegenRate;
	public float DamageResistance;
	public float LifeSteal;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Modifier);
		writer.Write(RegenDelay);
		writer.Write(RegenRate);
		writer.Write(DamageResistance);
		writer.Write(LifeSteal);
	}

	public void Deserialize(NetworkReader reader)
	{
		Modifier = reader.ReadSingle();
		RegenDelay = reader.ReadSingle();
		RegenRate = reader.ReadSingle();
		DamageResistance = reader.ReadSingle();
		LifeSteal = reader.ReadSingle();
	}

	public void Clone(HealthSettings other)
	{
		Modifier = other.Modifier;
		RegenDelay = other.RegenDelay;
		RegenRate = other.RegenRate;
		DamageResistance = other.DamageResistance;
		LifeSteal = other.LifeSteal;
	}

	public void SetModifierDefaults()
	{
		Modifier = 1.0f;
		RegenDelay = 1.0f;
		RegenRate = 1.0f;
		DamageResistance = 1.0f;
		LifeSteal = 1.0f;
	}

	public void Multiply(HealthSettings other)
	{
		Modifier *= other.Modifier;
		RegenDelay *= other.RegenDelay;
		RegenRate *= other.RegenRate;
		DamageResistance *= other.DamageResistance;
		LifeSteal *= other.LifeSteal;
	}
}

public class RespawnSettings
{
	public float Time;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Time);
	}

	public void Deserialize(NetworkReader reader)
	{
		Time = reader.ReadSingle();
	}

	public void Clone(RespawnSettings other)
	{
		Time = other.Time;
	}

	public void SetModifierDefaults()
	{
		Time = 1.0f;
	}

	public void Multiply(RespawnSettings other)
	{
		Time *= other.Time;
	}
}

public class MovementSettings
{
	public float Speed;
	public float Acceleration;
	public float JumpHeight;
	public float AirControl;
	public float SprintSpeedMultiplier;
	public float CrouchSpeedMultiplier;
	public float GravityMultiplier;
	public bool ThrustEnabled;
	public float ThrustHorizontalForce;
	public float ThrustVerticalForce;
	public float ThrustDelay;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Speed);
		writer.Write(Acceleration);
		writer.Write(JumpHeight);
		writer.Write(AirControl);
		writer.Write(SprintSpeedMultiplier);
		writer.Write(CrouchSpeedMultiplier);
		writer.Write(GravityMultiplier);
		writer.Write(ThrustEnabled);
		writer.Write(ThrustHorizontalForce);
		writer.Write(ThrustVerticalForce);
		writer.Write(ThrustDelay);
	}

	public void Deserialize(NetworkReader reader)
	{
		Speed = reader.ReadSingle();
		Acceleration = reader.ReadSingle();
		JumpHeight = reader.ReadSingle();
		AirControl = reader.ReadSingle();
		SprintSpeedMultiplier = reader.ReadSingle();
		CrouchSpeedMultiplier = reader.ReadSingle();
		GravityMultiplier = reader.ReadSingle();
		ThrustEnabled = reader.ReadBoolean();
		ThrustHorizontalForce = reader.ReadSingle();
		ThrustVerticalForce = reader.ReadSingle();
		ThrustDelay = reader.ReadSingle();
	}

	public void Clone(MovementSettings other)
	{
		Speed = other.Speed;
		Acceleration = other.Acceleration;
		JumpHeight = other.JumpHeight;
		AirControl = other.AirControl;
		SprintSpeedMultiplier = other.SprintSpeedMultiplier;
		CrouchSpeedMultiplier = other.CrouchSpeedMultiplier;
		GravityMultiplier = other.GravityMultiplier;
		ThrustEnabled = other.ThrustEnabled;
		ThrustHorizontalForce = other.ThrustHorizontalForce;
		ThrustVerticalForce = other.ThrustVerticalForce;
		ThrustDelay = other.ThrustDelay;
	}

	public void SetModifierDefaults()
	{
		Speed = 1.0f;
		Acceleration = 1.0f;
		JumpHeight = 1.0f;
		AirControl = 1.0f;
		SprintSpeedMultiplier = 1.0f;
		CrouchSpeedMultiplier = 1.0f;
		GravityMultiplier = 1.0f;
		ThrustEnabled = true;
		ThrustHorizontalForce = 1.0f;
		ThrustVerticalForce = 1.0f;
		ThrustDelay = 1.0f;
	}

	public void Multiply(MovementSettings other)
	{
		Speed *= other.Speed;
		Acceleration *= other.Acceleration;
		JumpHeight *= other.JumpHeight;
		AirControl *= other.AirControl;
		SprintSpeedMultiplier *= other.SprintSpeedMultiplier;
		CrouchSpeedMultiplier *= other.CrouchSpeedMultiplier;
		GravityMultiplier *= other.GravityMultiplier;
		ThrustEnabled = ThrustEnabled && other.ThrustEnabled;
		ThrustHorizontalForce *= other.ThrustHorizontalForce;
		ThrustVerticalForce *= other.ThrustVerticalForce;
		ThrustDelay *= other.ThrustDelay;
	}
}

public class MeleeSettings
{
	public float Damage;
	public float Rate;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Damage);
		writer.Write(Rate);
	}

	public void Deserialize(NetworkReader reader)
	{
		Damage = reader.ReadSingle();
		Rate = reader.ReadSingle();
	}

	public void Clone(MeleeSettings other)
	{
		Damage = other.Damage;
		Rate = other.Rate;
	}

	public void SetModifierDefaults()
	{
		Damage = 1.0f;
		Rate = 1.0f;
	}

	public void Multiply(MeleeSettings other)
	{
		Damage *= other.Damage;
		Rate *= other.Rate;
	}
}

public class PlayerWeaponSettings
{
	public WeaponType StartingPrimaryWeapon;
	public WeaponType StartingSecondaryWeapon;
	public InfiniteAmmoType InfiniteAmmo;
	public float DamageMultiplier;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write((short)StartingPrimaryWeapon);
		writer.Write((short)StartingSecondaryWeapon);
		writer.Write((byte)InfiniteAmmo);
		writer.Write(DamageMultiplier);
	}

	public void Deserialize(NetworkReader reader)
	{
		StartingPrimaryWeapon = (WeaponType)reader.ReadInt16();
		StartingSecondaryWeapon = (WeaponType)reader.ReadInt16();
		InfiniteAmmo = (InfiniteAmmoType)reader.ReadByte();
		DamageMultiplier = reader.ReadSingle();
	}

	public void Clone(PlayerWeaponSettings other)
	{
		StartingPrimaryWeapon = other.StartingPrimaryWeapon;
		StartingSecondaryWeapon = other.StartingSecondaryWeapon;
		InfiniteAmmo = other.InfiniteAmmo;
		DamageMultiplier = other.DamageMultiplier;
	}

	public void SetModifierDefaults()
	{
		DamageMultiplier = 1.0f;
		InfiniteAmmo = InfiniteAmmoType.None;
	}

	public void Multiply(PlayerWeaponSettings other)
	{
		DamageMultiplier *= other.DamageMultiplier;
		InfiniteAmmo = (InfiniteAmmoType)Mathf.Max((int)InfiniteAmmo, (int)other.InfiniteAmmo);
	}
}

public class NameTagSettings
{
	public bool Enabled;
	public bool HideBehindGeometry;
	public float Range;
	public bool ShowHealthBar;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Enabled);
		writer.Write(HideBehindGeometry);
		writer.Write(Range);
		writer.Write(ShowHealthBar);
	}

	public void Deserialize(NetworkReader reader)
	{
		Enabled = reader.ReadBoolean();
		HideBehindGeometry = reader.ReadBoolean();
		Range = reader.ReadSingle();
		ShowHealthBar = reader.ReadBoolean();
	}

	public void Clone(NameTagSettings other)
	{
		Enabled = other.Enabled;
		HideBehindGeometry = other.HideBehindGeometry;
		Range = other.Range;
		ShowHealthBar = other.ShowHealthBar;
	}

	public void SetModifierDefaults()
	{
		Enabled = true;
		HideBehindGeometry = true;
		Range = 1.0f;
		ShowHealthBar = true;
	}

	public void Multiply(NameTagSettings other)
	{
		Enabled = Enabled && other.Enabled;
		HideBehindGeometry = HideBehindGeometry && other.HideBehindGeometry;
		Range *= other.Range;
	}
}

public class RadarSettings
{
	public float Range;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Range);
	}

	public void Deserialize(NetworkReader reader)
	{
		Range = reader.ReadSingle();
	}

	public void Clone(RadarSettings other)
	{
		Range = other.Range;
	}

	public void SetModifierDefaults()
	{
		Range = 1.0f;
	}

	public void Multiply(RadarSettings other)
	{
		Range *= other.Range;
	}
}

#endregion

#region Weapon Settings

public class WeaponSettings
{
	public bool SpawnOnMap;
	public bool DropWhenKilled;
	public AssaultRifleSettings AssaultRifle;
	public SMGSettings Smg;
	public SniperSettings Sniper;
	public RocketLauncherSettings RocketLauncher;
	public ShotgunSettings Shotgun;
	public PistolSettings Pistol;
	public MachineGunSettings MachineGun;

	public WeaponSettings()
	{
		AssaultRifle = new AssaultRifleSettings();
		Smg = new SMGSettings();
		Sniper = new SniperSettings();
		RocketLauncher = new RocketLauncherSettings();
		Shotgun = new ShotgunSettings();
		Pistol = new PistolSettings();
		MachineGun = new MachineGunSettings();
	}

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(SpawnOnMap);
		writer.Write(DropWhenKilled);
		AssaultRifle.Serialize(writer);
		Smg.Serialize(writer);
		Sniper.Serialize(writer);
		RocketLauncher.Serialize(writer);
		Shotgun.Serialize(writer);
		Pistol.Serialize(writer);
		MachineGun.Serialize(writer);
	}

	public void Deserialize(NetworkReader reader)
	{
		SpawnOnMap = reader.ReadBoolean();
		DropWhenKilled = reader.ReadBoolean();
		AssaultRifle.Deserialize(reader);
		Smg.Deserialize(reader);
		Sniper.Deserialize(reader);
		RocketLauncher.Deserialize(reader);
		Shotgun.Deserialize(reader);
		Pistol.Deserialize(reader);
		MachineGun.Deserialize(reader);
	}
}

public class AssaultRifleSettings
{
	public float Damage;
	public float HeadShotDamageMultiplier;
	public int Ammo;
	public int MaxAmmo;
	public int ClipSize;
	public float FireRate;
	public float ReloadTime;
	public int BurstAmount;
	public float BurstRate;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Damage);
		writer.Write(HeadShotDamageMultiplier);
		writer.Write((short)Ammo);
		writer.Write((short)MaxAmmo);
		writer.Write((short)ClipSize);
		writer.Write(FireRate);
		writer.Write(ReloadTime);
		writer.Write((byte)BurstAmount);
		writer.Write(BurstRate);
	}

	public void Deserialize(NetworkReader reader)
	{
		Damage = reader.ReadSingle();
		HeadShotDamageMultiplier = reader.ReadSingle();
		Ammo = reader.ReadInt16();
		MaxAmmo = reader.ReadInt16();
		ClipSize = reader.ReadInt16();
		FireRate = reader.ReadSingle();
		ReloadTime = reader.ReadSingle();
		BurstAmount = reader.ReadByte();
		BurstRate = reader.ReadSingle();
	}
}

public class SMGSettings
{
	public float Damage;
	public float HeadShotDamageMultiplier;
	public int Ammo;
	public int MaxAmmo;
	public int ClipSize;
	public float FireRate;
	public float ReloadTime;
	public float Spread;
	public float AimDownSightsSpread;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Damage);
		writer.Write(HeadShotDamageMultiplier);
		writer.Write((short)Ammo);
		writer.Write((short)MaxAmmo);
		writer.Write((short)ClipSize);
		writer.Write(FireRate);
		writer.Write(ReloadTime);
		writer.Write(Spread);
		writer.Write(AimDownSightsSpread);
	}

	public void Deserialize(NetworkReader reader)
	{
		Damage = reader.ReadSingle();
		HeadShotDamageMultiplier = reader.ReadSingle();
		Ammo = reader.ReadInt16();
		MaxAmmo = reader.ReadInt16();
		ClipSize = reader.ReadInt16();
		FireRate = reader.ReadSingle();
		ReloadTime = reader.ReadSingle();
		Spread = reader.ReadSingle();
		AimDownSightsSpread = reader.ReadSingle();
	}
}

public class SniperSettings
{
	public float Damage;
	public float HeadShotDamageMultiplier;
	public int Ammo;
	public int MaxAmmo;
	public int ClipSize;
	public float FireRate;
	public float ReloadTime;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Damage);
		writer.Write(HeadShotDamageMultiplier);
		writer.Write((short)Ammo);
		writer.Write((short)MaxAmmo);
		writer.Write((short)ClipSize);
		writer.Write(FireRate);
		writer.Write(ReloadTime);
	}

	public void Deserialize(NetworkReader reader)
	{
		Damage = reader.ReadSingle();
		HeadShotDamageMultiplier = reader.ReadSingle();
		Ammo = reader.ReadInt16();
		MaxAmmo = reader.ReadInt16();
		ClipSize = reader.ReadInt16();
		FireRate = reader.ReadSingle();
		ReloadTime = reader.ReadSingle();
	}
}

public class RocketLauncherSettings
{
	public float Damage;
	public float Radius;
	public float InitialSpeed;
	public float Acceleration;
	public int Ammo;
	public int MaxAmmo;
	public int ClipSize;
	public float FireRate;
	public float ReloadTime;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Damage);
		writer.Write(Radius);
		writer.Write(InitialSpeed);
		writer.Write(Acceleration);
		writer.Write((short)Ammo);
		writer.Write((short)MaxAmmo);
		writer.Write((short)ClipSize);
		writer.Write(FireRate);
		writer.Write(ReloadTime);
	}

	public void Deserialize(NetworkReader reader)
	{
		Damage = reader.ReadSingle();
		Radius = reader.ReadSingle();
		InitialSpeed = reader.ReadSingle();
		Acceleration = reader.ReadSingle();
		Ammo = reader.ReadInt16();
		MaxAmmo = reader.ReadInt16();
		ClipSize = reader.ReadInt16();
		FireRate = reader.ReadSingle();
		ReloadTime = reader.ReadSingle();
	}
}

public class ShotgunSettings
{
	public float Damage;
	public float Range;
	public int NumPellets;
	public float Spread;
	public int Ammo;
	public int MaxAmmo;
	public int ClipSize;
	public float FireRate;
	public float ReloadTime;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Damage);
		writer.Write(Range);
		writer.Write((byte)NumPellets);
		writer.Write(Spread);
		writer.Write((short)Ammo);
		writer.Write((short)MaxAmmo);
		writer.Write((short)ClipSize);
		writer.Write(FireRate);
		writer.Write(ReloadTime);
	}

	public void Deserialize(NetworkReader reader)
	{
		Damage = reader.ReadSingle();
		Range = reader.ReadSingle();
		NumPellets = reader.ReadByte();
		Spread = reader.ReadSingle();
		Ammo = reader.ReadInt16();
		MaxAmmo = reader.ReadInt16();
		ClipSize = reader.ReadInt16();
		FireRate = reader.ReadSingle();
		ReloadTime = reader.ReadSingle();
	}
}

public class PistolSettings
{
	public float Damage;
	public float HeadShotDamageMultiplier;
	public int Ammo;
	public int MaxAmmo;
	public int ClipSize;
	public float FireRate;
	public float ReloadTime;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Damage);
		writer.Write(HeadShotDamageMultiplier);
		writer.Write((short)Ammo);
		writer.Write((short)MaxAmmo);
		writer.Write((short)ClipSize);
		writer.Write(FireRate);
		writer.Write(ReloadTime);
	}

	public void Deserialize(NetworkReader reader)
	{
		Damage = reader.ReadSingle();
		HeadShotDamageMultiplier = reader.ReadSingle();
		Ammo = reader.ReadInt16();
		MaxAmmo = reader.ReadInt16();
		ClipSize = reader.ReadInt16();
		FireRate = reader.ReadSingle();
		ReloadTime = reader.ReadSingle();
	}
}

public class MachineGunSettings
{
	public float Damage;
	public float HeadShotDamageMultiplier;
	public int Ammo;
	public int MaxAmmo;
	public int ClipSize;
	public float FireRate;
	public float ReloadTime;
	public float Spread;
	public float AimDownSightsSpread;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Damage);
		writer.Write(HeadShotDamageMultiplier);
		writer.Write((short)Ammo);
		writer.Write((short)MaxAmmo);
		writer.Write((short)ClipSize);
		writer.Write(FireRate);
		writer.Write(ReloadTime);
		writer.Write(Spread);
		writer.Write(AimDownSightsSpread);
	}

	public void Deserialize(NetworkReader reader)
	{
		Damage = reader.ReadSingle();
		HeadShotDamageMultiplier = reader.ReadSingle();
		Ammo = reader.ReadInt16();
		MaxAmmo = reader.ReadInt16();
		ClipSize = reader.ReadInt16();
		FireRate = reader.ReadSingle();
		ReloadTime = reader.ReadSingle();
		Spread = reader.ReadSingle();
		AimDownSightsSpread = reader.ReadSingle();
	}
}

#endregion

#region Grenade Settings

public class GrenadeSettings
{
	public float ThrowRate;
	public float MinThrowForce;
	public float MaxThrowForce;
	public bool DropWhenKilled;
	public GrenadeQuantitySettings[] Quantities;
	public FragSettings Frag;
	public TeslaSettings Tesla;
	public SpikeSettings Spike;

	public GrenadeSettings()
	{
		Quantities = new GrenadeQuantitySettings[(int)GrenadeType.NumTypes];
		for (int i = 0; i < (int)GrenadeType.NumTypes; i++)
		{
			Quantities[i] = new GrenadeQuantitySettings();
		}

		Frag = new FragSettings();
		Tesla = new TeslaSettings();
		Spike = new SpikeSettings();
	}

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(ThrowRate);
		writer.Write(MinThrowForce);
		writer.Write(MaxThrowForce);
		writer.Write(DropWhenKilled);

		for (int i = 0; i < (int)GrenadeType.NumTypes; i++)
		{
			Quantities[i].Serialize(writer);
		}

		Frag.Serialize(writer);
		Tesla.Serialize(writer);
		Spike.Serialize(writer);
	}

	public void Deserialize(NetworkReader reader)
	{
		ThrowRate = reader.ReadSingle();
		MinThrowForce = reader.ReadSingle();
		MaxThrowForce = reader.ReadSingle();
		DropWhenKilled = reader.ReadBoolean();

		for (int i = 0; i < (int)GrenadeType.NumTypes; i++)
		{
			Quantities[i].Deserialize(reader);
		}

		Frag.Deserialize(reader);
		Tesla.Deserialize(reader);
		Spike.Deserialize(reader);
	}
}

public class GrenadeQuantitySettings
{
	public int StartingAmount;
	public int MaxAmount;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write((short)StartingAmount);
		writer.Write((short)MaxAmount);
	}

	public void Deserialize(NetworkReader reader)
	{
		StartingAmount = reader.ReadInt16();
		MaxAmount = reader.ReadInt16();
	}
}

public class FragSettings
{
	public float Damage;
	public float Range;
	public float FuseTime;
	public float NoContactFuseTime;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Damage);
		writer.Write(Range);
		writer.Write(FuseTime);
		writer.Write(NoContactFuseTime);
	}

	public void Deserialize(NetworkReader reader)
	{
		Damage = reader.ReadSingle();
		Range = reader.ReadSingle();
		FuseTime = reader.ReadSingle();
		NoContactFuseTime = reader.ReadSingle();
	}
}

public class TeslaSettings
{
	public float DamagePerSecond;
	public float Range;
	public float FuseTime;
	public float SparkTime;
	public float ExplosiveDamage;
	public float ExplosiveRange;
	public bool IgnitedOnCollision;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(DamagePerSecond);
		writer.Write(Range);
		writer.Write(FuseTime);
		writer.Write(SparkTime);
		writer.Write(ExplosiveDamage);
		writer.Write(ExplosiveRange);
		writer.Write(IgnitedOnCollision);
	}

	public void Deserialize(NetworkReader reader)
	{
		DamagePerSecond = reader.ReadSingle();
		Range = reader.ReadSingle();
		FuseTime = reader.ReadSingle();
		SparkTime = reader.ReadSingle();
		ExplosiveDamage = reader.ReadSingle();
		ExplosiveRange = reader.ReadSingle();
		IgnitedOnCollision = reader.ReadBoolean();
	}
}

public class SpikeSettings
{
	public float Damage;
	public float Range;
	public float FuseTime;
	public float StickDamage;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Damage);
		writer.Write(Range);
		writer.Write(FuseTime);
		writer.Write(StickDamage);
	}

	public void Deserialize(NetworkReader reader)
	{
		Damage = reader.ReadSingle();
		Range = reader.ReadSingle();
		FuseTime = reader.ReadSingle();
		StickDamage = reader.ReadSingle();
	}
}

#endregion

#region PowerUp Settings

public class PowerUpSettings
{
	public float RespawnTime;
	public float SpinnerDuration;
	public bool SpawnOnMap;
	public int[] PowerUpWeights;
	public GrenadeCloudSettings GrenadeCloud;
	public BigHeadsSettings BigHeads;
	public int DamageResistDuration;
	public int SpeedBoostDuration;
	public int DamageBoostDuration;
	public SixPackSettings SixPack;

	public PowerUpSettings()
	{
		PowerUpWeights = new int[(int)PowerUpType.NumTypes];
		GrenadeCloud = new GrenadeCloudSettings();
		BigHeads = new BigHeadsSettings();
		SixPack = new SixPackSettings();
	}

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(RespawnTime);
		writer.Write(SpinnerDuration);
		writer.Write(SpawnOnMap);

		for (int i = 0; i < (int)PowerUpType.NumTypes; i++)
		{
			writer.Write(PowerUpWeights[i]);
		}

		GrenadeCloud.Serialize(writer);
		BigHeads.Serialize(writer);
		writer.Write((short)DamageResistDuration);
		writer.Write((short)SpeedBoostDuration);
		writer.Write((short)DamageBoostDuration);
		SixPack.Serialize(writer);
	}

	public void Deserialize(NetworkReader reader)
	{
		RespawnTime = reader.ReadSingle();
		SpinnerDuration = reader.ReadSingle();
		SpawnOnMap = reader.ReadBoolean();

		for (int i = 0; i < (int)PowerUpType.NumTypes; i++)
		{
			PowerUpWeights[i] = reader.ReadInt32();
		}

		GrenadeCloud.Deserialize(reader);
		BigHeads.Deserialize(reader);
		DamageResistDuration = reader.ReadInt16();
		SpeedBoostDuration = reader.ReadInt16();
		DamageBoostDuration = reader.ReadInt16();
		SixPack.Deserialize(reader);
	}
}

public class GrenadeCloudSettings
{
	public float Size;
	public float MinimumSpawnTime;
	public float MaximumSpawnTime;
	public GrenadeType GrenadeType;
	public float IntroDuration;
	public float Speed;
	public float GrenadeFuseTime;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Size);
		writer.Write(MinimumSpawnTime);
		writer.Write(MaximumSpawnTime);
		writer.Write((byte)GrenadeType);
		writer.Write(IntroDuration);
		writer.Write(Speed);
		writer.Write(GrenadeFuseTime);
	}

	public void Deserialize(NetworkReader reader)
	{
		Size = reader.ReadSingle();
		MinimumSpawnTime = reader.ReadSingle();
		MaximumSpawnTime = reader.ReadSingle();
		GrenadeType = (GrenadeType)reader.ReadByte();
		IntroDuration = reader.ReadSingle();
		Speed = reader.ReadSingle();
		GrenadeFuseTime = reader.ReadSingle();
	}
}

public class BigHeadsSettings
{
	public float Scale;
	public int Duration;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(Scale);
		writer.Write((short)Duration);
	}

	public void Deserialize(NetworkReader reader)
	{
		Scale = reader.ReadSingle();
		Duration = reader.ReadInt16();
	}
}

public class SixPackSettings
{
	public int Amount;

	public void Serialize(NetworkWriter writer)
	{
		writer.Write((short)Amount);
	}

	public void Deserialize(NetworkReader reader)
	{
		Amount = reader.ReadInt16();
	}
}

#endregion

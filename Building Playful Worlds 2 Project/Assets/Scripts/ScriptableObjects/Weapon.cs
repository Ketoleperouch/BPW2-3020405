using UnityEngine;

[CreateAssetMenu(menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponName = "Weapon";

    public float fireRate;
    public bool semiAuto = true;
    public float damage;
    public float recoil;
    [Range(0, 1)]
    public float accuracy;
    public float range;

    public ParticleSystem muzzleParticles;
    public GameObject impactParticles;
    public AudioClip[] shotSounds;
    public Sprite crosshair;

    public int totalAmmo;
    public int currentAmmoInClip;
    public int clipSize;

    public int ID;

    public bool outOfAmmo = false;
}
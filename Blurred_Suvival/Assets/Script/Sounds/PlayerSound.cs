using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    public GearEquipper gearEquipper;
    public void PlayFootStep()
    {
        SFXManager.Instance.PlayFootStep();
    }

    public void PlayWeaponSound()
    {
        if (gearEquipper.equippedWeapon != null)
        {
            SFXManager.Instance.PlayWeaponSound(gearEquipper.equippedWeapon.audioClip);
        }
        else
        {
            SFXManager.Instance.PlayWeaponSound();
        }
    }

    public void PlaySwing()
    {

    }
}

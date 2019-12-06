﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VFXTester : MonoBehaviour
{

    public List<VFXTesterCharClass> Characters = new List<VFXTesterCharClass>();
    public GameObject CharacterBasePrefab;

    public TMPro.TMP_Dropdown CharToUse;
    public TMPro.TMP_Dropdown ParticleLevel;
    public TMPro.TMP_Dropdown ParticleType;
    public TMPro.TMP_Dropdown CharacterClass;
    public Slider AttackSpeed;
    public Slider SpeedOfBullets;
    public Slider MountainDelay;
    private GameObject charOnScene;
    public TextMeshProUGUI AttackSpeedText;
    public TextMeshProUGUI SpeedOfBulletsText;
    public TextMeshProUGUI MountainDelayText;

    private void Start()
    {

        for (int i = 0; i <= Enum.GetValues(typeof(CharacterNameType)).Cast<int>().Last(); i++)
        {
            CharToUse.options.Add(new TMPro.TMP_Dropdown.OptionData(((CharacterNameType)i).ToString()));

        }

        for (int i = 0; i <= Enum.GetValues(typeof(CharacterLevelType)).Cast<int>().Last(); i++)
        {
            ParticleLevel.options.Add(new TMPro.TMP_Dropdown.OptionData(((CharacterLevelType)i).ToString()));

        }

        for (int i = 0; i <= Enum.GetValues(typeof(AttackParticleTypes)).Cast<int>().Last(); i++)
        {
            ParticleType.options.Add(new TMPro.TMP_Dropdown.OptionData(((AttackParticleTypes)i).ToString()));

        }

        for (int i = 0; i <= Enum.GetValues(typeof(CharacterClassType)).Cast<int>().Last(); i++)
        {
            CharacterClass.options.Add(new TMPro.TMP_Dropdown.OptionData(((CharacterClassType)i).ToString()));

        }

    }

    private void Update()
    {
        AttackSpeedText.text = AttackSpeed.value.ToString("F2");
        SpeedOfBulletsText.text = SpeedOfBullets.value.ToString("F2");
        MountainDelayText.text = MountainDelay.value.ToString("F2");
    }

    // Start is called before the first frame update
    public void CreateChar()
    {
        Destroy(charOnScene);
        BattleTileScript bts = GridManagerScript.Instance.GetBattleTile(new Vector2Int(3,9));
        charOnScene = Instantiate(CharacterBasePrefab, bts.transform.position, Quaternion.identity);
        GameObject child = Instantiate(Characters.Where(r=> r.CharName.ToString() == CharToUse.options[CharToUse.value].text).First().Char, charOnScene.transform.position, Quaternion.identity, charOnScene.transform);
        CharacterBase currentCharacter = charOnScene.GetComponent<CharacterBase>();
        currentCharacter.VFXTestMode = true;
        currentCharacter.UMS.CurrentTilePos = bts.Pos;
        for (int i = 0; i < currentCharacter.UMS.Pos.Count; i++)
        {
            currentCharacter.UMS.Pos[i] += bts.Pos;
            BattleTileScript cbts = GridManagerScript.Instance.GetBattleTile(currentCharacter.UMS.Pos[i]);
            currentCharacter.CurrentBattleTiles.Add(cbts);
        }
        currentCharacter.NextAttackLevel = (CharacterLevelType)Enum.Parse(typeof(CharacterLevelType), ParticleLevel.options[ParticleLevel.value].text);
        currentCharacter.CharInfo.ParticleID = (AttackParticleTypes)Enum.Parse(typeof(AttackParticleTypes), ParticleType.options[ParticleType.value].text);
        currentCharacter.CharInfo.AttackSpeedRatio = AttackSpeed.value;
        currentCharacter.UMS.Side = SideType.RightSide;
        currentCharacter.CharInfo.ClassType = (CharacterClassType)Enum.Parse(typeof(CharacterClassType), CharacterClass.options[CharacterClass.value].text);
        currentCharacter.CharInfo.BulletSpeed = SpeedOfBullets.value;
        currentCharacter.CharInfo.ParticleID = currentCharacter.CharInfo.ParticleID;
        currentCharacter.CurrentAttackTypeInfo = currentCharacter.AttackTypesInfo.Where(r => r.CharacterClass == currentCharacter.CharInfo.ClassType).First();
        currentCharacter.CharInfo.DamageStats.ChildrenBulletDelay = MountainDelay.value;
    }
}


[System.Serializable]
public class VFXTesterCharClass
{
    public GameObject Char;
    public CharacterNameType CharName;
}






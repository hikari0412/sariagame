using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.vMelee;

public class UISkillCD : MonoBehaviour
{
    public vMeleeManager meleeManager;
    public GameObject skill1UI;
    public GameObject skill2UI;
    public GameObject skill3UI;
    public GameObject exModeUI;
	
	bool isHideUi = true;

    private RectTransform skillCDUI1;
    private RectTransform skillCDUI2;
    private RectTransform skillCDUI3;
    private RectTransform exModeMask;
    private RectTransform exModeFrame;
    // Start is called before the first frame update
    void Start()
    {
        skillCDUI1 = skill1UI.transform.GetChild(1).GetComponent<RectTransform>();
        skillCDUI2 = skill2UI.transform.GetChild(1).GetComponent<RectTransform>();
        skillCDUI3 = skill3UI.transform.GetChild(1).GetComponent<RectTransform>();
        exModeMask = exModeUI.transform.GetChild(1).GetComponent<RectTransform>();
        exModeFrame = exModeUI.transform.GetChild(2).GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (meleeManager.currentSkill1CD > 0f) { meleeManager.currentSkill1CD -= Time.deltaTime; } else { meleeManager.currentSkill1CD = 0.01f; }
        if (meleeManager.currentSkill2CD > 0f) { meleeManager.currentSkill2CD -= Time.deltaTime; } else { meleeManager.currentSkill2CD = 0.01f; }
        if (meleeManager.currentSkill3CD > 0f) { meleeManager.currentSkill3CD -= Time.deltaTime; } else { meleeManager.currentSkill3CD = 0.01f; }
        if (meleeManager.exPoint < meleeManager.maxEX)
        {
            exModeMask.gameObject.SetActive(true);
            exModeFrame.gameObject.SetActive(false);
        }
        else
        {
            exModeMask.gameObject.SetActive(false);
        }

        if (meleeManager.exMode)
        {
            exModeMask.gameObject.SetActive(false);
            exModeFrame.gameObject.SetActive(true);
        }
        Vector3 tempScale = Vector3.one;
        if (isHideUi)
        {
            skillCDUI1.localScale = tempScale;
            skillCDUI2.localScale = tempScale;
            skillCDUI3.localScale = tempScale;
            return;
        }
        tempScale = skillCDUI1.localScale;
        tempScale.y = meleeManager.currentSkill1CD / meleeManager.skill1CD;
        skillCDUI1.localScale = tempScale;
        tempScale = skillCDUI2.localScale;
        tempScale.y = meleeManager.currentSkill2CD / meleeManager.skill2CD;
        skillCDUI2.localScale = tempScale;
        tempScale = skillCDUI3.localScale;
        tempScale.y = meleeManager.currentSkill3CD / meleeManager.skill3CD;
        skillCDUI3.localScale = tempScale;
    }
	
	public void HideWeaponIcon()
	{
		isHideUi = true;
	}
	
	public void ShowWeaponIcon()
	{
		isHideUi = false;
	}
}

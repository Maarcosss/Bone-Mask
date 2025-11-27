using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;

public class LanguageController : MonoBehaviour
{
    private bool active = false;
    public TMP_Dropdown languageDropdown;

    void Start()
    {
        int ID = PlayerPrefs.GetInt("LocaleKey", 0);
        if (languageDropdown != null)
        {
            languageDropdown.value = ID;
            languageDropdown.RefreshShownValue();
        }
        ChangeLocale(ID);
    }

    public void ChangeLocale(int localeID)
    {
        if(active)
        {
            return;
        }
        StartCoroutine(SetLocale(localeID));
    }
    
    private IEnumerator SetLocale(int localeID)
    {
        active = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
        PlayerPrefs.SetInt("LocaleKey", localeID);
        PlayerPrefs.Save();
        active = false;
    }
}

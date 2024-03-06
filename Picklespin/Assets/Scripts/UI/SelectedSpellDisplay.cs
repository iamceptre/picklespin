using TMPro;
using UnityEngine;

public class SelectedSpellDisplay : MonoBehaviour
{

    [SerializeField] private Attack attackScript;
    private TMP_Text selectedSpellText;
    private Animation anim;


    private void Start()
    {
        selectedSpellText = GetComponent<TMP_Text>();
        selectedSpellText.text = attackScript.bulletPrefab[attackScript.selectedBullet].gameObject.name.ToString();
        anim = GetComponent<Animation>();
    }

private void DisableMe()
    {
        gameObject.SetActive(false); 
    }

 public void UpdateText()
    {
        if (selectedSpellText != null)
        {
            selectedSpellText.text = attackScript.bulletPrefab[attackScript.selectedBullet].gameObject.name.ToString();
        }
            anim.Stop();
            anim.Play();

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;
    [SerializeField] Text dialogText;

    public void SetDialogText(string dialog)
    {
        dialogText.text = dialog;
    }


    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = string.Empty;

        foreach (char letter in dialog)
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/ lettersPerSecond);
        }
    }
}

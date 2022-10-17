using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenWeb : MonoBehaviour
{
    public string URL;

    public void OpenURl()
    {
        System.Diagnostics.Process.Start(URL);
    }
}

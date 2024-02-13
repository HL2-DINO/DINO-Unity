using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/** @file           PrintToolDict.cs
 *  @brief          Helper Unity utils script to update a TextMesh with the contents
 *                  of a ToolDictionary grabbed from UnityToolManager
 *
 *  @author         Hisham Iqbal
 *  @copyright      &copy; 2023 Hisham Iqbal
 */
public class PrintToolDict : MonoBehaviour
{
    public UnityToolManager toolMgr;
    public TMPro.TextMeshProUGUI meshText;
    IReadOnlyDictionary<int, ToolTrackingUtils.TrackedTool> ToolDictToPrint = new Dictionary<int, ToolTrackingUtils.TrackedTool>();
    // Update is called once per frame
    void Update()
    {
        PrintToolDictionary();
    }

    /// <summary>
    /// Grab the tool dictionary exposed by UnityToolManager, and print its contents
    /// </summary>
    private void PrintToolDictionary()
    {
        if (toolMgr == null) return;
        ToolDictToPrint = toolMgr.GetToolDictionary();

        meshText.text = ""; // to clear

        // cast to ToArray to avoid race-condition issues?
        foreach (var pair in ToolDictToPrint.ToArray())
        {
            meshText.text += pair.Key.ToString() + '\n';
            meshText.text += pair.Value.Tool_HoloFrame_LH.ToString("F3");
        }
    }
}
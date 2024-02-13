using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;
using ToolTrackingUtils;

/** @file           ToolConfigUtilities.cs
 *  @brief          Unity helper functions for handling the info we read from a config .json object
 *                  used with the family of HL2-DINO applications
 *
 *  @author         Hisham Iqbal
 *  @copyright      &copy; 2023 Hisham Iqbal
 */

namespace ToolConfigUtilities
{
    public static class JSONUtils
    {
        /// <summary>
        /// The top level keys we are expecting to read in the config file
        /// </summary>
        public enum MainFileKeys
        {
            fileSettings,
            tools
        }

        /// <summary>
        /// The current iteration of 'settings' related to the config object, which should contain
        /// right-handed information only, but can have metres or millimetres as units
        /// </summary>
        public enum FileSettingsKeys
        {
            units // valid values: m or mm
        }

        /// <summary>
        /// The info we require for each trackable tool definition as specified in \ref ToolTrackingUtils.TrackedTool
        /// </summary>
        public enum ToolKeys
        {
            name,
            id,
            coordinates
        }

        /// <summary>
        /// Template function to validate if a JObject contains some desired keys
        /// </summary>
        /// <typeparam name="TEnum">Enum of keys to check against</typeparam>
        /// <param name="jsonObject">JObject to 'validate'</param>
        /// <returns>True only if all keys in \param TEnum are contained in \param jsonObject </returns>
        public static bool CheckEnumKeys<TEnum>(JObject jsonObject) where TEnum : System.Enum
        {
            if (jsonObject == null) return false; // empty jobject

            if (!typeof(TEnum).IsEnum)
            {
                Debug.LogError($"{typeof(TEnum)} is not an enum type.");
                return false;
            }

            foreach (TEnum enumValue in System.Enum.GetValues(typeof(TEnum)))
            {
                string enumString = enumValue.ToString();
                if (!jsonObject.ContainsKey(enumString))
                {
                    Debug.LogError($"Key '{enumString}' does not exist in the JSON object.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Function which returns a properly populated JObject if it matches the expected config file structure for this app
        /// </summary>
        /// <param name="filepath">String filepath for .json config file</param>
        /// <returns>Null if invalid, or a JObject if valid</returns>
        public static JObject TryReadingToolConfigJSON(string filepath)
        {
            JObject ToolConfigJSON;
            try
            {
                ToolConfigJSON = JObject.Parse(System.IO.File.ReadAllText(filepath));
                if (CheckEnumKeys<MainFileKeys>(ToolConfigJSON)
                   && CheckEnumKeys<FileSettingsKeys>(ToolConfigJSON[MainFileKeys.fileSettings.ToString()].ToObject<JObject>()))
                {
                    return ToolConfigJSON;
                }
                else { return null; }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Reads \p configObject and tries to return a list of \p TrackedTool based on info contained in the JObject
        /// </summary>
        /// <param name="configObject">Configuration JSON object to parse</param>
        /// <returns>A list of \p TrackedTool if valid</returns>
        public static List<TrackedTool> CreateTrackedToolsetFromJSON(JObject configObject)
        {
            var listToReturn = new List<TrackedTool>();

            if (!CheckEnumKeys<MainFileKeys>(configObject)) return new List<TrackedTool>(0);

            JObject unitySettings = configObject[MainFileKeys.fileSettings.ToString()].ToObject<JObject>();
            var toolListJson = configObject[MainFileKeys.tools.ToString()];

            // read direction and units from JSON file
            if (!CheckEnumKeys<FileSettingsKeys>(unitySettings)) return new List<TrackedTool>(0);

            // for each tool in JObject ->
            foreach (JObject toolJson in toolListJson)
            {
                string toolName = ""; int toolID = -1;
                List<Vector3> coordinateSet = new List<Vector3>();
                if (!IsValidToolPacket(toolJson, unitySettings, ref toolID, ref toolName, ref coordinateSet)) continue;

                // the configObject should only contain right-handed information, so we have to convert to 
                // left handed to match Unity convention
                coordinateSet = MatrixUtilities.InvertCoordinatesZ(coordinateSet);

                // init our tracked tool
                TrackedTool tool = new TrackedTool();
                tool.ToolID = toolID;
                tool.ToolName = toolName;
                tool.ToolMarkerTriplets = coordinateSet;
                tool.Tool_HoloFrame_LH = Matrix4x4.identity;
                tool.VisibleToHoloLens = false;
                tool.TimestampLastSeen = 0;
                listToReturn.Add(tool);
            }

            return listToReturn;

        }

        /// <summary>
        /// Reads the \p toolObject and then tries to populate the other fields passed in if valid
        /// </summary>
        /// <param name="toolObject">The JSON equivalent of a \p TrackedTool</param>
        /// <param name="configSettings">File settings</param>
        /// <param name="integerID">Tool ID (numeric)</param>
        /// <param name="stringID">Tool String ID (for labelling)</param>
        /// <param name="CoordinateSet">Vector3 coordinate list (for positioning objects)</param>
        /// <returns>True if \p toolObject contains all the necessary info, false otherwise</returns>
        public static bool IsValidToolPacket(JObject toolObject, JObject configSettings, ref int integerID, ref string stringID, ref List<Vector3> CoordinateSet)
        {
            if (!CheckEnumKeys<ToolKeys>(toolObject)) { return false; }

            var nameToken = toolObject["name"];
            var idToken = toolObject["id"];
            var coordinatesToken = toolObject["coordinates"];

            if (nameToken.Type != JTokenType.String ||
                idToken.Type != JTokenType.Integer ||
                coordinatesToken.Type != JTokenType.Array)
            {
                return false;
            }

            integerID = (int)idToken;
            stringID = (string)nameToken;

            CoordinateSet = GetCoordinatesList(toolObject, configSettings);
            if (CoordinateSet.Count == 0) return false;

            return true;
        }

        /// <summary>
        /// Construct a List of Vector3 of marker coordinate centres, as parsed from \p toolObject
        /// </summary>
        /// <param name="toolObject">JSON object to parse</param>
        /// <param name="configSettings">File settings</param>
        /// <returns>A list of marker coordinates if valid, empty list otherwise</returns>
        public static List<Vector3> GetCoordinatesList(JObject toolObject, JObject configSettings)
        {
            List<Vector3> coordinatesList = new List<Vector3>();
            JArray toolCoordinateSet = (JArray)toolObject["coordinates"];

            string unitsString = configSettings[FileSettingsKeys.units.ToString()].ToString().ToLowerInvariant();
            if (!System.Enum.TryParse(unitsString, true, out MatrixUtilities.MatrixUnits coordinateUnits))
            {
                Debug.LogError($"Check units in JSON. \"{unitsString}\" is not a valid MatrixUtilities.MatrixUnits");
                return new List<Vector3>(0);
            }

            if (toolCoordinateSet == null)
            {
                Debug.LogError($"No coordinates contained in the config file for a tool");
                return new List<Vector3>(0);
            }

            foreach (JToken toolTriplet in toolCoordinateSet)
            {
                if (toolTriplet.Type != JTokenType.Array) return new List<Vector3>(0);
                if (!IsValidCoordinateArray(toolTriplet)) return new List<Vector3>(0);

                JArray coordinateArray = (JArray)toolTriplet;

                JToken xVal = coordinateArray[0];
                JToken yVal = coordinateArray[1];
                JToken zVal = coordinateArray[2];

                if (xVal.Type != JTokenType.String ||
                    yVal.Type != JTokenType.String ||
                    zVal.Type != JTokenType.String)
                {
                    return new List<Vector3>(0);
                }

                try
                {
                    float x = float.Parse(xVal.ToString());
                    float y = float.Parse(yVal.ToString());
                    float z = float.Parse(zVal.ToString());
                    Vector3 Location = new Vector3(x, y, z);

                    if (coordinateUnits == MatrixUtilities.MatrixUnits.mm) Location /= 1000;
                    coordinatesList.Add(Location);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                    return new List<Vector3>(0);
                }
            }
            return coordinatesList;
        }

        /// <summary>
        /// Helper function to check we have an array of 3 values
        /// </summary>
        /// <param name="coordinateSet"></param>
        /// <returns></returns>
        static bool IsValidCoordinateArray(JToken coordinateSet)
        {
            // may be changed in future to check other aspects?
            if (coordinateSet is JArray coords && coords.Count == 3) return true;
            else return false;
        }

        /// <summary>
        /// Publicly available helper function, to prepare a JSON string to pass into the HL2-DINO-DLL
        /// which will be used to set up the Tool Dictionary on the C++ side
        /// </summary>
        /// <param name="JSONFilepath">JSON filepath</param>
        /// <returns>Formatted JSON string, if filepath contains a valid JSON file. Empty string otherwise</returns>
        public static string GetJSONToolStringHL2(string JSONFilepath)
        {
            JObject toolConfig = TryReadingToolConfigJSON(JSONFilepath);
            if (toolConfig == null) return "";

            toolConfig = PrepareJObjectForHL2(toolConfig);

            return toolConfig.ToString(Newtonsoft.Json.Formatting.None); ;
        }

        /// <summary>
        /// A sanitising function which checks \param configObject is in the right format for the HL2-DINO Plugin
        /// (only works in metres in this version)
        /// </summary>
        /// <param name="configObject"></param>
        /// <returns> Reconfigured JObject which should have all coordinates in metres. </returns>
        static JObject PrepareJObjectForHL2(JObject configObject)
        {
            if (!CheckEnumKeys<MainFileKeys>(configObject)) return null;

            var configSettings = configObject[MainFileKeys.fileSettings.ToString()];
            string unitsString = configSettings[FileSettingsKeys.units.ToString()].ToString().ToLowerInvariant();
            if (!System.Enum.TryParse(unitsString, true, out MatrixUtilities.MatrixUnits coordinateUnits))
            {
                Debug.LogError($"Check units in JSON. \"{unitsString}\" is not a valid MatrixUtilities.MatrixUnits");
                return null;
            }

            // return untouched
            if (coordinateUnits == MatrixUtilities.MatrixUnits.m) return configObject;

            // else modify JSON object by scaling from mm into metres
            var toolArray = configObject[MainFileKeys.tools.ToString()];

            foreach (var tool in toolArray)
            {
                foreach (var coordinate in tool["coordinates"])
                {
                    for (int i = 0; i < coordinate.Count(); i++)
                    {
                        double value;
                        if (double.TryParse(coordinate[i].ToString(), out value))
                        {
                            // Convert to meters (divide by 1000)
                            coordinate[i] = (value / 1000).ToString("F5");
                        }
                    }
                }
            }

            // change to metres
            configSettings[FileSettingsKeys.units.ToString()] = "m";

            return configObject;
        }
    }
}


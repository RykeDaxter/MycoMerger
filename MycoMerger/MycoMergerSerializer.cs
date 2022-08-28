using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace MycoMerger
{
    /// <summary>
    /// MycoMerger Object for use with serialization.
    /// </summary>
    [DataContract]
    internal class MycoMergerObject
    {
        /// <summary>
        /// MycoMerger Data of a dictionary with key targetCardName and value resultCardName/MergeResult.
        /// </summary>
        [DataMember]
        internal Dictionary<string, string> MycoMerger { get; set; }
    }

    /// <summary>
    /// Serializer for MycoMerger between its formatted strings and dictionaries.
    /// </summary>
    /// <remarks>
    /// The MycoMerger extended property value in CardInfo is a custom-formatted string representation of the MycoMergerObject containing a dictionary.
    /// The MycoMerger data custom format of a string is: "TargetCard:ResultCard,TargetCard2:ResultCard2,…"
    /// </remarks>
    public static class MycoMergerSerializer
    {
        public static string JsonMMObjPrefix = $"{{\"{Plugin.PluginName}\":{{";
        public static string JsonMMObjPostfix = "}}";

        /// <summary>
        /// Returns formatted string as a dictionary. Removes whitespace from string in the process to ensure compatibility.
        /// </summary>
        /// <param name="MMDataString"></param>
        /// <returns>Dictionary representation of given string representing MycoMergerObject's dictionary value.</returns>
        public static Dictionary<string, string> MMDataStringToDict(string MMDataString)
        {
            if (string.IsNullOrWhiteSpace(MMDataString))
            {
                Plugin.Log.LogWarning($"A given formatted string representation of MycoMerger data was null, empty, or whitespace.");
                return new Dictionary<string, string>();
            }
            MMDataString = string.Join("", MMDataString.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

            try
            {
                string tempString = JsonMMObjPrefix;
                string[] tempStringArray;
                foreach (string mergeCombination in MMDataString.Split(','))
                {
                    tempStringArray = mergeCombination.Split(new[] { ':' }, 2);     // Split on the first colon, two substrings max
                    tempString = string.Concat(tempString, $"\"{tempStringArray[0]}\":\"{tempStringArray[1]}\",");
                }
                MMDataString = string.Concat(tempString.Remove(tempString.Length - 1), JsonMMObjPostfix);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"{e.GetType()} occurred when attempting to JSON-format the string: [{MMDataString}]. Error Message: {e.Message}");
                return new Dictionary<string, string>();
            }
            MemoryStream mStream = new(Encoding.Unicode.GetBytes(MMDataString));
            DataContractJsonSerializer jsonSerializer = new(typeof(MycoMergerObject),
                new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
            Dictionary<string, string> dict;

            mStream.Position = 0;

            try
            {
                dict = ((MycoMergerObject)jsonSerializer.ReadObject(mStream)).MycoMerger;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"{e.GetType()} occurred when attempting to JSON-serialize the string: [{MMDataString}]. Error Message: {e.Message}");
                //Plugin.Log.LogWarning($"Stack Trace: {e.StackTrace}");
                return new Dictionary<string, string>();
            }
            finally
            {
                mStream.Close();
            }

            return dict;
        }

        /// <summary>
        /// Returns dictionary as a formatted string. The string is the representation of the MycoMergerObject's dictionary value.
        /// </summary>
        /// <param name="dict"></param>
        /// <returns>String representation of MycoMergerObject's dictionary value equivalent to passed dictionary.</returns>
        public static string DictToMMDataString(Dictionary<string, string> dict)
        {
            if (dict == null)
            {
                Plugin.Log.LogWarning($"A given Dictionary representation of MergerData was null.");
                return "";
            }

            string MMDataString;
            MycoMergerObject mmObj = new();
            MemoryStream mStream = new();
            StreamReader streamReader;
            DataContractJsonSerializer jsonSerializer = new(typeof(MycoMergerObject),
                new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });

            mmObj.MycoMerger = dict;
            jsonSerializer.WriteObject(mStream, mmObj);
            mStream.Position = 0;
            streamReader = new(mStream);
            MMDataString = streamReader.ReadToEnd();
            streamReader.Close();
            mStream.Close();

            MMDataString = string.Join("", MMDataString.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

            try
            {
                MMDataString = MMDataString.Substring(JsonMMObjPrefix.Length, MMDataString.Length - JsonMMObjPrefix.Length - JsonMMObjPostfix.Length).Replace("\"", "");
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"{e.GetType()} occurred when attempting to MMData-format the string: [{MMDataString}]. Error Message: {e.Message}");
                return "";
            }

            return MMDataString;
        }
    }
}

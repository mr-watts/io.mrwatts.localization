#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace MrWatts.Internal.Localization.Editor
{
    public sealed class LocalizationKeyHelper : EditorWindow
    {
        private class Row
        {
            public long ID { get; }
            public string Key { get; }
            public IReadOnlyList<string> Variables { get; }
            public Row(long id, string key, List<string> variables)
            {
                ID = id;
                Key = key;
                Variables = variables;
            }
        }

        private Vector2 scrollPosition;
        private string tableGUID;
        private const string PATH_FOLDER = "Assets/MrWatts.Localization/Generated/";
        private const string TABLE_GUID_EDITOR_PREFS_KEY = "tableGUID";
        private const string PATH_FILE_KEYS = PATH_FOLDER + "LocalizationKey.cs";
        private const string PATH_FILE_VARIABLES = PATH_FOLDER + "LocalizationVariable.cs";

        [MenuItem("Mr. Watts/Windows/LocalizationKeyHelper")]
        public static void ShowWindow()
        {
            GetWindow(typeof(LocalizationKeyHelper));
        }

        private void OnGUI()
        {
            tableGUID = EditorPrefs.GetString(TABLE_GUID_EDITOR_PREFS_KEY);

            if (tableGUID == string.Empty)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Please reference a table from which you want to generate localization keys.");


                var stringTableField = (StringTableCollection)EditorGUILayout.ObjectField(null, typeof(StringTableCollection), true, GUILayout.MaxWidth(300));
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetString(TABLE_GUID_EDITOR_PREFS_KEY, stringTableField.SharedData.TableCollectionNameGuid.ToString());
                }

                GUIUtility.ExitGUI();
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                var stringTable = LocalizationEditorSettings.GetStringTableCollection(new Guid(tableGUID));
                EditorGUILayout.Space(10);
                var field = (StringTableCollection)EditorGUILayout.ObjectField(stringTable, typeof(StringTableCollection), true, GUILayout.MaxWidth(300));
                if (stringTable == null)
                {
                    Debug.LogWarning("Found a reference to an object that doesn't exist anymore. Maybe it is manually removed. Removing reference.");
                    EditorPrefs.SetString(TABLE_GUID_EDITOR_PREFS_KEY, string.Empty);
                    GUIUtility.ExitGUI();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (field == null)
                    {
                        EditorPrefs.SetString(TABLE_GUID_EDITOR_PREFS_KEY, string.Empty);
                    }
                    else
                    {
                        EditorPrefs.SetString(TABLE_GUID_EDITOR_PREFS_KEY, field.SharedData.TableCollectionNameGuid.ToString());
                    }
                }

                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(10);
                List<Row> rows = new List<Row>();

                foreach (LocalizationTableCollection.Row<StringTableEntry> item in stringTable.GetRowEnumerator())
                {
                    List<string>[] variables = new List<string>[item.TableEntries.Length];

                    for (int i = 0; i < item.TableEntries.Length; i++)
                    {
                        variables[i] = new List<string>();

                        if (item.TableEntries[i] != null)
                        {
                            foreach (Match match in Regex.Matches(item.TableEntries[i].Value, @"\{([^}]*)\}"))
                            {
                                variables[i].Add(match.Groups[1].Value.Split(':')[0]);
                            }

                            variables[i].Sort();

                            if (variables[i].Count != variables[0].Count)
                            {
                                Debug.LogError($"Not all entries of '{item.KeyEntry.Key}' have the same amount of variables.");
                            }
                            else if (!variables[i].SequenceEqual(variables[0]))
                            {
                                Debug.LogError($"Not all entries of '{item.KeyEntry.Key}' have the same variables. Doublecheck for typos!");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Some of the TableEntries of '{item.KeyEntry.Key}' are empty");
                        }
                    }

                    rows.Add(new Row(item.KeyEntry.Id, item.KeyEntry.Key, variables[0].Distinct().ToList()));
                }

                rows = rows.OrderBy(x => x.Key).ToList();

                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Table guid", EditorStyles.boldLabel, GUILayout.MaxWidth(100));
                EditorGUILayout.LabelField(tableGUID.ToString());
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(3);

                if (GUILayout.Button("Generate C# files", GUILayout.MaxWidth(200)))
                {
                    CreateCSharpFile(GetCSharpTextKeys(rows), PATH_FILE_KEYS);
                    CreateCSharpFile(GetCSharpTextVariables(rows), PATH_FILE_VARIABLES);
                }

                EditorGUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Key", EditorStyles.boldLabel, GUILayout.MaxWidth(300));
                EditorGUILayout.LabelField("KeyID", EditorStyles.boldLabel, GUILayout.MaxWidth(200));
                EditorGUILayout.LabelField("Variables", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(3);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                foreach (Row row in rows)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel(row.Key, GUILayout.MaxWidth(300), GUILayout.MaxHeight(20));
                    EditorGUILayout.SelectableLabel(row.ID.ToString(), GUILayout.MaxWidth(200), GUILayout.MaxHeight(20));

                    StringBuilder variableStringBuilder = new StringBuilder();
                    for (int i = 0; i < row.Variables.Count; i++)
                    {
                        variableStringBuilder.Append(row.Variables[i]);
                        if (i < row.Variables.Count - 1)
                        {
                            variableStringBuilder.Append(", ");
                        }
                    }

                    EditorGUILayout.SelectableLabel(variableStringBuilder.ToString(), GUILayout.MaxHeight(20));

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(10);
                EditorGUILayout.EndHorizontal();
            }
        }

        private string GetCSharpTextKeys(List<Row> rows)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("namespace MrWatts.Internal.Localization");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("    /// <summary>");
            stringBuilder.AppendLine("    /// This class is automatically generated by the LocalizationKeyHelper");
            stringBuilder.AppendLine("    /// Do not edit this file, any changes will be overridden");
            stringBuilder.AppendLine("    /// </summary>");
            stringBuilder.AppendLine("    public sealed class LocalizationKey");
            stringBuilder.AppendLine("    {");

            foreach (Row row in rows)
            {
                stringBuilder.Append("        public const long ").Append(row.Key.ToUpper()).Append(" = ").Append(row.ID).AppendLine(";");
            }

            stringBuilder.AppendLine("    }");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        private string GetCSharpTextVariables(List<Row> rows)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("namespace MrWatts.Internal.Localization");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("    /// <summary>");
            stringBuilder.AppendLine("    /// This class is automatically generated by the LocalizationKeyHelper");
            stringBuilder.AppendLine("    /// Do not edit this file, any changes will be overridden");
            stringBuilder.AppendLine("    /// </summary>");
            stringBuilder.AppendLine("    public sealed class LocalizationVariable");
            stringBuilder.AppendLine("    {");

            foreach (Row row in rows)
            {
                foreach (string variable in row.Variables)
                {
                    stringBuilder.Append("        public const string ").Append(row.Key.ToUpper()).Append("_").Append(variable.ToUpper()).Append(" = \"").Append(variable).AppendLine("\";");
                }
            }

            stringBuilder.AppendLine("    }");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        private void CreateCSharpFile(string text, string fileName)
        {
            if (!File.Exists(PATH_FOLDER))
            {
                Directory.CreateDirectory(PATH_FOLDER);
            }

            using (StreamWriter outfile = new StreamWriter(fileName))
            {
                outfile.Write(text);
            }

            Debug.Log($"Created C# File at location '{fileName}' with content:\n\n{text}");
            AssetDatabase.Refresh();
        }
    }
}
#endif
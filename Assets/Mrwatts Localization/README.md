# Table of Contents

[[_TOC_]]

# Importing the package

Add this scoped registry.

```json
"scopedRegistries": [
    {
      "name": "Mr. Watts UPM Registry",
      "url": "https://gitlab.com/api/v4/projects/27157125/packages/npm/",
      "scopes": [
        "io.mrwatts"
      ]
    }
  ]
```

Add the following dependency in the **manifest.json** file in the "Packages" folder.

*"io.mrwatts.localization": "1.0.0"*

The version number should be the latest version of the package (unless you want to target an older version on purpose).


# Creating LocalizationKeys

## Setting up the unity localization package

Follow the [quick start guide](https://docs.unity3d.com/Packages/com.unity.localization@1.0/manual/QuickStartGuideWithVariants.html) to set up your project to use localization keys.

## Generating the keys

Under _Assets/Localization/StringTables_, select the `Default Shared Data` object and copy it's Table collection name guid string. You can find this field in Untiy debug mode.
Open the LocalizationTableName.cs script and paste the copied guid in the `new Guid(<Place guid here>)` line.

Go to Unity editor and run the `Mr. Watts/Windows/LocalizationKeyHelper` script.

You can find your localization keys generated under `Scripts/generated/LocalizationKeys`.
They can now be referenced trough the LocalizationManager.


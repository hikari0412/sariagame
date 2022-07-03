using UnityEngine;

namespace PixelCrushers.EasySaveSupport
{

    /// <summary>
    /// This is an example SavedGameDataStorer for Moodkie's Easy Save 3.
    /// Add it to your Save System in place of any existing storers such as
    /// PlayerPrefsSavedGameDataStorer or DiskSavedGameDataStorer.
    /// </summary>
    public class ESSavedGameDataStorer : SavedGameDataStorer
    {
        [Tooltip("Tell Easy Save to use a separate file for each saved game.")]
        public bool useSeparateFiles = false;

        [Tooltip("Filename to contain save data, including path. If Use Separate Files is ticked, appends slot number each file.")]
        public string filename = "save";

        public virtual string GetFilename(int slotNumber)
        {
            return useSeparateFiles ? (filename + "_" + slotNumber) : filename;
        }

        public virtual string GetKey(int slotNumber)
        {
            return useSeparateFiles ? "pixelCrushers" : ("pixelCrushers_" + slotNumber);
        }

        public override bool HasDataInSlot(int slotNumber)
        {
            return ES3.KeyExists(GetKey(slotNumber), GetFilename(slotNumber));
        }

        public override SavedGameData RetrieveSavedGameData(int slotNumber)
        {
            if (!HasDataInSlot(slotNumber)) return null;
            var s = ES3.Load<string>(GetKey(slotNumber), GetFilename(slotNumber));
            return SaveSystem.Deserialize<SavedGameData>(s);
        }

        public override void StoreSavedGameData(int slotNumber, SavedGameData savedGameData)
        {
            var s = SaveSystem.Serialize(savedGameData);
            ES3.Save<string>(GetKey(slotNumber), s, GetFilename(slotNumber));
        }

        public override void DeleteSavedGameData(int slotNumber)
        {
            if (useSeparateFiles)
            {
                ES3.DeleteFile(GetFilename(slotNumber));
            }
            else
            {
                ES3.DeleteKey(GetKey(slotNumber), GetFilename(slotNumber));
            }
        }
    }
}

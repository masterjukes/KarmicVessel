using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ThunderRoad;
using UnityEngine;

namespace KarmicVessel.Tier3
{
    public class DaikokutenData
    {
        public Stack<string> items;
        public List<CreatureData> creatures;


        



        public void Load()
        {
            Debug.Log("Loading Daikokuten Data!");
            GameManager.platform.TryGetSavePath(out string path);
            if (File.Exists(path + "/" + Player.characterData.ID + ".DaikokutenData"))
            {
                var save = JsonConvert.DeserializeObject<DaikokutenData>(
                    File.ReadAllText(path + "/" + Player.characterData.ID + ".DaikokutenData"), Catalog.GetJsonNetSerializerSettings());
                this.items = save.items;;
                this.creatures = save.creatures;
                return;
            }
            Debug.Log("No save file found!");
            items = new Stack<string>();
            creatures = new List<CreatureData>();
        }
        
        public void SaveToJSON()
        {
            GameManager.platform.TryGetSavePath(out string path);
            Debug.Log("Saving stats to: " + path);
            string contents = JsonConvert.SerializeObject(this, Formatting.Indented, Catalog.GetJsonNetSerializerSettings());
            File.WriteAllText(path + "/" + Player.characterData.ID + ".DaikokutenData", contents);
        }


    }
}